"""
Re-render each flat RGBA icon in multiple visual styles, bake onto the same checkerboard
pattern, then run flat_rgba_from_checkerboard.process_rgb_u8_to_icon for clean 256x256 RGBA.

Usage:
  python m0_desktop/_icons/tools/render_styled_icons_from_flat.py
  python .../render_styled_icons_from_flat.py isometric pixel_art   # only listed styles
"""

from __future__ import annotations

import importlib.util
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageFilter

TOOLS = Path(__file__).resolve().parent
ICONS_ROOT = TOOLS.parent
FLAT_DIR = ICONS_ROOT / "flat"
MANIFEST = ICONS_ROOT / "icons_manifest.txt"

STYLES: tuple[str, ...] = (
    "minimal",
    "outline",
    "glyph",
    "duotone",
    "gradient",
    "glass",
    "neumorphic",
    "3d",
    "isometric",
    "pixel_art",
    "cartoon",
    "skeuomorphic",
    "vector_badge",
)

FIXED_CHECKER: tuple[int, int, int] = (46, 25, 18)
RENDER_SIDE = 1024
SUPERSAMPLE = 2

BLUE = np.array([0.12, 0.45, 0.95], dtype=np.float32)
BLUE2 = np.array([0.45, 0.25, 0.95], dtype=np.float32)
INK = np.array([0.08, 0.10, 0.14], dtype=np.float32)


def load_matte_module():
    path = TOOLS / "flat_rgba_from_checkerboard.py"
    spec = importlib.util.spec_from_file_location("flat_matte", path)
    if spec is None or spec.loader is None:
        raise RuntimeError("cannot load flat_rgba_from_checkerboard")
    mod = importlib.util.module_from_spec(spec)
    sys.modules["flat_matte"] = mod
    spec.loader.exec_module(mod)
    return mod


def load_manifest_labels() -> list[str]:
    lines: list[str] = []
    for raw in MANIFEST.read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        lines.append(line)
    return lines


def checker_rgb(height: int, width: int) -> np.ndarray:
    period, ox, oy = FIXED_CHECKER
    yy, xx = np.indices((height, width), dtype=np.int32)
    xs = (xx + ox) // period
    ys = (yy + oy) // period
    group = ((xs + ys) & 1).astype(bool)
    light = np.array([250, 250, 250], dtype=np.float32) / 255.0
    dark = np.array([200, 200, 205], dtype=np.float32) / 255.0
    return np.where(group[..., None], dark, light)


def composite_on_checker(
    foreground_rgb: np.ndarray, alpha: np.ndarray, checker: np.ndarray
) -> np.ndarray:
    a = np.clip(alpha[..., None], 0.0, 1.0)
    return foreground_rgb * a + checker * (1.0 - a)


def rgba_to_arrays(rgba: Image.Image) -> tuple[np.ndarray, np.ndarray]:
    rgba = rgba.convert("RGBA")
    arr = np.asarray(rgba).astype(np.float32) / 255.0
    return arr[:, :, :3], arr[:, :, 3]


def upscale_rgba(im: Image.Image, max_side: int) -> Image.Image:
    w, h = im.size
    s = max_side / float(max(w, h))
    nw = max(1, int(round(w * s)))
    nh = max(1, int(round(h * s)))
    return im.resize((nw, nh), Image.Resampling.LANCZOS)


def bbox_from_alpha(a: np.ndarray, thresh: float = 0.05) -> tuple[int, int, int, int]:
    ys, xs = np.where(a > thresh)
    if ys.size == 0:
        return 0, a.shape[0] - 1, 0, a.shape[1] - 1
    return int(ys.min()), int(ys.max()), int(xs.min()), int(xs.max())


def stroke_mask(alpha_u8: np.ndarray, radius: int) -> np.ndarray:
    im = Image.fromarray(alpha_u8, mode="L")
    k = max(3, radius * 2 + 1)
    dil = np.asarray(im.filter(ImageFilter.MaxFilter(k)))
    ero = np.asarray(im.filter(ImageFilter.MinFilter(k)))
    return (dil > 12) & (ero < 240)


def prepare_style_layers(
    style: str, rgb: np.ndarray, alpha: np.ndarray, checker: np.ndarray, rgba_big: Image.Image
) -> tuple[np.ndarray, np.ndarray]:
    """
    Return (foreground_rgb, alpha_for_composite), both float 0..1.
    """
    h, w, _ = rgb.shape
    a = np.clip(alpha, 0.0, 1.0)
    a_u8 = (a * 255.0).round().astype(np.uint8)

    if style == "minimal":
        fg = np.ones((h, w, 3), dtype=np.float32) * BLUE
        return fg, a

    if style == "glyph":
        dil = (
            np.asarray(Image.fromarray(a_u8, mode="L").filter(ImageFilter.MaxFilter(7))).astype(
                np.float32
            )
            / 255.0
        )
        fg = np.ones((h, w, 3), dtype=np.float32) * BLUE
        return fg, np.clip(dil, 0.0, 1.0)

    if style == "outline":
        ero = (
            np.asarray(Image.fromarray(a_u8, mode="L").filter(ImageFilter.MinFilter(11))).astype(
                np.float32
            )
            / 255.0
        )
        inner = ero > 0.55
        ring = (a > 0.06) & (~inner)
        fg = checker.copy()
        fg[ring] = INK
        return fg, a

    if style == "duotone":
        yy = np.linspace(0.0, 1.0, h, dtype=np.float32)[:, None]
        fg = (1.0 - yy) * BLUE + yy * BLUE2
        return fg, a

    if style == "gradient":
        y0, y1, x0, x1 = bbox_from_alpha(a)
        cy = (y0 + y1) / 2.0
        cx = (x0 + x1) / 2.0
        yy, xx = np.indices((h, w), dtype=np.float32)
        dist = np.sqrt((yy - cy) ** 2 + (xx - cx) ** 2)
        if np.any(a > 0.05):
            dmax = float(np.max(dist[a > 0.05]) + 1e-3)
        else:
            dmax = float(np.max(dist) + 1e-3)
        t = np.clip(dist / dmax, 0.0, 1.0)[..., None]
        c0 = np.array([0.15, 0.55, 1.0], dtype=np.float32)
        c1 = np.array([0.25, 0.85, 0.55], dtype=np.float32)
        fg = (1.0 - t) * c0 + t * c1
        return fg, a

    if style == "glass":
        base = np.ones((h, w, 3), dtype=np.float32) * BLUE
        yy, xx = np.indices((h, w), dtype=np.float32)
        stripe = np.exp(-(((xx - 0.35 * w - yy * 0.25) ** 2) / (2 * (0.08 * w) ** 2)))
        gloss = stripe[..., None] * np.array([0.55, 0.65, 0.95], dtype=np.float32)
        fg = np.clip(base + gloss * 0.55, 0.0, 1.0)
        return fg, a

    if style == "neumorphic":
        panel = np.ones((h, w, 3), dtype=np.float32) * np.array([0.88, 0.89, 0.91], dtype=np.float32)
        bump = np.ones((h, w, 3), dtype=np.float32) * np.array([0.93, 0.94, 0.96], dtype=np.float32)
        fg = panel.copy()
        fg[a > 0.08] = bump[a > 0.08]
        return fg, a

    if style == "3d":
        yy, xx = np.indices((h, w), dtype=np.float32)
        shade = 0.72 + 0.28 * ((xx + yy) / (w + h + 1e-3))
        fg = BLUE * shade[..., None]
        return fg, a

    if style == "isometric":
        im = Image.fromarray(
            np.dstack([(rgb * 255.0).clip(0, 255).astype(np.uint8), a_u8]), "RGBA"
        )
        w0, h0 = im.size
        skew = 0.28
        im2 = im.transform(
            (w0, h0),
            Image.AFFINE,
            (1, skew, -skew * h0 * 0.12, 0, 1, 0),
            resample=Image.Resampling.BICUBIC,
            fillcolor=(0, 0, 0, 0),
        )
        r2, a2 = rgba_to_arrays(im2)
        return r2, np.clip(a2, 0.0, 1.0)

    if style == "pixel_art":
        small = max(2, min(h, w) // 22)
        im = Image.fromarray(
            np.dstack([(rgb * 255.0).clip(0, 255).astype(np.uint8), a_u8]), "RGBA"
        )
        im2 = im.resize((max(1, w // small), max(1, h // small)), Image.Resampling.NEAREST)
        im3 = im2.resize((w, h), Image.Resampling.NEAREST)
        r2, a2 = rgba_to_arrays(im3)
        return r2, a2

    if style == "cartoon":
        ring = stroke_mask(a_u8, radius=5)
        fg = np.clip(rgb * 1.1 + 0.02, 0.0, 1.0)
        fg[ring] = INK
        return fg, a

    if style == "skeuomorphic":
        rng = np.random.default_rng(0)
        noise = rng.normal(0.0, 0.035, (h, w, 3)).astype(np.float32)
        leather = np.array([0.45, 0.32, 0.22], dtype=np.float32)
        tex = np.clip(leather + noise, 0.0, 1.0)
        fg = np.clip(rgb * 0.35 + tex * 0.65, 0.0, 1.0)
        return fg, a

    if style == "vector_badge":
        cy, cx = h // 2, w // 2
        rad = int(min(h, w) * 0.42)
        yy, xx = np.indices((h, w))
        disk = (yy - cy) ** 2 + (xx - cx) ** 2 <= rad * rad
        fg = checker.copy()
        badge = np.ones((h, w, 3), dtype=np.float32) * np.array([0.12, 0.18, 0.28], dtype=np.float32)
        fg[disk] = badge[disk]
        rw = int(w * 0.58)
        rh = int(h * 0.58)
        im_small = rgba_big.resize((rw, rh), Image.Resampling.LANCZOS)
        r_s, a_s = rgba_to_arrays(im_small)
        ox = (w - rw) // 2
        oy = (h - rh) // 2
        a_s = np.clip(a_s, 0.0, 1.0)
        sub = fg[oy : oy + rh, ox : ox + rw]
        blended = r_s * a_s[..., None] + sub * (1.0 - a_s[..., None])
        fg[oy : oy + rh, ox : ox + rw] = np.where(a_s[..., None] > 1e-3, blended, sub)
        return fg, a

    raise ValueError(f"unknown style {style}")


def build_rgb_u8_for_style(style: str, flat_path: Path) -> np.ndarray:
    im0 = Image.open(flat_path).convert("RGBA")
    im = upscale_rgba(im0, RENDER_SIDE)
    rgb, alpha = rgba_to_arrays(im)
    h, w, _ = rgb.shape
    checker = checker_rgb(h, w)

    fg, a_use = prepare_style_layers(style, rgb, alpha, checker, im)

    baked = composite_on_checker(fg, a_use, checker)
    return (np.clip(baked, 0.0, 1.0) * 255.0).round().astype(np.uint8)


def main() -> int:
    matte = load_matte_module()
    process = matte.process_rgb_u8_to_icon
    basename_for = matte.output_png_basename_for_label

    labels = load_manifest_labels()
    if not labels:
        print("empty manifest", file=sys.stderr)
        return 2

    argv_styles = [s for s in sys.argv[1:] if s in STYLES]
    styles: tuple[str, ...] = tuple(argv_styles) if argv_styles else STYLES

    for style in styles:
        out_dir = ICONS_ROOT / style
        out_dir.mkdir(parents=True, exist_ok=True)
        print(f"=== style {style} ({len(labels)} icons) ===")
        for i, label in enumerate(labels, start=1):
            name = basename_for(label)
            src = FLAT_DIR / name
            if not src.is_file():
                print(f"MISSING flat source {src}", file=sys.stderr)
                continue
            try:
                rgb_u8 = build_rgb_u8_for_style(style, src)
                icon = process(rgb_u8, SUPERSAMPLE, fixed_checker=FIXED_CHECKER)
                icon.save(out_dir / name, format="PNG")
            except Exception as exc:
                print(f"FAIL {style} {name}: {exc}", file=sys.stderr)
                continue
            if i % 25 == 0 or i == len(labels):
                print(f"  {i}/{len(labels)} {name}")
        print(f"done {style}")

    print("All styles finished.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
