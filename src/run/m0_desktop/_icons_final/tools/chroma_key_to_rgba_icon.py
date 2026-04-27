"""
Post-process a raster from an external image generator (solid chroma background).

Default pipeline targets chroma key #FF00FF: fixed key (255,0,255), soft alpha from
distance, magenta screen-kill pass, dematte, then resize. Override with --key-rgb
or --key-from-edges when needed.

Builds soft alpha from RGB distance to the key,
crops to opaque bbox, fits into 256x256 with padding, saves PNG RGBA.

This does not draw icon pixels; it only keys, trims, and resizes.
"""

from __future__ import annotations

import argparse
from pathlib import Path

import numpy as np
from PIL import Image


def edge_median_rgb(rgb: np.ndarray) -> np.ndarray:
    h, w, _ = rgb.shape
    edge = np.concatenate(
        [
            rgb[0].reshape(-1, 3),
            rgb[-1].reshape(-1, 3),
            rgb[:, 0].reshape(-1, 3),
            rgb[:, w - 1].reshape(-1, 3),
        ],
        axis=0,
    )
    return np.median(edge.astype(np.float64), axis=0)


def kill_bright_letterbox_alpha(rgb: np.ndarray, alpha: np.ndarray, level: float) -> np.ndarray:
    """Generator letterboxing often leaves white strips on chroma bg — force transparent."""
    r = rgb[:, :, 0]
    g = rgb[:, :, 1]
    b = rgb[:, :, 2]
    bright = (r >= level) & (g >= level) & (b >= level)
    return np.where(bright, 0.0, alpha)


def magenta_fringe_kill_mask(
    r: np.ndarray, g: np.ndarray, b: np.ndarray, a: np.ndarray
) -> np.ndarray:
    """
    Pixels that look like chroma / purple resample halos, not dominant-blue paint.

    Bicubic often leaves 1px columns with alpha ~0.5–0.8 and R,B high vs G; older
    rules only cleared alpha < 0.45, so those lines survived.
    """
    a = np.clip(a.astype(np.float32), 0.0, 1.0)
    # Classic thin semi-opaque pink ring
    fringe_lo = (r > 108.0) & (b > 108.0) & (g < 158.0) & (a > 0.0) & (a < 0.58)
    # Stronger purple/magenta smear after resize (higher alpha)
    fringe_hi = (
        (r > 118.0)
        & (b > 118.0)
        & (g < 138.0)
        & (a > 0.0)
        & (a < 0.84)
        & (r <= b + 62.0)
    )
    # Not a clean blue foreground (B clearly leads R and G)
    blue_keep = (b >= r + 30.0) & (b >= g + 6.0)
    return (fringe_lo | fringe_hi) & (~blue_keep)


def kill_pink_fringe_alpha(rgb: np.ndarray, alpha: np.ndarray) -> np.ndarray:
    """Remove magenta/purple halos (including post-resize high-alpha slivers)."""
    r = rgb[:, :, 0]
    g = rgb[:, :, 1]
    b = rgb[:, :, 2]
    a = np.clip(alpha.astype(np.float32), 0.0, 1.0)
    kill = magenta_fringe_kill_mask(r, g, b, a)
    return np.where(kill, 0.0, a)


def kill_screen_color_alpha(
    rgb: np.ndarray, alpha: np.ndarray, mode: str
) -> np.ndarray:
    """
    Hard-zero alpha where pixels still match a classic chroma screen, even when
    L2 distance to an edge-estimated key misses (e.g. interior #FF00FF vs border median).
    """
    r = rgb[:, :, 0]
    g = rgb[:, :, 1]
    b = rgb[:, :, 2]
    if mode == "magenta":
        screen = (r >= 210.0) & (b >= 210.0) & (g <= 95.0)
    elif mode == "green":
        screen = (g >= 210.0) & (r <= 95.0) & (b <= 95.0)
    else:
        return alpha
    return np.where(screen, 0.0, alpha)


def distance_alpha(
    rgb: np.ndarray, key_rgb: np.ndarray, inner: float, outer: float
) -> np.ndarray:
    d = np.linalg.norm(rgb.astype(np.float64) - key_rgb.reshape(1, 1, 3), axis=2)
    if outer <= inner:
        outer = inner + 1e-3
    t = (d - inner) / (outer - inner)
    return np.clip(t, 0.0, 1.0).astype(np.float32)


def dematte_foreground_rgb(
    rgb: np.ndarray,
    alpha: np.ndarray,
    key_rgb: np.ndarray,
    *,
    dematte_min_alpha: float,
) -> np.ndarray:
    """
    Remove chroma residue from RGB on semi-transparent edges.

    Assumes observed RGB ~= F * a + key * (1 - a) (straight-alpha style matting).
    """
    a = np.clip(alpha.astype(np.float64), 0.0, 1.0)
    key3 = key_rgb.reshape(1, 1, 3)
    rgb_d = rgb.astype(np.float64)
    safe = a >= dematte_min_alpha
    fg = np.zeros_like(rgb_d)
    denom = np.maximum(a[..., None], 1e-4)
    fg[safe] = ((rgb_d - key3 * (1.0 - a[..., None])) / denom)[safe]
    return np.clip(fg, 0.0, 255.0)


def bbox_from_alpha(alpha: np.ndarray, thresh: float) -> tuple[int, int, int, int]:
    ys, xs = np.where(alpha > thresh)
    if ys.size == 0:
        h, w = alpha.shape
        return 0, h - 1, 0, w - 1
    return int(ys.min()), int(ys.max()), int(xs.min()), int(xs.max())


def fit_to_square(rgba_u8: np.ndarray, out_side: int, pad: int) -> Image.Image:
    """rgba_u8 HxWx4 uint8, alpha 0..255."""
    a = rgba_u8[:, :, 3].astype(np.float32) / 255.0
    y0, y1, x0, x1 = bbox_from_alpha(a, 1.0 / 255.0)
    crop = rgba_u8[y0 : y1 + 1, x0 : x1 + 1].copy()
    ch, cw, _ = crop.shape
    inner = out_side - 2 * pad
    inner = max(inner, 1)
    s = min(inner / float(cw), inner / float(ch))
    nw = max(1, int(round(cw * s)))
    nh = max(1, int(round(ch * s)))
    im = Image.fromarray(crop, "RGBA")
    # LANCZOS tends to smear pink/white fringes between hard edges and transparency.
    im = im.resize((nw, nh), Image.Resampling.BICUBIC)
    canvas = Image.new("RGBA", (out_side, out_side), (0, 0, 0, 0))
    ox = (out_side - nw) // 2
    oy = (out_side - nh) // 2
    canvas.paste(im, (ox, oy), im)
    return canvas


def despickle_low_alpha_rgba(arr: np.ndarray, *, cap: float, neigh_cap: float) -> None:
    """Zero isolated low-alpha specks (1px halos) when neighbors are empty."""
    a = arr[:, :, 3].astype(np.float32) / 255.0
    h, w = a.shape
    if h < 3 or w < 3:
        return
    ap = np.pad(a, 1, mode="constant", constant_values=0.0)
    n = (
        ap[0:h, 1 : w + 1]
        + ap[2 : h + 2, 1 : w + 1]
        + ap[1 : h + 1, 0:w]
        + ap[1 : h + 1, 2 : w + 2]
    )
    speck = (a > 0.0) & (a < cap) & (n < neigh_cap)
    arr[speck] = 0


def cleanup_rgba_array(arr: np.ndarray, *, bright_level: float, zb: float) -> None:
    """In-place: drop near-white pixels and pink fringes; re-apply alpha floor."""
    rgb = arr[:, :, :3].astype(np.float32)
    r, g, b = rgb[:, :, 0], rgb[:, :, 1], rgb[:, :, 2]
    a = arr[:, :, 3].astype(np.float32) / 255.0
    bright = (r >= bright_level) & (g >= bright_level) & (b >= bright_level)
    a = np.where(bright, 0.0, a)
    kill = magenta_fringe_kill_mask(r, g, b, a)
    a = np.where(kill, 0.0, a)
    a = np.where(a < zb, 0.0, a)
    arr[:, :, 3] = (np.clip(a, 0.0, 1.0) * 255.0).round().astype(np.uint8)
    despickle_low_alpha_rgba(arr, cap=0.26, neigh_cap=0.14)
    dead = arr[:, :, 3] == 0
    arr[dead] = 0


def process(
    src: Path,
    dst: Path,
    *,
    key_rgb: tuple[float, float, float] | None,
    inner: float,
    outer: float,
    pad: int,
    zero_below: float,
    dematte_min_alpha: float,
    screen_kill: str,
    key_from_edges: bool,
    bright_kill_level: float,
) -> None:
    im = Image.open(src).convert("RGB")
    rgb = np.array(im, dtype=np.float64)
    if key_from_edges:
        key = edge_median_rgb(rgb)
    elif key_rgb is None:
        key = np.array((255.0, 0.0, 255.0), dtype=np.float64)
    else:
        key = np.array(key_rgb, dtype=np.float64)
    alpha = distance_alpha(rgb.astype(np.float64), key, inner, outer)
    alpha = kill_screen_color_alpha(rgb, alpha, screen_kill)
    alpha = kill_bright_letterbox_alpha(rgb, alpha, bright_kill_level)
    alpha = kill_pink_fringe_alpha(rgb, alpha)
    fg = dematte_foreground_rgb(
        rgb, alpha, key, dematte_min_alpha=dematte_min_alpha
    )
    a_clean = np.clip(alpha.astype(np.float32), 0.0, 1.0)
    zb = max(float(zero_below), 1.0 / 255.0)
    dead = a_clean < zb
    fg[dead] = 0.0
    a_clean = np.where(dead, 0.0, a_clean)
    rgb_u8 = np.clip(fg, 0, 255).astype(np.uint8)
    a_u8 = (np.clip(a_clean, 0.0, 1.0) * 255.0).round().astype(np.uint8)
    # Strip near-white from dematted fg (e.g. halos) before pack
    rw, gw, bw = rgb_u8[:, :, 0], rgb_u8[:, :, 1], rgb_u8[:, :, 2]
    wmask = (rw >= int(bright_kill_level)) & (gw >= int(bright_kill_level)) & (bw >= int(bright_kill_level))
    a_u8 = np.where(wmask, 0, a_u8)
    rgb_u8[wmask] = 0
    a_f = a_u8.astype(np.float32) / 255.0
    kill_dm = magenta_fringe_kill_mask(
        rgb_u8[:, :, 0].astype(np.float32),
        rgb_u8[:, :, 1].astype(np.float32),
        rgb_u8[:, :, 2].astype(np.float32),
        a_f,
    )
    a_u8 = np.where(kill_dm, 0, a_u8)
    rgb_u8[kill_dm] = 0
    rgba_u8 = np.dstack([rgb_u8, a_u8])
    out = fit_to_square(rgba_u8, 256, pad)
    arr = np.asarray(out, dtype=np.uint8).copy()
    cleanup_rgba_array(arr, bright_level=bright_kill_level, zb=zb)
    out = Image.fromarray(arr, "RGBA")
    dst.parent.mkdir(parents=True, exist_ok=True)
    out.save(dst, format="PNG")


def main() -> int:
    p = argparse.ArgumentParser(description="Chroma key + 256x256 RGBA icon output.")
    p.add_argument("source", type=Path, help="RGB image from generator (solid key bg).")
    p.add_argument("output", type=Path, help="Output PNG path (256x256 RGBA).")
    p.add_argument(
        "--key-rgb",
        type=float,
        nargs=3,
        metavar=("R", "G", "B"),
        default=None,
        help="Chroma key RGB 0..255 (default when omitted: 255 0 255).",
    )
    p.add_argument(
        "--key-from-edges",
        action="store_true",
        help="Estimate key from border median instead of default #FF00FF.",
    )
    p.add_argument(
        "--inner",
        type=float,
        default=8.0,
        help="Distance below this (L2 in RGB) is fully transparent.",
    )
    p.add_argument(
        "--outer",
        type=float,
        default=45.0,
        help="Distance above this is fully opaque; between inner/outer is soft edge.",
    )
    p.add_argument(
        "--pad",
        type=int,
        default=8,
        help="Padding inside 256 canvas after fit.",
    )
    p.add_argument(
        "--bright-kill",
        type=float,
        default=228.0,
        metavar="LEVEL",
        help="RGB channels >= this (0..255) become transparent (white letterboxing on chroma).",
    )
    p.add_argument(
        "--zero-below",
        type=float,
        default=0.08,
        help="Force RGBA to transparent where alpha is below this (removes WPF gray 'box' halos).",
    )
    p.add_argument(
        "--dematte-min-alpha",
        type=float,
        default=0.08,
        help="Only apply chroma dematte where alpha is at least this (avoids unstable division).",
    )
    p.add_argument(
        "--screen-kill",
        choices=("magenta", "green", "none"),
        default="magenta",
        help="Force transparent on classic chroma screen colors (fixes #FF00FF interior vs border key).",
    )
    args = p.parse_args()
    key = tuple(args.key_rgb) if args.key_rgb is not None else None
    process(
        args.source,
        args.output,
        key_rgb=key,
        inner=args.inner,
        outer=args.outer,
        pad=args.pad,
        zero_below=args.zero_below,
        dematte_min_alpha=args.dematte_min_alpha,
        screen_kill=args.screen_kill,
        key_from_edges=args.key_from_edges,
        bright_kill_level=args.bright_kill,
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
