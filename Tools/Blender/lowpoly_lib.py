"""Low-poly helpers for Tomoru Machi town assets (Blender 4.2, Z-up, meters).

All meshes share one palette texture (M_TownPalette) so Unity can batch them.
Parts that glow at night use a second material (M_Glow) on the same palette.
"""
import bpy, bmesh, math, os

# name -> RGB hex. Order defines the swatch index in the palette image.
PALETTE = [
    ("wood", "B9835A"), ("wood_dark", "8A5A3B"), ("wood_gray", "8C8378"),
    ("plaster", "EFE6D6"), ("plaster_dirty", "CFC4B0"), ("roof", "5D6B7A"),
    ("roof_dark", "47525E"), ("lamp", "FFC56B"), ("sakura", "F2B8C6"),
    ("moss", "7FA36B"), ("fog", "AFC3CF"), ("night", "2C3550"),
    ("cream", "F7F0E3"), ("vermilion", "D9614C"), ("stone", "A39E95"),
    ("stone_dark", "7A766F"), ("asphalt", "6E6C6A"), ("dirt", "B79E7C"),
    ("grass", "93B373"), ("grass_dark", "6F9158"), ("cedar", "4E6B4A"),
    ("bark", "6B4B36"), ("water", "6FA3B5"), ("glass", "4A5A66"),
    ("metal", "8E969C"), ("rust", "9A5B3C"), ("white", "F4F1EA"),
    ("indigo", "34476B"), ("mustard", "D9A940"), ("paper_old", "D8CBB0"),
    ("shutter", "9AA0A3"), ("leaf", "6E9A55"), ("sand", "D9CBAA"),
    ("black", "2E2B28"), ("red_dark", "9E3B32"), ("blue_sign", "4F79A6"),
    ("roof_new", "6A84A0"), ("tarp", "3F7FC4"), ("flower_y", "F2D14B"), ("grime", "6B6458"),
]
IDX = {n: i for i, (n, _) in enumerate(PALETTE)}
SW = 4          # swatch size in px
COLS = 8
GLOW = {"lamp"}  # colors that go on the glow material

def hex2rgb(h):
    return tuple(int(h[i:i + 2], 16) / 255.0 for i in (0, 2, 4))

def swatch_uv(color):
    i = IDX[color]
    rows = math.ceil(len(PALETTE) / COLS)
    cx = (i % COLS) * SW + SW / 2
    cy = (i // COLS) * SW + SW / 2
    size_x, size_y = COLS * SW, rows * SW
    return (cx / size_x, 1.0 - cy / size_y)

def palette_image(path=None):
    rows = math.ceil(len(PALETTE) / COLS)
    w, h = COLS * SW, rows * SW
    img = bpy.data.images.get("T_TownPalette") or bpy.data.images.new("T_TownPalette", w, h, alpha=False)
    px = [0.0] * (w * h * 4)
    for i, (_, hx) in enumerate(PALETTE):
        r, g, b = hex2rgb(hx)
        x0, y0 = (i % COLS) * SW, (i // COLS) * SW
        for y in range(y0, y0 + SW):
            for x in range(x0, x0 + SW):
                yy = h - 1 - y  # blender images start bottom-left
                k = (yy * w + x) * 4
                px[k:k + 4] = [r, g, b, 1.0]
    img.pixels = px
    if path:
        img.filepath_raw = path
        img.file_format = "PNG"
        img.save()
    return img

_MATS = {}
def materials():
    if _MATS:
        return _MATS
    img = palette_image()
    for name, emit in (("M_TownPalette", False), ("M_Glow", True)):
        m = bpy.data.materials.new(name)
        m.use_nodes = True
        nt = m.node_tree
        bsdf = nt.nodes["Principled BSDF"]
        tex = nt.nodes.new("ShaderNodeTexImage")
        tex.image = img
        tex.interpolation = "Closest"
        nt.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
        bsdf.inputs["Roughness"].default_value = 0.85
        if emit:
            nt.links.new(tex.outputs["Color"], bsdf.inputs["Emission Color"])
            bsdf.inputs["Emission Strength"].default_value = 0.0
        _MATS[name] = m
    return _MATS

def set_glow(strength):
    materials()["M_Glow"].node_tree.nodes["Principled BSDF"].inputs["Emission Strength"].default_value = strength

# ---------------------------------------------------------------- primitives
def _mesh(name, verts, faces, color):
    me = bpy.data.meshes.new(name)
    me.from_pydata(verts, [], faces)
    me.update()
    ob = bpy.data.objects.new(name, me)
    bpy.context.scene.collection.objects.link(ob)
    m = materials()
    me.materials.append(m["M_TownPalette"])
    me.materials.append(m["M_Glow"])
    paint(ob, color)
    return ob

def paint(ob, color):
    me = ob.data
    uvl = me.uv_layers.get("UVMap") or me.uv_layers.new(name="UVMap")
    u, v = swatch_uv(color)
    for loop in me.loops:
        uvl.data[loop.index].uv = (u, v)
    mi = 1 if color in GLOW else 0
    for p in me.polygons:
        p.material_index = mi
        p.use_smooth = False
    return ob

def _xf(verts, loc=(0, 0, 0), rot=0.0):
    c, s = math.cos(rot), math.sin(rot)
    return [(x * c - y * s + loc[0], x * s + y * c + loc[1], z + loc[2]) for x, y, z in verts]

def box(sx, sy, sz, loc=(0, 0, 0), color="plaster", rot=0.0, name="box"):
    """Box sitting on loc z (base), centered in x/y."""
    x, y = sx / 2, sy / 2
    v = [(-x, -y, 0), (x, -y, 0), (x, y, 0), (-x, y, 0), (-x, -y, sz), (x, -y, sz), (x, y, sz), (-x, y, sz)]
    f = [(0, 3, 2, 1), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)]
    return _mesh(name, _xf(v, loc, rot), f, color)

def gable(sx, sy, h, loc=(0, 0, 0), color="roof", rot=0.0, thick=0.2, name="roof"):
    """Gable roof, ridge along X. sx/sy include overhang. Base at loc z."""
    x, y = sx / 2, sy / 2
    v = [(-x, -y, 0), (x, -y, 0), (x, 0, h), (-x, 0, h), (x, y, 0), (-x, y, 0),
         (-x, -y, -thick), (x, -y, -thick), (x, y, -thick), (-x, y, -thick)]
    f = [(0, 1, 2, 3), (3, 2, 4, 5), (0, 3, 5), (1, 4, 2),
         (0, 6, 7, 1), (4, 8, 9, 5), (6, 9, 8, 7), (0, 5, 9, 6), (1, 7, 8, 4)]
    return _mesh(name, _xf(v, loc, rot), f, color)

def hip(sx, sy, h, loc=(0, 0, 0), color="roof", rot=0.0, ridge=0.4, name="hiproof"):
    """Hip roof: ridge length = ridge * sx."""
    x, y, r = sx / 2, sy / 2, sx * ridge / 2
    v = [(-x, -y, 0), (x, -y, 0), (x, y, 0), (-x, y, 0), (-r, 0, h), (r, 0, h)]
    f = [(0, 1, 5, 4), (2, 3, 4, 5), (1, 2, 5), (3, 0, 4), (0, 3, 2, 1)]
    return _mesh(name, _xf(v, loc, rot), f, color)

def lean(sx, sy, h, loc=(0, 0, 0), color="roof", rot=0.0, name="awning"):
    """Single-slope roof/awning, high edge at +y."""
    x, y = sx / 2, sy / 2
    t = 0.12
    v = [(-x, -y, 0), (x, -y, 0), (x, y, h), (-x, y, h), (-x, -y, t), (x, -y, t), (x, y, h + t), (-x, y, h + t)]
    f = [(0, 3, 2, 1), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)]
    return _mesh(name, _xf(v, loc, rot), f, color)

def cyl(r, h, loc=(0, 0, 0), color="wood", seg=8, r2=None, name="cyl"):
    r2 = r if r2 is None else r2
    v, f = [], []
    for i in range(seg):
        a = 2 * math.pi * i / seg
        v.append((r * math.cos(a), r * math.sin(a), 0))
    for i in range(seg):
        a = 2 * math.pi * i / seg
        v.append((r2 * math.cos(a), r2 * math.sin(a), h))
    for i in range(seg):
        j = (i + 1) % seg
        f.append((i, j, seg + j, seg + i))
    f.append(tuple(range(seg - 1, -1, -1)))
    if r2 > 0.001:
        f.append(tuple(range(seg, 2 * seg)))
    return _mesh(name, _xf(v, loc), f, color)

def cone(r, h, loc=(0, 0, 0), color="cedar", seg=7, name="cone"):
    v = [(r * math.cos(2 * math.pi * i / seg), r * math.sin(2 * math.pi * i / seg), 0) for i in range(seg)]
    v.append((0, 0, h))
    f = [(i, (i + 1) % seg, seg) for i in range(seg)] + [tuple(range(seg - 1, -1, -1))]
    return _mesh(name, _xf(v, loc), f, color)

def blob(r, loc=(0, 0, 0), color="leaf", sz=1.0, name="blob"):
    """Low-poly icosphere-ish blob for foliage."""
    me = bpy.data.meshes.new(name)
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, subdivisions=1, radius=r)
    for vtx in bm.verts:
        vtx.co.z *= sz
    bm.to_mesh(me)
    bm.free()
    verts = [tuple(vv.co) for vv in me.vertices]
    faces = [tuple(p.vertices) for p in me.polygons]
    bpy.data.meshes.remove(me)
    return _mesh(name, _xf(verts, loc), faces, color)

def slab(corners, color, thick=0.05, name="slab"):
    """Thin plate lying on four (x, y, z) corners, thickened upward. Used for tarps and patches on roofs."""
    top = [(x, y, z + thick) for x, y, z in corners]
    v = list(corners) + top
    f = [(0, 3, 2, 1), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)]
    return _mesh(name, v, f, color)

def plank(length, width, center, angle_deg=0.0, color="wood_gray", thick=0.06, name="plank"):
    """Board in the XZ plane (on a -Y facing wall), rotated by angle_deg around Y. center = (x, y, z)."""
    a = math.radians(angle_deg)
    c, s_ = math.cos(a), math.sin(a)
    l, w, t = length / 2, width / 2, thick / 2
    v = []
    for yy in (-t, t):
        for x, z in ((-l, -w), (l, -w), (l, w), (-l, w)):
            v.append((center[0] + x * c - z * s_, center[1] + yy, center[2] + x * s_ + z * c))
    f = [(0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0)]
    return _mesh(name, v, f, color)

def join(parts, name, origin=(0, 0, 0)):
    """Join parts into one object named `name`, origin at world `origin`."""
    parts = [p for p in parts if p]
    ctx = bpy.context
    for o in ctx.selected_objects:
        o.select_set(False)
    for p in parts:
        p.select_set(True)
    ctx.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    ob = ctx.view_layer.objects.active
    ob.name = name
    ob.data.name = name
    # consistent outward normals
    me = ob.data
    bm = bmesh.new(); bm.from_mesh(me)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(me); bm.free()
    # move origin
    ctx.scene.cursor.location = origin
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    ob.select_set(False)
    return ob
