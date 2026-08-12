namespace LeatherNesting.Geometry.Features;

/// <summary>Validates notch features against their contour context.</summary>
public sealed class NotchValidator
{
    private readonly ToleranceProfile _tolerance;

    public NotchValidator(ToleranceProfile? tolerance = null)
    {
        _tolerance = tolerance ?? ToleranceProfile.Default;
    }

    /// <summary>Validates a notch against its contour. Returns diagnostics for any issues found.</summary>
    public NotchValidationResult Validate(NotchFeature notch, Loop2D contour, IReadOnlyList<NotchFeature> existingNotches)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        ValidateDimensions(notch, errors, warnings);
        ValidateAnchorPosition(notch, contour, errors, warnings);
        ValidateOverlap(notch, existingNotches, errors, warnings);
        ValidateMaterialClearance(notch, contour, errors, warnings);

        return new NotchValidationResult(
            errors.Count == 0,
            errors,
            warnings);
    }

    private void ValidateDimensions(NotchFeature notch, List<string> errors, List<string> warnings)
    {
        if (notch.Width <= 0 || double.IsNaN(notch.Width) || double.IsInfinity(notch.Width))
            errors.Add($"剪口宽度无效：{notch.Width}。");

        if (notch.Depth <= 0 || double.IsNaN(notch.Depth) || double.IsInfinity(notch.Depth))
            errors.Add($"剪口深度无效：{notch.Depth}。");

        if (notch.Width > 100) // arbitrary large threshold
            warnings.Add($"剪口宽度 {notch.Width:F1}mm 异常大，请确认。");

        if (notch.Depth > 50)
            warnings.Add($"剪口深度 {notch.Depth:F1}mm 异常大，请确认。");
    }

    private void ValidateAnchorPosition(NotchFeature notch, Loop2D contour, List<string> errors, List<string> warnings)
    {
        if (contour.Length <= 0)
        {
            errors.Add("轮廓长度为 0，无法放置剪口。");
            return;
        }

        if (notch.AnchorArcLength < 0 || notch.AnchorArcLength > contour.Length)
        {
            errors.Add($"剪口锚点位置 {notch.AnchorArcLength:F3}mm 超出轮廓长度 {contour.Length:F3}mm。");
            return;
        }

        var anchorPoint = contour.PointAt(notch.AnchorArcLength / contour.Length);

        // Check proximity to corners: if anchor is within width/2 of a curve endpoint, warn
        var halfWidth = notch.Width / 2;
        foreach (var curve in contour.Curves)
        {
            if (curve.StartPoint.DistanceTo(anchorPoint) < halfWidth + _tolerance.TopologyToleranceMm)
                warnings.Add("剪口锚点靠近轮廓拐角，可能导致剪口超出实际材料。");
            if (curve.EndPoint.DistanceTo(anchorPoint) < halfWidth + _tolerance.TopologyToleranceMm)
                warnings.Add("剪口锚点靠近轮廓拐角，可能导致剪口超出实际材料。");
        }
    }

    private void ValidateOverlap(NotchFeature notch, IReadOnlyList<NotchFeature> existing, List<string> errors, List<string> warnings)
    {
        var halfWidth = notch.Width / 2;
        foreach (var other in existing)
        {
            if (ReferenceEquals(notch, other) || other.ContourId != notch.ContourId) continue;
            var centerDist = Math.Abs(notch.AnchorArcLength - other.AnchorArcLength);
            var minSafeDistance = halfWidth + other.Width / 2 + _tolerance.TopologyToleranceMm;

            if (centerDist < minSafeDistance)
            {
                errors.Add($"剪口与位于 {other.AnchorArcLength:F3}mm 的另一剪口重叠。");
                return;
            }

            if (centerDist < minSafeDistance * 2)
                warnings.Add($"剪口与相邻剪口（{other.AnchorArcLength:F3}mm）间距较小。");
        }
    }

    private void ValidateMaterialClearance(NotchFeature notch, Loop2D contour, List<string> errors, List<string> warnings)
    {
        // Check if the notch depth exceeds local material thickness
        // Stage 2: simplified check — depth should not exceed 1/3 of the minimum distance
        // from the anchor to the opposite side of the contour
        var anchorPoint = contour.PointAt(notch.AnchorArcLength / contour.Length);

        // Find the minimum distance from the anchor to any point on the contour
        // except the immediate neighborhood
        var minClearance = double.MaxValue;
        var searchRadius = contour.Length * 0.1; // exclude 10% of contour on each side

        for (var t = 0.0; t <= 1.0; t += 0.01)
        {
            var arcLen = t * contour.Length;
            var distToAnchor = Math.Abs(arcLen - notch.AnchorArcLength);
            if (distToAnchor < searchRadius) continue;

            var point = contour.PointAt(t);
            var dist = anchorPoint.DistanceTo(point);
            if (dist < minClearance) minClearance = dist;
        }

        if (notch.Depth > minClearance / 2)
            warnings.Add($"剪口深度 {notch.Depth:F1}mm 超过局部材料厚度的一半，可能导致穿孔。");

        if (notch.Depth > minClearance)
            errors.Add($"剪口深度 {notch.Depth:F1}mm 超过局部材料厚度 {minClearance:F1}mm，会导致穿孔。");
    }

    }

public sealed record NotchValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);