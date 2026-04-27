"""
Run chroma_key_to_rgba_icon for every flatraw_<style>_<NNN>.png found in assets.

Use after dropping batches from GenerateImage. Example:
  python chroma_all_flatraw_assets_in_folder.py
  python chroma_all_flatraw_assets_in_folder.py --only-style glyph
"""

from __future__ import annotations

import argparse
import importlib.util
import re
import subprocess
import sys
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
ICONS = TOOLS.parent
MANIFEST = ICONS / "icons_manifest.txt"
CHROMA = TOOLS / "chroma_key_to_rgba_icon.py"
DEFAULT_ASSETS = Path(
    r"C:\Users\teres\.cursor\projects\c-Users-teres-source-repos-m-src-run\assets"
)
PAT = re.compile(r"^flatraw_([a-z0-9_]+)_(\d{3})\.png$", re.I)


def load_manifest() -> list[str]:
    lines: list[str] = []
    for raw in MANIFEST.read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        lines.append(line)
    return lines


def load_basename_fn():
    p = TOOLS / "flat_rgba_from_checkerboard.py"
    spec = importlib.util.spec_from_file_location("fr", p)
    m = importlib.util.module_from_spec(spec)
    assert spec.loader
    spec.loader.exec_module(m)
    return m.output_png_basename_for_label


def main() -> int:
    p = argparse.ArgumentParser()
    p.add_argument("--assets", type=Path, default=DEFAULT_ASSETS)
    p.add_argument(
        "--only-style",
        default=None,
        help="Process only this style stem (e.g. glyph, duotone)",
    )
    args = p.parse_args()
    labels = load_manifest()
    n = len(labels)
    basename_for = load_basename_fn()
    done = 0
    for f in sorted(args.assets.glob("flatraw_*.png")):
        m = PAT.match(f.name)
        if not m:
            continue
        style, idx_s = m.group(1), m.group(2)
        if args.only_style and style != args.only_style:
            continue
        i = int(idx_s)
        if i < 1 or i > n:
            print("skip bad index", f.name, file=sys.stderr)
            continue
        out_dir = ICONS / style
        out_dir.mkdir(parents=True, exist_ok=True)
        dst = out_dir / basename_for(labels[i - 1])
        subprocess.run(
            [sys.executable, str(CHROMA), str(f), str(dst)],
            check=True,
        )
        done += 1
        print(f"{style} {i}/{n} -> {dst.relative_to(ICONS)}")
    print("processed", done)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
