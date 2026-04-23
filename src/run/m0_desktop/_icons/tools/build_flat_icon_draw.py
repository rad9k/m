"""
One-off codegen: writes flat_icon_draw.py with render_shape() covering every shape_id
from manifest_label_to_shape.LABEL_SHAPE values. Deterministic geometry per id (no RNG).
"""

from __future__ import annotations

import math
from pathlib import Path

TOOLS = Path(__file__).resolve().parent


def main() -> None:
    import importlib.util

    spec = importlib.util.spec_from_file_location("ml", TOOLS / "manifest_label_to_shape.py")
    m = importlib.util.module_from_spec(spec)
    assert spec.loader
    spec.loader.exec_module(m)
    shape_ids = sorted(set(m.LABEL_SHAPE.values()))
    lines: list[str] = []
    lines.append('"""Auto-generated flat icon rasterizers. Do not edit by hand — run build_flat_icon_draw.py."""\n')
    lines.append("from __future__ import annotations\n")
    lines.append("import math\n")
    lines.append("from PIL import Image, ImageDraw\n")
    lines.append("SIDE = 256\nCX = CY = 128\n")
    lines.append(
        "BLUE = (43, 104, 240, 255)\nBLUE_D = (25, 65, 180, 255)\nWHITE = (255, 255, 255, 230)\nINK = (20, 28, 48, 255)\n"
    )
    lines.append(
        "def _new():\n    im = Image.new('RGBA', (SIDE, SIDE), (0, 0, 0, 0))\n    return im, ImageDraw.Draw(im)\n"
    )
    lines.append(
        "def _poly(d, pts, fill=None, outline=None, w=0):\n    d.polygon([(int(x), int(y)) for x, y in pts], fill=fill, outline=outline, width=w)\n"
    )
    lines.append(
        "def _line(d, a, b, fill, width):\n    d.line([(int(a[0]), int(a[1])), (int(b[0]), int(b[1]))], fill=fill, width=width)\n"
    )
    lines.append(
        "def _circ(d, xy, fill=None, outline=None, w=0):\n    d.ellipse(xy, fill=fill, outline=outline, width=w)\n"
    )

    for sid in shape_ids:
        lines.append(f"\n\ndef _impl_{sid}():\n")
        lines.append("    im, d = _new()\n")
        lines.extend(_body_lines(sid))
        lines.append("    return im\n")

    lines.append("\n\ndef render_shape(shape_id: str) -> Image.Image:\n")
    lines.append("    match shape_id:\n")
    for sid in shape_ids:
        lines.append(f'        case "{sid}":\n')
        lines.append(f"            return _impl_{sid}()\n")
    lines.append('        case _:\n            raise KeyError(shape_id)\n')

    out = TOOLS / "flat_icon_draw.py"
    out.write_text("".join(lines), encoding="utf-8")
    print("wrote", out, "shapes", len(shape_ids))


def _body_lines(sid: str) -> list[str]:
    """Return draw calls as strings; tuned heuristics from shape_id words."""
    h = sum(ord(c) for c in sid)
    L = sid.lower()
    o: list[str] = []

    def poly(pts: list[tuple[float, float]], fill="BLUE", outline=None, w=0):
        pf = "BLUE" if fill == "BLUE" else fill
        ol = "None" if outline is None else outline
        o.append(
            f"    _poly(d, {[(round(x, 1), round(y, 1)) for x, y in pts]!r}, fill={pf}, outline={ol}, w={w})\n"
        )

    def line(ax, ay, bx, by, w=8):
        o.append(f"    _line(d, ({ax:.1f}, {ay:.1f}), ({bx:.1f}, {by:.1f}), BLUE, {w})\n")

    def circ(x1, y1, x2, y2, fill=None, outline="BLUE", w=0):
        o.append(f"    _circ(d, ({int(x1)},{int(y1)},{int(x2)},{int(y2)}), fill={fill}, outline={outline}, w={w})\n")

    # --- keyword-family heuristics (geometry only) ---
    if "lightning" in L:
        poly(
            [(135, 48), (92, 128), (118, 128), (98, 208), (168, 108), (128, 108), (165, 48)],
            "BLUE",
            "BLUE_D",
            2,
        )
        return o
    if "magnifier" in L or "lens" in L:
        o.append("    _circ(d, (70, 70, 150, 150), fill=None, outline=BLUE, w=10)\n")
        o.append("    _line(d, (138, 138), (200, 200), BLUE, 12)\n")
        if "slash" in L:
            o.append("    _line(d, (80, 180), (190, 70), INK, 8)\n")
        return o
    if "bullseye" in L or sid == "edge_line_bullseye":
        o.append("    _circ(d, (48,48,208,208), fill=None, outline=BLUE, w=8)\n")
        o.append("    _circ(d, (72,72,184,184), fill=None, outline=BLUE_D, w=6)\n")
        o.append("    _circ(d, (100,100,156,156), fill=BLUE, outline=None, w=0)\n")
        if "edge" in L or sid == "edge_line_bullseye":
            o.append("    _line(d, (40.0, 128.0), (216.0, 128.0), BLUE, 6)\n")
        return o
    if "play" in L and "circle" in L or "pin_play" in L:
        o.append("    _circ(d, (48,48,208,208), fill=None, outline=BLUE, w=8)\n")
        poly([(118, 88), (118, 168), (178, 128)], "BLUE", None, 0)
        if "pin" in L:
            o.append("    _circ(d, (168,56,208,96), fill=BLUE_D, outline=None, w=0)\n")
        return o
    if "eye" in L:
        o.append("    d.chord((60, 100, 196, 170), 0, 180, fill=None, outline=BLUE, width=8)\n")
        if "slash" in L or "slot" in L:
            o.append("    _line(d, (70, 170), (200, 90), INK, 10)\n")
        if "stack" in L or "layers" in L:
            o.append("    d.rectangle((68, 172, 188, 188), fill=BLUE_D)\n")
        if "slot" in L:
            o.append("    d.rounded_rectangle((96, 118, 160, 152), 8, fill=WHITE)\n")
        return o
    if "tray" in L or "import" in L and "arrow" in L:
        o.append("    d.rounded_rectangle((72, 88, 184, 188), 12, outline=BLUE, width=8)\n")
        poly([(128, 52), (98, 100), (158, 100)], "BLUE", None, 0)
        if "double" in L or "meta_double" in L:
            o.append("    _line(d, (108, 60), (108, 92), BLUE, 6)\n")
            o.append("    _line(d, (148, 60), (148, 92), BLUE, 6)\n")
        if "meta" in L and "layer" in L:
            o.append("    d.rectangle((168, 96, 188, 176), fill=BLUE_D)\n")
        if "small_layer" in L:
            o.append("    d.rectangle((60, 96, 78, 176), fill=BLUE_D)\n")
        if "band_only" in L:
            o.append("    d.rectangle((72, 120, 184, 132), fill=BLUE_D)\n")
        return o
    if "arrow" in L and "return" in L:
        o.append("    d.arc((72, 88, 184, 200), 45, 270, fill=BLUE, width=12)\n")
        poly([(72, 128), (48, 148), (72, 168)], "BLUE", None, 0)
        return o
    if "arrow" in L and ("right" in L or "taper" in L or "forward" in L):
        poly([(56, 128), (188, 128), (160, 96), (160, 112), (200, 128), (160, 144), (160, 160)], "BLUE", "BLUE_D", 2)
        return o
    if "arrow" in L and "bidir" in L:
        poly([(128, 56), (88, 96), (128, 136), (168, 96)], "BLUE", None, 0)
        poly([(128, 120), (88, 160), (128, 200), (168, 160)], "BLUE", None, 0)
        return o
    if "refresh" in L or "arrows_refresh" in L:
        o.append("    d.arc((56, 56, 128, 128), 200, 470, fill=BLUE, width=12)\n")
        o.append("    d.arc((128, 128, 200, 200), 20, 290, fill=BLUE, width=12)\n")
        return o
    if ("diamond" in L and "hollow" in L) or "aggregation" in L:
        o.append(
            "    _poly(d, [(128, 56), (200, 128), (128, 200), (56, 128)], fill=None, outline=BLUE, w=10)\n"
        )
        return o
    if "diamond" in L and ("crystal" in L or "facets" in L):
        poly([(128, 48), (188, 128), (128, 208), (68, 128)], "BLUE", "BLUE_D", 2)
        poly([(128, 78), (158, 128), (128, 178), (98, 128)], "WHITE", None, 0)
        return o
    if "diamond" in L and "branch" in L:
        poly([(128, 48), (188, 128), (128, 208), (68, 128)], "BLUE", None, 0)
        return o
    if "line_two_nodes" in L or "edge_segment" in L or "two_nodes_line" in L:
        o.append("    _circ(d, (56,96,96,136), fill=BLUE, outline=None, w=0)\n")
        o.append("    _circ(d, (160,96,200,136), fill=BLUE, outline=None, w=0)\n")
        w = 6 if "dashed" in L else 10
        o.append(f"    _line(d, (96, 116), (160, 116), BLUE, {w})\n")
        return o
    if "rounded_rect" in L or "class" in L and "rounded" in L:
        o.append("    d.rounded_rectangle((56, 88, 200, 168), 18, outline=BLUE, width=8, fill=WHITE)\n")
        return o
    if "tag" in L:
        o.append("    _poly(d, [(70,88),(186,88),(200,128),(186,168),(70,168),(56,128)], fill=BLUE, outline=BLUE_D, w=2)\n")
        if "outline" in L or "dashed" in L:
            o.append("    d.rectangle((80, 108, 176, 148), outline=WHITE, width=3)\n")
        return o
    if "tags_three" in L:
        for ox in (-40, 0, 40):
            o.append(
                f"    d.polygon([({108+ox},88),({168+ox},100),({168+ox},156),({108+ox},168),({78+ox},128)], fill=BLUE, outline=BLUE_D)\n"
            )
        return o
    if "puzzle" in L:
        o.append(
            "    _poly(d, [(70,70),(186,70),(186,120),(160,120),(160,140),(186,140),(186,186),(70,186),(70,140),(96,140),(96,120),(70,120)], fill=BLUE, outline=BLUE_D, w=2)\n"
        )
        return o
    if "flask" in L:
        o.append("    _poly(d, [(108,56),(148,56),(138,96),(138,180),(118,200),(158,200),(138,180),(138,96)], fill=BLUE, outline=BLUE_D, w=2)\n")
        return o
    if "funnel" in L:
        o.append("    _poly(d, [(56,72),(200,72),(150,200),(106,200)], fill=BLUE, outline=BLUE_D, w=2)\n")
        return o
    if "cog" in L or "gear" in L:
        o.append("    _circ(d, (76,76,180,180), fill=None, outline=BLUE, w=10)\n")
        for i in range(6):
            ang = math.radians(i * 60 - 90)
            x1, y1 = 128 + 40 * math.cos(ang), 128 + 40 * math.sin(ang)
            x2, y2 = 128 + 88 * math.cos(ang), 128 + 88 * math.sin(ang)
            o.append(f"    _line(d, ({x1:.1f},{y1:.1f}), ({x2:.1f},{y2:.1f}), BLUE, 16)\n")
        o.append("    _circ(d, (108,108,148,148), fill=WHITE, outline=None, w=0)\n")
        if "lightning" in L or "overlay" in L:
            poly([(148, 72), (128, 118), (168, 118)], "INK", None, 0)
        return o
    if "cube" in L or "parcel" in L:
        o.append(
            "    _poly(d, [(128,56),(200,96),(128,136),(56,96)], fill=BLUE, outline=BLUE_D, w=2)\n"
        )
        o.append(
            "    _poly(d, [(56,96),(128,136),(128,216),(56,176)], fill=BLUE_D, outline=BLUE_D, w=2)\n"
        )
        o.append(
            "    _poly(d, [(128,136),(200,96),(200,176),(128,216)], fill=(60,90,200,255), outline=BLUE_D, w=2)\n"
        )
        if "orb" in L:
            o.append("    _circ(d, (108,108,148,148), fill=WHITE, outline=BLUE, w=4)\n")
        if "bracket" in L:
            o.append("    _line(d, (48, 80), (48, 176), BLUE, 10)\n")
            o.append("    _line(d, (208, 80), (208, 176), BLUE, 10)\n")
        return o
    if "palette" in L or sid.startswith("fill_circle"):
        if "green" in L:
            o.append("    _circ(d, (38,38,218,218), fill=(50,180,90,255), outline=None, w=0)\n")
        elif "red" in L:
            o.append("    _circ(d, (38,38,218,218), fill=(220,70,70,255), outline=None, w=0)\n")
        elif "blue" in L:
            o.append("    _circ(d, (38,38,218,218), fill=BLUE, outline=None, w=0)\n")
        else:
            o.append("    _circ(d, (70,96,110,136), fill=(220,60,60,255), outline=None, w=0)\n")
            o.append("    _circ(d, (108,96,148,136), fill=(60,180,80,255), outline=None, w=0)\n")
            o.append("    _circ(d, (146,96,186,136), fill=BLUE, outline=None, w=0)\n")
        return o
    if "paint_bucket" in L:
        o.append(
            "    _poly(d, [(88,72),(168,96),(148,196),(68,160)], fill=BLUE, outline=BLUE_D, w=3)\n"
        )
        o.append("    _poly(d, [(168,96),(200,120),(176,140)], fill=(60,90,200,255), outline=None, w=0)\n")
        return o
    if "pencil" in L:
        o.append(
            "    _poly(d, [(96,48),(140,52),(168,200),(124,196)], fill=(245,200,120,255), outline=INK, w=2)\n"
        )
        return o
    if "folder" in L:
        o.append("    d.rounded_rectangle((52, 100, 204, 188), 10, fill=BLUE, outline=BLUE_D, width=2)\n")
        o.append("    _poly(d, [(52,100),(92,68),(160,68),(160,100)], fill=BLUE_D, outline=None, w=0)\n")
        if "tabs" in L:
            o.append("    d.rectangle((72, 88, 112, 100), fill=WHITE)\n")
            o.append("    d.rectangle((120, 88, 160, 100), fill=WHITE)\n")
        if "lens" in L or "front_lens" in L:
            o.append("    _circ(d, (140, 130, 188, 178), fill=None, outline=WHITE, w=6)\n")
        return o
    if "document" in L or "lines" in L and "list" in L:
        o.append("    d.rounded_rectangle((68, 56, 188, 200), 8, outline=BLUE, width=8)\n")
        for y in (88, 116, 144, 172):
            o.append(f"    _line(d, (88, {y}), (168, {y}), BLUE_D, 6)\n")
        return o
    if "expand" in L:
        for tpl in [
            ((56, 72), (88, 72), (88, 56), (56, 88)),
            ((200, 72), (168, 72), (168, 56), (200, 88)),
            ((56, 184), (88, 184), (88, 200), (56, 168)),
            ((200, 184), (168, 184), (168, 200), (200, 168)),
        ]:
            poly(list(tpl), "BLUE", None, 0)
        return o
    if "silhouette" in L or "person" in L:
        o.append("    _circ(d, (108,56,148,96), fill=BLUE, outline=None, w=0)\n")
        o.append(
            "    _poly(d, [(78,108),(178,108),(188,200),(68,200)], fill=BLUE, outline=BLUE_D, w=2)\n"
        )
        return o
    if "infinity" in L or "while" in sid:
        o.append("    d.arc((56, 96, 128, 176), 0, 360, fill=BLUE, width=12)\n")
        o.append("    d.arc((128, 96, 200, 176), 0, 360, fill=BLUE, width=12)\n")
        return o
    if "wildcard" in L or "four_blocks" in L:
        for ox, oy in ((56, 56), (144, 56), (56, 144), (144, 144)):
            o.append(f"    d.rounded_rectangle(({ox},{oy},{ox+48},{oy+48}), 6, fill=BLUE)\n")
        return o
    if "terminal" in L or "vertical_bar" in L:
        o.append("    d.rectangle((112, 48, 152, 208), fill=BLUE)\n")
        return o
    if "bars" in L:
        heights = (40, 70, 100) if "tall" in L and "mirror" not in L else (100, 70, 40) if "mirror" in L else (30, 50, 70)
        if "short" in L:
            heights = (24, 36, 48)
        for i, h in enumerate(heights):
            x = 76 + i * 44
            o.append(f"    d.rectangle(({x}, {200-h}, {x+28}, 200), fill=BLUE)\n")
        return o
    if "balance" in L:
        o.append("    _line(d, (48, 128), (208, 128), BLUE, 8)\n")
        o.append("    _line(d, (128, 128), (128, 88), BLUE, 6)\n")
        poly([(48, 128), (68, 168), (28, 168)], "BLUE", None, 0)
        poly([(208, 128), (188, 168), (228, 168)], "BLUE", None, 0)
        return o
    if "chain" in L:
        o.append("    _circ(d, (72, 96, 132, 156), fill=None, outline=BLUE, w=12)\n")
        o.append("    _circ(d, (124, 96, 184, 156), fill=None, outline=BLUE, w=12)\n")
        return o
    if "chevron_stack" in L:
        for oy in (60, 96, 132):
            poly([(128, oy), (88, oy + 28), (168, oy + 28)], "BLUE", None, 0)
        return o
    if "tree" in L:
        o.append("    _line(d, (128, 48), (128, 180), BLUE, 10)\n")
        o.append("    _line(d, (128, 100), (72, 72), BLUE, 8)\n")
        o.append("    _line(d, (128, 100), (184, 72), BLUE, 8)\n")
        if "slash" in L or "cut" in L:
            o.append("    _line(d, (56, 200), (200, 56), INK, 10)\n")
        return o
    if "roots" in L:
        o.append("    _line(d, (128, 180), (128, 120), BLUE, 10)\n")
        for ang in (-60, -20, 20, 60):
            rad = math.radians(ang + 90)
            o.append(
                f"    _line(d, (128, 120), ({128 + 70*math.cos(rad):.1f}, {120 + 70*math.sin(rad):.1f}), BLUE, 8)\n"
            )
        return o
    if "inherit" in L and "stack" in L:
        o.append("    d.rectangle((60, 140, 196, 168), fill=BLUE_D)\n")
        o.append("    _line(d, (128, 140), (128, 80), BLUE, 8)\n")
        o.append("    _line(d, (128, 100), (88, 70), BLUE, 6)\n")
        o.append("    _line(d, (128, 100), (168, 70), BLUE, 6)\n")
        return o
    if "crosshair" in L or "frame_crosshair" in L:
        o.append("    d.rounded_rectangle((68, 68, 188, 188), 6, outline=BLUE, width=6)\n")
        o.append("    _line(d, (128, 68), (128, 188), BLUE, 4)\n")
        o.append("    _line(d, (68, 128), (188, 128), BLUE, 4)\n")
        return o
    if "burst" in L or "spark" in L:
        for i in range(8):
            ang = math.radians(i * 45)
            x2, y2 = 128 + 90 * math.cos(ang), 128 + 90 * math.sin(ang)
            o.append(f"    _line(d, (128, 128), ({x2:.1f}, {y2:.1f}), BLUE, 10)\n")
        o.append("    _line(d, (118, 128), (138, 128), INK, 6)\n")
        o.append("    _line(d, (128, 118), (128, 138), INK, 6)\n")
        return o
    if "keys" in L:
        for i, ox in enumerate((56, 108, 160)):
            o.append(
                f"    d.rounded_rectangle(({ox}, 96, {ox+36}, 180), 6, outline=BLUE, width=6)\n"
            )
            o.append(f"    _circ(d, ({ox+18}, 88, {ox+26}, 96), fill=BLUE, outline=None, w=0)\n")
        if "ring" in L:
            o.append("    _circ(d, (88, 88, 168, 168), fill=None, outline=BLUE_D, width=8)\n")
        if "star" in L:
            for i in range(6):
                ang = math.radians(i * 60)
                o.append(
                    f"    _line(d, (128,128), ({128+70*math.cos(ang):.1f},{128+70*math.sin(ang):.1f}), BLUE_D, 6)\n"
                )
        return o
    if "parallel" in L and "slash" in L:
        o.append("    _line(d, (72, 60), (72, 196), BLUE, 10)\n")
        o.append("    _line(d, (112, 60), (112, 196), BLUE, 10)\n")
        o.append("    _line(d, (48, 200), (208, 56), INK, 8)\n")
        return o
    if "loop_arrow_self" in L:
        o.append("    d.arc((60, 60, 196, 196), 30, 330, fill=BLUE, width=12)\n")
        o.append("    _line(d, (56, 128), (200, 128), INK, 6)\n")
        return o
    if "atom" in L:
        o.append("    _circ(d, (88, 88, 168, 168), fill=None, outline=BLUE, width=8)\n")
        for rot in (0, 60, 120):
            o.append(
                f"    d.arc((48,48,208,208), {rot}, {rot+120}, fill=BLUE_D, width=6)\n"
            )
        if "tick" in L or "next" in L:
            poly([(188, 88), (168, 128), (208, 128)], "WHITE", None, 0)
        return o
    if "hook" in L or "anchor" in L:
        flip = -1 if "down" in L else 1
        o.append(
            f"    d.arc((88, {'120' if flip==1 else '72'}, 168, {'168' if flip==1 else '120'}), 0, 180, fill=BLUE, width=12)\n"
        )
        o.append(f"    d.rectangle((120, {128 - 40*flip}, 140, {128 + 40*flip}), fill=BLUE)\n")
        return o
    if "bracket" in L or "braces" in L:
        o.append("    _line(d, (72, 72), (72, 184), BLUE, 12)\n")
        o.append("    _line(d, (184, 72), (184, 184), BLUE, 12)\n")
        if "dot" in L:
            o.append("    _circ(d, (100, 118, 116, 134), fill=INK, outline=None, w=0)\n")
        if "curly" in L or "wide" in L:
            o.append("    d.arc((72, 72, 120, 120), 90, 270, fill=BLUE, width=8)\n")
            o.append("    d.arc((136, 136, 184, 184), 270, 450, fill=BLUE, width=8)\n")
        return o
    if "chevron_left" in L or "chevron_right" in L:
        left = "left" in L
        if left:
            poly([(160, 64), (88, 128), (160, 192)], "BLUE", None, 0)
        else:
            poly([(96, 64), (168, 128), (96, 192)], "BLUE", None, 0)
        return o
    if "at_on" in L:
        o.append("    d.rounded_rectangle((56, 72, 200, 184), 16, outline=BLUE, width=8, fill=WHITE)\n")
        o.append("    _poly(d, [(128,88),(108,148),(148,148)], fill=BLUE, outline=None, w=0)\n")
        if "circle" in L:
            o.append("    _circ(d, (96, 96, 160, 160), fill=None, outline=BLUE, width=8)\n")
        return o
    if "globe" in L:
        o.append("    _circ(d, (56, 56, 200, 200), fill=None, outline=BLUE, width=10)\n")
        o.append("    _line(d, (128, 56), (128, 200), BLUE, 4)\n")
        o.append("    d.arc((56, 56, 200, 200), 20, 160, fill=BLUE_D, width=6)\n")
        return o
    if "monitor" in L:
        o.append("    d.rounded_rectangle((60, 72, 196, 160), 6, outline=BLUE, width=8)\n")
        o.append("    d.rectangle((108, 160, 148, 188), fill=BLUE)\n")
        return o
    if "spiral" in L or "decorator" in sid:
        o.append("    d.arc((72, 72, 184, 184), 0, 540, fill=BLUE, width=10)\n")
        return o
    if "scroll" in L:
        o.append("    d.rounded_rectangle((72, 64, 184, 192), 10, outline=BLUE, width=8)\n")
        for y in (92, 120, 148):
            o.append(f"    _line(d, (96, {y}), (168, {y}), BLUE_D, 5)\n")
        o.append("    _circ(d, (168, 48, 200, 80), fill=None, outline=BLUE, width=6)\n")
        return o
    if "stacked_import" in L:
        for oy in (56, 88, 120):
            poly([(128, oy), (98, oy + 24), (158, oy + 24)], "BLUE", None, 0)
        return o
    if "dashed_arrow" in L:
        o.append("    _circ(d, (72, 96, 112, 136), fill=BLUE, outline=None, w=0)\n")
        o.append("    _circ(d, (144, 96, 184, 136), fill=BLUE, outline=None, w=0)\n")
        for x in range(112, 152, 12):
            o.append(f"    d.rectangle(({x}, 118, {x+6}, 126), fill=BLUE)\n")
        return o
    if "nested" in L:
        o.append("    d.rectangle((56, 56, 200, 200), outline=BLUE, width=6)\n")
        o.append("    d.rectangle((80, 80, 176, 176), outline=BLUE_D, width=6)\n")
        o.append("    _circ(d, (108, 108, 148, 148), fill=BLUE, outline=None, w=0)\n")
        return o
    if "question" in L:
        o.append("    _circ(d, (72, 72, 184, 184), fill=None, outline=BLUE, width=10)\n")
        o.append("    _line(d, (128, 140), (128, 168), BLUE, 8)\n")
        o.append("    _circ(d, (120, 108, 136, 124), fill=BLUE, outline=None, w=0)\n")
        return o
    if "door" in L or "escape" in L:
        o.append("    d.rectangle((88, 72, 168, 200), outline=BLUE, width=8)\n")
        poly([(168, 128), (200, 128), (200, 148), (168, 148)], "BLUE", None, 0)
        return o
    if "slash" in L and "lead" in L or "trail" in L:
        o.append("    _line(d, (88, 88), (168, 168), INK, 10)\n")
        o.append("    _circ(d, (72, 160, 92, 180), fill=BLUE, outline=None, w=0)\n")
        return o
    if "wave" in L and "curve" in L or sid == "wave_s_curve":
        pts = [(48, 128)]
        for i in range(9):
            t = i / 8.0
            x = 48 + t * 160
            y = 128 + 40 * math.sin(t * math.pi * 2)
            pts.append((x, y))
        poly(pts, "BLUE", "BLUE_D", 4)
        return o
    if "window" in L and "wrench" in L:
        o.append("    d.rectangle((64, 72, 192, 168), outline=BLUE, width=8)\n")
        o.append("    _line(d, (96, 188), (168, 96), BLUE, 10)\n")
        return o
    if "loop_band" in L:
        o.append("    d.arc((56, 72, 200, 200), 40, 320, fill=BLUE, width=14)\n")
        if "vertex" in L or "dot" in L:
            o.append("    _circ(d, (108, 108, 148, 148), fill=WHITE, outline=BLUE, w=4)\n")
        if "edge" in L:
            o.append("    _line(d, (200, 128), (228, 128), BLUE, 8)\n")
        return o
    if "funnel" in L and "arrow" in L:
        o.append("    _poly(d, [(56,72),(200,72),(150,160),(106,160)], fill=BLUE, outline=BLUE_D, w=2)\n")
        poly([(128, 48), (98, 88), (158, 88)], "WHITE", None, 0)
        return o
    if "bold" in L:
        for i, w in enumerate((18, 24, 30)):
            y = 100 + i * 28
            o.append(f"    d.rectangle((72, {y}, 184, {y+w//3}), fill=BLUE)\n")
        return o
    if "plate" in L or "stamp" in L:
        o.append("    d.rounded_rectangle((64, 88, 192, 168), 8, fill=BLUE, outline=BLUE_D, width=2)\n")
        if "notch" in L:
            o.append("    _poly(d, [(128,88),(138,72),(148,88)], fill=WHITE, outline=None, w=0)\n")
        return o
    if "ribbon" in L and "name" in L:
        o.append("    d.rounded_rectangle((72, 108, 184, 148), 10, fill=BLUE, outline=None, w=0)\n")
        o.append("    _poly(d, [(72,128),(56,128),(72,108)], fill=BLUE_D, outline=None, w=0)\n")
        return o
    if "ribbon" in L and "snake" in L:
        pts = [(48, 160)]
        for i in range(12):
            t = i / 11.0
            x = 48 + t * 160
            y = 128 + 36 * math.sin(t * math.pi * 3)
            pts.append((x, y))
        poly(pts, "BLUE", "BLUE_D", 6)
        return o
    if "ribbon" in L and "through" in L:
        o.append("    d.rounded_rectangle((88, 108, 168, 148), 20, outline=BLUE, width=8)\n")
        o.append(
            "    _poly(d, [(40,128),(216,128),(200,108),(56,108)], fill=BLUE, outline=BLUE_D, w=3)\n"
        )
        return o

    # fallback: unique n-gon from sid (deterministic, graphical, no text)
    n = 5 + (h % 6)
    r = 70 + (len(sid) % 25)
    pts = []
    for i in range(n):
        ang = math.radians(i * 360 / n - 90)
        pts.append((128 + r * math.cos(ang), 128 + r * math.sin(ang)))
    poly(pts, "BLUE", "BLUE_D", 3)
    return o


if __name__ == "__main__":
    main()
