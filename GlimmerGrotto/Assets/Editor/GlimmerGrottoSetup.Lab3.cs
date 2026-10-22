using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// ------------------------------------------------------------------
// Glimmer Grotto - Lab 3 kurulum aracı (Glimmer Grotto, part 2)
// Menü: Glimmer Grotto > 2. Lab 3'ü kur   (önce Lab 2 kurulmuş olmalı)
//   Part 1: "Fatih (Player)" GameObject'i, Player sorting layer'ı, Idling klibi,
//           Player Animator Controller'ı ve Animator
//   Part 2: Running (hız 2) ve Climbing (hız 0,5) klipleri; isRunning / isClimbing
//           parametreleri; Has Exit Time kapalı, süre 0 olan geçişler
//   Part 3: Platforms'a Tilemap Collider 2D (Merge) + Composite Collider 2D +
//           statik Rigidbody 2D; oyuncuya Capsule Collider 2D + dinamik Rigidbody 2D
//           (Freeze Rotation Z); oyuncu Assets/Prefabs altında prefab
// Birden çok kez çalıştırılabilir; var olanları günceller.
// ------------------------------------------------------------------
public static partial class GlimmerGrottoSetup
{
    const string AnimationsFolder = "Assets/Animations";
    const string PrefabsFolder = "Assets/Prefabs";
    const string ControllerPath = "Assets/Animations/Player.controller";
    const string PlayerName = "Fatih (Player)";
    const string PlayerPrefabPath = "Assets/Prefabs/Fatih (Player).prefab";
    const string PlayerSortingLayer = "Player";

    // Kılavuzdaki player_purple = bu paketteki mor ceketli SPA_Player_Bronze
    // Satırlar: shoot 0-4, idle 5-8, roll 9-13, climb 14-15, jump 16-18, run 19-28
    const string PlayerSheet = "SPA_Player_Bronze";

    [MenuItem("Glimmer Grotto/2. Lab 3'ü kur", true)]
    static bool CanSetupLab3() => !EditorApplication.isPlayingOrWillChangePlaymode;  // Play modunda menü gri

    [MenuItem("Glimmer Grotto/2. Lab 3'ü kur")]
    public static void SetupLab3()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelScene) == null)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto", "Önce Glimmer Grotto > 1. Lab 2'yi kur'u çalıştır.", "Tamam");
            return;
        }

        var sprites = LoadSprites(PlayerSheet);
        if (sprites.Count < 29)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                PlayerSheet + " kesilmemiş. Önce Glimmer Grotto > 1. Lab 2'yi kur'u çalıştır.", "Tamam");
            return;
        }
        System.Func<int, Sprite> frame = i => sprites[PlayerSheet + "_" + i];

        EnsureFolder(AnimationsFolder);
        EnsureFolder(PrefabsFolder);

        // Part 1.4: Player sorting layer, listenin en altında (en son çizilir, her şeyin önünde)
        EnsureSortingLayer(PlayerSortingLayer);
        if (!SortingLayer.layers.Any(l => l.name == PlayerSortingLayer))
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                "Player sorting layer'ı eklenemedi. Edit > Project Settings > Tags and Layers > Sorting Layers " +
                "altında en alta 'Player' ekleyip aracı tekrar çalıştır.", "Tamam");
            return;
        }

        // Part 1.5 + 2.2 + 2.5: klipler (hepsi Loop Time açık)
        AnimationClip idling = MakeLoopingClip("Idling", frame, 5, 6, 7, 8);
        AnimationClip running = MakeLoopingClip("Running", frame, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28);
        AnimationClip climbing = MakeLoopingClip("Climbing", frame, 14, 15);

        // Part 1.6 + 2.3 + 2.4 + 2.5: controller
        AnimatorController controller = MakePlayerController(idling, running, climbing);

        Scene scene = EditorSceneManager.OpenScene(LevelScene, OpenSceneMode.Single);

        // Part 3.2: platformların çarpışması
        GameObject platforms = Find(scene, "Platforms");
        if (platforms == null || platforms.GetComponent<Tilemap>() == null)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto", "Platforms tilemap'i bulunamadı. Önce Lab 2'yi kur.", "Tamam");
            return;
        }
        var tilemapCollider = GetOrAdd<TilemapCollider2D>(platforms);
        var composite = GetOrAdd<CompositeCollider2D>(platforms);   // Rigidbody 2D'yi de kendisi ekler
        var groundBody = GetOrAdd<Rigidbody2D>(platforms);
        groundBody.bodyType = RigidbodyType2D.Static;                // zemin hareket etmez
        composite.geometryType = CompositeCollider2D.GeometryType.Outlines;
        tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;  // her ada tek bir dış çizgi

        // Part 1.4 + 3.3: oyuncu
        GameObject player = Find(scene, PlayerName);
        if (player != null && PrefabUtility.IsAnyPrefabInstanceRoot(player))
        {
            // Yeniden kaydederken Prefab Variant oluşmasın diye bağlantıyı çöz
            PrefabUtility.UnpackPrefabInstance(player, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        if (player == null)
        {
            player = new GameObject(PlayerName);
            SceneManager.MoveGameObjectToScene(player, scene);
        }
        player.transform.SetParent(null);
        // Soldaki zeminin biraz üstü: Play'e basınca düşüp yere basar
        player.transform.position = new Vector3(LevelLeft + 3.5f, 1f, 0f);
        player.transform.rotation = Quaternion.identity;
        player.transform.localScale = Vector3.one;

        var spriteRenderer = GetOrAdd<SpriteRenderer>(player);
        spriteRenderer.sprite = frame(5);                  // ilk idle karesi; animasyon değiştirir
        spriteRenderer.sortingLayerName = PlayerSortingLayer;
        spriteRenderer.sortingOrder = 0;

        var animator = GetOrAdd<Animator>(player);
        animator.runtimeAnimatorController = controller;

        // Kapsül gövdeye oturur: kare 32 px; karakter 22 px geniş, 28 px yüksek, ayakları karenin tabanında
        var capsule = GetOrAdd<CapsuleCollider2D>(player);
        capsule.direction = CapsuleDirection2D.Vertical;
        capsule.size = new Vector2(0.5f, 0.875f);
        capsule.offset = new Vector2(-0.03125f, -0.0625f);

        var body = GetOrAdd<Rigidbody2D>(player);
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 1f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;  // yüzüstü devrilmesin

        // Işık yeni Player katmanını da aydınlatsın (yoksa oyuncu siyah görünür)
        LightAllSortingLayers(Find(scene, "Global Light 2D"));

        // Part 3.1: prefab (sahnedeki nesne mavi olur, prefab'ın örneği)
        PrefabUtility.SaveAsPrefabAssetAndConnect(player, PlayerPrefabPath, InteractionMode.AutomatedAction);

        MarkAllDirty(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Selection.activeGameObject = Find(scene, PlayerName);
        if (saved)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                "Lab 3 kuruldu.\n\nPlay'e bas: Fatih düşer, yere basar ve idle oynar.\n" +
                "Window > Animation > Animator penceresinde isRunning / isClimbing kutularını işaretleyerek " +
                "koşma ve tırmanmayı dene.",
                "Tamam");
        }
    }

    // ---------------------------------------------------------------- Part 1 + 2

    // Unity'nin sprite'lardan klip oluşturmasıyla aynı: 12 kare/sn, i. kare i / 12 saniyede.
    static AnimationClip MakeLoopingClip(string name, System.Func<int, Sprite> frame, params int[] frames)
    {
        string path = AnimationsFolder + "/" + name + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        bool isNew = clip == null;
        if (isNew)
        {
            clip = new AnimationClip();
        }
        clip.frameRate = 12;

        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe { time = i / clip.frameRate, value = frame(frames[i]) };
        }
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;  // son kareden sonra başa döner
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        if (isNew)
        {
            AssetDatabase.CreateAsset(clip, path);
        }
        else
        {
            EditorUtility.SetDirty(clip);
        }
        return clip;
    }

    static AnimatorController MakePlayerController(AnimationClip idling, AnimationClip running, AnimationClip climbing)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
        {
            AssetDatabase.DeleteAsset(ControllerPath);  // her seferinde temiz kur
        }
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("isRunning", AnimatorControllerParameterType.Bool);
        controller.AddParameter("isClimbing", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idle = machine.AddState("Idling", new Vector3(300, 0, 0));
        idle.motion = idling;
        idle.speed = 1f;
        AnimatorState run = machine.AddState("Running", new Vector3(560, -90, 0));
        run.motion = running;
        run.speed = 2f;      // hızlı koşu
        AnimatorState climb = machine.AddState("Climbing", new Vector3(560, 90, 0));
        climb.motion = climbing;
        climb.speed = 0.5f;  // iki kare tam hızda telaşlı görünür
        machine.defaultState = idle;  // Entry -> Idling

        // Idling merkez: her durumun bir girişi ve bir çıkışı var
        Connect(idle, run, "isRunning", true);
        Connect(run, idle, "isRunning", false);
        Connect(idle, climb, "isClimbing", true);
        Connect(climb, idle, "isClimbing", false);

        EditorUtility.SetDirty(controller);
        return controller;
    }

    // Piksel sanatı platformer'ı anında tepki vermeli: exit time yok, karışma (blend) yok.
    static void Connect(AnimatorState from, AnimatorState to, string parameter, bool value)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.exitTime = 0f;
        transition.hasFixedDuration = true;
        transition.duration = 0f;
        transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, parameter);
    }

    // ---------------------------------------------------------------- yardımcılar

    static void EnsureSortingLayer(string name)
    {
        if (SortingLayer.layers.Any(l => l.name == name))
        {
            return;
        }
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("m_SortingLayers");
        layers.InsertArrayElementAtIndex(layers.arraySize);           // listenin sonuna
        SerializedProperty layer = layers.GetArrayElementAtIndex(layers.arraySize - 1);
        layer.FindPropertyRelative("name").stringValue = name;
        layer.FindPropertyRelative("uniqueID").intValue = 1789237485;  // benzersiz, sıfır olmayan kimlik
        tagManager.ApplyModifiedPropertiesWithoutUndo();
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    static void MarkAllDirty(Scene scene)
    {
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
    }
}
