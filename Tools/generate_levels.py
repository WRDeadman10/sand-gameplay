import uuid
from pathlib import Path

levels_dir = Path("Assets/_Project/Data/Levels")
levels_dir.mkdir(parents=True, exist_ok=True)

configs = [
    {
        "num": 2,
        "name": "Level 2 - Dual Color Drain",
        "board": (6, 7),
        "walls": [],
        "tank_size": (6, 2),
        "tank_offset": 0,
        "mode": 0,
        "color_id": 2, # Blue
        "fill_ratio": 0.75,
        "pixel_art_guid": "",
        "mapping": 0,
        "box_size": (2, 2),
        "box_start": (0, 0),
        "box_color": 2, # Blue box
    },
    {
        "num": 3,
        "name": "Level 3 - Color Filter",
        "board": (6, 7),
        "walls": [{"x": 1, "y": 2}, {"x": 4, "y": 2}],
        "tank_size": (6, 2),
        "tank_offset": 0,
        "mode": 0,
        "color_id": 3, # Green
        "fill_ratio": 0.8,
        "pixel_art_guid": "",
        "mapping": 0,
        "box_size": (2, 2),
        "box_start": (2, 0),
        "box_color": 3, # Green
    },
    {
        "num": 4,
        "name": "Level 4 - Twin Pillars",
        "board": (7, 8),
        "walls": [{"x": 0, "y": 3}, {"x": 6, "y": 3}, {"x": 3, "y": 2}],
        "tank_size": (7, 2),
        "tank_offset": 0,
        "mode": 0,
        "color_id": 1, # Red
        "fill_ratio": 0.75,
        "pixel_art_guid": "",
        "mapping": 0,
        "box_size": (2, 2),
        "box_start": (2, 0),
        "box_color": 1, # Red
    },
    {
        "num": 5,
        "name": "Level 5 - The Corner Turn",
        "board": (7, 8),
        "walls": [{"x": 2, "y": 4}, {"x": 2, "y": 3}, {"x": 4, "y": 2}],
        "tank_size": (7, 2),
        "tank_offset": 0,
        "mode": 0,
        "color_id": 0, # Yellow
        "fill_ratio": 0.8,
        "pixel_art_guid": "",
        "mapping": 0,
        "box_size": (2, 2),
        "box_start": (4, 0),
        "box_color": 0, # Yellow
    },
    {
        "num": 6,
        "name": "Level 6 - Sweet Heart",
        "board": (6, 7),
        "walls": [],
        "tank_size": (6, 2),
        "tank_offset": 0,
        "mode": 1, # PixelArt
        "color_id": 1, # Red
        "fill_ratio": 0.75,
        "pixel_art_guid": "8e11a22b33c44d55e66f77a88b99c011",
        "mapping": 1, # NearestPaletteColor
        "box_size": (2, 2),
        "box_start": (2, 0),
        "box_color": 1, # Red
    },
    {
        "num": 7,
        "name": "Level 7 - Red & White Pyramid",
        "board": (6, 7),
        "walls": [{"x": 0, "y": 3}, {"x": 5, "y": 3}],
        "tank_size": (6, 2),
        "tank_offset": 0,
        "mode": 1, # PixelArt
        "color_id": 1, # Red
        "fill_ratio": 0.75,
        "pixel_art_guid": "8e11a22b33c44d55e66f77a88b99c022",
        "mapping": 1, # NearestPaletteColor
        "box_size": (2, 2),
        "box_start": (2, 0),
        "box_color": 1, # Red
    },
    {
        "num": 8,
        "name": "Level 8 - Sun Horizon",
        "board": (6, 8),
        "walls": [{"x": 2, "y": 3}, {"x": 3, "y": 3}],
        "tank_size": (6, 2),
        "tank_offset": 0,
        "mode": 1, # PixelArt
        "color_id": 0, # Yellow
        "fill_ratio": 0.75,
        "pixel_art_guid": "cc89dc14f5999b14d949d4e0f1a3d592",
        "mapping": 0, # ToneOfFillColor
        "box_size": (2, 2),
        "box_start": (1, 0),
        "box_color": 0, # Yellow
    },
    {
        "num": 9,
        "name": "Level 9 - Zigzag Corridor",
        "board": (7, 8),
        "walls": [{"x": 1, "y": 2}, {"x": 3, "y": 4}, {"x": 5, "y": 2}],
        "tank_size": (7, 2),
        "tank_offset": 0,
        "mode": 0,
        "color_id": 4, # Pink
        "fill_ratio": 0.85,
        "pixel_art_guid": "",
        "mapping": 0,
        "box_size": (2, 2),
        "box_start": (0, 0),
        "box_color": 4, # Pink
    },
    {
        "num": 10,
        "name": "Level 10 - Puzzle Master",
        "board": (7, 8),
        "walls": [{"x": 0, "y": 2}, {"x": 6, "y": 2}],
        "tank_size": (7, 2),
        "tank_offset": 0,
        "mode": 1, # PixelArt
        "color_id": 2, # Blue
        "fill_ratio": 0.8,
        "pixel_art_guid": "8e11a22b33c44d55e66f77a88b99c033",
        "mapping": 1, # NearestPaletteColor
        "box_size": (2, 2),
        "box_start": (3, 0),
        "box_color": 2, # Blue
    },
    {
        "num": 11,
        "name": "Level 11 - Grand Waterfall",
        "board": (8, 9),
        "walls": [{"x": 2, "y": 3}, {"x": 5, "y": 3}, {"x": 2, "y": 5}, {"x": 5, "y": 5}],
        "tank_size": (8, 2),
        "tank_offset": 0,
        "mode": 0,
        "color_id": 2, # Blue
        "fill_ratio": 0.85,
        "pixel_art_guid": "",
        "mapping": 0,
        "box_size": (3, 2),
        "box_start": (2, 0),
        "box_color": 2, # Blue
    }
]

for cfg in configs:
    num_str = f"{cfg['num']:02d}"
    filename = f"Level_{num_str}.asset"
    asset_path = levels_dir / filename
    meta_path = levels_dir / f"{filename}.meta"
    
    asset_guid = uuid.uuid4().hex
    
    walls_yaml = "wallCells: []"
    if cfg["walls"]:
        w_list = [f"  - {{x: {w['x']}, y: {w['y']}}}" for w in cfg["walls"]]
        walls_yaml = "wallCells:\n" + "\n".join(w_list)
        
    pixel_field = "{fileID: 0}"
    if cfg["pixel_art_guid"]:
        pixel_field = f"{{fileID: 2800000, guid: {cfg['pixel_art_guid']}, type: 3}}"
        
    asset_content = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 0eb88e9dde43109498a2ff6d2db48e74, type: 3}}
  m_Name: Level_{num_str}
  m_EditorClassIdentifier: SandGameplay.Runtime::SandGameplay.Data.LevelConfig
  displayName: {cfg['name']}
  boardSize: {{x: {cfg['board'][0]}, y: {cfg['board'][1]}}}
  {walls_yaml}
  tankSize: {{x: {cfg['tank_size'][0]}, y: {cfg['tank_size'][1]}}}
  tankColumnOffset: {cfg['tank_offset']}
  tankPattern:
    mode: {cfg['mode']}
    colorId: {cfg['color_id']}
    solidFillRatio: {cfg['fill_ratio']}
    solidTone: 7
    pixelArt: {pixel_field}
    pixelArtMapping: {cfg['mapping']}
  containerFootprint: {{x: {cfg['box_size'][0]}, y: {cfg['box_size'][1]}}}
  containerStartCell: {{x: {cfg['box_start'][0]}, y: {cfg['box_start'][1]}}}
  containerColorId: {cfg['box_color']}
  colorRule: 0
  completionThreshold: 1
  assistThreshold: 0.97
  palette: {{fileID: 11400000, guid: 158354045e688754fbcb2f8bf218c7cc, type: 2}}
  simulation: {{fileID: 11400000, guid: 6a8ab6df93762e647ac8f4e1632b91d4, type: 2}}
"""

    meta_content = f"""fileFormatVersion: 2
guid: {asset_guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    asset_path.write_text(asset_content, encoding="utf-8")
    meta_path.write_text(meta_content, encoding="utf-8")
    print(f"Generated {filename} (guid: {asset_guid})")

print("All 10 levels generated successfully!")
