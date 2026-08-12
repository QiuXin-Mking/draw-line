namespace LeatherNesting.Desktop.ViewModels;

public enum ImportWizardStep { SelectFile, UnitReview, Recognition, RepairDecision, Committed }

/// <summary>Stage 1 session state only. It cannot mutate a project until a later Commit use case approves it.</summary>
public sealed class ImportWizardViewModel
{
    public ImportWizardStep Step { get; private set; } = ImportWizardStep.SelectFile;
    public string? SelectedPath { get; private set; }
    public void Select(string path) { SelectedPath = path; Step = ImportWizardStep.UnitReview; }
    public void Cancel() { SelectedPath = null; Step = ImportWizardStep.SelectFile; }
}
