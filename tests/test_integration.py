import json
import subprocess
import tempfile
import unittest
from pathlib import Path


class DemoIntegrationTests(unittest.TestCase):
    def test_generates_dxf_png_and_summary_for_all_three_leathers(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            output_dir = Path(temporary_directory) / "output"
            subprocess.run(
                ["python3", "leather_nesting_demo.py", "--output-dir", str(output_dir)],
                check=True,
                text=True,
                capture_output=True,
                timeout=5,
            )

            summary = json.loads((output_dir / "summary.json").read_text(encoding="utf-8"))
            self.assertEqual(set(summary["runs"]), {"2000x1000", "2000x4000", "2000x9000"})
            for name, run in summary["runs"].items():
                self.assertTrue((output_dir / f"{name}.dxf").is_file())
                self.assertTrue((output_dir / f"{name}.png").is_file())
                self.assertGreaterEqual(run["utilization_percent"], 0)
                self.assertGreater(len(run["placed_indices"]), 9)
                self.assertEqual(sum(run["placed_counts"].values()), len(run["placed_indices"]))
