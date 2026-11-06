using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// ------------------------------------------------------------------
// Glimmer Grotto - Lab 4 kurulum aracı (Glimmer Grotto, part 3)
// Menü: Glimmer Grotto > 3. Lab 4'ü kur   (önce Lab 2 ve Lab 3 kurulmuş olmalı)
//   Part 1: oyuncuya Player Input (InputSystem_Actions, Default Map Player,
//           Behavior Send Messages) ve PlayerMovement
//   Part 5: Rigidbody 2D Gravity Scale 8, Collision Detection Continuous;
//           Run Speed 10, Jump Speed 25
//   Part 6: Ground katmanı (User Layer 6), Platforms bu katmanda
// Değişiklikler prefab'a yazılır (Overrides > Apply All ile aynı sonuç).
// Birden çok kez çalıştırılabilir. Lab 3 aracını sonra yeniden çalıştırırsan
// Gravity Scale 1'e döner: o zaman bunu da tekrar çalıştır.
// ------------------------------------------------------------------
public static partial class GlimmerGrottoSetup
{
    const string ActionsPath = "Assets/Settings/InputSystem_Actions.inputactions";
    const string GroundLayer = "Ground";
    const int GroundLayerIndex = 6;   // kılavuzdaki gibi User Layer 6

    [MenuItem("Glimmer Grotto/3. Lab 4'ü kur", true)]
    static bool CanSetupLab4() => !EditorApplication.isPlayingOrWillChangePlaymode;  // Play modunda menü gri

    [MenuItem("Glimmer Grotto/3. Lab 4'ü kur")]
    public static void SetupLab4()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelScene) == null || prefab == null)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                "Önce Glimmer Grotto > 1. Lab 2'yi kur ve 2. Lab 3'ü kur'u çalıştır.", "Tamam");
            return;
        }

        // Part 1.1: oyunun eylemleri (Move, Jump...). Proje genelindeki actions; yoksa dosyanın kendisi.
        InputActionAsset actions = InputSystem.actions;
        if (actions == null)
        {
            actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
        }
        if (actions == null)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto", ActionsPath + " bulunamadı.", "Tamam");
            return;
        }

        // Part 6.2: Ground katmanı
        if (!EnsureLayer(GroundLayerIndex, GroundLayer))
        {
            return;
        }

        // Part 1.2 + 1.3 + 5.4 + 5.5: prefab'ın içine yaz (sahnedeki örnek onu izler)
        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        try
        {
            var input = GetOrAdd<PlayerInput>(root);
            input.actions = actions;
            input.defaultActionMap = "Player";
            input.notificationBehavior = PlayerNotifications.SendMessages;  // OnMove, OnJump...

            var movement = GetOrAdd<PlayerMovement>(root);   // Player Input'un altında
            var serialized = new SerializedObject(movement);
            serialized.FindProperty("runSpeed").floatValue = 10f;
            serialized.FindProperty("jumpSpeed").floatValue = 25f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var body = GetOrAdd<Rigidbody2D>(root);
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 8f;                                              // ağır, çabuk biten zıplama
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;   // hızlı inişte zeminden geçmesin
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            root.transform.localScale = Vector3.one;
            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        Scene scene = EditorSceneManager.OpenScene(LevelScene, OpenSceneMode.Single);

        // Part 6.2: yalnızca Platforms Ground katmanında (Background ve Water'da collider yok)
        GameObject platforms = Find(scene, "Platforms");
        if (platforms == null)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto", "Platforms bulunamadı. Önce Lab 2 ve Lab 3'ü kur.", "Tamam");
            return;
        }
        platforms.layer = LayerMask.NameToLayer(GroundLayer);

        // Sahnedeki Fatih: prefab'dan farklı kalmış ayar varsa geri al, yoksa ekle
        GameObject player = Find(scene, PlayerName);
        if (player != null && PrefabUtility.IsPartOfPrefabInstance(player))
        {
            foreach (Component component in player.GetComponents<Component>())
            {
                if (!(component is Transform) && PrefabUtility.IsPartOfPrefabInstance(component))
                {
                    PrefabUtility.RevertObjectOverride(component, InteractionMode.AutomatedAction);
                }
            }
            player.transform.localScale = Vector3.one;
        }
        else
        {
            if (player != null)
            {
                Object.DestroyImmediate(player);
            }
            player = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            player.transform.position = new Vector3(LevelLeft + 3.5f, 1f, 0f);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Selection.activeGameObject = player;
        if (saved)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                "Lab 4 kuruldu.\n\nPlay'e bas ve Game görünümüne bir kez tıkla.\n" +
                "A / D veya oklar: koş.  Space: zıpla (yalnızca yerdeyken).",
                "Tamam");
        }
    }

    // Tags and Layers > Layers listesinde index'teki boş yuvaya ad yazar.
    static bool EnsureLayer(int index, string name)
    {
        if (LayerMask.NameToLayer(name) == index)
        {
            return true;
        }
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty slot = tagManager.FindProperty("layers").GetArrayElementAtIndex(index);
        if (!string.IsNullOrEmpty(slot.stringValue) && slot.stringValue != name)
        {
            EditorUtility.DisplayDialog("Glimmer Grotto",
                "User Layer " + index + " dolu ('" + slot.stringValue + "'). Edit > Project Settings > Tags and Layers " +
                "altında onu boşaltıp aracı tekrar çalıştır.", "Tamam");
            return false;
        }
        slot.stringValue = name;
        tagManager.ApplyModifiedPropertiesWithoutUndo();
        return true;
    }
}
