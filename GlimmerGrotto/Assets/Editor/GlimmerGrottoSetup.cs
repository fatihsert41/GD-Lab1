using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// ------------------------------------------------------------------
// Glimmer Grotto - Lab 2 kurulum aracı (yalnızca Editor'de çalışır, build'e girmez)
// Menü: Glimmer Grotto > 1. Lab 2'yi kur
//   Part 1: sprite'lar 32 PPU (kapı 64), Point filtre, sıkıştırma yok
//   Part 2: sprite sheet'ler Grid By Cell Size ile kesilir
//   Part 3: karo (tile) varlıkları ve MainPalette
//   Part 4: Tilemaps > Background, Water, Platforms + sorting layer'lar
//   Part 5: GrottoRuleTile (13 kural) ve onunla boyanmış bölüm
//   Part 6: Platforms üzerinde GroundPainter (sağdaki boş alana boyar)
// Birden çok kez çalıştırılabilir; var olanları günceller.
// ------------------------------------------------------------------
public static partial class GlimmerGrottoSetup
{
    const string SpritesFolder = "Assets/Sprites";
    const string TilesFolder = "Assets/Tiles";
    const string ScenesFolder = "Assets/Scenes";
    const string LevelScene = "Assets/Scenes/Level1.unity";
    const string TemplateScene = "Assets/Settings/Scenes/URP2DSceneTemplate.unity";
    const string RuleTilePath = "Assets/Sprites/GrottoRuleTile.asset";
    const string PalettePath = "Assets/Tiles/MainPalette.prefab";

    const string Terrain = "SPA_Rock_Grass_Water";
    const string BackgroundSheet = "SPA_Background";

    // Kesim ayarları (Lab 3 kılavuzu Tablo 1 ile aynı)
    class Sheet
    {
        public string file;
        public bool multiple = true;
        public float ppu = 32;
        public int cell = 32, padding = 6, offsetX, offsetY;
        public int[] cellsPerRow;  // null: tam ızgara; aksi halde her satırdaki dolu hücre sayısı
    }

    static readonly Sheet[] Sheets =
    {
        new Sheet { file = "SPA_Rock_Grass_Water" },                      // 30 karo
        new Sheet { file = "SPA_Background" },                            // 16 duvar karosu
        new Sheet { file = "SPA_Player_Bronze", cellsPerRow = new[] { 5, 4, 5, 2, 3, 10 } },
        new Sheet { file = "SPA_Player_Ginger", cellsPerRow = new[] { 5, 4, 5, 2, 3, 10 } },
        new Sheet { file = "SPA_Enemy_Right_Red", offsetX = 10, offsetY = 2 },
        new Sheet { file = "SPA_Enemy_Right_Yellow", offsetX = 10, offsetY = 2 },
        new Sheet { file = "SPA_Coins" },                                 // 12 kare
        new Sheet { file = "SPA_Ladder", multiple = false },
        new Sheet { file = "Spikes", multiple = false },
        new Sheet { file = "doorway-white", multiple = false, ppu = 64 }, // 128 px / 64 = 2 x 2 birim
    };

    // Bölüm: '#' kaya (GrottoRuleTile), 'w' su, '.' boş. Yukarıdan aşağı satırlar.
    // 48 x 27 karo = 16:9; kamera tam olarak bu alanı gösterir.
    static readonly string[] LevelMap =
    {
        "################################################",
        "################################################",
        "#####...#######....#####################..######",
        "####.....#####......####.##########.###....#####",
        "###.......###........##...########...#......####",
        "##.........#.........#.....######.............##",
        "#..........#.................###...............#",
        "#.............................#................#",
        "#..............................................#",
        "#..............................................#",
        "#.......................................####...#",
        "#..............................................#",
        "#..####........................................#",
        "#..............................................#",
        "#..............................................#",
        "#.............................#######..........#",
        "#..............................................#",
        "#...........####...............................#",
        "#.........................#....................#",
        "#.......................#####..............#####",
        "######..................#####.....##.......#####",
        "######....#####..................####....#######",
        "######....#...#..........#......######...#######",
        "#######wwww#####...###..........######...#######",
        "#######wwww#####...###...#######################",
        "################...###...#######################",
        "################wwwwwwwwww######################",
    };
    const int LevelLeft = -23;   // haritanın sol sütunu (x)
    const int LevelBottom = -8;  // haritanın alt satırı (y)
    const int EdgePadding = 2;   // kenardaki kaya, görüş alanının dışına bu kadar uzar
    // Karoları %0,2 büyüt: komşularla piksel altı örtüşme. Karo kenarı tam bir piksel merkezine
    // denk geldiğinde Unity sheet'teki şeffaf boşluktan örnek alır ve siyah bir çizgi çıkar;
    // örtüşme sayesinde o piksel her iki karonun da içinde kalır.
    const float SeamFixScale = 1.002f;

    [MenuItem("Glimmer Grotto/1. Lab 2'yi kur")]
    public static void SetupLab2()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EnsureFolder(TilesFolder);
        EnsureFolder(ScenesFolder);

        // Part 1 + 2
        foreach (Sheet sheet in Sheets)
        {
            ImportSheet(sheet);
        }
        AssetDatabase.Refresh();

        Dictionary<string, Sprite> terrain = LoadSprites(Terrain);
        Dictionary<string, Sprite> background = LoadSprites(BackgroundSheet);
        if (terrain.Count < 30 || background.Count < 16)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                "Sprite'lar kesilemedi. Console'daki hatalara bak.", "Tamam");
            return;
        }

        // Part 3: karo varlıkları
        var terrainTiles = new Tile[30];
        for (int i = 0; i < 30; i++)
        {
            terrainTiles[i] = MakeTile(terrain[Terrain + "_" + i]);
        }
        var backgroundTiles = new Tile[16];
        for (int i = 0; i < 16; i++)
        {
            backgroundTiles[i] = MakeTile(background[BackgroundSheet + "_" + i]);
        }

        // Part 5: rule tile
        RuleTile ruleTile = MakeRuleTile(terrain);

        // Part 3 + 5: palet
        MakePalette(terrainTiles, backgroundTiles, ruleTile);

        // Part 4 + 5 + 6: sahne
        bool ok = BuildScene(terrainTiles, backgroundTiles, ruleTile);

        AssetDatabase.SaveAssets();
        if (ok)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                "Lab 2 kuruldu.\n\nLevel1 sahnesi açık. Tile Palette için: Window > 2D > Tile Palette.",
                "Tamam");
        }
    }

    // ---------------------------------------------------------------- Part 1 + 2

    static void ImportSheet(Sheet sheet)
    {
        string path = SpritesFolder + "/" + sheet.file + ".png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("Bulunamadı: " + path);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = sheet.multiple ? SpriteImportMode.Multiple : SpriteImportMode.Single;
        importer.spritePixelsPerUnit = sheet.ppu;
        importer.filterMode = FilterMode.Point;                           // keskin pikseller
        importer.textureCompression = TextureImporterCompression.Uncompressed; // renk kayması yok
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        if (!sheet.multiple)
        {
            return;
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();

        // Aynı ada sahip sprite'ın kimliğini koru: karolar ona bağlı kalsın.
        var existing = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);

        int step = sheet.cell + sheet.padding;
        int columns = (texture.width - sheet.offsetX + sheet.padding) / step;
        int rows = (texture.height - sheet.offsetY + sheet.padding) / step;

        var rects = new List<SpriteRect>();
        int index = 0;
        for (int row = 0; row < rows; row++)
        {
            int cellsInRow = sheet.cellsPerRow != null
                ? (row < sheet.cellsPerRow.Length ? sheet.cellsPerRow[row] : 0)
                : columns;
            for (int col = 0; col < cellsInRow; col++)
            {
                int x = sheet.offsetX + col * step;
                int yFromTop = sheet.offsetY + row * step;
                string name = sheet.file + "_" + index;
                rects.Add(new SpriteRect
                {
                    name = name,
                    // Unity'de dokunun (0,0) noktası sol alt köşe
                    rect = new Rect(x, texture.height - yFromTop - sheet.cell, sheet.cell, sheet.cell),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    spriteID = existing.TryGetValue(name, out GUID id) ? id : GUID.Generate(),
                });
                index++;
            }
        }

        provider.SetSpriteRects(rects.ToArray());
        var names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        names.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
        provider.Apply();
        importer.SaveAndReimport();
    }

    static Dictionary<string, Sprite> LoadSprites(string file)
    {
        return AssetDatabase.LoadAllAssetsAtPath(SpritesFolder + "/" + file + ".png")
            .OfType<Sprite>()
            .ToDictionary(s => s.name, s => s);
    }

    // ---------------------------------------------------------------- Part 3

    static Tile MakeTile(Sprite sprite)
    {
        string path = TilesFolder + "/" + sprite.name + ".asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            AssetDatabase.CreateAsset(tile, path);
        }
        else
        {
            tile.sprite = sprite;
            EditorUtility.SetDirty(tile);
        }
        return tile;
    }

    static void MakePalette(Tile[] t, Tile[] bg, RuleTile ruleTile)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath) != null)
        {
            AssetDatabase.DeleteAsset(PalettePath);
        }

        var root = new GameObject("MainPalette");
        root.AddComponent<Grid>();
        var layer = new GameObject("Layer1");
        layer.transform.SetParent(root.transform, false);
        Tilemap map = layer.AddComponent<Tilemap>();
        layer.AddComponent<TilemapRenderer>();

        // Kutu: köşeler, kenarlar, dolgu (sağ sütun Flip X ile)
        Put(map, 0, 0, t[0]); Put(map, 1, 0, t[6]); Put(map, 2, 0, t[0], true);
        Put(map, 0, -1, t[5]); Put(map, 1, -1, t[11]); Put(map, 2, -1, t[5], true);
        Put(map, 0, -2, t[15]); Put(map, 1, -2, t[16]); Put(map, 2, -2, t[15], true);
        // Varyantlar: çimen üstler, yanlar, alt kenarlar, dolgular
        Put(map, 4, 0, t[7]); Put(map, 5, 0, t[8]); Put(map, 6, 0, t[1]); Put(map, 7, 0, t[2]);
        Put(map, 4, -1, t[10]); Put(map, 5, -1, t[12]); Put(map, 6, -1, t[21]); Put(map, 7, -1, t[22]);
        Put(map, 4, -2, t[17]);
        // Sütun
        Put(map, 9, 0, t[4]); Put(map, 9, -1, t[9]); Put(map, 9, -2, t[14]);
        // Çıkıntı ve tek blok
        Put(map, 11, 0, t[23]); Put(map, 12, 0, t[24]); Put(map, 13, 0, t[23], true);
        Put(map, 11, -2, t[3]);
        // İç köşeler ve diğer parçalar
        Put(map, 13, -2, t[26]); Put(map, 14, -2, t[25]); Put(map, 15, -2, t[20]); Put(map, 16, -2, t[27]);
        Put(map, 13, -3, t[13]); Put(map, 14, -3, t[18]); Put(map, 15, -3, t[19]);
        // Su
        Put(map, 18, 0, t[29]); Put(map, 18, -1, t[28]);
        // Arka plan (4 x 4)
        for (int i = 0; i < 16; i++)
        {
            Put(map, i % 4, -5 - i / 4, bg[i]);
        }
        // Rule tile ve onu gösteren su karosu oku
        Put(map, 9, -6, ruleTile);
        Put(map, 6, -6, t[28]); Put(map, 7, -6, t[28]); Put(map, 7, -5, t[28]); Put(map, 7, -7, t[28]);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PalettePath);
        var settings = ScriptableObject.CreateInstance<GridPalette>();
        settings.name = "Palette Settings";
        settings.cellSizing = GridPalette.CellSizing.Automatic;
        AssetDatabase.AddObjectToAsset(settings, prefab);
        AssetDatabase.SaveAssets();
        Object.DestroyImmediate(root);
    }

    static void Put(Tilemap map, int x, int y, TileBase tile, bool flipX = false)
    {
        var position = new Vector3Int(x, y, 0);
        map.SetTile(position, tile);
        if (flipX)
        {
            map.SetTransformMatrix(position, Matrix4x4.Scale(new Vector3(-1, 1, 1)));
        }
    }

    // ---------------------------------------------------------------- Part 5

    static readonly Vector3Int[] NeighborPositions =
    {
        new Vector3Int(-1, 1, 0), new Vector3Int(0, 1, 0), new Vector3Int(1, 1, 0),
        new Vector3Int(-1, 0, 0),                           new Vector3Int(1, 0, 0),
        new Vector3Int(-1, -1, 0), new Vector3Int(0, -1, 0), new Vector3Int(1, -1, 0),
    };

    static RuleTile MakeRuleTile(Dictionary<string, Sprite> terrain)
    {
        System.Func<int, Sprite> S = i => terrain[Terrain + "_" + i];
        const RuleTile.TilingRuleOutput.Transform Fixed = RuleTile.TilingRuleOutput.Transform.Fixed;
        const RuleTile.TilingRuleOutput.Transform MirrorX = RuleTile.TilingRuleOutput.Transform.MirrorX;
        const RuleTile.TilingRuleOutput.Transform Rotated = RuleTile.TilingRuleOutput.Transform.Rotated;

        RuleTile ruleTile = AssetDatabase.LoadAssetAtPath<RuleTile>(RuleTilePath);
        bool isNew = ruleTile == null;
        if (isNew)
        {
            ruleTile = ScriptableObject.CreateInstance<RuleTile>();
        }

        ruleTile.m_DefaultSprite = S(6);
        ruleTile.m_DefaultColliderType = Tile.ColliderType.Sprite;

        // Komşu deseni 3 x 3, yukarıdan aşağı: sol üst, üst, sağ üst / sol, (merkez), sağ / sol alt, alt, sağ alt
        // '+' bu rule tile olmalı (yeşil ok), 'x' olmamalı (kırmızı çarpı), '.' fark etmez.
        // Özel durumlar üstte, genel kurallar altta: Unity ilk eşleşen kuralda durur.
        ruleTile.m_TilingRules = new List<RuleTile.TilingRule>
        {
            Rule(1,  ".x.x.x.x.", Fixed,   S(3)),                  // tek blok
            Rule(2,  ".x.x.x.+.", Fixed,   S(4)),                  // sütun üstü
            Rule(3,  ".+.x.x.+.", Fixed,   S(9)),                  // sütun ortası
            Rule(4,  ".+.x.x.x.", Fixed,   S(14)),                 // sütun altı
            Rule(5,  ".x.x.+.x.", MirrorX, S(23)),                 // çıkıntı ucu
            Rule(6,  ".x.+.+.x.", Fixed,   S(24)),                 // çıkıntı ortası
            Rule(7,  ".x.x.+.+.", MirrorX, S(0)),                  // üst köşe
            Rule(8,  ".+.x.+.x.", MirrorX, S(15)),                 // alt köşe
            Rule(9,  "x+.+.+.+.", Rotated, S(26)),                 // iç köşe (Fill'in üstünde!)
            Rule(10, ".x.+.+.+.", Fixed,   S(6), S(7), S(8)),      // üst kenar: çimen
            Rule(11, ".+.+.+.x.", Fixed,   S(16), S(17)),          // alt kenar
            Rule(12, ".+.x.+.+.", MirrorX, S(5), S(10)),           // yan
            Rule(13, ".+.+.+.+.", Fixed,   S(11), S(12), S(21), S(22)), // dolgu
        };

        if (isNew)
        {
            AssetDatabase.CreateAsset(ruleTile, RuleTilePath);
        }
        else
        {
            EditorUtility.SetDirty(ruleTile);
        }
        return ruleTile;
    }

    static RuleTile.TilingRule Rule(int id, string pattern, RuleTile.TilingRuleOutput.Transform transform,
        params Sprite[] sprites)
    {
        var rule = new RuleTile.TilingRule
        {
            m_Id = id,
            m_Sprites = sprites,
            m_Output = sprites.Length > 1
                ? RuleTile.TilingRuleOutput.OutputSprite.Random
                : RuleTile.TilingRuleOutput.OutputSprite.Single,
            m_PerlinScale = 0.5f,
            m_RandomTransform = RuleTile.TilingRuleOutput.Transform.Fixed,
            m_RuleTransform = transform,
            m_ColliderType = Tile.ColliderType.Sprite,
            m_NeighborPositions = new List<Vector3Int>(NeighborPositions),
            m_Neighbors = new List<int>(),
        };
        for (int i = 0; i < 9; i++)
        {
            if (i == 4)
            {
                continue;  // merkez: karonun kendisi
            }
            rule.m_Neighbors.Add(pattern[i] == '+' ? RuleTile.TilingRuleOutput.Neighbor.This
                : pattern[i] == 'x' ? RuleTile.TilingRuleOutput.Neighbor.NotThis
                : 0);
        }
        return rule;
    }

    // ---------------------------------------------------------------- Part 4 + 5 + 6

    static bool BuildScene(Tile[] t, Tile[] bg, RuleTile ruleTile)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelScene) == null &&
            !AssetDatabase.CopyAsset(TemplateScene, LevelScene))
        {
            EditorUtility.DisplayDialog("Glimmer Grotto", "Sahne şablonu bulunamadı: " + TemplateScene, "Tamam");
            return false;
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(LevelScene, true) };

        Scene scene = EditorSceneManager.OpenScene(LevelScene, OpenSceneMode.Single);

        GameObject gridObject = Find(scene, "Tilemaps");
        if (gridObject == null)
        {
            gridObject = new GameObject("Tilemaps", typeof(Grid));
            SceneManager.MoveGameObjectToScene(gridObject, scene);
        }

        // Liste sırası = çizim sırası değil; çizimi sorting layer'lar belirler.
        Tilemap background = EnsureTilemap(scene, gridObject.transform, "Background", 0);
        Tilemap water = EnsureTilemap(scene, gridObject.transform, "Water", 1);
        Tilemap platforms = EnsureTilemap(scene, gridObject.transform, "Platforms", 2);

        background.ClearAllTiles();
        water.ClearAllTiles();
        platforms.ClearAllTiles();

        int height = LevelMap.Length;
        int width = LevelMap.Max(row => row.Length);

        // Arka plan: duvar taşları her sırayla yan yana gelebilir
        var random = new System.Random(462);
        for (int x = LevelLeft - 6; x < LevelLeft + width + 32; x++)
        {
            for (int y = LevelBottom - 6; y < LevelBottom + height + 6; y++)
            {
                background.SetTile(new Vector3Int(x, y, 0), bg[random.Next(bg.Length)]);
            }
        }

        // Kaya ve su
        for (int row = 0; row < height; row++)
        {
            string line = LevelMap[row];
            int y = LevelBottom + (height - 1 - row);
            for (int col = 0; col < line.Length; col++)
            {
                var cell = new Vector3Int(LevelLeft + col, y, 0);
                if (line[col] == '#')
                {
                    platforms.SetTile(cell, ruleTile);
                }
                else if (line[col] == 'w')
                {
                    bool waterAbove = row > 0 && LevelMap[row - 1][col] == 'w';
                    water.SetTile(cell, waterAbove ? t[28] : t[29]);  // 29 yüzey, 28 derin su
                }
            }
        }

        // Kenar dolgusu: haritanın kenarındaki kaya, kameranın dışına EdgePadding karo uzar.
        // Böylece rule tile kenarda "boşluk" görüp çimen ya da dış yüz çizmez.
        for (int col = -EdgePadding; col < width + EdgePadding; col++)
        {
            for (int row = -EdgePadding; row < height + EdgePadding; row++)
            {
                if (col >= 0 && col < width && row >= 0 && row < height)
                {
                    continue;  // haritanın içi zaten boyandı
                }
                int edgeCol = Mathf.Clamp(col, 0, width - 1);
                int edgeRow = Mathf.Clamp(row, 0, height - 1);
                if (LevelMap[edgeRow][edgeCol] == '#')
                {
                    platforms.SetTile(new Vector3Int(LevelLeft + col, LevelBottom + (height - 1 - row), 0), ruleTile);
                }
            }
        }
        platforms.RefreshAllTiles();

        // Part 6: GroundPainter, bölümün sağındaki boş alana boyar (menüden "Paint Ground")
        GroundPainter painter = platforms.GetComponent<GroundPainter>();
        if (painter == null)
        {
            painter = platforms.gameObject.AddComponent<GroundPainter>();
        }
        var so = new SerializedObject(painter);
        so.FindProperty("targetTilemap").objectReferenceValue = platforms;
        so.FindProperty("groundTile").objectReferenceValue = ruleTile;
        so.FindProperty("firstColumn").intValue = LevelLeft + width + EdgePadding + 2;
        so.FindProperty("columns").intValue = 26;
        so.FindProperty("bottomRow").intValue = LevelBottom;
        so.FindProperty("startHeight").intValue = LevelBottom + 3;
        so.FindProperty("lowestSurface").intValue = LevelBottom + 2;
        so.FindProperty("highestSurface").intValue = LevelBottom + 6;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Kamera: bölümün ortasına
        GameObject cameraObject = Find(scene, "Main Camera");
        Camera camera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;
        if (camera != null)
        {
            camera.orthographic = true;
            // 16:9 ekranda 48 x 27 karonun tamamı: yükseklik / 2
            camera.orthographicSize = height / 2f;
            camera.transform.position = new Vector3(LevelLeft + width / 2f, LevelBottom + height / 2f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
        }

        // Global Light 2D: şablonda yalnızca Default katmanını aydınlatıyor. URP 2D'de ışık almayan
        // sprite siyah çizilir; bu yüzden ışığı bütün sorting layer'lara uygula.
        LightAllSortingLayers(Find(scene, "Global Light 2D"));

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Component component in root.GetComponentsInChildren<Component>(true))
            {
                if (component != null)
                {
                    EditorUtility.SetDirty(component);
                }
            }
        }
        EditorSceneManager.MarkSceneDirty(scene);
        return EditorSceneManager.SaveScene(scene);
    }

    static Tilemap EnsureTilemap(Scene scene, Transform grid, string name, int siblingIndex)
    {
        GameObject go = Find(scene, name);
        if (go == null)
        {
            go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
        }
        go.transform.SetParent(grid, false);
        go.transform.SetSiblingIndex(siblingIndex);

        if (!SortingLayer.layers.Any(l => l.name == name))
        {
            Debug.LogWarning("Sorting layer eksik: " + name + " (Edit > Project Settings > Tags and Layers)");
        }
        TilemapRenderer renderer = go.GetComponent<TilemapRenderer>();
        renderer.sortingLayerName = name;  // her tilemap kendi sorting layer'ında
        renderer.sortingOrder = 0;

        Tilemap tilemap = go.GetComponent<Tilemap>();
        tilemap.orientation = Tilemap.Orientation.Custom;
        tilemap.orientationMatrix = Matrix4x4.Scale(new Vector3(SeamFixScale, SeamFixScale, 1f));
        return tilemap;
    }

    static void LightAllSortingLayers(GameObject lightObject)
    {
        if (lightObject == null)
        {
            Debug.LogWarning("Global Light 2D bulunamadı.");
            return;
        }
        foreach (Component component in lightObject.GetComponents<Component>())
        {
            if (component == null || component.GetType().Name != "Light2D")
            {
                continue;
            }
            var so = new SerializedObject(component);
            SerializedProperty targets = so.FindProperty("m_ApplyToSortingLayers");
            if (targets == null || !targets.isArray)
            {
                continue;
            }
            SortingLayer[] layers = SortingLayer.layers;
            targets.arraySize = layers.Length;
            for (int i = 0; i < layers.Length; i++)
            {
                targets.GetArrayElementAtIndex(i).intValue = layers[i].id;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    // ---------------------------------------------------------------- yardımcılar

    static GameObject Find(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }
        }
        return null;
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
