using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// ------------------------------------------------------------------
// Glimmer Grotto - Lab 5 kurulum aracı (Glimmer Grotto, part 4)
// Menü: Glimmer Grotto > 4. Lab 5'i kur   (önce Lab 2, 3 ve 4 kurulmuş olmalı)
//   Part 1: Climbing tilemap'i (layer + sorting layer Climbing), merdiven karosu
//           (MainPalette'te), dört noktalı fizik şekli, iki merdiven; Tilemap Collider 2D
//           (Merge) + Composite Collider 2D (Is Trigger, Polygons) + statik Rigidbody 2D
//   Part 2-4: PlayerMovement'ta Climb Speed 5 (kod Assets/Scripts/PlayerMovement.cs'te)
//   Part 5: Main Camera'da Cinemachine Brain
//   Part 6: Background bölümün sınırına kırpılır, Polygon Collider 2D; Player ve
//           Background layer'ları; Layer Collision Matrix'te Background hiçbir şeyle çarpışmaz
//   Part 7: State-Driven Camera: Run (4.5), Idle (3.5), Ladder (6) kameraları, her biri
//           Position Composer (Dead Zone 0.2) + Confiner 2D; talimatlar, CameraBlends
// Birden çok kez çalıştırılabilir; var olanları yeniden kurar.
// ------------------------------------------------------------------
public static partial class GlimmerGrottoSetup
{
    const string LadderSheet = "SPA_Ladder";
    const string ClimbingName = "Climbing";
    const int ClimbingLayerIndex = 7;
    const int PlayerLayerIndex = 8;
    const int BackgroundLayerIndex = 9;
    const string BlendsPath = "Assets/Animations/CameraBlends.asset";
    const string StateDrivenName = "State-Driven Camera";
    const string AnyCamera = "**ANY CAMERA**";   // From/To listelerinin ilk satırı

    // Merdivenler: sütun (x) ve alttan üste karo satırları (y). Tabanda durur, en üst
    // karo yanındaki platformun üst karosuyla aynı hizada biter (kılavuzdaki ipucu).
    static readonly (int x, int bottom, int top)[] Ladders =
    {
        (-21, -1, 6),   // başlangıcın solu: zeminden soldaki "####" platformuna, 8 karo
        (21, 0, 8),     // sağ taraf: yükseltiden sağ üstteki "####" platformuna, 9 karo
    };

    [MenuItem("Glimmer Grotto/4. Lab 5'i kur", true)]
    static bool CanSetupLab5() => !EditorApplication.isPlayingOrWillChangePlaymode;  // Play modunda menü gri

    [MenuItem("Glimmer Grotto/4. Lab 5'i kur")]
    public static void SetupLab5()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelScene) == null || prefab == null ||
            prefab.GetComponent<PlayerMovement>() == null)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                "Önce menüdeki 1, 2 ve 3. adımları sırayla çalıştır (Lab 2, 3 ve 4).", "Tamam");
            return;
        }

        // Part 1.1 + 6.3: layer'lar (kod için) ve Climbing sorting layer'ı (çizim için)
        if (!EnsureLayer(GroundLayerIndex, GroundLayer) ||
            !EnsureLayer(ClimbingLayerIndex, ClimbingName) ||
            !EnsureLayer(PlayerLayerIndex, "Player") ||
            !EnsureLayer(BackgroundLayerIndex, "Background"))
        {
            return;
        }
        EnsureSortingLayerBefore(ClimbingName, PlayerSortingLayer);   // Platforms ile Player arasına
        if (!SortingLayer.layers.Any(l => l.name == ClimbingName))
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                "Climbing sorting layer'ı eklenemedi. Tags and Layers > Sorting Layers altında Platforms ile " +
                "Player arasına 'Climbing' ekleyip aracı tekrar çalıştır.", "Tamam");
            return;
        }
        IgnoreAllCollisions(BackgroundLayerIndex);   // Part 6.3: Background satırı ve sütunu boş

        // Part 1.2 + 1.3: merdiven karosu ve fizik şekli
        Sprite ladderSprite = SetLadderPhysicsShape();
        if (ladderSprite == null)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto", SpritesFolder + "/" + LadderSheet + ".png bulunamadı.", "Tamam");
            return;
        }
        Tile ladderTile = MakeTile(ladderSprite);   // Assets/Tiles/SPA_Ladder.asset
        AddToPalette(ladderTile, new Vector3Int(18, -3, 0));

        // Part 2 + 6.3: oyuncu prefab'ı: Climb Speed 5, Player layer'ı
        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        try
        {
            root.layer = PlayerLayerIndex;
            var movement = new SerializedObject(root.GetComponent<PlayerMovement>());
            movement.FindProperty("climbSpeed").floatValue = 5f;
            movement.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        Scene scene = EditorSceneManager.OpenScene(LevelScene, OpenSceneMode.Single);
        GameObject grid = Find(scene, "Tilemaps");
        GameObject backgroundObject = Find(scene, "Background");
        GameObject player = Find(scene, PlayerName);
        GameObject mainCamera = Find(scene, "Main Camera");
        if (grid == null || backgroundObject == null || player == null || mainCamera == null)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                "Tilemaps, Background, " + PlayerName + " ya da Main Camera bulunamadı. Önce Lab 2, 3 ve 4'ü kur.", "Tamam");
            return;
        }
        if (PrefabUtility.IsPartOfPrefabInstance(player))
        {
            // Sahnedeki örnekte layer farklı kalmışsa prefab'dakine dön
            var so = new SerializedObject(player);
            PrefabUtility.RevertPropertyOverride(so.FindProperty("m_Layer"), InteractionMode.AutomatedAction);
        }
        player.layer = PlayerLayerIndex;

        // Part 1.1 + 1.2 + 1.4: Climbing tilemap'i
        Tilemap climbing = EnsureTilemap(scene, grid.transform, ClimbingName, 3);   // sorting layer Climbing
        climbing.gameObject.layer = ClimbingLayerIndex;
        climbing.ClearAllTiles();
        foreach (var ladder in Ladders)
        {
            for (int y = ladder.bottom; y <= ladder.top; y++)
            {
                climbing.SetTile(new Vector3Int(ladder.x, y, 0), ladderTile);
            }
        }
        var ladderCollider = GetOrAdd<TilemapCollider2D>(climbing.gameObject);
        var ladderComposite = GetOrAdd<CompositeCollider2D>(climbing.gameObject);   // Rigidbody 2D'yi de ekler
        var ladderBody = GetOrAdd<Rigidbody2D>(climbing.gameObject);
        ladderBody.bodyType = RigidbodyType2D.Static;                     // merdiven düşmesin
        ladderCollider.compositeOperation = Collider2D.CompositeOperation.Merge;   // her merdiven tek şekil
        ladderComposite.isTrigger = true;                                 // içinden geçilir, temas yine sayılır
        ladderComposite.geometryType = CompositeCollider2D.GeometryType.Polygons;  // içi dolu: ortada da sayılır

        // Işık yeni Climbing sorting layer'ını da aydınlatsın (yoksa merdiven siyah görünür)
        LightAllSortingLayers(Find(scene, "Global Light 2D"));

        // Part 6.1 + 6.2: arka plan bölümün sınırıdır; kamera onun dışını göstermez
        int width = LevelMap.Max(row => row.Length);
        int height = LevelMap.Length;
        Tilemap background = backgroundObject.GetComponent<Tilemap>();
        TrimTilemap(background, new RectInt(LevelLeft, LevelBottom, width, height));
        backgroundObject.layer = BackgroundLayerIndex;
        var bounds = GetOrAdd<PolygonCollider2D>(backgroundObject);
        float left = LevelLeft, right = LevelLeft + width, bottom = LevelBottom, top = LevelBottom + height;
        bounds.pathCount = 1;
        bounds.SetPath(0, new[]   // dört köşe; beşinci nokta alt kenarda kalabilir
        {
            new Vector2(left, top), new Vector2(left, bottom), new Vector2(0f, bottom),
            new Vector2(right, bottom), new Vector2(right, top),
        });

        // Part 5: Main Camera'yı artık Cinemachine Brain yönetir
        var brain = GetOrAdd<CinemachineBrain>(mainCamera);
        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 2f);

        // Part 7.3: CameraBlends (özel geçişler)
        CinemachineBlenderSettings blends = MakeCameraBlends();

        // Part 5 + 7: State-Driven Camera ve üç çocuk kamera (eski kurulumu sil, yeniden kur)
        foreach (string old in new[] { StateDrivenName, "Follow Camera", "CinemachineCamera" })
        {
            foreach (GameObject go in scene.GetRootGameObjects().Where(g => g.name == old).ToArray())
            {
                Object.DestroyImmediate(go);
            }
        }
        var stateDrivenObject = new GameObject(StateDrivenName);
        SceneManager.MoveGameObjectToScene(stateDrivenObject, scene);
        stateDrivenObject.transform.SetSiblingIndex(0);   // kameralar listenin en üstünde
        var stateDriven = stateDrivenObject.AddComponent<CinemachineStateDrivenCamera>();

        Vector3 start = player.transform.position + new Vector3(0f, 0f, -10f);
        CinemachineCamera runCamera = MakeFollowCamera("Run Camera", stateDrivenObject.transform, 4.5f, player.transform, bounds, start);
        CinemachineCamera idleCamera = MakeFollowCamera("Idle Camera", stateDrivenObject.transform, 3.5f, player.transform, bounds, start);
        CinemachineCamera ladderCamera = MakeFollowCamera("Ladder Camera", stateDrivenObject.transform, 6f, player.transform, bounds, start);

        stateDriven.AnimatedTarget = player.GetComponent<Animator>();
        stateDriven.LayerIndex = 0;   // Base Layer
        stateDriven.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 2f);
        stateDriven.CustomBlends = blends;
        stateDriven.Instructions = new[]
        {
            Instruction("Running", runCamera, 0f),
            Instruction("Idling", idleCamera, 1f),     // bir saniye durunca yaklaşır
            Instruction("Climbing", ladderCamera, 0f), // merdivende beklemeden uzaklaşır
        };

        MarkAllDirty(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Selection.activeGameObject = stateDrivenObject;
        if (saved)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                "Lab 5 kuruldu.\n\nPlay'e bas ve Game görünümüne tıkla. Fatih'in hemen solundaki merdivende " +
                "W / S ile tırman. Kamera Fatih'i izler; dururken yaklaşır, koşarken ve merdivende uzaklaşır.\n\n" +
                "Game görünümünün en-boy oranını 16:9 yap.",
                "Tamam");
        }
    }

    // ---------------------------------------------------------------- Part 1

    // Sprite Editor > Custom Physics Shape ile aynı: raylardan biraz içeride, tepeden tabana tek dikdörtgen.
    static Sprite SetLadderPhysicsShape()
    {
        string path = SpritesFolder + "/" + LadderSheet + ".png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return null;
        }
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();

        // 32 x 32 piksel; raylar 5-8 ve 23-26. sütunlarda. Noktalar sprite'ın ortasına göre piksel cinsinden:
        // x = 7..25 -> -9..+9, y = 0..32 -> -16..+16. Üst üste karolar boşluksuz tek şekle birleşir.
        var shape = new List<Vector2[]>
        {
            new[] { new Vector2(-9f, 16f), new Vector2(-9f, -16f), new Vector2(9f, -16f), new Vector2(9f, 16f) },
        };
        var outlines = provider.GetDataProvider<ISpritePhysicsOutlineDataProvider>();
        foreach (SpriteRect rect in provider.GetSpriteRects())
        {
            outlines.SetOutlines(rect.spriteID, shape);
        }
        provider.Apply();
        importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
    }

    static void AddToPalette(TileBase tile, Vector3Int cell)
    {
        GameObject palette = AssetDatabase.LoadAssetAtPath<GameObject>(PalettePath);
        Tilemap map = palette != null ? palette.GetComponentInChildren<Tilemap>() : null;
        if (map == null)
        {
            Debug.LogWarning("MainPalette bulunamadı; merdiven karosunu palete elle sürükle.");
            return;
        }
        map.SetTile(cell, tile);
        EditorUtility.SetDirty(map);
        PrefabUtility.SavePrefabAsset(palette);
        if (!AssetDatabase.LoadAllAssetsAtPath(PalettePath).OfType<GridPalette>().Any())
        {
            // Tile Palette penceresi paleti bu ayar nesnesinden tanır
            var settings = ScriptableObject.CreateInstance<GridPalette>();
            settings.name = "Palette Settings";
            settings.cellSizing = GridPalette.CellSizing.Automatic;
            AssetDatabase.AddObjectToAsset(settings, palette);
        }
    }

    // ---------------------------------------------------------------- Part 6

    static void TrimTilemap(Tilemap map, RectInt keep)
    {
        map.CompressBounds();
        BoundsInt b = map.cellBounds;
        for (int x = b.xMin; x < b.xMax; x++)
        {
            for (int y = b.yMin; y < b.yMax; y++)
            {
                if (!keep.Contains(new Vector2Int(x, y)))
                {
                    map.SetTile(new Vector3Int(x, y, 0), null);
                }
            }
        }
        map.CompressBounds();
    }

    // Layer Collision Matrix: bu layer'ın satırı ve sütunu tamamen boş
    static void IgnoreAllCollisions(int layer)
    {
        var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/Physics2DSettings.asset")[0]);
        SerializedProperty matrix = settings.FindProperty("m_LayerCollisionMatrix");
        for (int i = 0; i < matrix.arraySize; i++)
        {
            SerializedProperty row = matrix.GetArrayElementAtIndex(i);
            row.uintValue = i == layer ? 0u : row.uintValue & ~(1u << layer);
        }
        settings.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------------------------------------------------------------- Part 5 + 7

    static CinemachineCamera MakeFollowCamera(string name, Transform parent, float size, Transform target,
        Collider2D bounds, Vector3 start)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = start;

        var camera = go.AddComponent<CinemachineCamera>();
        LensSettings lens = LensSettings.Default;
        lens.OrthographicSize = size;   // görünüşün yarı yüksekliği: 4.5 -> 9 karo
        camera.Lens = lens;
        camera.Target.TrackingTarget = target;

        var composer = go.AddComponent<CinemachinePositionComposer>();   // Position Control
        composer.CameraDistance = 10f;
        composer.Damping = Vector3.one;
        ScreenComposerSettings composition = ScreenComposerSettings.Default;   // Screen Position (0, 0)
        composition.DeadZone.Enabled = true;
        composition.DeadZone.Size = new Vector2(0.2f, 0.2f);
        composer.Composition = composition;

        var confiner = go.AddComponent<CinemachineConfiner2D>();   // Add Extension
        confiner.BoundingShape2D = bounds;
        return camera;
    }

    // State seçimi Cinemachine'in editöründeki gibi: "Base Layer.Durum" adının hash'i
    static CinemachineStateDrivenCamera.Instruction Instruction(string state, CinemachineCamera camera, float wait)
    {
        return new CinemachineStateDrivenCamera.Instruction
        {
            FullHash = Animator.StringToHash("Base Layer." + state),
            Camera = camera,
            ActivateAfter = wait,
            MinDuration = 0f,
        };
    }

    static CinemachineBlenderSettings MakeCameraBlends()
    {
        var blends = AssetDatabase.LoadAssetAtPath<CinemachineBlenderSettings>(BlendsPath);
        bool isNew = blends == null;
        if (isNew)
        {
            blends = ScriptableObject.CreateInstance<CinemachineBlenderSettings>();
        }
        const CinemachineBlendDefinition.Styles Ease = CinemachineBlendDefinition.Styles.EaseInOut;
        blends.CustomBlends = new[]
        {
            Blend("Run Camera", "Idle Camera", Ease, 3f),   // durunca yavaşça yaklaş
            Blend("Idle Camera", "Run Camera", Ease, 1f),   // koşunca hızla uzaklaş
            Blend(AnyCamera, "Ladder Camera", Ease, 1f),
            Blend("Ladder Camera", AnyCamera, Ease, 3f),
        };
        if (isNew)
        {
            AssetDatabase.CreateAsset(blends, BlendsPath);
        }
        else
        {
            EditorUtility.SetDirty(blends);
        }
        return blends;
    }

    static CinemachineBlenderSettings.CustomBlend Blend(string from, string to,
        CinemachineBlendDefinition.Styles style, float seconds)
    {
        return new CinemachineBlenderSettings.CustomBlend
        {
            From = from,
            To = to,
            Blend = new CinemachineBlendDefinition(style, seconds),
        };
    }

    // ---------------------------------------------------------------- yardımcılar

    // Sorting layer'ı listede "before" katmanının hemen önüne ekler (yani onun arkasında çizilir).
    static void EnsureSortingLayerBefore(string name, string before)
    {
        if (SortingLayer.layers.Any(l => l.name == name))
        {
            return;
        }
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("m_SortingLayers");
        int index = layers.arraySize;
        for (int i = 0; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == before)
            {
                index = i;
                break;
            }
        }
        layers.InsertArrayElementAtIndex(index);   // "before" bir aşağı kayar
        SerializedProperty layer = layers.GetArrayElementAtIndex(index);
        layer.FindPropertyRelative("name").stringValue = name;
        layer.FindPropertyRelative("uniqueID").intValue = 1286312341;   // benzersiz, sıfır olmayan kimlik
        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }
}
