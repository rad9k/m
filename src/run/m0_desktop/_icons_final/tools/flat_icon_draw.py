"""Auto-generated flat icon rasterizers. Do not edit by hand — run build_flat_icon_draw.py."""
from __future__ import annotations
import math
from PIL import Image, ImageDraw
SIDE = 256
CX = CY = 128
BLUE = (43, 104, 240, 255)
BLUE_D = (25, 65, 180, 255)
WHITE = (255, 255, 255, 230)
INK = (20, 28, 48, 255)
def _new():
    im = Image.new('RGBA', (SIDE, SIDE), (0, 0, 0, 0))
    return im, ImageDraw.Draw(im)
def _poly(d, pts, fill=None, outline=None, w=0):
    d.polygon([(int(x), int(y)) for x, y in pts], fill=fill, outline=outline, width=w)
def _line(d, a, b, fill, width):
    d.line([(int(a[0]), int(a[1])), (int(b[0]), int(b[1]))], fill=fill, width=width)
def _circ(d, xy, fill=None, outline=None, w=0):
    d.ellipse(xy, fill=fill, outline=outline, width=w)


def _impl_anchor_hook_down():
    im, d = _new()
    d.arc((88, 72, 168, 120), 0, 180, fill=BLUE, width=12)
    d.rectangle((120, 168, 140, 88), fill=BLUE)
    return im


def _impl_anchor_hook_up():
    im, d = _new()
    d.arc((88, 120, 168, 168), 0, 180, fill=BLUE, width=12)
    d.rectangle((120, 88, 140, 168), fill=BLUE)
    return im


def _impl_angle_brackets_pair():
    im, d = _new()
    _line(d, (72, 72), (72, 184), BLUE, 12)
    _line(d, (184, 72), (184, 184), BLUE, 12)
    return im


def _impl_arc_port_hooks():
    im, d = _new()
    d.arc((88, 120, 168, 168), 0, 180, fill=BLUE, width=12)
    d.rectangle((120, 88, 140, 168), fill=BLUE)
    return im


def _impl_arrow_down_double_stroke():
    im, d = _new()
    _poly(d, [(128.0, 34.0), (183.3, 52.0), (217.4, 99.0), (217.4, 157.0), (183.3, 204.0), (128.0, 222.0), (72.7, 204.0), (38.6, 157.0), (38.6, 99.0), (72.7, 52.0)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_arrow_down_thick_shaft():
    im, d = _new()
    _poly(d, [(128.0, 36.0), (182.1, 53.6), (215.5, 99.6), (215.5, 156.4), (182.1, 202.4), (128.0, 220.0), (73.9, 202.4), (40.5, 156.4), (40.5, 99.6), (73.9, 53.6)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_arrow_into_funnel():
    im, d = _new()
    _poly(d, [(56,72),(200,72),(150,200),(106,200)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_arrow_return_hook():
    im, d = _new()
    d.arc((72, 88, 184, 200), 45, 270, fill=BLUE, width=12)
    _poly(d, [(72, 128), (48, 148), (72, 168)], fill=BLUE, outline=None, w=0)
    return im


def _impl_arrow_right_taper():
    im, d = _new()
    _poly(d, [(56, 128), (188, 128), (160, 96), (160, 112), (200, 128), (160, 144), (160, 160)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_arrows_bidir_vertical():
    im, d = _new()
    _poly(d, [(128, 56), (88, 96), (128, 136), (168, 96)], fill=BLUE, outline=None, w=0)
    _poly(d, [(128, 120), (88, 160), (128, 200), (168, 160)], fill=BLUE, outline=None, w=0)
    return im


def _impl_arrows_expand_corners():
    im, d = _new()
    _poly(d, [(56, 72), (88, 72), (88, 56), (56, 88)], fill=BLUE, outline=None, w=0)
    _poly(d, [(200, 72), (168, 72), (168, 56), (200, 88)], fill=BLUE, outline=None, w=0)
    _poly(d, [(56, 184), (88, 184), (88, 200), (56, 168)], fill=BLUE, outline=None, w=0)
    _poly(d, [(200, 184), (168, 184), (168, 200), (200, 168)], fill=BLUE, outline=None, w=0)
    return im


def _impl_arrows_refresh_circle():
    im, d = _new()
    d.arc((56, 56, 128, 128), 200, 470, fill=BLUE, width=12)
    d.arc((128, 128, 200, 200), 20, 290, fill=BLUE, width=12)
    return im


def _impl_at_on_circle_node():
    im, d = _new()
    d.rounded_rectangle((56, 72, 200, 184), 16, outline=BLUE, width=8, fill=WHITE)
    _poly(d, [(128,88),(108,148),(148,148)], fill=BLUE, outline=None, w=0)
    _circ(d, (96, 96, 160, 160), fill=None, outline=BLUE, width=8)
    return im


def _impl_at_on_rounded_rect():
    im, d = _new()
    d.rounded_rectangle((56, 88, 200, 168), 18, outline=BLUE, width=8, fill=WHITE)
    return im


def _impl_atom_next_tick():
    im, d = _new()
    _circ(d, (88, 88, 168, 168), fill=None, outline=BLUE, width=8)
    d.arc((48,48,208,208), 0, 120, fill=BLUE_D, width=6)
    d.arc((48,48,208,208), 60, 180, fill=BLUE_D, width=6)
    d.arc((48,48,208,208), 120, 240, fill=BLUE_D, width=6)
    _poly(d, [(188, 88), (168, 128), (208, 128)], fill=WHITE, outline=None, w=0)
    return im


def _impl_atom_orbits_three():
    im, d = _new()
    _circ(d, (88, 88, 168, 168), fill=None, outline=BLUE, width=8)
    d.arc((48,48,208,208), 0, 120, fill=BLUE_D, width=6)
    d.arc((48,48,208,208), 60, 180, fill=BLUE_D, width=6)
    d.arc((48,48,208,208), 120, 240, fill=BLUE_D, width=6)
    return im


def _impl_atom_tick_edge_vector():
    im, d = _new()
    _circ(d, (88, 88, 168, 168), fill=None, outline=BLUE, width=8)
    d.arc((48,48,208,208), 0, 120, fill=BLUE_D, width=6)
    d.arc((48,48,208,208), 60, 180, fill=BLUE_D, width=6)
    d.arc((48,48,208,208), 120, 240, fill=BLUE_D, width=6)
    _poly(d, [(188, 88), (168, 128), (208, 128)], fill=WHITE, outline=None, w=0)
    return im


def _impl_backslash_thick():
    im, d = _new()
    _poly(d, [(128.0, 43.0), (208.8, 101.7), (178.0, 196.8), (78.0, 196.8), (47.2, 101.7)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_balance_beam():
    im, d = _new()
    _line(d, (48, 128), (208, 128), BLUE, 8)
    _line(d, (128, 128), (128, 88), BLUE, 6)
    _poly(d, [(48, 128), (68, 168), (28, 168)], fill=BLUE, outline=None, w=0)
    _poly(d, [(208, 128), (188, 168), (228, 168)], fill=BLUE, outline=None, w=0)
    return im


def _impl_bars_short_three():
    im, d = _new()
    d.rectangle((76, 176, 104, 200), fill=BLUE)
    d.rectangle((120, 164, 148, 200), fill=BLUE)
    d.rectangle((164, 152, 192, 200), fill=BLUE)
    return im


def _impl_bars_tall_three():
    im, d = _new()
    d.rectangle((76, 160, 104, 200), fill=BLUE)
    d.rectangle((120, 130, 148, 200), fill=BLUE)
    d.rectangle((164, 100, 192, 200), fill=BLUE)
    return im


def _impl_bars_tall_three_mirror():
    im, d = _new()
    d.rectangle((76, 100, 104, 200), fill=BLUE)
    d.rectangle((120, 130, 148, 200), fill=BLUE)
    d.rectangle((164, 160, 192, 200), fill=BLUE)
    return im


def _impl_binary_tree_three():
    im, d = _new()
    _line(d, (128, 48), (128, 180), BLUE, 10)
    _line(d, (128, 100), (72, 72), BLUE, 8)
    _line(d, (128, 100), (184, 72), BLUE, 8)
    return im


def _impl_book_question_mark_shape():
    im, d = _new()
    _circ(d, (72, 72, 184, 184), fill=None, outline=BLUE, width=10)
    _line(d, (128, 140), (128, 168), BLUE, 8)
    _circ(d, (120, 108, 136, 124), fill=BLUE, outline=None, w=0)
    return im


def _impl_box_arrow_out_right():
    im, d = _new()
    _poly(d, [(56, 128), (188, 128), (160, 96), (160, 112), (200, 128), (160, 144), (160, 160)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_box_meta_corner_dot():
    im, d = _new()
    _poly(d, [(128.0, 39.0), (190.9, 65.1), (217.0, 128.0), (190.9, 190.9), (128.0, 217.0), (65.1, 190.9), (39.0, 128.0), (65.1, 65.1)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_braces_curly_pair_wide():
    im, d = _new()
    _line(d, (72, 72), (72, 184), BLUE, 12)
    _line(d, (184, 72), (184, 184), BLUE, 12)
    d.arc((72, 72, 120, 120), 90, 270, fill=BLUE, width=8)
    d.arc((136, 136, 184, 184), 270, 450, fill=BLUE, width=8)
    return im


def _impl_brackets_dot_trailing():
    im, d = _new()
    _line(d, (72, 72), (72, 184), BLUE, 12)
    _line(d, (184, 72), (184, 184), BLUE, 12)
    _circ(d, (100, 118, 116, 134), fill=INK, outline=None, w=0)
    return im


def _impl_brackets_property_dot():
    im, d = _new()
    _line(d, (72, 72), (72, 184), BLUE, 12)
    _line(d, (184, 72), (184, 184), BLUE, 12)
    _circ(d, (100, 118, 116, 134), fill=INK, outline=None, w=0)
    return im


def _impl_brackets_ribbon_tab():
    im, d = _new()
    _line(d, (72, 72), (72, 184), BLUE, 12)
    _line(d, (184, 72), (184, 184), BLUE, 12)
    return im


def _impl_bullseye_rings():
    im, d = _new()
    _circ(d, (48,48,208,208), fill=None, outline=BLUE, w=8)
    _circ(d, (72,72,184,184), fill=None, outline=BLUE_D, w=6)
    _circ(d, (100,100,156,156), fill=BLUE, outline=None, w=0)
    return im


def _impl_burst_rays_plus_center():
    im, d = _new()
    _line(d, (128, 128), (218.0, 128.0), BLUE, 10)
    _line(d, (128, 128), (191.6, 191.6), BLUE, 10)
    _line(d, (128, 128), (128.0, 218.0), BLUE, 10)
    _line(d, (128, 128), (64.4, 191.6), BLUE, 10)
    _line(d, (128, 128), (38.0, 128.0), BLUE, 10)
    _line(d, (128, 128), (64.4, 64.4), BLUE, 10)
    _line(d, (128, 128), (128.0, 38.0), BLUE, 10)
    _line(d, (128, 128), (191.6, 64.4), BLUE, 10)
    _line(d, (118, 128), (138, 128), INK, 6)
    _line(d, (128, 118), (128, 138), INK, 6)
    return im


def _impl_chain_link_tag():
    im, d = _new()
    _poly(d, [(70,88),(186,88),(200,128),(186,168),(70,168),(56,128)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_chain_two_links():
    im, d = _new()
    _circ(d, (72, 96, 132, 156), fill=None, outline=BLUE, w=12)
    _circ(d, (124, 96, 184, 156), fill=None, outline=BLUE, w=12)
    return im


def _impl_checker_fade_blocks():
    im, d = _new()
    _poly(d, [(128.0, 39.0), (190.9, 65.1), (217.0, 128.0), (190.9, 190.9), (128.0, 217.0), (65.1, 190.9), (39.0, 128.0), (65.1, 65.1)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_chevron_left_block():
    im, d = _new()
    _poly(d, [(160, 64), (88, 128), (160, 192)], fill=BLUE, outline=None, w=0)
    return im


def _impl_chevron_right_block():
    im, d = _new()
    _poly(d, [(96, 64), (168, 128), (96, 192)], fill=BLUE, outline=None, w=0)
    return im


def _impl_chevrons_stack_up():
    im, d = _new()
    _poly(d, [(128.0, 41.0), (203.3, 84.5), (203.3, 171.5), (128.0, 215.0), (52.7, 171.5), (52.7, 84.5)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_chip_die_outline():
    im, d = _new()
    _poly(d, [(128.0, 42.0), (183.3, 62.1), (212.7, 113.1), (202.5, 171.0), (157.4, 208.8), (98.6, 208.8), (53.5, 171.0), (43.3, 113.1), (72.7, 62.1)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_circle_dot_ring():
    im, d = _new()
    _poly(d, [(128.0, 43.0), (188.1, 67.9), (213.0, 128.0), (188.1, 188.1), (128.0, 213.0), (67.9, 188.1), (43.0, 128.0), (67.9, 67.9)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_clock_arc_chain_link():
    im, d = _new()
    _circ(d, (72, 96, 132, 156), fill=None, outline=BLUE, w=12)
    _circ(d, (124, 96, 184, 156), fill=None, outline=BLUE, w=12)
    return im


def _impl_cloud_dashed_query():
    im, d = _new()
    _poly(d, [(128.0, 40.0), (211.7, 100.8), (179.7, 199.2), (76.3, 199.2), (44.3, 100.8)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_cog_lightning_overlay():
    im, d = _new()
    _poly(d, [(135, 48), (92, 128), (118, 128), (98, 208), (168, 108), (128, 108), (165, 48)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_cog_six_spoke():
    im, d = _new()
    _circ(d, (76,76,180,180), fill=None, outline=BLUE, w=10)
    _line(d, (128.0,88.0), (128.0,40.0), BLUE, 16)
    _line(d, (162.6,108.0), (204.2,84.0), BLUE, 16)
    _line(d, (162.6,148.0), (204.2,172.0), BLUE, 16)
    _line(d, (128.0,168.0), (128.0,216.0), BLUE, 16)
    _line(d, (93.4,148.0), (51.8,172.0), BLUE, 16)
    _line(d, (93.4,108.0), (51.8,84.0), BLUE, 16)
    _circ(d, (108,108,148,148), fill=WHITE, outline=None, w=0)
    return im


def _impl_columns_lines_formal():
    im, d = _new()
    _poly(d, [(128.0, 38.0), (191.6, 64.4), (218.0, 128.0), (191.6, 191.6), (128.0, 218.0), (64.4, 191.6), (38.0, 128.0), (64.4, 64.4)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_corner_branch_left():
    im, d = _new()
    _poly(d, [(128.0, 40.0), (184.6, 60.6), (214.7, 112.7), (204.2, 172.0), (158.1, 210.7), (97.9, 210.7), (51.8, 172.0), (41.3, 112.7), (71.4, 60.6)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_corner_branch_right():
    im, d = _new()
    _poly(d, [(128.0, 39.0), (180.3, 56.0), (212.6, 100.5), (212.6, 155.5), (180.3, 200.0), (128.0, 217.0), (75.7, 200.0), (43.4, 155.5), (43.4, 100.5), (75.7, 56.0)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_cr_lf_glyph():
    im, d = _new()
    _poly(d, [(128.0, 47.0), (185.3, 70.7), (209.0, 128.0), (185.3, 185.3), (128.0, 209.0), (70.7, 185.3), (47.0, 128.0), (70.7, 70.7)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_crosshair_over_lens():
    im, d = _new()
    _circ(d, (70, 70, 150, 150), fill=None, outline=BLUE, w=10)
    _line(d, (138, 138), (200, 200), BLUE, 12)
    return im


def _impl_cube_isometric_solid():
    im, d = _new()
    _poly(d, [(128,56),(200,96),(128,136),(56,96)], fill=BLUE, outline=BLUE_D, w=2)
    _poly(d, [(56,96),(128,136),(128,216),(56,176)], fill=BLUE_D, outline=BLUE_D, w=2)
    _poly(d, [(128,136),(200,96),(200,176),(128,216)], fill=(60,90,200,255), outline=BLUE_D, w=2)
    return im


def _impl_cube_orb_vertex():
    im, d = _new()
    _poly(d, [(128,56),(200,96),(128,136),(56,96)], fill=BLUE, outline=BLUE_D, w=2)
    _poly(d, [(56,96),(128,136),(128,216),(56,176)], fill=BLUE_D, outline=BLUE_D, w=2)
    _poly(d, [(128,136),(200,96),(200,176),(128,216)], fill=(60,90,200,255), outline=BLUE_D, w=2)
    _circ(d, (108,108,148,148), fill=WHITE, outline=BLUE, w=4)
    return im


def _impl_cube_small_brackets():
    im, d = _new()
    _poly(d, [(128,56),(200,96),(128,136),(56,96)], fill=BLUE, outline=BLUE_D, w=2)
    _poly(d, [(56,96),(128,136),(128,216),(56,176)], fill=BLUE_D, outline=BLUE_D, w=2)
    _poly(d, [(128,136),(200,96),(200,176),(128,216)], fill=(60,90,200,255), outline=BLUE_D, w=2)
    _line(d, (48, 80), (48, 176), BLUE, 10)
    _line(d, (208, 80), (208, 176), BLUE, 10)
    return im


def _impl_dashed_arrow_open_head():
    im, d = _new()
    _circ(d, (72, 96, 112, 136), fill=BLUE, outline=None, w=0)
    _circ(d, (144, 96, 184, 136), fill=BLUE, outline=None, w=0)
    d.rectangle((112, 118, 118, 126), fill=BLUE)
    d.rectangle((124, 118, 130, 126), fill=BLUE)
    d.rectangle((136, 118, 142, 126), fill=BLUE)
    d.rectangle((148, 118, 154, 126), fill=BLUE)
    return im


def _impl_dashed_frame_meta():
    im, d = _new()
    _poly(d, [(128.0, 41.0), (203.3, 84.5), (203.3, 171.5), (128.0, 215.0), (52.7, 171.5), (52.7, 84.5)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_dashed_frame_plain():
    im, d = _new()
    _poly(d, [(128.0, 40.0), (196.8, 73.1), (213.8, 147.6), (166.2, 207.3), (89.8, 207.3), (42.2, 147.6), (59.2, 73.1)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_diamond_branch_fill():
    im, d = _new()
    _poly(d, [(128, 48), (188, 128), (128, 208), (68, 128)], fill=BLUE, outline=None, w=0)
    return im


def _impl_diamond_crystal_facets():
    im, d = _new()
    _poly(d, [(128, 48), (188, 128), (128, 208), (68, 128)], fill=BLUE, outline=BLUE_D, w=2)
    _poly(d, [(128, 78), (158, 128), (128, 178), (98, 128)], fill=WHITE, outline=None, w=0)
    return im


def _impl_diamond_hollow_agg():
    im, d = _new()
    _poly(d, [(128, 56), (200, 128), (128, 200), (56, 128)], fill=None, outline=BLUE, w=10)
    return im


def _impl_document_lines():
    im, d = _new()
    d.rounded_rectangle((68, 56, 188, 200), 8, outline=BLUE, width=8)
    _line(d, (88, 88), (168, 88), BLUE_D, 6)
    _line(d, (88, 116), (168, 116), BLUE_D, 6)
    _line(d, (88, 144), (168, 144), BLUE_D, 6)
    _line(d, (88, 172), (168, 172), BLUE_D, 6)
    return im


def _impl_door_arrow_exit():
    im, d = _new()
    d.rectangle((88, 72, 168, 200), outline=BLUE, width=8)
    _poly(d, [(168, 128), (200, 128), (200, 148), (168, 148)], fill=BLUE, outline=None, w=0)
    return im


def _impl_dot_leading_brackets():
    im, d = _new()
    _line(d, (72, 72), (72, 184), BLUE, 12)
    _line(d, (184, 72), (184, 184), BLUE, 12)
    _circ(d, (100, 118, 116, 134), fill=INK, outline=None, w=0)
    return im


def _impl_dot_trail_slash():
    im, d = _new()
    _line(d, (88, 88), (168, 168), INK, 10)
    _circ(d, (72, 160, 92, 180), fill=BLUE, outline=None, w=0)
    return im


def _impl_dots_vertical_column():
    im, d = _new()
    _poly(d, [(128.0, 38.0), (198.4, 71.9), (215.7, 148.0), (167.0, 209.1), (89.0, 209.1), (40.3, 148.0), (57.6, 71.9)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_edge_hollow_circle_end():
    im, d = _new()
    _poly(d, [(128.0, 36.0), (199.9, 70.6), (217.7, 148.5), (167.9, 210.9), (88.1, 210.9), (38.3, 148.5), (56.1, 70.6)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_edge_line_bullseye():
    im, d = _new()
    _circ(d, (48,48,208,208), fill=None, outline=BLUE, w=8)
    _circ(d, (72,72,184,184), fill=None, outline=BLUE_D, w=6)
    _circ(d, (100,100,156,156), fill=BLUE, outline=None, w=0)
    _line(d, (40.0, 128.0), (216.0, 128.0), BLUE, 6)
    return im


def _impl_edge_plus_starburst():
    im, d = _new()
    _line(d, (128, 128), (218.0, 128.0), BLUE, 10)
    _line(d, (128, 128), (191.6, 191.6), BLUE, 10)
    _line(d, (128, 128), (128.0, 218.0), BLUE, 10)
    _line(d, (128, 128), (64.4, 191.6), BLUE, 10)
    _line(d, (128, 128), (38.0, 128.0), BLUE, 10)
    _line(d, (128, 128), (64.4, 64.4), BLUE, 10)
    _line(d, (128, 128), (128.0, 38.0), BLUE, 10)
    _line(d, (128, 128), (191.6, 64.4), BLUE, 10)
    _line(d, (118, 128), (138, 128), INK, 6)
    _line(d, (128, 118), (128, 138), INK, 6)
    return im


def _impl_edge_question_orb():
    im, d = _new()
    _circ(d, (72, 72, 184, 184), fill=None, outline=BLUE, width=10)
    _line(d, (128, 140), (128, 168), BLUE, 8)
    _circ(d, (120, 108, 136, 124), fill=BLUE, outline=None, w=0)
    return im


def _impl_edge_segment_dashed():
    im, d = _new()
    _circ(d, (56,96,96,136), fill=BLUE, outline=None, w=0)
    _circ(d, (160,96,200,136), fill=BLUE, outline=None, w=0)
    _line(d, (96, 116), (160, 116), BLUE, 6)
    return im


def _impl_eye_open():
    im, d = _new()
    d.chord((60, 100, 196, 170), 0, 180, fill=None, outline=BLUE, width=8)
    return im


def _impl_eye_slash():
    im, d = _new()
    d.chord((60, 100, 196, 170), 0, 180, fill=None, outline=BLUE, width=8)
    _line(d, (70, 170), (200, 90), INK, 10)
    return im


def _impl_eye_slot_pill():
    im, d = _new()
    d.chord((60, 100, 196, 170), 0, 180, fill=None, outline=BLUE, width=8)
    _line(d, (70, 170), (200, 90), INK, 10)
    d.rounded_rectangle((96, 118, 160, 152), 8, fill=WHITE)
    return im


def _impl_field_equals_sign():
    im, d = _new()
    _poly(d, [(128.0, 41.0), (196.0, 73.8), (212.8, 147.4), (165.7, 206.4), (90.3, 206.4), (43.2, 147.4), (60.0, 73.8)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_fill_circle_blue():
    im, d = _new()
    _circ(d, (38,38,218,218), fill=BLUE, outline=None, w=0)
    return im


def _impl_fill_circle_green():
    im, d = _new()
    _circ(d, (38,38,218,218), fill=(50,180,90,255), outline=None, w=0)
    return im


def _impl_fill_circle_red():
    im, d = _new()
    _circ(d, (38,38,218,218), fill=(220,70,70,255), outline=None, w=0)
    return im


def _impl_flag_pennon_one():
    im, d = _new()
    _poly(d, [(128.0, 43.0), (182.6, 62.9), (211.7, 113.2), (201.6, 170.5), (157.1, 207.9), (98.9, 207.9), (54.4, 170.5), (44.3, 113.2), (73.4, 62.9)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_flag_pennon_two():
    im, d = _new()
    _poly(d, [(128.0, 43.0), (182.6, 62.9), (211.7, 113.2), (201.6, 170.5), (157.1, 207.9), (98.9, 207.9), (54.4, 170.5), (44.3, 113.2), (73.4, 62.9)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_flask_conical():
    im, d = _new()
    _poly(d, [(108,56),(148,56),(138,96),(138,180),(118,200),(158,200),(138,180),(138,96)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_folder_front_lens():
    im, d = _new()
    _circ(d, (70, 70, 150, 150), fill=None, outline=BLUE, w=10)
    _line(d, (138, 138), (200, 200), BLUE, 12)
    return im


def _impl_folder_tab_open():
    im, d = _new()
    d.rounded_rectangle((52, 100, 204, 188), 10, fill=BLUE, outline=BLUE_D, width=2)
    _poly(d, [(52,100),(92,68),(160,68),(160,100)], fill=BLUE_D, outline=None, w=0)
    return im


def _impl_folder_tabs_three():
    im, d = _new()
    d.rounded_rectangle((52, 100, 204, 188), 10, fill=BLUE, outline=BLUE_D, width=2)
    _poly(d, [(52,100),(92,68),(160,68),(160,100)], fill=BLUE_D, outline=None, w=0)
    d.rectangle((72, 88, 112, 100), fill=WHITE)
    d.rectangle((120, 88, 160, 100), fill=WHITE)
    return im


def _impl_frame_crosshair_small():
    im, d = _new()
    d.rounded_rectangle((68, 68, 188, 188), 6, outline=BLUE, width=6)
    _line(d, (128, 68), (128, 188), BLUE, 4)
    _line(d, (68, 128), (188, 128), BLUE, 4)
    return im


def _impl_funnel_trapezoid():
    im, d = _new()
    _poly(d, [(56,72),(200,72),(150,200),(106,200)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_glasses_on_rect():
    im, d = _new()
    _poly(d, [(128.0, 43.0), (178.0, 59.2), (208.8, 101.7), (208.8, 154.3), (178.0, 196.8), (128.0, 213.0), (78.0, 196.8), (47.2, 154.3), (47.2, 101.7), (78.0, 59.2)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_globe_meridians_inner():
    im, d = _new()
    _circ(d, (56, 56, 200, 200), fill=None, outline=BLUE, width=10)
    _line(d, (128, 56), (128, 200), BLUE, 4)
    d.arc((56, 56, 200, 200), 20, 160, fill=BLUE_D, width=6)
    return im


def _impl_hex_node_lens():
    im, d = _new()
    _circ(d, (70, 70, 150, 150), fill=None, outline=BLUE, w=10)
    _line(d, (138, 138), (200, 200), BLUE, 12)
    return im


def _impl_hook_j_curve():
    im, d = _new()
    d.arc((88, 120, 168, 168), 0, 180, fill=BLUE, width=12)
    d.rectangle((120, 88, 140, 168), fill=BLUE)
    return im


def _impl_hook_tail_curve():
    im, d = _new()
    d.arc((88, 120, 168, 168), 0, 180, fill=BLUE, width=12)
    d.rectangle((120, 88, 140, 168), fill=BLUE)
    return im


def _impl_horizontal_bar_thick():
    im, d = _new()
    _poly(d, [(128.0, 38.0), (185.9, 59.1), (216.6, 112.4), (205.9, 173.0), (158.8, 212.6), (97.2, 212.6), (50.1, 173.0), (39.4, 112.4), (70.1, 59.1)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_infinity_loop_band():
    im, d = _new()
    d.arc((56, 96, 128, 176), 0, 360, fill=BLUE, width=12)
    d.arc((128, 96, 200, 176), 0, 360, fill=BLUE, width=12)
    return im


def _impl_keys_on_ring():
    im, d = _new()
    d.rounded_rectangle((56, 96, 92, 180), 6, outline=BLUE, width=6)
    _circ(d, (74, 88, 82, 96), fill=BLUE, outline=None, w=0)
    d.rounded_rectangle((108, 96, 144, 180), 6, outline=BLUE, width=6)
    _circ(d, (126, 88, 134, 96), fill=BLUE, outline=None, w=0)
    d.rounded_rectangle((160, 96, 196, 180), 6, outline=BLUE, width=6)
    _circ(d, (178, 88, 186, 96), fill=BLUE, outline=None, w=0)
    _circ(d, (88, 88, 168, 168), fill=None, outline=BLUE_D, width=8)
    return im


def _impl_keys_star_burst():
    im, d = _new()
    _line(d, (128, 128), (218.0, 128.0), BLUE, 10)
    _line(d, (128, 128), (191.6, 191.6), BLUE, 10)
    _line(d, (128, 128), (128.0, 218.0), BLUE, 10)
    _line(d, (128, 128), (64.4, 191.6), BLUE, 10)
    _line(d, (128, 128), (38.0, 128.0), BLUE, 10)
    _line(d, (128, 128), (64.4, 64.4), BLUE, 10)
    _line(d, (128, 128), (128.0, 38.0), BLUE, 10)
    _line(d, (128, 128), (191.6, 64.4), BLUE, 10)
    _line(d, (118, 128), (138, 128), INK, 6)
    _line(d, (128, 118), (128, 138), INK, 6)
    return im


def _impl_label_card_slash():
    im, d = _new()
    _poly(d, [(128.0, 42.0), (202.5, 85.0), (202.5, 171.0), (128.0, 214.0), (53.5, 171.0), (53.5, 85.0)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_layers_eye_stack():
    im, d = _new()
    d.chord((60, 100, 196, 170), 0, 180, fill=None, outline=BLUE, width=8)
    d.rectangle((68, 172, 188, 188), fill=BLUE_D)
    return im


def _impl_lightning_bolt():
    im, d = _new()
    _poly(d, [(135, 48), (92, 128), (118, 128), (98, 208), (168, 108), (128, 108), (165, 48)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_lightning_inner_ring():
    im, d = _new()
    _poly(d, [(135, 48), (92, 128), (118, 128), (98, 208), (168, 108), (128, 108), (165, 48)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_line_two_nodes_solid():
    im, d = _new()
    _circ(d, (56,96,96,136), fill=BLUE, outline=None, w=0)
    _circ(d, (160,96,200,136), fill=BLUE, outline=None, w=0)
    _line(d, (96, 116), (160, 116), BLUE, 10)
    return im


def _impl_list_lines_bullets():
    im, d = _new()
    d.rounded_rectangle((68, 56, 188, 200), 8, outline=BLUE, width=8)
    _line(d, (88, 88), (168, 88), BLUE_D, 6)
    _line(d, (88, 116), (168, 116), BLUE_D, 6)
    _line(d, (88, 144), (168, 144), BLUE_D, 6)
    _line(d, (88, 172), (168, 172), BLUE_D, 6)
    return im


def _impl_loop_arrow_self_cross():
    im, d = _new()
    d.arc((60, 60, 196, 196), 30, 330, fill=BLUE, width=12)
    _line(d, (56, 128), (200, 128), INK, 6)
    return im


def _impl_loop_band_orbit_dot():
    im, d = _new()
    d.arc((56, 72, 200, 200), 40, 320, fill=BLUE, width=14)
    _circ(d, (108, 108, 148, 148), fill=WHITE, outline=BLUE, w=4)
    return im


def _impl_loop_band_touch_edge():
    im, d = _new()
    d.arc((56, 72, 200, 200), 40, 320, fill=BLUE, width=14)
    _line(d, (200, 128), (228, 128), BLUE, 8)
    return im


def _impl_magnifier_alone():
    im, d = _new()
    _circ(d, (70, 70, 150, 150), fill=None, outline=BLUE, w=10)
    _line(d, (138, 138), (200, 200), BLUE, 12)
    return im


def _impl_magnifier_on_edge_line():
    im, d = _new()
    _circ(d, (70, 70, 150, 150), fill=None, outline=BLUE, w=10)
    _line(d, (138, 138), (200, 200), BLUE, 12)
    return im


def _impl_magnifier_slash():
    im, d = _new()
    _circ(d, (70, 70, 150, 150), fill=None, outline=BLUE, w=10)
    _line(d, (138, 138), (200, 200), BLUE, 12)
    _line(d, (80, 180), (190, 70), INK, 8)
    return im


def _impl_monitor_frame_inner():
    im, d = _new()
    d.rounded_rectangle((60, 72, 196, 160), 6, outline=BLUE, width=8)
    d.rectangle((108, 160, 148, 188), fill=BLUE)
    return im


def _impl_nested_lines_targets():
    im, d = _new()
    d.rectangle((56, 56, 200, 200), outline=BLUE, width=6)
    d.rectangle((80, 80, 176, 176), outline=BLUE_D, width=6)
    _circ(d, (108, 108, 148, 148), fill=BLUE, outline=None, w=0)
    return im


def _impl_nodes_pulse_arc():
    im, d = _new()
    _poly(d, [(128.0, 43.0), (208.8, 101.7), (178.0, 196.8), (78.0, 196.8), (47.2, 101.7)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_paint_bucket_tilt():
    im, d = _new()
    _poly(d, [(88,72),(168,96),(148,196),(68,160)], fill=BLUE, outline=BLUE_D, w=3)
    _poly(d, [(168,96),(200,120),(176,140)], fill=(60,90,200,255), outline=None, w=0)
    return im


def _impl_palette_three_circles():
    im, d = _new()
    _circ(d, (70,96,110,136), fill=(220,60,60,255), outline=None, w=0)
    _circ(d, (108,96,148,136), fill=(60,180,80,255), outline=None, w=0)
    _circ(d, (146,96,186,136), fill=BLUE, outline=None, w=0)
    return im


def _impl_parallel_lines_slash():
    im, d = _new()
    _line(d, (72, 60), (72, 196), BLUE, 10)
    _line(d, (112, 60), (112, 196), BLUE, 10)
    _line(d, (48, 200), (208, 56), INK, 8)
    return im


def _impl_parcel_tied_cube():
    im, d = _new()
    _poly(d, [(128,56),(200,96),(128,136),(56,96)], fill=BLUE, outline=BLUE_D, w=2)
    _poly(d, [(56,96),(128,136),(128,216),(56,176)], fill=BLUE_D, outline=BLUE_D, w=2)
    _poly(d, [(128,136),(200,96),(200,176),(128,216)], fill=(60,90,200,255), outline=BLUE_D, w=2)
    return im


def _impl_pencil_slant():
    im, d = _new()
    _poly(d, [(96,48),(140,52),(168,200),(124,196)], fill=(245,200,120,255), outline=INK, w=2)
    return im


def _impl_person_arrow_command():
    im, d = _new()
    _circ(d, (108,56,148,96), fill=BLUE, outline=None, w=0)
    _poly(d, [(78,108),(178,108),(188,200),(68,200)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_pin_circle_roots():
    im, d = _new()
    _line(d, (128, 180), (128, 120), BLUE, 10)
    _line(d, (128, 120), (188.6, 155.0), BLUE, 8)
    _line(d, (128, 120), (151.9, 185.8), BLUE, 8)
    _line(d, (128, 120), (104.1, 185.8), BLUE, 8)
    _line(d, (128, 120), (67.4, 155.0), BLUE, 8)
    return im


def _impl_pin_on_label_rect():
    im, d = _new()
    _poly(d, [(128.0, 41.0), (179.1, 57.6), (210.7, 101.1), (210.7, 154.9), (179.1, 198.4), (128.0, 215.0), (76.9, 198.4), (45.3, 154.9), (45.3, 101.1), (76.9, 57.6)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_pin_play_triangle():
    im, d = _new()
    _circ(d, (48,48,208,208), fill=None, outline=BLUE, w=8)
    _poly(d, [(118, 88), (118, 168), (178, 128)], fill=BLUE, outline=None, w=0)
    _circ(d, (168,56,208,96), fill=BLUE_D, outline=None, w=0)
    return im


def _impl_plate_on_block():
    im, d = _new()
    d.rounded_rectangle((64, 88, 192, 168), 8, fill=BLUE, outline=BLUE_D, width=2)
    return im


def _impl_play_in_circle():
    im, d = _new()
    _circ(d, (48,48,208,208), fill=None, outline=BLUE, w=8)
    _poly(d, [(118, 88), (118, 168), (178, 128)], fill=BLUE, outline=None, w=0)
    return im


def _impl_plus_vertex_badge():
    im, d = _new()
    _poly(d, [(128.0, 41.0), (179.1, 57.6), (210.7, 101.1), (210.7, 154.9), (179.1, 198.4), (128.0, 215.0), (76.9, 198.4), (45.3, 154.9), (45.3, 101.1), (76.9, 57.6)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_puzzle_piece_single():
    im, d = _new()
    _poly(d, [(70,70),(186,70),(186,120),(160,120),(160,140),(186,140),(186,186),(70,186),(70,140),(96,140),(96,120),(70,120)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_rectangle_rotate_arrows():
    im, d = _new()
    _poly(d, [(128.0, 35.0), (187.8, 56.8), (219.6, 111.9), (208.5, 174.5), (159.8, 215.4), (96.2, 215.4), (47.5, 174.5), (36.4, 111.9), (68.2, 56.8)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_ribbon_name_banner():
    im, d = _new()
    d.rounded_rectangle((72, 108, 184, 148), 10, fill=BLUE, outline=None, w=0)
    _poly(d, [(72,128),(56,128),(72,108)], fill=BLUE_D, outline=None, w=0)
    return im


def _impl_ribbon_through_loop():
    im, d = _new()
    d.rounded_rectangle((88, 108, 168, 148), 20, outline=BLUE, width=8)
    _poly(d, [(40,128),(216,128),(200,108),(56,108)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_ribbon_wave_snake():
    im, d = _new()
    _poly(d, [(48, 160), (48.0, 128.0), (62.5, 155.2), (77.1, 163.6), (91.6, 147.5), (106.2, 117.9), (120.7, 95.3), (135.3, 95.3), (149.8, 117.9), (164.4, 147.5), (178.9, 163.6), (193.5, 155.2), (208.0, 128.0)], fill=BLUE, outline=BLUE_D, w=6)
    return im


def _impl_ring_dot_crosshair():
    im, d = _new()
    d.rounded_rectangle((68, 68, 188, 188), 6, outline=BLUE, width=6)
    _line(d, (128, 68), (128, 188), BLUE, 4)
    _line(d, (68, 128), (188, 128), BLUE, 4)
    return im


def _impl_roots_three_tendrils():
    im, d = _new()
    _line(d, (128, 180), (128, 120), BLUE, 10)
    _line(d, (128, 120), (188.6, 155.0), BLUE, 8)
    _line(d, (128, 120), (151.9, 185.8), BLUE, 8)
    _line(d, (128, 120), (104.1, 185.8), BLUE, 8)
    _line(d, (128, 120), (67.4, 155.0), BLUE, 8)
    return im


def _impl_rounded_rect_classic():
    im, d = _new()
    d.rounded_rectangle((56, 88, 200, 168), 18, outline=BLUE, width=8, fill=WHITE)
    return im


def _impl_ruler_arrow_vertical():
    im, d = _new()
    _poly(d, [(128.0, 38.0), (191.6, 64.4), (218.0, 128.0), (191.6, 191.6), (128.0, 218.0), (64.4, 191.6), (38.0, 128.0), (64.4, 64.4)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_scroll_gear_lines():
    im, d = _new()
    _circ(d, (76,76,180,180), fill=None, outline=BLUE, w=10)
    _line(d, (128.0,88.0), (128.0,40.0), BLUE, 16)
    _line(d, (162.6,108.0), (204.2,84.0), BLUE, 16)
    _line(d, (162.6,148.0), (204.2,172.0), BLUE, 16)
    _line(d, (128.0,168.0), (128.0,216.0), BLUE, 16)
    _line(d, (93.4,148.0), (51.8,172.0), BLUE, 16)
    _line(d, (93.4,108.0), (51.8,84.0), BLUE, 16)
    _circ(d, (108,108,148,148), fill=WHITE, outline=None, w=0)
    return im


def _impl_silhouette_head_shoulders():
    im, d = _new()
    _circ(d, (108,56,148,96), fill=BLUE, outline=None, w=0)
    _poly(d, [(78,108),(178,108),(188,200),(68,200)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_simple_banner_tab():
    im, d = _new()
    _poly(d, [(128.0, 41.0), (179.1, 57.6), (210.7, 101.1), (210.7, 154.9), (179.1, 198.4), (128.0, 215.0), (76.9, 198.4), (45.3, 154.9), (45.3, 101.1), (76.9, 57.6)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_slash_lead_dot():
    im, d = _new()
    _line(d, (88, 88), (168, 168), INK, 10)
    _circ(d, (72, 160, 92, 180), fill=BLUE, outline=None, w=0)
    return im


def _impl_small_rect_on_cube():
    im, d = _new()
    _poly(d, [(128,56),(200,96),(128,136),(56,96)], fill=BLUE, outline=BLUE_D, w=2)
    _poly(d, [(56,96),(128,136),(128,216),(56,176)], fill=BLUE_D, outline=BLUE_D, w=2)
    _poly(d, [(128,136),(200,96),(200,176),(128,216)], fill=(60,90,200,255), outline=BLUE_D, w=2)
    return im


def _impl_sound_arcs_three():
    im, d = _new()
    _poly(d, [(128.0, 42.0), (209.8, 101.4), (178.5, 197.6), (77.5, 197.6), (46.2, 101.4)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_spark_burst_small():
    im, d = _new()
    _line(d, (128, 128), (218.0, 128.0), BLUE, 10)
    _line(d, (128, 128), (191.6, 191.6), BLUE, 10)
    _line(d, (128, 128), (128.0, 218.0), BLUE, 10)
    _line(d, (128, 128), (64.4, 191.6), BLUE, 10)
    _line(d, (128, 128), (38.0, 128.0), BLUE, 10)
    _line(d, (128, 128), (64.4, 64.4), BLUE, 10)
    _line(d, (128, 128), (128.0, 38.0), BLUE, 10)
    _line(d, (128, 128), (191.6, 64.4), BLUE, 10)
    _line(d, (118, 128), (138, 128), INK, 6)
    _line(d, (128, 118), (128, 138), INK, 6)
    return im


def _impl_spiral_pipe_stem():
    im, d = _new()
    d.arc((72, 72, 184, 184), 0, 540, fill=BLUE, width=10)
    return im


def _impl_split_document_panes():
    im, d = _new()
    d.rounded_rectangle((68, 56, 188, 200), 8, outline=BLUE, width=8)
    _line(d, (88, 88), (168, 88), BLUE_D, 6)
    _line(d, (88, 116), (168, 116), BLUE_D, 6)
    _line(d, (88, 144), (168, 144), BLUE_D, 6)
    _line(d, (88, 172), (168, 172), BLUE_D, 6)
    return im


def _impl_square_corner_ticks():
    im, d = _new()
    _poly(d, [(128.0, 39.0), (185.2, 59.8), (215.6, 112.5), (205.1, 172.5), (158.4, 211.6), (97.6, 211.6), (50.9, 172.5), (40.4, 112.5), (70.8, 59.8)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_square_double_frame():
    im, d = _new()
    _poly(d, [(128.0, 39.0), (205.1, 83.5), (205.1, 172.5), (128.0, 217.0), (50.9, 172.5), (50.9, 83.5)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_stack_plates_branch_side():
    im, d = _new()
    d.rounded_rectangle((64, 88, 192, 168), 8, fill=BLUE, outline=BLUE_D, width=2)
    return im


def _impl_stacked_import_arrows():
    im, d = _new()
    d.rounded_rectangle((72, 88, 184, 188), 12, outline=BLUE, width=8)
    _poly(d, [(128, 52), (98, 100), (158, 100)], fill=BLUE, outline=None, w=0)
    return im


def _impl_stacked_planks_tab():
    im, d = _new()
    _poly(d, [(128.0, 40.0), (204.2, 84.0), (204.2, 172.0), (128.0, 216.0), (51.8, 172.0), (51.8, 84.0)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_stacked_rects_depth():
    im, d = _new()
    _poly(d, [(128.0, 39.0), (180.3, 56.0), (212.6, 100.5), (212.6, 155.5), (180.3, 200.0), (128.0, 217.0), (75.7, 200.0), (43.4, 155.5), (43.4, 100.5), (75.7, 56.0)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_stamp_rect_notch():
    im, d = _new()
    d.rounded_rectangle((64, 88, 192, 168), 8, fill=BLUE, outline=BLUE_D, width=2)
    _poly(d, [(128,88),(138,72),(148,88)], fill=WHITE, outline=None, w=0)
    return im


def _impl_tag_outline_dashed():
    im, d = _new()
    _poly(d, [(70,88),(186,88),(200,128),(186,168),(70,168),(56,128)], fill=BLUE, outline=BLUE_D, w=2)
    d.rectangle((80, 108, 176, 148), outline=WHITE, width=3)
    return im


def _impl_tag_pentagon():
    im, d = _new()
    _poly(d, [(70,88),(186,88),(200,128),(186,168),(70,168),(56,128)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_tags_three_offset():
    im, d = _new()
    _poly(d, [(70,88),(186,88),(200,128),(186,168),(70,168),(56,128)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_template_stack_ruler():
    im, d = _new()
    d.rounded_rectangle((64, 88, 192, 168), 8, fill=BLUE, outline=BLUE_D, width=2)
    return im


def _impl_terminal_vertical_bar_wide():
    im, d = _new()
    d.rectangle((112, 48, 152, 208), fill=BLUE)
    return im


def _impl_three_bars_bold_weight():
    im, d = _new()
    d.rectangle((76, 170, 104, 200), fill=BLUE)
    d.rectangle((120, 150, 148, 200), fill=BLUE)
    d.rectangle((164, 130, 192, 200), fill=BLUE)
    return im


def _impl_three_circles_cluster():
    im, d = _new()
    _poly(d, [(128.0, 37.0), (181.5, 54.4), (214.5, 99.9), (214.5, 156.1), (181.5, 201.6), (128.0, 219.0), (74.5, 201.6), (41.5, 156.1), (41.5, 99.9), (74.5, 54.4)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_three_key_shapes():
    im, d = _new()
    _poly(d, [(128.0, 42.0), (202.5, 85.0), (202.5, 171.0), (128.0, 214.0), (53.5, 171.0), (53.5, 85.0)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_trapezoid_pipe_out():
    im, d = _new()
    _poly(d, [(128.0, 40.0), (184.6, 60.6), (214.7, 112.7), (204.2, 172.0), (158.1, 210.7), (97.9, 210.7), (51.8, 172.0), (41.3, 112.7), (71.4, 60.6)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_tray_arrow_double_outline():
    im, d = _new()
    d.rounded_rectangle((72, 88, 184, 188), 12, outline=BLUE, width=8)
    _poly(d, [(128, 52), (98, 100), (158, 100)], fill=BLUE, outline=None, w=0)
    _line(d, (108, 60), (108, 92), BLUE, 6)
    _line(d, (148, 60), (148, 92), BLUE, 6)
    return im


def _impl_tray_arrow_down():
    im, d = _new()
    d.rounded_rectangle((72, 88, 184, 188), 12, outline=BLUE, width=8)
    _poly(d, [(128, 52), (98, 100), (158, 100)], fill=BLUE, outline=None, w=0)
    return im


def _impl_tray_arrow_meta_double_band():
    im, d = _new()
    d.rounded_rectangle((72, 88, 184, 188), 12, outline=BLUE, width=8)
    _poly(d, [(128, 52), (98, 100), (158, 100)], fill=BLUE, outline=None, w=0)
    _line(d, (108, 60), (108, 92), BLUE, 6)
    _line(d, (148, 60), (148, 92), BLUE, 6)
    return im


def _impl_tray_arrow_meta_layer():
    im, d = _new()
    d.rounded_rectangle((72, 88, 184, 188), 12, outline=BLUE, width=8)
    _poly(d, [(128, 52), (98, 100), (158, 100)], fill=BLUE, outline=None, w=0)
    d.rectangle((168, 96, 188, 176), fill=BLUE_D)
    return im


def _impl_tray_arrow_small_layer():
    im, d = _new()
    d.rounded_rectangle((72, 88, 184, 188), 12, outline=BLUE, width=8)
    _poly(d, [(128, 52), (98, 100), (158, 100)], fill=BLUE, outline=None, w=0)
    d.rectangle((60, 96, 78, 176), fill=BLUE_D)
    return im


def _impl_tray_meta_band_only():
    im, d = _new()
    d.rounded_rectangle((72, 88, 184, 188), 12, outline=BLUE, width=8)
    _poly(d, [(128, 52), (98, 100), (158, 100)], fill=BLUE, outline=None, w=0)
    d.rectangle((72, 120, 184, 132), fill=BLUE_D)
    return im


def _impl_tree_cut_diagonal():
    im, d = _new()
    _line(d, (128, 48), (128, 180), BLUE, 10)
    _line(d, (128, 100), (72, 72), BLUE, 8)
    _line(d, (128, 100), (184, 72), BLUE, 8)
    _line(d, (56, 200), (200, 56), INK, 10)
    return im


def _impl_tree_fork_down():
    im, d = _new()
    _line(d, (128, 48), (128, 180), BLUE, 10)
    _line(d, (128, 100), (72, 72), BLUE, 8)
    _line(d, (128, 100), (184, 72), BLUE, 8)
    return im


def _impl_tree_slash_diagonal():
    im, d = _new()
    _line(d, (128, 48), (128, 180), BLUE, 10)
    _line(d, (128, 100), (72, 72), BLUE, 8)
    _line(d, (128, 100), (184, 72), BLUE, 8)
    _line(d, (56, 200), (200, 56), INK, 10)
    return im


def _impl_two_bars_gap_center():
    im, d = _new()
    d.rectangle((76, 170, 104, 200), fill=BLUE)
    d.rectangle((120, 150, 148, 200), fill=BLUE)
    d.rectangle((164, 130, 192, 200), fill=BLUE)
    return im


def _impl_two_nodes_line_plus():
    im, d = _new()
    _circ(d, (56,96,96,136), fill=BLUE, outline=None, w=0)
    _circ(d, (160,96,200,136), fill=BLUE, outline=None, w=0)
    _line(d, (96, 116), (160, 116), BLUE, 10)
    return im


def _impl_two_squares_slash():
    im, d = _new()
    _poly(d, [(128.0, 41.0), (179.1, 57.6), (210.7, 101.1), (210.7, 154.9), (179.1, 198.4), (128.0, 215.0), (76.9, 198.4), (45.3, 154.9), (45.3, 101.1), (76.9, 57.6)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def _impl_vertex_arrow_lens():
    im, d = _new()
    _circ(d, (70, 70, 150, 150), fill=None, outline=BLUE, w=10)
    _line(d, (138, 138), (200, 200), BLUE, 12)
    return im


def _impl_wave_arrow_forward():
    im, d = _new()
    _poly(d, [(56, 128), (188, 128), (160, 96), (160, 112), (200, 128), (160, 144), (160, 160)], fill=BLUE, outline=BLUE_D, w=2)
    return im


def _impl_wave_s_curve():
    im, d = _new()
    _poly(d, [(48, 128), (48.0, 128.0), (68.0, 156.3), (88.0, 168.0), (108.0, 156.3), (128.0, 128.0), (148.0, 99.7), (168.0, 88.0), (188.0, 99.7), (208.0, 128.0)], fill=BLUE, outline=BLUE_D, w=4)
    return im


def _impl_wildcard_four_blocks():
    im, d = _new()
    d.rounded_rectangle((56,56,104,104), 6, fill=BLUE)
    d.rounded_rectangle((144,56,192,104), 6, fill=BLUE)
    d.rounded_rectangle((56,144,104,192), 6, fill=BLUE)
    d.rounded_rectangle((144,144,192,192), 6, fill=BLUE)
    return im


def _impl_window_wrench():
    im, d = _new()
    d.rectangle((64, 72, 192, 168), outline=BLUE, width=8)
    _line(d, (96, 188), (168, 96), BLUE, 10)
    return im


def _impl_wireframe_three_cells():
    im, d = _new()
    _poly(d, [(128.0, 37.0), (181.5, 54.4), (214.5, 99.9), (214.5, 156.1), (181.5, 201.6), (128.0, 219.0), (74.5, 201.6), (41.5, 156.1), (41.5, 99.9), (74.5, 54.4)], fill=BLUE, outline=BLUE_D, w=3)
    return im


def render_shape(shape_id: str) -> Image.Image:
    match shape_id:
        case "anchor_hook_down":
            return _impl_anchor_hook_down()
        case "anchor_hook_up":
            return _impl_anchor_hook_up()
        case "angle_brackets_pair":
            return _impl_angle_brackets_pair()
        case "arc_port_hooks":
            return _impl_arc_port_hooks()
        case "arrow_down_double_stroke":
            return _impl_arrow_down_double_stroke()
        case "arrow_down_thick_shaft":
            return _impl_arrow_down_thick_shaft()
        case "arrow_into_funnel":
            return _impl_arrow_into_funnel()
        case "arrow_return_hook":
            return _impl_arrow_return_hook()
        case "arrow_right_taper":
            return _impl_arrow_right_taper()
        case "arrows_bidir_vertical":
            return _impl_arrows_bidir_vertical()
        case "arrows_expand_corners":
            return _impl_arrows_expand_corners()
        case "arrows_refresh_circle":
            return _impl_arrows_refresh_circle()
        case "at_on_circle_node":
            return _impl_at_on_circle_node()
        case "at_on_rounded_rect":
            return _impl_at_on_rounded_rect()
        case "atom_next_tick":
            return _impl_atom_next_tick()
        case "atom_orbits_three":
            return _impl_atom_orbits_three()
        case "atom_tick_edge_vector":
            return _impl_atom_tick_edge_vector()
        case "backslash_thick":
            return _impl_backslash_thick()
        case "balance_beam":
            return _impl_balance_beam()
        case "bars_short_three":
            return _impl_bars_short_three()
        case "bars_tall_three":
            return _impl_bars_tall_three()
        case "bars_tall_three_mirror":
            return _impl_bars_tall_three_mirror()
        case "binary_tree_three":
            return _impl_binary_tree_three()
        case "book_question_mark_shape":
            return _impl_book_question_mark_shape()
        case "box_arrow_out_right":
            return _impl_box_arrow_out_right()
        case "box_meta_corner_dot":
            return _impl_box_meta_corner_dot()
        case "braces_curly_pair_wide":
            return _impl_braces_curly_pair_wide()
        case "brackets_dot_trailing":
            return _impl_brackets_dot_trailing()
        case "brackets_property_dot":
            return _impl_brackets_property_dot()
        case "brackets_ribbon_tab":
            return _impl_brackets_ribbon_tab()
        case "bullseye_rings":
            return _impl_bullseye_rings()
        case "burst_rays_plus_center":
            return _impl_burst_rays_plus_center()
        case "chain_link_tag":
            return _impl_chain_link_tag()
        case "chain_two_links":
            return _impl_chain_two_links()
        case "checker_fade_blocks":
            return _impl_checker_fade_blocks()
        case "chevron_left_block":
            return _impl_chevron_left_block()
        case "chevron_right_block":
            return _impl_chevron_right_block()
        case "chevrons_stack_up":
            return _impl_chevrons_stack_up()
        case "chip_die_outline":
            return _impl_chip_die_outline()
        case "circle_dot_ring":
            return _impl_circle_dot_ring()
        case "clock_arc_chain_link":
            return _impl_clock_arc_chain_link()
        case "cloud_dashed_query":
            return _impl_cloud_dashed_query()
        case "cog_lightning_overlay":
            return _impl_cog_lightning_overlay()
        case "cog_six_spoke":
            return _impl_cog_six_spoke()
        case "columns_lines_formal":
            return _impl_columns_lines_formal()
        case "corner_branch_left":
            return _impl_corner_branch_left()
        case "corner_branch_right":
            return _impl_corner_branch_right()
        case "cr_lf_glyph":
            return _impl_cr_lf_glyph()
        case "crosshair_over_lens":
            return _impl_crosshair_over_lens()
        case "cube_isometric_solid":
            return _impl_cube_isometric_solid()
        case "cube_orb_vertex":
            return _impl_cube_orb_vertex()
        case "cube_small_brackets":
            return _impl_cube_small_brackets()
        case "dashed_arrow_open_head":
            return _impl_dashed_arrow_open_head()
        case "dashed_frame_meta":
            return _impl_dashed_frame_meta()
        case "dashed_frame_plain":
            return _impl_dashed_frame_plain()
        case "diamond_branch_fill":
            return _impl_diamond_branch_fill()
        case "diamond_crystal_facets":
            return _impl_diamond_crystal_facets()
        case "diamond_hollow_agg":
            return _impl_diamond_hollow_agg()
        case "document_lines":
            return _impl_document_lines()
        case "door_arrow_exit":
            return _impl_door_arrow_exit()
        case "dot_leading_brackets":
            return _impl_dot_leading_brackets()
        case "dot_trail_slash":
            return _impl_dot_trail_slash()
        case "dots_vertical_column":
            return _impl_dots_vertical_column()
        case "edge_hollow_circle_end":
            return _impl_edge_hollow_circle_end()
        case "edge_line_bullseye":
            return _impl_edge_line_bullseye()
        case "edge_plus_starburst":
            return _impl_edge_plus_starburst()
        case "edge_question_orb":
            return _impl_edge_question_orb()
        case "edge_segment_dashed":
            return _impl_edge_segment_dashed()
        case "eye_open":
            return _impl_eye_open()
        case "eye_slash":
            return _impl_eye_slash()
        case "eye_slot_pill":
            return _impl_eye_slot_pill()
        case "field_equals_sign":
            return _impl_field_equals_sign()
        case "fill_circle_blue":
            return _impl_fill_circle_blue()
        case "fill_circle_green":
            return _impl_fill_circle_green()
        case "fill_circle_red":
            return _impl_fill_circle_red()
        case "flag_pennon_one":
            return _impl_flag_pennon_one()
        case "flag_pennon_two":
            return _impl_flag_pennon_two()
        case "flask_conical":
            return _impl_flask_conical()
        case "folder_front_lens":
            return _impl_folder_front_lens()
        case "folder_tab_open":
            return _impl_folder_tab_open()
        case "folder_tabs_three":
            return _impl_folder_tabs_three()
        case "frame_crosshair_small":
            return _impl_frame_crosshair_small()
        case "funnel_trapezoid":
            return _impl_funnel_trapezoid()
        case "glasses_on_rect":
            return _impl_glasses_on_rect()
        case "globe_meridians_inner":
            return _impl_globe_meridians_inner()
        case "hex_node_lens":
            return _impl_hex_node_lens()
        case "hook_j_curve":
            return _impl_hook_j_curve()
        case "hook_tail_curve":
            return _impl_hook_tail_curve()
        case "horizontal_bar_thick":
            return _impl_horizontal_bar_thick()
        case "infinity_loop_band":
            return _impl_infinity_loop_band()
        case "keys_on_ring":
            return _impl_keys_on_ring()
        case "keys_star_burst":
            return _impl_keys_star_burst()
        case "label_card_slash":
            return _impl_label_card_slash()
        case "layers_eye_stack":
            return _impl_layers_eye_stack()
        case "lightning_bolt":
            return _impl_lightning_bolt()
        case "lightning_inner_ring":
            return _impl_lightning_inner_ring()
        case "line_two_nodes_solid":
            return _impl_line_two_nodes_solid()
        case "list_lines_bullets":
            return _impl_list_lines_bullets()
        case "loop_arrow_self_cross":
            return _impl_loop_arrow_self_cross()
        case "loop_band_orbit_dot":
            return _impl_loop_band_orbit_dot()
        case "loop_band_touch_edge":
            return _impl_loop_band_touch_edge()
        case "magnifier_alone":
            return _impl_magnifier_alone()
        case "magnifier_on_edge_line":
            return _impl_magnifier_on_edge_line()
        case "magnifier_slash":
            return _impl_magnifier_slash()
        case "monitor_frame_inner":
            return _impl_monitor_frame_inner()
        case "nested_lines_targets":
            return _impl_nested_lines_targets()
        case "nodes_pulse_arc":
            return _impl_nodes_pulse_arc()
        case "paint_bucket_tilt":
            return _impl_paint_bucket_tilt()
        case "palette_three_circles":
            return _impl_palette_three_circles()
        case "parallel_lines_slash":
            return _impl_parallel_lines_slash()
        case "parcel_tied_cube":
            return _impl_parcel_tied_cube()
        case "pencil_slant":
            return _impl_pencil_slant()
        case "person_arrow_command":
            return _impl_person_arrow_command()
        case "pin_circle_roots":
            return _impl_pin_circle_roots()
        case "pin_on_label_rect":
            return _impl_pin_on_label_rect()
        case "pin_play_triangle":
            return _impl_pin_play_triangle()
        case "plate_on_block":
            return _impl_plate_on_block()
        case "play_in_circle":
            return _impl_play_in_circle()
        case "plus_vertex_badge":
            return _impl_plus_vertex_badge()
        case "puzzle_piece_single":
            return _impl_puzzle_piece_single()
        case "rectangle_rotate_arrows":
            return _impl_rectangle_rotate_arrows()
        case "ribbon_name_banner":
            return _impl_ribbon_name_banner()
        case "ribbon_through_loop":
            return _impl_ribbon_through_loop()
        case "ribbon_wave_snake":
            return _impl_ribbon_wave_snake()
        case "ring_dot_crosshair":
            return _impl_ring_dot_crosshair()
        case "roots_three_tendrils":
            return _impl_roots_three_tendrils()
        case "rounded_rect_classic":
            return _impl_rounded_rect_classic()
        case "ruler_arrow_vertical":
            return _impl_ruler_arrow_vertical()
        case "scroll_gear_lines":
            return _impl_scroll_gear_lines()
        case "silhouette_head_shoulders":
            return _impl_silhouette_head_shoulders()
        case "simple_banner_tab":
            return _impl_simple_banner_tab()
        case "slash_lead_dot":
            return _impl_slash_lead_dot()
        case "small_rect_on_cube":
            return _impl_small_rect_on_cube()
        case "sound_arcs_three":
            return _impl_sound_arcs_three()
        case "spark_burst_small":
            return _impl_spark_burst_small()
        case "spiral_pipe_stem":
            return _impl_spiral_pipe_stem()
        case "split_document_panes":
            return _impl_split_document_panes()
        case "square_corner_ticks":
            return _impl_square_corner_ticks()
        case "square_double_frame":
            return _impl_square_double_frame()
        case "stack_plates_branch_side":
            return _impl_stack_plates_branch_side()
        case "stacked_import_arrows":
            return _impl_stacked_import_arrows()
        case "stacked_planks_tab":
            return _impl_stacked_planks_tab()
        case "stacked_rects_depth":
            return _impl_stacked_rects_depth()
        case "stamp_rect_notch":
            return _impl_stamp_rect_notch()
        case "tag_outline_dashed":
            return _impl_tag_outline_dashed()
        case "tag_pentagon":
            return _impl_tag_pentagon()
        case "tags_three_offset":
            return _impl_tags_three_offset()
        case "template_stack_ruler":
            return _impl_template_stack_ruler()
        case "terminal_vertical_bar_wide":
            return _impl_terminal_vertical_bar_wide()
        case "three_bars_bold_weight":
            return _impl_three_bars_bold_weight()
        case "three_circles_cluster":
            return _impl_three_circles_cluster()
        case "three_key_shapes":
            return _impl_three_key_shapes()
        case "trapezoid_pipe_out":
            return _impl_trapezoid_pipe_out()
        case "tray_arrow_double_outline":
            return _impl_tray_arrow_double_outline()
        case "tray_arrow_down":
            return _impl_tray_arrow_down()
        case "tray_arrow_meta_double_band":
            return _impl_tray_arrow_meta_double_band()
        case "tray_arrow_meta_layer":
            return _impl_tray_arrow_meta_layer()
        case "tray_arrow_small_layer":
            return _impl_tray_arrow_small_layer()
        case "tray_meta_band_only":
            return _impl_tray_meta_band_only()
        case "tree_cut_diagonal":
            return _impl_tree_cut_diagonal()
        case "tree_fork_down":
            return _impl_tree_fork_down()
        case "tree_slash_diagonal":
            return _impl_tree_slash_diagonal()
        case "two_bars_gap_center":
            return _impl_two_bars_gap_center()
        case "two_nodes_line_plus":
            return _impl_two_nodes_line_plus()
        case "two_squares_slash":
            return _impl_two_squares_slash()
        case "vertex_arrow_lens":
            return _impl_vertex_arrow_lens()
        case "wave_arrow_forward":
            return _impl_wave_arrow_forward()
        case "wave_s_curve":
            return _impl_wave_s_curve()
        case "wildcard_four_blocks":
            return _impl_wildcard_four_blocks()
        case "window_wrench":
            return _impl_window_wrench()
        case "wireframe_three_cells":
            return _impl_wireframe_three_cells()
        case _:
            raise KeyError(shape_id)
