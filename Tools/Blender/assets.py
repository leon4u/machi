"""Tomoru Machi 3D asset builders. Every asset faces -Y (toward the camera / street).
Origin = ground center of the footprint. Each builder returns the joined object."""
import math, random
from lowpoly_lib import box, gable, hip, lean, cyl, cone, blob, join

# ------------------------------------------------------------------ props
def street_lamp(lit=True, name="prop_street_lamp"):
    lamp = "lamp" if lit else "paper_old"
    return join([
        cyl(0.18, 0.3, color="stone_dark", seg=6),
        cyl(0.08, 3.6, (0, 0, 0.3), color="metal", seg=6),
        box(0.9, 0.08, 0.08, (0.35, 0, 3.75), color="metal"),
        box(0.36, 0.36, 0.42, (0.75, 0, 3.4), color=lamp),
        hip(0.6, 0.6, 0.25, (0.75, 0, 3.82), color="roof_dark", ridge=0.0),
    ], name)

def utility_pole(name="prop_utility_pole"):
    return join([
        cyl(0.16, 8.0, color="wood_gray", seg=6, r2=0.12),
        box(1.8, 0.12, 0.12, (0, 0, 7.2), color="wood_gray"),
        box(1.2, 0.12, 0.12, (0, 0, 6.6), color="wood_gray"),
        cyl(0.22, 0.6, (0.35, 0.2, 5.6), color="metal", seg=6),
    ], name)

def planter(name="prop_planter", flower="sakura"):
    parts = [box(0.9, 0.45, 0.4, color="wood_dark"),
             blob(0.38, (0, 0, 0.6), color="leaf", sz=0.8)]
    if flower:
        parts += [blob(0.1, (0.15, -0.15, 0.85), color=flower), blob(0.1, (-0.2, -0.1, 0.8), color=flower)]
    return join(parts, name)

def pot_plant(name="prop_pot_plant"):
    return join([cyl(0.22, 0.38, color="rust", seg=7, r2=0.27),
                 blob(0.32, (0, 0, 0.62), color="leaf", sz=1.1)], name)

def bench(name="prop_bench"):
    return join([box(1.6, 0.45, 0.08, (0, 0, 0.42), color="wood"),
                 box(0.08, 0.4, 0.42, (-0.65, 0, 0), color="wood_dark"),
                 box(0.08, 0.4, 0.42, (0.65, 0, 0), color="wood_dark"),
                 box(1.6, 0.06, 0.35, (0, 0.2, 0.55), color="wood")], name)

def vending_machine(name="prop_vending_machine"):
    return join([box(1.0, 0.8, 1.85, color="white"),
                 box(0.8, 0.05, 0.75, (0, -0.41, 0.95), color="lamp"),
                 box(0.8, 0.05, 0.25, (0, -0.41, 0.25), color="black"),
                 box(1.02, 0.82, 0.1, (0, 0, 1.85), color="vermilion")], name)

def crate(name="prop_crate", fill="mustard"):
    return join([box(0.7, 0.5, 0.35, color="wood"),
                 blob(0.12, (-0.15, 0, 0.4), color=fill), blob(0.12, (0.12, 0.05, 0.4), color=fill),
                 blob(0.11, (0, -0.1, 0.42), color=fill)], name)

def fence(length=4.0, name="prop_fence"):
    parts = [box(length, 0.08, 0.1, (0, 0, 0.75), color="wood"), box(length, 0.08, 0.1, (0, 0, 0.35), color="wood")]
    n = int(length / 1.0) + 1
    for i in range(n):
        parts.append(box(0.12, 0.12, 1.0, (-length / 2 + i * length / (n - 1), 0, 0), color="wood_dark"))
    return join(parts, name)

def cedar(h=7.0, name="tree_cedar"):
    return join([cyl(0.25, h * 0.25, color="bark", seg=5),
                 cone(1.6, h * 0.45, (0, 0, h * 0.2), color="cedar"),
                 cone(1.2, h * 0.4, (0, 0, h * 0.45), color="cedar"),
                 cone(0.8, h * 0.3, (0, 0, h * 0.7), color="cedar")], name)

def round_tree(name="tree_round", color="leaf"):
    return join([cyl(0.22, 1.6, color="bark", seg=5),
                 blob(1.4, (0, 0, 2.6), color=color, sz=0.85),
                 blob(0.9, (0.7, 0.3, 2.2), color=color, sz=0.8)], name)

def sakura_tree(name="tree_sakura"):
    return join([cyl(0.25, 1.8, color="bark", seg=5),
                 box(0.15, 0.15, 1.2, (0.4, 0, 1.5), color="bark"),
                 blob(1.5, (0, 0, 2.9), color="sakura", sz=0.75),
                 blob(1.0, (1.0, 0.2, 2.6), color="sakura", sz=0.7),
                 blob(0.9, (-0.9, -0.3, 2.5), color="sakura", sz=0.7)], name)

def bush(name="prop_bush"):
    return join([blob(0.6, (0, 0, 0.35), color="grass_dark", sz=0.7),
                 blob(0.45, (0.45, 0.1, 0.3), color="leaf", sz=0.7)], name)

def weeds(name="prop_weeds"):
    rnd = random.Random(3)
    parts = []
    for i in range(6):
        parts.append(cone(0.08, 0.35 + rnd.random() * 0.3, (rnd.uniform(-0.4, 0.4), rnd.uniform(-0.3, 0.3), 0), color="moss", seg=4))
    return join(parts, name)

def rock(name="prop_rock"):
    return join([blob(0.6, (0, 0, 0.2), color="stone", sz=0.6)], name)

def paper_lantern(name="prop_paper_lantern"):
    return join([cyl(0.22, 0.5, (0, 0, 0), color="lamp", seg=8),
                 cyl(0.18, 0.06, (0, 0, 0.5), color="black", seg=8),
                 cyl(0.18, 0.06, (0, 0, -0.06), color="black", seg=8),
                 box(0.03, 0.03, 0.3, (0, 0, 0.56), color="black")], name)

def rail_track(length=10.0, name="rail_track"):
    parts = [box(length, 2.4, 0.15, color="stone_dark"),
             box(length, 0.08, 0.14, (0, -0.72, 0.27), color="rust"),
             box(length, 0.08, 0.14, (0, 0.72, 0.27), color="rust")]
    n = int(length / 0.8)
    for i in range(n):
        parts.append(box(0.22, 2.0, 0.12, (-length / 2 + 0.4 + i * 0.8, 0, 0.15), color="wood_dark"))
    return join(parts, name)

def platform(length=20.0, name="rail_platform"):
    return join([box(length, 3.0, 0.9, color="stone"),
                 box(length, 0.3, 0.02, (0, -1.35, 0.9), color="white"),
                 box(length, 0.15, 0.02, (0, -1.0, 0.9), color="mustard")], name)

# ------------------------------------------------------------ station area
def station_sign(stage=0):
    if stage == 0:
        parts = [box(0.15, 0.15, 2.4, (-1.0, 0, 0), color="wood_gray"),
                 box(0.15, 0.15, 2.2, (1.0, 0, 0), color="wood_gray"),
                 box(2.4, 0.08, 0.8, (0, -0.1, 1.3), color="paper_old"),
                 box(0.7, 0.03, 0.5, (-0.5, -0.16, 1.45), color="white"),   # peeling paper
                 box(0.4, 0.03, 0.3, (0.6, -0.16, 1.4), color="plaster_dirty"),
                 box(0.5, 0.5, 0.15, (0, 0, 2.4), color="metal"),
                 box(0.3, 0.3, 0.25, (0, 0, 2.15), color="paper_old"),      # dead lamp
                 cone(0.08, 0.5, (0.9, -0.2, 0), color="moss", seg=4),
                 cone(0.08, 0.4, (-0.9, -0.25, 0), color="moss", seg=4)]
    else:
        parts = [box(0.15, 0.15, 2.4, (-1.0, 0, 0), color="wood_dark"),
                 box(0.15, 0.15, 2.4, (1.0, 0, 0), color="wood_dark"),
                 box(2.4, 0.08, 0.8, (0, -0.1, 1.3), color="white"),
                 box(2.4, 0.09, 0.12, (0, -0.11, 1.5), color="blue_sign"),
                 box(0.6, 0.6, 0.12, (0, 0, 2.45), color="roof_dark"),
                 box(0.32, 0.32, 0.3, (0, 0, 2.15), color="lamp")]
    return join(parts, f"station_sign_s{stage}")

def waiting_room(stage=0):
    w, d, h = 10.0, 5.0, 3.2
    parts = [box(w + 0.3, d + 0.3, 0.3, color="stone_dark"),
             box(w, d, h, (0, 0, 0.3), color="plaster_dirty" if stage == 0 else "plaster"),
             box(w + 0.05, d + 0.05, 0.6, (0, 0, 0.3), color="wood_dark"),
             hip(w + 1.4, d + 1.4, 1.8, (0, 0, h + 0.3), color="roof_dark" if stage == 0 else "roof", ridge=0.55),
             box(1.4, 0.1, 2.3, (0, -d / 2 - 0.02, 0.3), color="wood_dark" if stage == 0 else "glass")]
    for x in (-3.2, -1.8, 1.8, 3.2):
        if stage == 0:
            parts += [box(1.1, 0.08, 1.0, (x, -d / 2 - 0.03, 1.3), color="glass"),
                      box(1.3, 0.06, 0.18, (x, -d / 2 - 0.08, 1.55), color="wood_gray"),
                      box(1.3, 0.06, 0.18, (x, -d / 2 - 0.08, 1.95), color="wood_gray")]
        else:
            parts += [box(1.1, 0.08, 1.0, (x, -d / 2 - 0.03, 1.3), color="lamp"),
                      box(0.06, 0.1, 1.0, (x, -d / 2 - 0.05, 1.3), color="wood_dark")]
    parts.append(box(3.0, 0.1, 0.5, (0, -d / 2 - 0.05, h - 0.4), color="white" if stage else "paper_old"))
    if stage == 0:
        parts += [box(1.2, 0.1, 1.6, (0.2, -d / 2 - 0.08, 0.6), color="wood_gray")]  # board over door
        for x in (-4.5, 4.4, 2.5):
            parts.append(cone(0.1, 0.45, (x, -d / 2 - 0.3, 0.3), color="moss", seg=4))
    else:
        parts.append(box(0.5, 0.5, 0.4, (0, -d / 2 - 0.4, h - 0.3), color="lamp"))
    return join(parts, f"station_waiting_room_s{stage}")

def bus_stop(name="station_bus_stop"):
    return join([cyl(0.05, 2.4, color="metal", seg=6),
                 cyl(0.3, 0.05, (0, -0.03, 2.3), color="blue_sign", seg=10),
                 box(0.4, 0.15, 0.5, (0, 0, 1.4), color="white"),
                 box(2.2, 1.2, 0.08, (1.7, 0.4, 2.3), color="roof"),
                 box(0.08, 0.08, 2.3, (0.7, 0.9, 0), color="metal"),
                 box(0.08, 0.08, 2.3, (2.7, 0.9, 0), color="metal"),
                 box(2.0, 0.05, 1.5, (1.7, 0.95, 0.7), color="glass"),
                 box(1.6, 0.4, 0.08, (1.7, 0.6, 0.42), color="wood"),
                 box(0.08, 0.35, 0.42, (1.0, 0.6, 0), color="metal"),
                 box(0.08, 0.35, 0.42, (2.4, 0.6, 0), color="metal")], name)

def office_hut(stage=0):
    w, d, h = 4.5, 3.5, 2.6
    parts = [box(w + 0.2, d + 0.2, 0.25, color="stone_dark"),
             box(w, d, h, (0, 0, 0.25), color="wood_gray" if stage == 0 else "wood"),
             lean(w + 0.6, d + 0.6, 0.6, (0, 0, h + 0.25), color="roof_dark" if stage == 0 else "roof"),
             box(0.9, 0.08, 2.0, (-1.2, -d / 2 - 0.02, 0.25), color="wood_dark"),
             box(1.4, 0.08, 0.9, (0.9, -d / 2 - 0.02, 1.2), color="glass"),
             box(1.5, 0.1, 0.1, (0.9, -d / 2 - 0.06, 2.12), color="wood_dark")]
    if stage == 0:
        rnd = random.Random(7)
        for i in range(9):  # vines creeping over the walls and roof
            parts.append(blob(0.35 + rnd.random() * 0.25, (rnd.uniform(-w / 2, w / 2), -d / 2 - 0.05, rnd.uniform(0.4, 2.8)), color="moss", sz=0.6))
        for i in range(4):
            parts.append(blob(0.5, (rnd.uniform(-w / 2, w / 2), rnd.uniform(-d / 2, d / 2), h + 0.6), color="grass_dark", sz=0.4))
        parts += [box(0.6, 0.5, 0.45, (1.8, -d / 2 - 0.6, 0), color="paper_old"),
                  box(0.5, 0.4, 0.4, (1.6, -d / 2 - 0.7, 0.45), color="paper_old")]
    else:
        parts += [box(0.35, 0.35, 0.35, (-0.5, -d / 2 - 0.2, 2.1), color="lamp"),
                  cyl(0.2, 0.35, (0.2, -d / 2 - 0.5, 0), color="rust", seg=7, r2=0.25),
                  blob(0.3, (0.2, -d / 2 - 0.5, 0.6), color="leaf"),
                  box(1.0, 0.06, 0.35, (0.9, -d / 2 - 0.05, 2.35), color="white")]
    return join(parts, f"office_hut_s{stage}")

# ----------------------------------------------------------- shop street
def _machiya(w=6.0, d=7.0, wall="plaster", ground="wood", roof="roof", awning="roof"):
    """Two-storey shop-house shell; returns parts list. Front at y=-d/2."""
    return [box(w + 0.2, d + 0.2, 0.25, color="stone_dark"),
            box(w, d, 3.0, (0, 0, 0.25), color=ground),
            box(w, d - 1.0, 2.6, (0, 0.5, 3.25), color=wall),
            box(w + 0.02, d - 0.98, 0.15, (0, 0.5, 3.25), color="wood_dark"),
            lean(w + 0.4, 1.3, 0.45, (0, -d / 2 - 0.15, 2.9), color=awning),
            gable(w + 0.8, d + 0.2, 1.7, (0, 0.5, 5.85), color=roof),
            box(w + 0.9, 0.25, 0.25, (0, 0.5, 7.55), color="roof_dark")]

def _upper_windows(w, d, color="glass"):
    y = -d / 2 + 0.5 - 0.03
    return [box(1.4, 0.08, 0.9, (-w / 4, y, 4.1), color=color), box(1.4, 0.08, 0.9, (w / 4, y, 4.1), color=color),
            box(1.5, 0.1, 0.08, (-w / 4, y - 0.02, 4.55), color="wood_dark"), box(1.5, 0.1, 0.08, (w / 4, y - 0.02, 4.55), color="wood_dark")]

def _lattice(w, y, z0, h, color="wood_dark", step=0.22):
    n = int(w / step)
    return [box(0.06, 0.06, h, (-w / 2 + i * step + step / 2, y, z0), color=color) for i in range(n)]

def chizurudo(stage=0):
    """Wagashi shop Chizuru-do. 0 closed/faded, 1 facade fixed (First Hour), 2 open two days a week, 3 open."""
    w, d = 6.0, 7.0
    dirty = stage == 0
    parts = _machiya(w, d, wall="plaster_dirty" if dirty else "plaster", ground="wood_gray" if dirty else "wood")
    parts += _upper_windows(w, d, "glass")
    fy = -d / 2 - 0.04
    # signboard on the awning
    parts.append(box(2.8, 0.12, 0.7, (0, -d / 2 - 0.85, 3.45), color="paper_old" if dirty else "wood_dark"))
    if not dirty:
        parts.append(box(2.4, 0.13, 0.45, (0, -d / 2 - 0.87, 3.57), color="cream"))
    if stage <= 1:  # sliding doors closed
        parts += [box(w - 0.6, 0.08, 2.3, (0, fy, 0.25), color="wood_gray" if dirty else "wood_dark")]
        parts += _lattice(w - 0.8, fy - 0.06, 0.4, 2.0, color="wood_dark" if not dirty else "wood_gray")
        parts.append(box(0.5, 0.03, 0.6, (0.9, fy - 0.12, 1.2), color="white"))  # paper notice
    else:  # doors open: dark interior + noren
        parts += [box(w - 0.6, 0.08, 2.3, (0, fy + 0.3, 0.25), color="black"),
                  box(1.2, 0.08, 2.3, (-w / 2 + 0.9, fy, 0.25), color="wood_dark"),
                  box(1.2, 0.08, 2.3, (w / 2 - 0.9, fy, 0.25), color="wood_dark")]
        for i in range(3):
            parts.append(box(0.9, 0.04, 0.9, (-1.0 + i * 1.0, fy - 0.15, 1.75), color="indigo"))
        parts.append(box(3.2, 0.06, 0.06, (0, fy - 0.15, 2.66), color="wood_dark"))
        # display bench with wagashi trays
        parts += [box(1.8, 0.6, 0.7, (-1.8, -d / 2 - 0.6, 0.0), color="wood"),
                  box(0.5, 0.35, 0.06, (-2.2, -d / 2 - 0.6, 0.7), color="cream"),
                  box(0.5, 0.35, 0.06, (-1.4, -d / 2 - 0.6, 0.7), color="cream"),
                  blob(0.08, (-2.3, -d / 2 - 0.6, 0.82), color="sakura"), blob(0.08, (-2.1, -d / 2 - 0.65, 0.82), color="moss"),
                  blob(0.08, (-1.5, -d / 2 - 0.6, 0.82), color="sakura"), blob(0.08, (-1.3, -d / 2 - 0.62, 0.82), color="white")]
    if dirty:
        rnd = random.Random(11)
        for i in range(7):
            parts.append(cone(0.08, 0.4 + rnd.random() * 0.3, (rnd.uniform(-w / 2, w / 2), -d / 2 - 0.4 - rnd.random() * 0.4, 0), color="moss", seg=4))
        parts.append(box(0.32, 0.32, 0.4, (w / 2 - 0.4, -d / 2 - 0.25, 2.3), color="paper_old"))  # dead lamp
        parts.append(cyl(0.22, 0.35, (-w / 2 + 0.3, -d / 2 - 0.5, 0), color="rust", seg=7, r2=0.27))   # empty pot
    else:
        parts.append(box(0.32, 0.32, 0.4, (w / 2 - 0.4, -d / 2 - 0.25, 2.3), color="lamp"))
        for x in ((w / 2 - 0.4,) if stage >= 2 else (w / 2 - 0.4, -w / 2 + 0.4)):
            parts += [cyl(0.22, 0.35, (x, -d / 2 - 0.5, 0), color="rust", seg=7, r2=0.27),
                      blob(0.3, (x, -d / 2 - 0.5, 0.6), color="leaf"), blob(0.08, (x + 0.1, -d / 2 - 0.65, 0.8), color="sakura")]
    if stage >= 3:  # standing menu board + bench for customers
        parts += [box(0.6, 0.1, 0.9, (2.3, -d / 2 - 1.3, 0.1), color="wood_dark"), box(0.5, 0.11, 0.7, (2.3, -d / 2 - 1.31, 0.2), color="cream"),
                  box(1.6, 0.45, 0.08, (0.6, -d / 2 - 1.1, 0.42), color="vermilion"),
                  box(0.08, 0.4, 0.42, (-0.1, -d / 2 - 1.1, 0), color="wood_dark"), box(0.08, 0.4, 0.42, (1.3, -d / 2 - 1.1, 0), color="wood_dark")]
    return join(parts, f"chizurudo_s{stage}")

def harue_store(name="harue_general_store"):
    w, d = 7.0, 7.0
    parts = _machiya(w, d, wall="plaster", ground="wood", awning="cream")
    parts += _upper_windows(w, d, "glass")
    fy = -d / 2 - 0.04
    parts += [box(w - 0.6, 0.08, 2.4, (0, fy + 0.4, 0.25), color="black"),
              box(3.4, 0.12, 0.6, (0, -d / 2 - 0.85, 3.45), color="blue_sign"),
              box(3.0, 0.13, 0.3, (0, -d / 2 - 0.87, 3.6), color="white"),
              box(2.6, 0.9, 0.6, (-1.5, -d / 2 - 0.6, 0), color="wood")]
    for i, c in enumerate(("mustard", "leaf", "vermilion")):
        parts += [box(0.7, 0.5, 0.25, (-2.4 + i * 0.85, -d / 2 - 0.6, 0.6), color="wood_dark"),
                  blob(0.12, (-2.5 + i * 0.85, -d / 2 - 0.6, 0.92), color=c), blob(0.12, (-2.25 + i * 0.85, -d / 2 - 0.55, 0.92), color=c)]
    parts += [box(1.0, 0.8, 1.85, (2.6, -d / 2 - 0.5, 0), color="white"),
              box(0.8, 0.05, 0.75, (2.6, -d / 2 - 0.92, 0.95), color="lamp"),
              box(1.02, 0.82, 0.1, (2.6, -d / 2 - 0.5, 1.85), color="vermilion"),
              box(0.35, 0.35, 0.35, (1.2, -d / 2 - 0.25, 2.35), color="lamp")]
    return join(parts, name)

def empty_shop(variant="a"):
    w, d = 6.0, 7.0
    if variant == "a":   # rolled-down metal shutter
        parts = _machiya(w, d, wall="plaster_dirty", ground="plaster_dirty", awning="roof_dark", roof="roof_dark")
        parts += _upper_windows(w, d, "glass")
        parts += [box(w - 0.6, 0.1, 2.5, (0, -d / 2 - 0.05, 0.25), color="shutter"),
                  box(2.8, 0.12, 0.6, (0, -d / 2 - 0.85, 3.45), color="paper_old")]
        for i in range(8):
            parts.append(box(w - 0.6, 0.12, 0.03, (0, -d / 2 - 0.06, 0.5 + i * 0.28), color="metal"))
    else:                # boarded-up single storey with lean-to roof
        parts = [box(w + 0.2, d + 0.2, 0.25, color="stone_dark"),
                 box(w, d, 3.4, (0, 0, 0.25), color="wood_gray"),
                 lean(w + 0.6, d + 0.6, 1.2, (0, 0, 3.65), color="roof_dark"),
                 box(w - 1.0, 0.08, 2.2, (0, -d / 2 - 0.02, 0.4), color="glass")]
        for z in (0.9, 1.5, 2.1):
            parts.append(box(w - 0.6, 0.08, 0.25, (0, -d / 2 - 0.08, z), color="wood_gray"))
        parts.append(box(0.3, 0.12, 2.2, (0, -d / 2 - 0.13, 0.4), color="wood_dark"))
    return join(parts, f"shop_empty_{variant}")

def generic_house(w=7.0, d=7.0, name="house_generic", roof="roof"):
    parts = _machiya(w, d, wall="plaster", ground="wood_gray", roof=roof, awning=roof)
    parts += _upper_windows(w, d, "glass")
    parts += [box(w - 0.6, 0.08, 2.3, (0, -d / 2 - 0.02, 0.25), color="wood_dark")]
    return join(parts, name)

# ------------------------------------------------- blockouts (fogged regions)
# Simple massing models in fog colour. They stand in for regions not yet built;
# replace them with full models when the region's chapter is in production.
def bo_bathhouse():
    return join([box(16, 12, 4.5, color="fog"), gable(17.5, 13.5, 3.5, (0, 0, 4.5), color="fog"),
                 box(6, 3, 3.5, (0, -7.5, 0), color="fog"), gable(7, 4, 1.8, (0, -7.5, 3.5), color="fog", rot=0),
                 cyl(0.7, 15, (6.5, 4, 0), color="fog", seg=8)], "bo_bathhouse")

def bo_ryokan():
    return join([box(20, 10, 7, color="fog"), gable(21.5, 11.5, 3, (0, 0, 7), color="fog"),
                 box(8, 10, 4, (12, 4, 0), color="fog"), gable(9, 11, 2, (12, 4, 4), color="fog"),
                 box(14, 0.6, 1.8, (-2, -9, 0), color="fog")], "bo_ryokan")

def bo_shop(name):
    return join([box(6, 7, 3.25, color="fog"), box(6, 6, 2.6, (0, 0.5, 3.25), color="fog"),
                 gable(6.8, 7.2, 1.7, (0, 0.5, 5.85), color="fog")], name)

def bo_warehouse():
    return join([box(10, 6, 4, color="fog"), gable(11, 7, 2, (0, 0, 4), color="fog")], "bo_river_warehouse")

def bo_bridge():
    parts = [box(4, 18, 0.4, (0, 0, 1.2), color="fog")]
    for y in (-6, 0, 6):
        parts.append(box(3.6, 0.8, 1.2, (0, y, 0), color="fog"))
    parts += [box(0.2, 18, 0.9, (-1.9, 0, 1.6), color="fog"), box(0.2, 18, 0.9, (1.9, 0, 1.6), color="fog")]
    return join(parts, "bo_stone_bridge")

def bo_torii():
    return join([cyl(0.3, 5, (-2, 0, 0), color="fog", seg=8), cyl(0.3, 5, (2, 0, 0), color="fog", seg=8),
                 box(6.2, 0.6, 0.5, (0, 0, 5), color="fog"), box(5, 0.4, 0.35, (0, 0, 4.1), color="fog")], "bo_torii")

def bo_shrine():
    return join([box(9, 7, 1.0, color="fog"), box(7, 5, 3.2, (0, 0, 1.0), color="fog"),
                 hip(10, 8, 3.2, (0, 0, 4.2), color="fog", ridge=0.5)], "bo_shrine_honden")

def bo_stairs(steps=14, rise=0.5, run=0.6):
    parts = [box(3.0, run, rise * (i + 1), (0, -i * run, 0), color="fog") for i in range(steps)]
    return join(parts, "bo_shrine_stairs")

def bo_plaza_stage():
    return join([box(8, 5, 1.0, color="fog"), box(8, 0.3, 3.5, (0, 2.35, 1.0), color="fog"),
                 lean(8.6, 5.6, 0.6, (0, 0, 4.0), color="fog", rot=math.pi)], "bo_festival_stage")

def bo_source_hut():
    return join([box(5, 4, 2.8, color="fog"), gable(5.8, 4.8, 1.4, (0, 0, 2.8), color="fog"),
                 cyl(0.3, 1.2, (3.5, 0, 0), color="fog")], "bo_spring_source_hut")
