using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// ------------------------------------------------------------------
// Lecture 01 - Activity 1 + 2 kurulum aracı (yalnızca Editor)
// Menü: Lecture 01 > Karakteri kur
//   Activity 1: daire, kare ve üçgen sprite'lardan bir karakter; renk, ölçek,
//               dönüş ve Order in Layer ile önde/arkada; sahne FirstScene
//   Activity 2: PrintToConsole.cs, Head nesnesine eklenir
// ------------------------------------------------------------------
public static class Lecture01Setup
{
    const string ScenePath = "Assets/Scenes/FirstScene.unity";
    const string TemplateScene = "Assets/Settings/Scenes/URP2DSceneTemplate.unity";

    [MenuItem("Lecture 01/Karakteri kur", true)]
    static bool CanSetup() => !EditorApplication.isPlayingOrWillChangePlaymode;

    [MenuItem("Lecture 01/Karakteri kur")]
    public static void Setup()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
        {
            AssetDatabase.DeleteAsset(ScenePath);  // her seferinde temiz sahne
        }
        AssetDatabase.CopyAsset(TemplateScene, ScenePath);
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var character = new GameObject("Character");
        SceneManager.MoveGameObjectToScene(character, scene);

        // Order in Layer: büyük sayı önde çizilir
        GameObject arms1 = Part("Square", "Left Arm", character, new Vector2(-0.75f, -0.35f), new Vector2(0.25f, 1.1f), 25f, Hex("8A8F98"), -1);
        GameObject arms2 = Part("Square", "Right Arm", character, new Vector2(0.75f, -0.35f), new Vector2(0.25f, 1.1f), -25f, Hex("8A8F98"), -1);
        GameObject leg1 = Part("Square", "Left Leg", character, new Vector2(-0.28f, -1.55f), new Vector2(0.3f, 0.9f), 0f, Hex("2F3542"), -1);
        GameObject leg2 = Part("Square", "Right Leg", character, new Vector2(0.28f, -1.55f), new Vector2(0.3f, 0.9f), 0f, Hex("2F3542"), -1);
        GameObject body = Part("Square", "Body", character, new Vector2(0f, -0.5f), new Vector2(1.2f, 1.4f), 0f, Hex("3A7BD5"), 0);
        GameObject head = Part("Circle", "Head", character, new Vector2(0f, 0.75f), new Vector2(1.1f, 1.1f), 0f, Hex("F5CBA7"), 1);
        GameObject eye1 = Part("Circle", "Left Eye", character, new Vector2(-0.2f, 0.85f), new Vector2(0.18f, 0.18f), 0f, Hex("1E1E1E"), 2);
        GameObject eye2 = Part("Circle", "Right Eye", character, new Vector2(0.2f, 0.85f), new Vector2(0.18f, 0.18f), 0f, Hex("1E1E1E"), 2);
        GameObject mouth = Part("Square", "Mouth", character, new Vector2(0f, 0.5f), new Vector2(0.35f, 0.07f), 0f, Hex("A93226"), 2);
        GameObject hat = Part("Triangle", "Hat", character, new Vector2(0.05f, 1.55f), new Vector2(0.9f, 0.7f), -10f, Hex("E74C3C"), 2);
        if (arms1 == null || arms2 == null || leg1 == null || leg2 == null || body == null || head == null ||
            eye1 == null || eye2 == null || mouth == null || hat == null)
        {
            EditorUtility.DisplayDialog("Lecture 01",
                "GameObject > 2D Object > Sprites menüsü bulunamadı. Console'a bak.", "Tamam");
            return;
        }

        // Activity 2
        head.AddComponent<PrintToConsole>();

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = head;
        EditorUtility.DisplayDialog("Lecture 01",
            "FirstScene kuruldu.\n\nPlay'e bas: Console'da karakterin mesajı görünür.", "Tamam");
    }

    // Unity'nin kendi menüsüyle sprite oluşturur (GameObject > 2D Object > Sprites > ...)
    static GameObject Part(string shape, string name, GameObject parent, Vector2 position, Vector2 scale,
        float rotationZ, Color color, int order)
    {
        Selection.activeGameObject = null;
        if (!EditorApplication.ExecuteMenuItem("GameObject/2D Object/Sprites/" + shape))
        {
            Debug.LogError("Menü bulunamadı: GameObject/2D Object/Sprites/" + shape);
            return null;
        }
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            return null;
        }
        go.name = name;
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = new Vector3(position.x, position.y, 0f);
        go.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
        var renderer = go.GetComponent<SpriteRenderer>();
        renderer.color = color;
        renderer.sortingOrder = order;
        EditorUtility.SetDirty(go);
        return go;
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }
}
