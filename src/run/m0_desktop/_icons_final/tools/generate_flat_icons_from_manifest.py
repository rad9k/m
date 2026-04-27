"""
Build canonical flat-style RGBA PNGs (256x256) for every line in icons_manifest.txt.

These are the sources for render_styled_icons_from_flat.py (other style folders).
Simple geometry + monogram; not meant as final art direction, only a consistent baseline.
"""

from __future__ import annotations

import importlib.util
import os
import re
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

TOOLS = Path(__file__).resolve().parent
ICONS_ROOT = TOOLS.parent
MANIFEST = ICONS_ROOT / "icons_manifest.txt"
FLAT_DIR = ICONS_ROOT / "flat"

SIDE = 256
BRAND = (43, 104, 240, 255)  # blue
FILL_INK = (255, 255, 255, 255)


def load_basename_fn():
    path = TOOLS / "flat_rgba_from_checkerboard.py"
    spec = importlib.util.spec_from_file_location("flat_rgba_names", path)
    if spec is None or spec.loader is None:
        raise RuntimeError("cannot load flat_rgba_from_checkerboard")
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod.output_png_basename_for_label


def load_manifest_lines() -> list[str]:
    lines: list[str] = []
    for raw in MANIFEST.read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        lines.append(line)
    return lines


def resolve_font_file() -> Path | None:
    roots = [
        Path(os.environ.get("SYSTEMROOT", r"C:\Windows")) / "Fonts",
        Path(r"C:\Windows\Fonts"),
    ]
    candidates = [
        "segoeuib.ttf",
        "segoeui.ttf",
        "arialbd.ttf",
        "arial.ttf",
    ]
    for root in roots:
        if not root.is_dir():
            continue
        for name in candidates:
            p = root / name
            if p.is_file():
                return p
    return None


def abbrev_for_label(label: str) -> str:
    """Short text for the glyph (2-4 chars)."""
    s = label.strip()
    if s in ("(?<ANY>)", "(?<LAST>)"):
        return "ANY" if "ANY" in s else "LST"
    s = re.sub(r"[$]+", "", s)
    parts = re.findall(r"[A-Z][a-z]+|[A-Z]+(?=[A-Z][a-z]|\b)|\d+", s)
    if len(parts) >= 2:
        mono = "".join(p[0].upper() for p in parts[:5])
        return mono[:4]
    alnum = re.sub(r"[^a-zA-Z0-9]", "", s)
    if not alnum:
        return "?"
    alnum = alnum.upper()
    if len(alnum) <= 4:
        return alnum
    return (alnum[:2] + alnum[-2:])[:4]


def draw_flat_icon(label: str, font_file: Path | None) -> Image.Image:
    im = Image.new("RGBA", (SIDE, SIDE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    pad = 28
    draw.ellipse((pad, pad, SIDE - pad, SIDE - pad), fill=BRAND)

    text = abbrev_for_label(label)
    max_w = SIDE - 2 * (pad + 36)
    f: ImageFont.FreeTypeFont | ImageFont.ImageFont
    if font_file is not None:
        f = ImageFont.truetype(str(font_file), size=40)
        for trial in range(72, 17, -2):
            try:
                f = ImageFont.truetype(str(font_file), size=trial)
            except OSError:
                break
            bbox = draw.textbbox((0, 0), text, font=f, stroke_width=2)
            w, h = bbox[2] - bbox[0], bbox[3] - bbox[1]
            if w <= max_w and h <= max_w * 1.15:
                break
    else:
        f = ImageFont.load_default()

    bbox = draw.textbbox((0, 0), text, font=f, stroke_width=2)
    tw, th = bbox[2] - bbox[0], bbox[3] - bbox[1]
    tx = (SIDE - tw) // 2 - bbox[0]
    ty = (SIDE - th) // 2 - bbox[1]
    draw.text((tx, ty), text, font=f, fill=FILL_INK, stroke_width=2, stroke_fill=(15, 40, 120, 255))
    return im


def main() -> int:
    basename_for = load_basename_fn()
    labels = load_manifest_lines()
    if not labels:
        print("empty manifest", file=sys.stderr)
        return 2
    FLAT_DIR.mkdir(parents=True, exist_ok=True)
    font_file = resolve_font_file()
    for i, label in enumerate(labels, start=1):
        name = basename_for(label)
        out = FLAT_DIR / name
        try:
            icon = draw_flat_icon(label, font_file)
            icon.save(out, format="PNG")
        except Exception as exc:
            print(f"FAIL {name}: {exc}", file=sys.stderr)
            return 1
        if i % 40 == 0 or i == len(labels):
            print(f"{i}/{len(labels)} {name}")
    print(f"Wrote {len(labels)} icons to {FLAT_DIR}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
