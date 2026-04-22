"""
Convert flat-style keyword preview PNGs (RGB, checkerboard "transparency" baked into pixels)
into square 256x256 RGBA icons with clean alpha for WPF (premultiplied RGB in PNG).

Typical workflow:
  1) Export/generate one PNG per keyword into an input folder (same stem as output name).
  2) Run this tool with --input-dir and --output-dir.
  3) Optional --manifest lists output basenames (one per line); only those with matching
     <name>.png in input are processed. Lines starting with # are ignored.

Example:
  python flat_rgba_from_checkerboard.py ^
    --input-dir "%USERPROFILE%\\.cursor\\projects\\...\\assets" ^
    --output-dir "..\\flat" ^
    --manifest "..\\flat\\icons_manifest.txt"

Dependencies: Pillow, NumPy.
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageFilter


def chroma_u8(rgb_u8: np.ndarray) -> np.ndarray:
    mx = np.max(rgb_u8, axis=-1)
    mn = np.min(rgb_u8, axis=-1)
    return (mx - mn).astype(np.int16)


def border_ring_mask(height: int, width: int, thickness: int) -> np.ndarray:
    yy, xx = np.indices((height, width), dtype=np.int32)
    outer = (
        (xx < thickness)
        | (xx >= width - thickness)
        | (yy < thickness)
        | (yy >= height - thickness)
    )
    inner_thickness = max(thickness + 8, thickness * 2)
    inner = (
        (xx >= inner_thickness)
        & (xx < width - inner_thickness)
        & (yy >= inner_thickness)
        & (yy < height - inner_thickness)
    )
    return outer & (~inner)


def top_background_row_index(sat: np.ndarray) -> int:
    height, width = sat.shape
    best_y = 0
    best_count = -1
    for y in range(min(height, 80)):
        count = int((sat[y] <= 8).sum())
        if count > best_count:
            best_count = count
            best_y = y
    return best_y


def estimate_period_from_acf(rgb_u8: np.ndarray, sat: np.ndarray) -> int:
    _, width, _ = rgb_u8.shape
    y0 = top_background_row_index(sat)
    line = rgb_u8[y0].astype(np.float32) / 255.0
    lum = 0.2126 * line[:, 0] + 0.7152 * line[:, 1] + 0.0722 * line[:, 2]
    lum = lum - float(np.mean(lum))

    acf = np.correlate(lum, lum, mode="full")
    acf = acf[acf.size // 2 :]
    if acf[0] == 0:
        return 46
    acf = acf / float(acf[0])

    best_p = 46
    best_v = -1.0
    for period in range(6, min(200, acf.size // 3)):
        value = float(acf[period])
        if value > best_v:
            best_v = value
            best_p = period
    return int(best_p)


def calibrate_phase_offsets(
    period: int, xs: np.ndarray, ys: np.ndarray, cols: np.ndarray
) -> tuple[int, int, float]:
    best: tuple[float, int, int] | None = None
    for offset_x in range(period):
        xs_q = (xs + offset_x) // period
        for offset_y in range(period):
            ys_q = (ys + offset_y) // period
            group = ((xs_q + ys_q) & 1).astype(bool)
            c0 = cols[~group]
            c1 = cols[group]
            if c0.shape[0] < 200 or c1.shape[0] < 200:
                continue
            variance0 = float(np.mean(np.var(c0.astype(np.float32), axis=0)))
            variance1 = float(np.mean(np.var(c1.astype(np.float32), axis=0)))
            score = variance0 + variance1
            if best is None or score < best[0]:
                best = (score, offset_x, offset_y)
    if best is None:
        return 0, 0, float("inf")
    return best[2], best[1], float(best[0])


def checkerboard_params(rgb_u8: np.ndarray) -> tuple[int, int, int]:
    height, width, _ = rgb_u8.shape
    sat = chroma_u8(rgb_u8)

    border_mask = border_ring_mask(height, width, 48) & (sat <= 8)
    flat_index = np.flatnonzero(border_mask)
    ys_all = (flat_index // width).astype(np.int32)
    xs_all = (flat_index % width).astype(np.int32)
    cols_all = rgb_u8.reshape(-1, 3)[flat_index].astype(np.float32)

    rng = np.random.default_rng(0)
    sample_size = 8000
    if cols_all.shape[0] > sample_size:
        selection = rng.choice(cols_all.shape[0], size=sample_size, replace=False)
        xs = xs_all[selection]
        ys = ys_all[selection]
        cols = cols_all[selection]
    else:
        xs, ys, cols = xs_all, ys_all, cols_all

    period0 = estimate_period_from_acf(rgb_u8, sat)
    candidates = sorted(
        {
            max(6, period0 - 1),
            period0,
            min(period0 + 1, 120),
        }
    )

    best: tuple[float, int, int, int] | None = None
    for period in candidates:
        offset_y, offset_x, score = calibrate_phase_offsets(period, xs, ys, cols)
        if best is None or score < best[0]:
            best = (score, period, offset_x, offset_y)

    if best is None:
        return 46, 0, 0
    _, period, offset_x, offset_y = best
    return period, offset_x, offset_y


def checker_group(period: int, offset_x: int, offset_y: int, height: int, width: int) -> np.ndarray:
    xs = (np.arange(width, dtype=np.int32)[None, :] + offset_x) // period
    ys = (np.arange(height, dtype=np.int32)[:, None] + offset_y) // period
    return ((xs + ys) & 1).astype(bool)


def checker_colors(
    rgb_u8: np.ndarray, group: np.ndarray, border_mask: np.ndarray
) -> tuple[np.ndarray, np.ndarray]:
    c0 = rgb_u8[border_mask & (~group)].astype(np.float32)
    c1 = rgb_u8[border_mask & group].astype(np.float32)
    return np.median(c0, axis=0), np.median(c1, axis=0)


def dilate_bool(mask: np.ndarray, iterations: int) -> np.ndarray:
    m = mask.astype(np.uint8)
    for _ in range(iterations):
        m = (
            np.maximum.reduce(
                [
                    m,
                    np.pad(m, ((1, 0), (0, 0)), mode="constant")[:-1, :],
                    np.pad(m, ((0, 1), (0, 0)), mode="constant")[1:, :],
                    np.pad(m, ((0, 0), (1, 0)), mode="constant")[:, :-1],
                    np.pad(m, ((0, 0), (0, 1)), mode="constant")[:, 1:],
                ]
            )
            > 0
        ).astype(np.uint8)
    return m.astype(bool)


def bbox_from_mask(mask: np.ndarray) -> tuple[int, int, int, int]:
    ys, xs = np.where(mask)
    if ys.size == 0:
        raise RuntimeError("empty mask")
    return int(xs.min()), int(ys.min()), int(xs.max()), int(ys.max())


def to_square_canvas_pair(
    rgb: np.ndarray, alpha: np.ndarray, mask: np.ndarray
) -> tuple[np.ndarray, np.ndarray]:
    x0, y0, x1, y1 = bbox_from_mask(mask)
    sub_m = mask[y0 : y1 + 1, x0 : x1 + 1]
    expanded = dilate_bool(sub_m, iterations=30)
    ys, xs = np.where(expanded)
    x0n = x0 + int(xs.min())
    x1n = x0 + int(xs.max())
    y0n = y0 + int(ys.min())
    y1n = y0 + int(ys.max())

    crop_rgb = rgb[y0n : y1n + 1, x0n : x1n + 1, :]
    crop_a = alpha[y0n : y1n + 1, x0n : x1n + 1]

    crop_h, crop_w, _ = crop_rgb.shape
    side = max(crop_w, crop_h)

    out_rgb = np.zeros((side, side, 3), dtype=np.float32)
    out_a = np.zeros((side, side), dtype=np.float32)

    pad_x = (side - crop_w) // 2
    pad_y = (side - crop_h) // 2
    out_rgb[pad_y : pad_y + crop_h, pad_x : pad_x + crop_w, :] = crop_rgb
    out_a[pad_y : pad_y + crop_h, pad_x : pad_x + crop_w] = crop_a

    return out_rgb, out_a


def estimate_foreground_color(rgb_u8: np.ndarray, alpha_seed: np.ndarray) -> np.ndarray:
    rgb = rgb_u8.astype(np.float32) / 255.0
    sat = np.max(rgb, axis=-1) - np.min(rgb, axis=-1)
    opaque = (alpha_seed > 0.95) & (sat > 0.15)
    if int(opaque.sum()) < 50:
        opaque = (alpha_seed > 0.90) & (sat > 0.10)
    if int(opaque.sum()) < 50:
        opaque = alpha_seed > 0.90
    return np.median(rgb_u8[opaque].astype(np.float32), axis=0)


def alpha_from_unmix(
    color: np.ndarray, background: np.ndarray, foreground: np.ndarray
) -> np.ndarray:
    epsilon = 1e-3
    numerator = np.sum((color - background) * (foreground - background), axis=-1)
    denominator = np.sum((foreground - background) ** 2, axis=-1) + epsilon
    return np.clip(numerator / denominator, 0.0, 1.0).astype(np.float32)


def recover_rgb(color: np.ndarray, background: np.ndarray, alpha: np.ndarray) -> np.ndarray:
    epsilon = 1e-3
    alpha_safe = np.maximum(alpha[:, :, None], epsilon)
    foreground = (color - (1.0 - alpha_safe) * background) / alpha_safe
    return np.clip(foreground, 0.0, 1.0)


def premultiplied_rgb_on_black_resize_rgba(
    rgba_u8: np.ndarray, supersample: int
) -> tuple[np.ndarray, np.ndarray]:
    image = Image.fromarray(rgba_u8, "RGBA")

    if supersample > 1:
        width, height = image.size
        image = image.resize((width * supersample, height * supersample), Image.Resampling.NEAREST)

    width, height = image.size
    black = Image.new("RGB", (width, height), (0, 0, 0))
    black.paste(image, mask=image.split()[3])

    rgb_pm = (
        np.asarray(black.resize((256, 256), Image.Resampling.LANCZOS)).astype(np.float32) / 255.0
    )
    alpha256 = (
        np.asarray(image.resize((256, 256), Image.Resampling.LANCZOS).split()[3]).astype(np.float32)
        / 255.0
    )
    return rgb_pm, alpha256


def luminance(rgb: np.ndarray) -> np.ndarray:
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    return 0.2126 * r + 0.7152 * g + 0.0722 * b


def unpremultiply(rgb_pm: np.ndarray, alpha: np.ndarray) -> np.ndarray:
    epsilon = 2e-2
    alpha_safe = np.maximum(alpha, epsilon)
    rgb = rgb_pm / alpha_safe[:, :, None]
    rgb = np.clip(rgb, 0.0, 1.0)

    lum = luminance(rgb)
    kill = (alpha < 0.08) & (lum > 0.85)
    rgb[kill, :] = 0.0
    return rgb


def median_alpha_u8(alpha_u8: np.ndarray) -> np.ndarray:
    image = Image.fromarray(alpha_u8, mode="L")
    filtered = image.filter(ImageFilter.MedianFilter(size=3)).point(lambda v: 0 if v < 10 else v)
    return np.asarray(filtered)


def finalize_premultiplied_straight_alpha_png(
    rgb_straight: np.ndarray, alpha: np.ndarray
) -> Image.Image:
    alpha2 = np.clip(alpha, 0.0, 1.0)
    alpha2 = np.where(alpha2 < 2.0 / 255.0, 0.0, alpha2)

    rgb_pm = np.clip(rgb_straight, 0.0, 1.0) * alpha2[:, :, None]

    rgb_u8 = (np.clip(rgb_pm, 0.0, 1.0) * 255.0).round().astype(np.uint8)
    alpha_u8 = (np.clip(alpha2, 0.0, 1.0) * 255.0).round().astype(np.uint8)
    return Image.fromarray(np.dstack([rgb_u8, alpha_u8]), "RGBA")


def process_rgb_u8_to_icon(
    rgb_u8: np.ndarray,
    supersample: int,
    fixed_checker: tuple[int, int, int] | None = None,
) -> Image.Image:
    height, width, _ = rgb_u8.shape
    sat = chroma_u8(rgb_u8)

    period, offset_x, offset_y = (
        fixed_checker if fixed_checker is not None else checkerboard_params(rgb_u8)
    )
    group = checker_group(period, offset_x, offset_y, height, width)

    border_mask = border_ring_mask(height, width, 48) & (sat <= 8)
    b0_u8, b1_u8 = checker_colors(rgb_u8, group, border_mask)

    background = np.where(group[..., None], b1_u8, b0_u8).astype(np.float32) / 255.0
    color = rgb_u8.astype(np.float32) / 255.0

    distance0 = np.linalg.norm(color * 255.0 - b0_u8[None, None, :], axis=-1)
    distance1 = np.linalg.norm(color * 255.0 - b1_u8[None, None, :], axis=-1)
    alpha_seed = np.clip((np.minimum(distance0, distance1) - 6.0) / 18.0, 0.0, 1.0)
    alpha_seed = np.maximum(
        alpha_seed,
        np.clip((sat.astype(np.float32) / 255.0 - 0.02) / 0.06, 0.0, 1.0),
    )

    foreground_u8 = estimate_foreground_color(rgb_u8, alpha_seed)
    foreground = foreground_u8.astype(np.float32) / 255.0

    alpha = alpha_from_unmix(color * 255.0, background * 255.0, foreground * 255.0)
    alpha = np.where(alpha < 2.0 / 255.0, 0.0, alpha)

    if not np.any(alpha > (1.0 / 255.0)):
        # Rare: unmix + scrub wipes everything (bad foreground estimate / odd background).
        # Fall back to the distance/chroma seed alpha and recompute foreground colors.
        alpha = np.clip(alpha_seed, 0.0, 1.0)

    rgb_foreground = recover_rgb(color, background, alpha)

    opaque_mask = alpha > (1.0 / 255.0)
    if not np.any(opaque_mask):
        # Last resort: chroma-based mask (works when background is neutral but not a checkerboard).
        sat_u8 = chroma_u8(rgb_u8)
        alpha = np.clip((sat_u8.astype(np.float32) / 255.0 - 0.02) / 0.08, 0.0, 1.0)
        rgb_foreground = recover_rgb(color, background, alpha)
        opaque_mask = alpha > (1.0 / 255.0)

    alpha = np.where(alpha < 2.0 / 255.0, 0.0, alpha)
    opaque_mask = alpha > (1.0 / 255.0)
    if not np.any(opaque_mask):
        raise RuntimeError(
            "empty foreground mask after matting; try a different source image or checker calibration"
        )

    rgb_out = np.zeros_like(color)
    rgb_out[opaque_mask] = rgb_foreground[opaque_mask]

    rgb_square, alpha_square = to_square_canvas_pair(rgb_out, alpha, opaque_mask)

    rgba_square_u8 = np.dstack(
        [
            (np.clip(rgb_square, 0.0, 1.0) * 255.0).round().astype(np.uint8),
            (np.clip(alpha_square, 0.0, 1.0) * 255.0).round().astype(np.uint8)[:, :, None],
        ]
    )

    rgb_pm256, alpha256 = premultiplied_rgb_on_black_resize_rgba(rgba_square_u8, supersample)

    alpha_u8 = (np.clip(alpha256, 0.0, 1.0) * 255.0).round().astype(np.uint8)
    alpha_u8 = median_alpha_u8(alpha_u8)
    alpha256 = alpha_u8.astype(np.float32) / 255.0

    rgb256 = unpremultiply(rgb_pm256, alpha256)

    return finalize_premultiplied_straight_alpha_png(rgb256, alpha256)


def load_manifest(path: Path) -> list[str]:
    lines: list[str] = []
    for raw in path.read_text(encoding="utf-8").splitlines():
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        lines.append(line)
    return lines


# Output filenames that must stay stable for this repo (pilot icons, Windows-safe names).
SPECIAL_OUTPUT_NAMES: dict[str, str] = {
    "(?<ANY>)": "_pattern_ANY_.png",
    "(?<LAST>)": "_pattern_LAST_.png",
}


def strip_manifest_noise(line: str) -> str:
    line = line.strip()
    prefix = "masz tu liste hasel:"
    if line.lower().startswith(prefix):
        line = line[len(prefix) :].strip()
    return line


def sanitize_windows_filename_stem(stem: str) -> str:
    stem = re.sub(r'[<>:"/\\|?*]', "_", stem)
    stem = stem.strip()
    if not stem:
        raise ValueError("empty label after sanitization")
    return stem


def output_png_basename_for_label(label: str) -> str:
    """Map a keyword line from the manifest to the output (and source) PNG filename."""
    label = strip_manifest_noise(label)
    if label in SPECIAL_OUTPUT_NAMES:
        return SPECIAL_OUTPUT_NAMES[label]
    stem = label[:-4] if label.lower().endswith(".png") else label
    return sanitize_windows_filename_stem(stem) + ".png"


def collect_inputs(
    input_dir: Path,
    manifest: list[str] | None,
    skip_prefixes: tuple[str, ...],
) -> tuple[list[tuple[Path, Path]], list[str]]:
    """Return (pairs (src_png, dst_basename.png), missing_basenames)."""
    pairs: list[tuple[Path, Path]] = []
    missing: list[str] = []
    if manifest is not None:
        for entry in manifest:
            out_name = output_png_basename_for_label(entry)
            src = input_dir / out_name
            if not src.is_file():
                missing.append(out_name)
                print(f"SKIP missing input: {src.name}", file=sys.stderr)
                continue
            if any(src.name.startswith(prefix) for prefix in skip_prefixes):
                print(f"SKIP junk name: {src.name}", file=sys.stderr)
                continue
            pairs.append((src, Path(out_name)))
        return pairs, missing

    for src in sorted(input_dir.glob("*.png")):
        if any(src.name.startswith(prefix) for prefix in skip_prefixes):
            continue
        if src.name.lower().endswith("_regen.png"):
            continue
        pairs.append((src, Path(src.name)))
    return pairs, []


def parse_args() -> argparse.Namespace:
    default_out = Path(__file__).resolve().parent.parent / "flat"
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--input-dir",
        type=Path,
        required=True,
        help="Folder containing source RGB PNGs (checkerboard background).",
    )
    parser.add_argument(
        "--output-dir",
        type=Path,
        default=default_out,
        help=f"Destination folder for 256x256 RGBA PNGs (default: {default_out}).",
    )
    parser.add_argument(
        "--manifest",
        type=Path,
        default=None,
        help="Optional UTF-8 text file: one output filename stem or name.png per line.",
    )
    parser.add_argument(
        "--supersample",
        type=int,
        default=2,
        help="Integer scale before downscale to 256 (default 2). Use 1 to disable.",
    )
    parser.add_argument(
        "--report-missing",
        type=Path,
        default=None,
        help="When using --manifest, write missing expected input basenames (one per line).",
    )
    parser.add_argument(
        "--fixed-checker",
        type=str,
        default=None,
        help="Skip checker auto-calibration; use 'period,ox,oy' e.g. 46,25,18 (faster batch runs).",
    )
    return parser.parse_args()


def parse_fixed_checker(text: str) -> tuple[int, int, int]:
    parts = text.split(",")
    if len(parts) != 3:
        raise ValueError("--fixed-checker must be period,ox,oy")
    return int(parts[0].strip()), int(parts[1].strip()), int(parts[2].strip())


def main() -> int:
    args = parse_args()
    input_dir: Path = args.input_dir
    output_dir: Path = args.output_dir
    output_dir.mkdir(parents=True, exist_ok=True)

    manifest = load_manifest(args.manifest) if args.manifest else None
    skip_prefixes = ("c__Users_",)

    pairs, missing = collect_inputs(input_dir, manifest, skip_prefixes)
    if args.report_missing is not None and missing:
        args.report_missing.parent.mkdir(parents=True, exist_ok=True)
        args.report_missing.write_text("\n".join(missing) + "\n", encoding="utf-8")
        print(f"Wrote missing list ({len(missing)}): {args.report_missing}")
    if not pairs:
        print("No matching input PNGs to process.", file=sys.stderr)
        return 2

    fixed_checker: tuple[int, int, int] | None = None
    if args.fixed_checker:
        fixed_checker = parse_fixed_checker(args.fixed_checker)

    ok = 0
    for src, out_name in pairs:
        rgb_u8 = np.asarray(Image.open(src).convert("RGB"))
        icon = process_rgb_u8_to_icon(
            rgb_u8, max(1, int(args.supersample)), fixed_checker=fixed_checker
        )
        dst = output_dir / out_name.name
        icon.save(dst, format="PNG")
        print(f"OK {src.name} -> {dst}")
        ok += 1

    print(f"Done. Wrote {ok} file(s) to {output_dir}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
