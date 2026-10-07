"""Generate the prototype's sprite set.

All gameplay/UI shapes are rendered in white or grayscale so Unity can tint them
(SpriteRenderer.color / Image.color). Shapes are drawn 4x supersampled and box-filtered
down for clean anti-aliased edges.

Usage:
    python Tools/ArtGen/generate_sprites.py

Output: Assets/_Project/Art/Sprites/*.png and Assets/_Project/Art/Patterns/*.png
Requires: Pillow, numpy.
"""

from pathlib import Path
import math

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

SUPERSAMPLE = 4
PROJECT_ROOT = Path(__file__).resolve().parents[2]
SPRITE_DIR = PROJECT_ROOT / "Assets" / "_Project" / "Art" / "Sprites"
PATTERN_DIR = PROJECT_ROOT / "Assets" / "_Project" / "Art" / "Patterns"


def rounded_mask(width, height, radius, inset=0.0):
    """Anti-aliased rounded-rectangle coverage mask in [0, 1]."""
    big = Image.new("L", (width * SUPERSAMPLE, height * SUPERSAMPLE), 0)
    draw = ImageDraw.Draw(big)
    left = inset * SUPERSAMPLE
    top = inset * SUPERSAMPLE
    right = (width - inset) * SUPERSAMPLE - 1
    bottom = (height - inset) * SUPERSAMPLE - 1
    draw.rounded_rectangle((left, top, right, bottom), radius=max(0.0, radius) * SUPERSAMPLE, fill=255)
    small = big.resize((width, height), Image.Resampling.BOX)
    return np.asarray(small, dtype=np.float32) / 255.0


def polygon_mask(width, height, points):
    big = Image.new("L", (width * SUPERSAMPLE, height * SUPERSAMPLE), 0)
    draw = ImageDraw.Draw(big)
    draw.polygon([(x * SUPERSAMPLE, y * SUPERSAMPLE) for x, y in points], fill=255)
    small = big.resize((width, height), Image.Resampling.BOX)
    return np.asarray(small, dtype=np.float32) / 255.0


def vertical_ramp(height, width, stops):
    """stops: list of (t in [0,1] from top, value). Returns HxW array."""
    ys = (np.arange(height, dtype=np.float32) + 0.5) / height
    ts = np.array([s[0] for s in stops], dtype=np.float32)
    vs = np.array([s[1] for s in stops], dtype=np.float32)
    column = np.interp(ys, ts, vs)
    return np.repeat(column[:, None], width, axis=1)


def save_rgba(path, value, alpha):
    """value: HxW gray or HxWx3 color in [0,1]; alpha: HxW in [0,1]."""
    path.parent.mkdir(parents=True, exist_ok=True)
    if value.ndim == 2:
        rgb = np.repeat(value[:, :, None], 3, axis=2)
    else:
        rgb = value
    rgba = np.concatenate([rgb, alpha[:, :, None]], axis=2)
    rgba = np.clip(rgba, 0.0, 1.0)
    Image.fromarray((rgba * 255.0 + 0.5).astype(np.uint8), "RGBA").save(path)
    print(f"wrote {path.relative_to(PROJECT_ROOT)} ({value.shape[1]}x{value.shape[0]})")


def panel_rounded():
    size, radius = 160, 48
    alpha = rounded_mask(size, size, radius)
    save_rgba(SPRITE_DIR / "spr_panel_rounded.png", np.ones((size, size), np.float32), alpha)


def ring_rounded():
    size, radius, thickness = 160, 48, 10
    outer = rounded_mask(size, size, radius)
    inner = rounded_mask(size, size, radius - thickness, inset=thickness)
    save_rgba(SPRITE_DIR / "spr_ring_rounded.png", np.ones((size, size), np.float32), np.clip(outer - inner, 0.0, 1.0))


def cell_tile():
    size, inset, radius = 160, 7, 26
    alpha = rounded_mask(size, size, radius, inset=inset)
    shade = vertical_ramp(size, size, [(0.0, 0.70), (0.14, 0.86), (0.30, 1.0), (1.0, 1.0)])
    save_rgba(SPRITE_DIR / "spr_cell_tile.png", shade, alpha)


def box_body():
    # 160 px = 1 board cell at PPU 160. Front face = bottom 34 px, rim highlight = top 14 px.
    size, radius = 160, 36
    alpha = rounded_mask(size, size, radius)
    shade = vertical_ramp(size, size, [
        (0.0, 1.0), (0.09, 1.0), (0.12, 0.93), (0.78, 0.90),
        (0.785, 1.0), (0.80, 0.80), (1.0, 0.64)])
    xs = (np.arange(size, dtype=np.float32) + 0.5) / size
    side = 1.0 - 0.10 * np.clip(np.abs(xs - 0.5) * 2.0 - 0.75, 0.0, 1.0) / 0.25
    shade = shade * side[None, :]
    save_rgba(SPRITE_DIR / "spr_box_body.png", shade, alpha)


def box_interior():
    size, radius = 96, 14
    alpha = rounded_mask(size, size, radius)
    shade = vertical_ramp(size, size, [(0.0, 0.45), (0.16, 0.80), (0.32, 1.0), (1.0, 1.0)])
    save_rgba(SPRITE_DIR / "spr_box_interior.png", shade, alpha)


def pill():
    width, height = 96, 48
    alpha = rounded_mask(width, height, height // 2)
    save_rgba(SPRITE_DIR / "spr_pill.png", np.ones((height, width), np.float32), alpha)


def tank_frame():
    size, radius, thickness = 160, 40, 12
    outer = rounded_mask(size, size, radius)
    inner = rounded_mask(size, size, radius - thickness, inset=thickness)
    shade = vertical_ramp(size, size, [(0.0, 1.0), (0.5, 0.92), (1.0, 0.80)])
    save_rgba(SPRITE_DIR / "spr_tank_frame.png", shade, np.clip(outer - inner, 0.0, 1.0))


def tank_lip():
    width, height, radius = 160, 64, 22
    alpha = rounded_mask(width, height, radius)
    shade = vertical_ramp(height, width, [(0.0, 1.0), (0.18, 0.95), (0.55, 0.78), (1.0, 0.62)])
    save_rgba(SPRITE_DIR / "spr_tank_lip.png", shade, alpha)


def star():
    size = 128
    cx = cy = size / 2.0
    outer_r, inner_r = size * 0.46, size * 0.20
    points = []
    for i in range(10):
        angle = -math.pi / 2.0 + i * math.pi / 5.0
        r = outer_r if i % 2 == 0 else inner_r
        points.append((cx + r * math.cos(angle), cy + r * math.sin(angle)))
    alpha = polygon_mask(size, size, points)
    save_rgba(SPRITE_DIR / "spr_star.png", np.ones((size, size), np.float32), alpha)


def sparkle():
    size = 128
    cx = cy = size / 2.0
    long_r, short_r = size * 0.48, size * 0.07
    points = []
    for i in range(8):
        angle = -math.pi / 2.0 + i * math.pi / 4.0
        r = long_r if i % 2 == 0 else short_r
        points.append((cx + r * math.cos(angle), cy + r * math.sin(angle)))
    alpha = polygon_mask(size, size, points)
    ys, xs = np.mgrid[0:size, 0:size].astype(np.float32)
    dist = np.sqrt((xs + 0.5 - cx) ** 2 + (ys + 0.5 - cy) ** 2) / (size * 0.5)
    glow = np.clip(1.0 - dist / 0.45, 0.0, 1.0) ** 2 * 0.65
    save_rgba(SPRITE_DIR / "spr_sparkle.png", np.ones((size, size), np.float32), np.maximum(alpha, glow))


def soft_circle():
    size = 128
    ys, xs = np.mgrid[0:size, 0:size].astype(np.float32)
    center = size / 2.0
    dist = np.sqrt((xs + 0.5 - center) ** 2 + (ys + 0.5 - center) ** 2) / center
    alpha = np.clip(1.0 - dist, 0.0, 1.0) ** 2
    save_rgba(SPRITE_DIR / "spr_soft_circle.png", np.ones((size, size), np.float32), alpha)


def rays():
    size, ray_count = 512, 12
    ys, xs = np.mgrid[0:size, 0:size].astype(np.float32)
    center = size / 2.0
    dx = xs + 0.5 - center
    dy = ys + 0.5 - center
    dist = np.sqrt(dx * dx + dy * dy) / center
    angle = np.arctan2(dy, dx)
    wave = 0.5 + 0.5 * np.cos(angle * ray_count)
    wedge = np.clip((wave - 0.30) / 0.45, 0.0, 1.0)
    falloff = np.clip(1.0 - dist, 0.0, 1.0) ** 1.2
    core = np.clip(1.0 - dist / 0.40, 0.0, 1.0) ** 2 * 0.45
    alpha = np.clip(wedge * falloff * 0.70 + core, 0.0, 1.0)
    image = Image.fromarray((alpha * 255.0).astype(np.uint8), "L").filter(ImageFilter.GaussianBlur(3.0))
    alpha = np.asarray(image, dtype=np.float32) / 255.0
    save_rgba(SPRITE_DIR / "spr_rays.png", np.ones((size, size), np.float32), alpha)


def confetti():
    width, height = 32, 16
    alpha = rounded_mask(width, height, 4)
    save_rgba(SPRITE_DIR / "spr_confetti.png", np.ones((height, width), np.float32), alpha)


def button():
    width, height, radius = 256, 112, 44
    alpha = rounded_mask(width, height, radius)
    shade = vertical_ramp(height, width, [
        (0.0, 1.0), (0.08, 1.0), (0.12, 0.94), (0.78, 0.90),
        (0.80, 0.74), (1.0, 0.62)])
    save_rgba(SPRITE_DIR / "spr_button.png", shade, alpha)


def icon_restart():
    size = 128
    big_size = size * SUPERSAMPLE
    big = Image.new("L", (big_size, big_size), 0)
    draw = ImageDraw.Draw(big)
    stroke = 15 * SUPERSAMPLE
    pad = 22 * SUPERSAMPLE
    box = (pad, pad, big_size - pad, big_size - pad)
    draw.arc(box, start=-35, end=245, fill=255, width=stroke)
    center = big_size / 2.0
    radius = (big_size - 2 * pad) / 2.0 - stroke / 2.0
    tip_angle = math.radians(-35)
    tip_x = center + radius * math.cos(tip_angle)
    tip_y = center + radius * math.sin(tip_angle)
    head = 26 * SUPERSAMPLE
    draw.polygon([
        (tip_x + head * 0.95, tip_y - head * 0.15),
        (tip_x - head * 0.55, tip_y - head * 0.95),
        (tip_x - head * 0.25, tip_y + head * 0.85)], fill=255)
    small = big.resize((size, size), Image.Resampling.BOX)
    alpha = np.asarray(small, dtype=np.float32) / 255.0
    save_rgba(SPRITE_DIR / "spr_icon_restart.png", np.ones((size, size), np.float32), alpha)


def background():
    width, height = 540, 960
    top = np.array([0.24, 0.17, 0.39], np.float32)
    bottom = np.array([0.11, 0.08, 0.19], np.float32)
    t = vertical_ramp(height, width, [(0.0, 0.0), (1.0, 1.0)])
    rgb = top[None, None, :] * (1.0 - t[:, :, None]) + bottom[None, None, :] * t[:, :, None]
    blobs = Image.new("L", (width, height), 0)
    draw = ImageDraw.Draw(blobs)
    for cx, cy, r, v in [(90, 140, 160, 70), (470, 330, 190, 55), (120, 760, 210, 45), (440, 880, 150, 40)]:
        draw.ellipse((cx - r, cy - r, cx + r, cy + r), fill=v)
    blobs = blobs.filter(ImageFilter.GaussianBlur(70))
    glow = np.asarray(blobs, dtype=np.float32) / 255.0
    tint = np.array([0.52, 0.40, 0.78], np.float32)
    rgb = rgb + glow[:, :, None] * tint[None, None, :] * 0.55
    ys, xs = np.mgrid[0:height, 0:width].astype(np.float32)
    vx = (xs / width - 0.5) * 2.0
    vy = (ys / height - 0.5) * 2.0
    vignette = 1.0 - 0.28 * np.clip(vx * vx * 0.6 + vy * vy * 0.4, 0.0, 1.0)
    rgb = rgb * vignette[:, :, None]
    save_rgba(SPRITE_DIR / "spr_background.png", rgb, np.ones((height, width), np.float32))


def pattern_sun():
    """36x12 tone pattern for a 6x2-cell tank. Gray value = tone (luminance); alpha 0 = empty."""
    width, height, filled_rows = 36, 12, 9
    lum = np.full((height, width), 0.55, np.float32)
    alpha = np.zeros((height, width), np.float32)
    alpha[height - filled_rows:, :] = 1.0
    # Image rows: 0 = top. Sand occupies rows 3..11.
    for x in range(width):
        hill = 2.2 + 1.3 * math.sin(x * 0.42) + 0.8 * math.sin(x * 0.17 + 1.3)
        hill_top = height - int(round(hill)) - 1
        for y in range(max(hill_top, height - filled_rows), height):
            lum[y, x] = 0.22
    sun_cx, sun_cy, sun_r = 24.5, 6.0, 2.6
    for y in range(height - filled_rows, height):
        for x in range(width):
            d = math.hypot(x + 0.5 - sun_cx, y + 0.5 - sun_cy)
            if d <= sun_r:
                lum[y, x] = 0.95
            elif d <= sun_r + 1.2 and lum[y, x] > 0.3:
                lum[y, x] = 0.78
    for x in range(4, 14):
        y = 5 if x % 3 != 0 else 4
        lum[y, x] = 0.8
    PATTERN_DIR.mkdir(parents=True, exist_ok=True)
    rgba = np.stack([lum, lum, lum, alpha], axis=2)
    Image.fromarray((np.clip(rgba, 0.0, 1.0) * 255.0 + 0.5).astype(np.uint8), "RGBA").save(PATTERN_DIR / "pattern_sun.png")
    print(f"wrote {(PATTERN_DIR / 'pattern_sun.png').relative_to(PROJECT_ROOT)} ({width}x{height})")


def main():
    panel_rounded()
    ring_rounded()
    cell_tile()
    box_body()
    box_interior()
    pill()
    tank_frame()
    tank_lip()
    star()
    sparkle()
    soft_circle()
    rays()
    confetti()
    button()
    icon_restart()
    background()
    pattern_sun()


if __name__ == "__main__":
    main()
