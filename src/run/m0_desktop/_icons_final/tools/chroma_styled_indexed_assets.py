"""
Apply chroma_key_to_rgba_icon to GenerateImage outputs for a named style.

Expects files: flatraw_<style>_<NNN>.png in the Cursor assets folder, where NNN is
the 1-based line index in icons_manifest.txt (same convention as flatraw_001.png).

Writes: m0_desktop/_icons/<style>/<basename_from_manifest>.png

Style folder names (match GenerateImage filename stem): minimal, outline, glyph,
duotone, gradient, glass, neumorphic, 3d, isometric, pixel_art, cartoon,
skeuomorphic, vector_badge.

Example after generating assets/flatraw_minimal_001.png .. 010.png:
  python chroma_styled_indexed_assets.py minimal 1 10
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
    p.add_argument(
        "style",
        help="Folder name under _icons/, must match flatraw_<style>_NNN.png stem",
    )
    p.add_argument("start", type=int, help="1-based manifest line inclusive")
    p.add_argument("end", type=int, help="1-based manifest line inclusive")
    p.add_argument("--assets", type=Path, default=DEFAULT_ASSETS)
    args = p.parse_args()
    style = args.style.strip().replace("/", "_").replace(" ", "_")
    if not style or ".." in style:
        print("invalid style", file=sys.stderr)
        return 2

    labels = load_manifest()
    n = len(labels)
    if args.start < 1 or args.end > n or args.start > args.end:
        print(f"range must be within 1..{n}", file=sys.stderr)
        return 2

    out_dir = ICONS / style
    out_dir.mkdir(parents=True, exist_ok=True)
    basename_for = load_basename_fn()

    for i in range(args.start, args.end + 1):
        src = args.assets / f"flatraw_{style}_{i:03d}.png"
        if not src.is_file():
            print(f"missing {src.name} (line {i} {labels[i - 1]!r})", file=sys.stderr)
            return 1
        label = labels[i - 1]
        dst = out_dir / basename_for(label)
        subprocess.run(
            [sys.executable, str(CHROMA), str(src), str(dst)],
            check=True,
        )
        print(f"{style} {i}/{n} -> {dst.relative_to(ICONS)}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
