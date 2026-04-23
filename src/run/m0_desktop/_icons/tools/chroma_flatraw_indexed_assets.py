"""
Apply chroma_key_to_rgba_icon to flatraw_NNN.png files in Cursor assets folder.

GenerateImage should save as flatraw_001.png ... flatraw_172.png (1-based index =
line number in icons_manifest.txt). Then run:

  python chroma_flatraw_indexed_assets.py 1 172
  python chroma_flatraw_indexed_assets.py 6 15   # partial range

Requires: assets under CURSOR_ASSETS (default path below).
"""

from __future__ import annotations

import argparse
import importlib.util
import subprocess
import sys
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
ICONS = TOOLS.parent
MANIFEST = ICONS / "icons_manifest.txt"
CHROMA = TOOLS / "chroma_key_to_rgba_icon.py"
FLAT = ICONS / "flat"
# Cursor workspace assets folder where GenerateImage writes flatraw_NNN.png.
DEFAULT_ASSETS = Path(
    r"C:\Users\teres\.cursor\projects\c-Users-teres-source-repos-m-src-run\assets"
)


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
    p.add_argument("start", type=int, help="1-based manifest line (inclusive)")
    p.add_argument("end", type=int, help="1-based manifest line (inclusive)")
    p.add_argument(
        "--assets",
        type=Path,
        default=DEFAULT_ASSETS,
        help="Folder containing flatraw_NNN.png",
    )
    args = p.parse_args()
    labels = load_manifest()
    n = len(labels)
    if args.start < 1 or args.end > n or args.start > args.end:
        print(f"range must be within 1..{n}", file=sys.stderr)
        return 2
    basename_for = load_basename_fn()
    FLAT.mkdir(parents=True, exist_ok=True)
    for i in range(args.start, args.end + 1):
        label = labels[i - 1]
        src = args.assets / f"flatraw_{i:03d}.png"
        if not src.is_file():
            print(f"missing {src.name} for line {i} {label!r}", file=sys.stderr)
            return 1
        dst = FLAT / basename_for(label)
        subprocess.run(
            [sys.executable, str(CHROMA), str(src), str(dst)],
            check=True,
        )
        print(f"{i}/{n} -> {dst.name}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
