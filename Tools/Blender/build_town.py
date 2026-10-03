"""Build the whole Kirimi Town layout, export FBX per asset + full town, write layout JSON,
render previews and save a .blend.

Run:  python3 build_town.py <output_dir> [--no-render]
(or inside Blender: blender -b -P build_town.py -- <output_dir>)
"""
import bpy, math, json, os, sys, random

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import lowpoly_lib as L
import assets as A

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else sys.argv[1:]
OUT = os.path.abspath(argv[0] if argv else "out")
RENDER = "--no-render" not in argv
for sub in ("fbx/assets", "fbx/town", "renders", "textures"):
    os.makedirs(os.path.join(OUT, sub), exist_ok=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
L.palette_image(os.path.join(OUT, "textures", "T_TownPalette.png"))

def coll(name, parent=None):
    c = bpy.data.collections.new(name)
    (parent or scene.collection).children.link(c)
    return c

def move_to(ob, c):
    for old in list(ob.users_collection):
        old.objects.unlink(ob)
    c.objects.link(ob)

# ------------------------------------------------------------ 1. assets
C_ASSETS = coll("Assets")
ASSET = {}
def reg(ob, batch, region):
    move_to(ob, C_ASSETS)
    ob["batch"], ob["region"] = batch, region
    ASSET[ob.name] = ob
    return ob

for s in (0, 1):
    reg(A.station_sign(s), 1, "R1_station")
    reg(A.waiting_room(s), 1, "R1_station")
    reg(A.office_hut(s), 1, "R1_station")
reg(A.bus_stop(), 1, "R1_station")
reg(A.rail_track(10.0), 1, "R1_station")
reg(A.platform(24.0), 1, "R1_station")
for s in range(4):
    reg(A.chizurudo(s), 1, "R2_shop_street_front")
reg(A.harue_store(), 1, "R2_shop_street_front")
reg(A.empty_shop("a"), 1, "R2_shop_street_front")
reg(A.empty_shop("b"), 1, "R2_shop_street_front")
reg(A.generic_house(), 1, "common")
for f in (A.street_lamp, A.utility_pole, A.planter, A.pot_plant, A.bench, A.vending_machine,
          A.crate, A.fence, A.cedar, A.round_tree, A.sakura_tree, A.bush, A.weeds, A.rock, A.paper_lantern):
    reg(f(), 1, "common")
reg(A.street_lamp(lit=False, name="prop_street_lamp_off"), 1, "common")
for f in (A.bo_bathhouse, A.bo_ryokan, A.bo_warehouse, A.bo_bridge, A.bo_torii, A.bo_shrine,
          A.bo_stairs, A.bo_plaza_stage, A.bo_source_hut):
    reg(f(), 0, "blockout")
for n in ("bo_shop_diner", "bo_shop_teahouse", "bo_shop_souvenir", "bo_shop_barber", "bo_shop_generic"):
    reg(A.bo_shop(n), 0, "blockout")

# ------------------------------------------------------------ 2. terrain
RIVER = [(100, -13), (55, -17), (20, -22), (-5, -24), (-35, -25), (-65, -30), (-100, -35)]
SHRINE = (-5.0, -61.0)

def seg_dist(px, py, ax, ay, bx, by):
    dx, dy = bx - ax, by - ay
    t = max(0, min(1, ((px - ax) * dx + (py - ay) * dy) / (dx * dx + dy * dy)))
    return math.hypot(px - ax - t * dx, py - ay - t * dy)

def river_d(x, y):
    return min(seg_dist(x, y, *RIVER[i], *RIVER[i + 1]) for i in range(len(RIVER) - 1))

def river_y(x):
    for (ax, ay), (bx, by) in zip(RIVER, RIVER[1:]):
        if bx <= x <= ax:
            return ay + (x - ax) / (bx - ax) * (by - ay)
    return RIVER[-1][1]

def height(x, y):
    rnd = random.Random(int(x * 73856093) ^ int(y * 19349663))
    h = 0.0
    if y > 32: h += (y - 32) * 0.55
    if y < -66: h += (-66 - y) * 0.6
    if abs(x) > 82: h += (abs(x) - 82) * 0.6
    ds = math.hypot(x - SHRINE[0], y - SHRINE[1])
    if ds < 9: h = max(h, 7.0)
    elif ds < 22: h = max(h, 7.0 * (1 - (ds - 9) / 13) ** 1.3)
    if h > 0.6 and ds >= 9: h += rnd.uniform(-0.4, 0.4)
    d = river_d(x, y)
    if d < 5: h = -1.6
    elif d < 9: h = -1.6 + (h + 1.6) * (d - 5) / 4
    return h

def build_terrain():
    X0, X1, Y0, Y1, S = -100, 100, -84, 76, 4
    nx, ny = (X1 - X0) // S, (Y1 - Y0) // S
    verts = [(X0 + i * S, Y0 + j * S, height(X0 + i * S, Y0 + j * S)) for j in range(ny + 1) for i in range(nx + 1)]
    faces, cols = [], []
    for j in range(ny):
        for i in range(nx):
            a = j * (nx + 1) + i; b, c, d = a + 1, a + nx + 2, a + nx + 1
            tris = ((a, b, c), (a, c, d)) if (i + j) % 2 == 0 else ((a, b, d), (b, c, d))
            for t in tris:
                faces.append(t)
                hs = [verts[k][2] for k in t]
                avg, rng = sum(hs) / 3, max(hs) - min(hs)
                cols.append("sand" if avg < -0.4 else "dirt" if rng > 4.0 else "cedar" if avg > 14 else "grass_dark" if avg > 2.5 else "grass")
    ob = L._mesh("terrain", verts, faces, "grass")
    uvl = ob.data.uv_layers["UVMap"]
    for p, c in zip(ob.data.polygons, cols):
        u, v = L.swatch_uv(c)
        for li in p.loop_indices:
            uvl.data[li].uv = (u, v)
    return ob

def build_water():
    pts = []
    for x in range(100, -101, -5):
        pts.append((x, river_y(x)))
    verts, faces = [], []
    for k, (x, y) in enumerate(pts):
        verts += [(x, y - 5.5, -0.7), (x, y + 5.5, -0.7)]
        if k:
            a = 2 * (k - 1)
            faces.append((a, a + 2, a + 3, a + 1))
    return L._mesh("river_water", verts, faces, "water")

C_TOWN = coll("Town")
REG = {}
def region_coll(r):
    if r not in REG:
        REG[r] = coll(r, C_TOWN)
    return REG[r]

ground = coll("Ground", C_TOWN)
move_to(build_terrain(), ground)
move_to(build_water(), ground)
roads = [  # (name, sx, sy, center, color)
    ("road_main_street", 79, 7, (-6.5, 10), "asphalt"),
    ("road_station_plaza", 20, 8, (-56, 9), "stone"),
    ("road_bath_forecourt", 12, 5, (42, 9), "stone"),
    ("road_to_bridge", 4, 13, (-5, 0), "stone"),
    ("path_riverside", 85, 2.5, (-17.5, -15), "dirt"),
    ("road_festival_plaza", 20, 12, (-5, -38), "sand"),
]
for n, sx, sy, (cx, cy), c in roads:
    ob = L.box(sx, sy, 0.06 + 0.01 * len(ground.objects), (cx, cy, 0.0), color=c, name=n)
    move_to(ob, ground)

# ------------------------------------------------------------ 3. layout
LAYOUT = []
def place(asset, x, y, rot_deg=0, region="R1_station", z=None, tag=None):
    src = ASSET[asset]
    ob = src.copy()            # linked duplicate (shares mesh)
    ob.location = (x, y, height(x, y) if z is None and abs(height(x, y)) > 0.5 else (z or 0.0))
    ob.rotation_euler = (0, 0, math.radians(rot_deg))
    region_coll(region).objects.link(ob)
    LAYOUT.append({"asset": asset, "region": region, "tag": tag or "",
                   "blender": {"pos": [round(v, 3) for v in ob.location], "rot_z_deg": rot_deg},
                   "unity": {"pos": [round(ob.location[0], 3), round(ob.location[2], 3), round(ob.location[1], 3)],
                             "rot_y_deg": (-rot_deg) % 360}})
    return ob

# R1 station area (Vertical Slice). Stage-0 variants placed; stage-1 swapped in by the game.
place("rail_platform", -55, 21, z=0)
for x in range(-95, -34, 10):
    place("rail_track", x, 24.5, z=0)
place("station_waiting_room_s0", -55, 15.5, tag="restorable:station_waiting_room")
place("station_sign_s0", -50, 11, tag="restorable:station_sign")
place("office_hut_s0", -61, 9.5, rot_deg=0, tag="restorable:office_hut")
place("station_bus_stop", -45, 5.5)
place("prop_bench", -58, 6.5)
place("prop_utility_pole", -66, 12)
place("prop_vending_machine", -47.5, 14.5)
for x, y in ((-64, 6), (-49, 6)):
    place("prop_weeds", x, y)

# R2 shop street front (north row faces the street / camera)
R2 = "R2_shop_street_front"
place("chizurudo_s0", -35, 17, region=R2, tag="restorable:chizurudo")
place("harue_general_store", -26.5, 17, region=R2)
place("shop_empty_a", -18, 17, region=R2)
place("shop_empty_b", -11, 17, region=R2)
place("house_generic", -35, 3, rot_deg=180, region=R2)
place("shop_empty_a", -26.5, 3, rot_deg=180, region=R2)
place("house_generic", -17, 3, rot_deg=180, region=R2)
for x in (-40, -28, -16, -4):
    place("prop_street_lamp", x, 13.0, region=R2, tag="lamp")
for x in (-36, -21, -8):
    place("prop_utility_pole", x, 6.8, region=R2)
place("prop_planter", -22.5, 13.2, region=R2)
place("prop_weeds", -14, 13.0, region=R2)

# R3/R4 and beyond: blockouts in fog colour
R3, R4, R5, R6, R7, R8, R9 = ("R3_public_bath", "R4_shop_street_rear", "R5_riverside", "R6_ryokan",
                               "R7_shrine", "R8_festival_plaza", "R9_mountain_source")
place("bo_bathhouse", 42, 20, region=R3)
place("bo_shop_diner", 6, 17, region=R4)
place("bo_shop_teahouse", 15, 17, region=R4)
place("bo_shop_generic", 24, 17, region=R4)
place("bo_shop_souvenir", 6, 3, rot_deg=180, region=R4)
place("bo_shop_barber", 15, 3, rot_deg=180, region=R4)
place("bo_shop_generic", 24, 3, rot_deg=180, region=R4)
place("bo_river_warehouse", -28, -12, region=R5)
place("bo_stone_bridge", -5, river_y(-5), region=R5, z=-0.3)
for x in range(-55, 25, 9):
    place("tree_sakura", x, -12.8, region=R5)
place("bo_ryokan", 44, -3, region=R6)
place("bo_festival_stage", -5, -41, region=R8, z=0.0)
place("bo_torii", -5, -44.5, region=R7, z=0.0)
place("bo_shrine_stairs", -5, -45.5, region=R7, z=0.0)
place("bo_shrine_honden", -5, -62, region=R7, z=7.0)
place("bo_spring_source_hut", 40, 52, region=R9)

# scenery: cedars on slopes, a few round trees in town gaps
SC = "scenery"
rnd = random.Random(5)
for _ in range(110):
    x, y = rnd.uniform(-98, 98), rnd.uniform(36, 74)
    if abs(x - 40) < 5: continue
    place("tree_cedar", x, y, region=SC)
for _ in range(45):
    x, y = rnd.uniform(-35, 25), rnd.uniform(-82, -50)
    if math.hypot(x - SHRINE[0], y - SHRINE[1]) < 10 or abs(x + 5) < 3: continue
    place("tree_cedar", x, y, region=SC)
for _ in range(20):
    x, y = rnd.uniform(-98, 98), rnd.uniform(-82, -40)
    if river_d(x, y) < 10 or (-20 < x < 10 and -50 < y < -30): continue
    place("tree_round", x, y, region=SC)
for x, y in ((-70, 15), (-72, 2), (-42, -6), (30, -1), (-20, -6), (55, 22), (60, 10)):
    place("tree_round", x, y, region=SC)

with open(os.path.join(OUT, "town_layout.json"), "w", encoding="utf-8") as f:
    json.dump({"note": "Unity coords: x=east, y=up, z=north. Assets face -Z. rot_y in degrees.",
               "regions": {"R1_station": "Ch1", "R2_shop_street_front": "Ch1-2", "R3_public_bath": "Ch3",
                           "R4_shop_street_rear": "Ch5", "R5_riverside": "Ch6", "R6_ryokan": "Ch6-7",
                           "R7_shrine": "Ch8", "R8_festival_plaza": "Ch8", "R9_mountain_source": "S2"},
               "instances": LAYOUT}, f, ensure_ascii=False, indent=1)

# ------------------------------------------------------------ 4. export
def select_only(obs):
    for o in bpy.context.view_layer.objects:
        o.select_set(False)
    for o in obs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = obs[0]

FBX = dict(use_selection=True, apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
           axis_forward="-Z", axis_up="Y", bake_space_transform=True, object_types={"MESH"},
           mesh_smooth_type="FACE", use_mesh_modifiers=True, path_mode="STRIP")
for n, ob in ASSET.items():
    select_only([ob])
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, "fbx/assets", n + ".fbx"), **FBX)
select_only(list(ground.objects))
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, "fbx/town", "town_ground.fbx"), **FBX)
select_only([o for o in C_TOWN.all_objects])
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, "fbx/town", "town_full_layout.fbx"), **FBX)

# ------------------------------------------------------------ 5. renders
def setup_render():
    scene.render.engine = "CYCLES"
    scene.cycles.device = "CPU"
    scene.cycles.samples = 40
    try:
        scene.cycles.use_denoising = True
        scene.cycles.denoiser = "OPENIMAGEDENOISE"
    except Exception:
        pass
    scene.view_settings.view_transform = "Standard"
    scene.render.film_transparent = False

def world(color, strength):
    w = bpy.data.worlds.get("W") or bpy.data.worlds.new("W")
    w.use_nodes = True
    bg = w.node_tree.nodes["Background"]
    bg.inputs["Color"].default_value = (*color, 1)
    bg.inputs["Strength"].default_value = strength
    scene.world = w

def sun(strength, color=(1, 0.95, 0.85)):
    s = bpy.data.objects.get("Sun")
    if not s:
        s = bpy.data.objects.new("Sun", bpy.data.lights.new("Sun", "SUN"))
        scene.collection.objects.link(s)
    s.data.energy = strength
    s.data.color = color
    s.data.angle = math.radians(8)
    s.rotation_euler = (math.radians(42), math.radians(-18), math.radians(35))

def camera(center, scale, pitch=45, res=(1920, 1200)):
    cam = bpy.data.objects.get("Cam")
    if not cam:
        cam = bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam"))
        scene.collection.objects.link(cam)
    cam.data.type = "ORTHO"
    cam.data.ortho_scale = scale
    cam.data.clip_end = 2000
    p = math.radians(pitch)
    dist = 300
    cx, cy, cz = center
    cam.location = (cx, cy - dist * math.cos(p), cz + dist * math.sin(p))
    cam.rotation_euler = (math.pi / 2 - p, 0, 0)
    scene.camera = cam
    scene.render.resolution_x, scene.render.resolution_y = res

def shot(path):
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    print("rendered", path)

def day():
    world((0.78, 0.86, 0.95), 0.9); sun(3.2); L.set_glow(0.0)

def night():
    world((0.10, 0.13, 0.25), 0.35); sun(0.25, (0.6, 0.7, 1.0)); L.set_glow(6.0)

def swap_stage(tag, asset):
    for o in C_TOWN.all_objects:
        if o.data and o.name.startswith(tag.split(":")[1]) and "_s" in o.name:
            o.data = ASSET[asset].data

if RENDER:
    setup_render()
    C_ASSETS.hide_render = True
    R = lambda n: os.path.join(OUT, "renders", n)
    day(); camera((0, -2, 0), 205, res=(1920, 1440)); shot(R("01_town_overview_day.png"))
    camera((-38, 11, 0), 52); shot(R("02_vertical_slice_area_stage0_day.png"))
    # restored versions for the vertical slice
    for o in C_TOWN.all_objects:
        if o.data and o.data.name in ("station_sign_s0", "station_waiting_room_s0", "office_hut_s0", "chizurudo_s0"):
            o.data = ASSET[o.data.name[:-1] + "1"].data
    shot(R("03_vertical_slice_area_restored_day.png"))
    night(); shot(R("04_vertical_slice_area_restored_night.png"))
    # asset sheet: line up assets away from the town
    C_ASSETS.hide_render = False
    C_TOWN.hide_render = True
    rows = [["chizurudo_s0", "chizurudo_s1", "chizurudo_s2", "chizurudo_s3"],
            ["station_waiting_room_s0", "station_waiting_room_s1", "office_hut_s0", "office_hut_s1"],
            ["harue_general_store", "shop_empty_a", "shop_empty_b", "house_generic"],
            ["station_sign_s0", "station_sign_s1", "station_bus_stop", "prop_street_lamp", "prop_utility_pole",
             "prop_vending_machine", "prop_planter", "prop_bench", "tree_sakura", "tree_cedar", "tree_round"]]
    for o in C_ASSETS.objects:
        o.hide_render = True
    floor = L.box(140, 100, 0.05, (0, -625, -0.05), color="grass", name="sheet_floor")
    y = -600
    for row in rows:
        gap = 13 if len(row) <= 4 else 5.5
        x0 = -gap * (len(row) - 1) / 2
        for i, n in enumerate(row):
            ob = ASSET[n]; ob.hide_render = False
            ob.location = (x0 + i * gap, y, 0)
        y -= 17
    day(); camera((0, -626, 0), 70, pitch=40, res=(1800, 1600)); shot(R("05_asset_sheet_day.png"))
    night(); shot(R("06_asset_sheet_night.png"))
    for o in C_ASSETS.objects:
        o.location = (0, 0, 0); o.hide_render = False
    bpy.data.objects.remove(floor)
    C_TOWN.hide_render = False
    day()

bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, "kirimi_town.blend"))
print("DONE", len(ASSET), "assets,", len(LAYOUT), "instances")
