using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ------------------------------------------------------------------
// Cipher Quest - kurulum aracı (yalnızca Editor'de çalışır, build'e girmez)
// Menü: Cipher Quest > 1. Sahneleri kur
//   Kılavuzun Part 5-10 sahne işlerini yapar: CoreGame arayüzü, Gameplay
//   nesnesi ve referansları, düğme bağlantıları, QUIT, giriş/kazanma
//   yazıları, kamera rengi, Scene List ve Player ayarları.
//   Birden çok kez çalıştırılabilir: var olan nesneleri adıyla bulur,
//   yoksa oluşturur, ayarlarını yeniden uygular.
// Menü: Cipher Quest > 2. Windows build al
//   Depo kökündeki Builds/CipherQuest klasörüne .exe üretir.
// ------------------------------------------------------------------
public static class CipherQuestSetup
{
    const string ScenesFolder = "Assets/Scenes/";
    static readonly string[] SceneOrder = { "StartMenu", "CoreGame", "WinScreen" };

    // Kılavuzdaki renkler
    static readonly Color CameraColor = Hex("101820");
    static readonly Color PanelColor = Hex("1C2A38");
    static readonly Color GuessBoxColor = Hex("0E2A1E");
    static readonly Color NeonGreen = Hex("3DFF8C");
    static readonly Color DarkText = Hex("101820");
    static readonly Color MutedText = Hex("8FA3B8");

    [MenuItem("Cipher Quest/1. Sahneleri kur (Part 5-10)")]
    public static void SetupAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        SetBuildScenes();
        SetPlayerSettings();

        bool ok = SetupCoreGame() && SetupStartMenu() && SetupWinScreen();

        EditorSceneManager.OpenScene(ScenePath("StartMenu"), OpenSceneMode.Single);

        if (ok)
        {
            EditorUtility.DisplayDialog(
                "Cipher Quest",
                "Sahneler kuruldu ve kaydedildi.\n\nStartMenu açık: Play'e basıp oyunu dene.",
                "Tamam");
        }
    }

    [MenuItem("Cipher Quest/2. Windows build al")]
    public static void BuildWindows()
    {
        string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Builds", "CipherQuest"));
        Directory.CreateDirectory(folder);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath("StartMenu"), ScenePath("CoreGame"), ScenePath("WinScreen") },
            locationPathName = Path.Combine(folder, "Cipher Quest.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            EditorUtility.RevealInFinder(options.locationPathName);
        }
        else
        {
            EditorUtility.DisplayDialog("Cipher Quest", "Build başarısız: Console'daki hatalara bak.", "Tamam");
        }
    }

    // ---------------------------------------------------------------- Part 3, 10

    static void SetBuildScenes()
    {
        var scenes = new EditorBuildSettingsScene[SceneOrder.Length];
        for (int i = 0; i < SceneOrder.Length; i++)
        {
            scenes[i] = new EditorBuildSettingsScene(ScenePath(SceneOrder[i]), true);
        }
        EditorBuildSettings.scenes = scenes;
    }

    static void SetPlayerSettings()
    {
        PlayerSettings.companyName = "Fatih Sert";
        PlayerSettings.productName = "Cipher Quest";
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.SplashScreen.showUnityLogo = false;
    }

    // ---------------------------------------------------------------- Part 5-9

    static bool SetupCoreGame()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath("CoreGame"), OpenSceneMode.Single);
        Transform canvas = RequireCanvas(scene);
        GameObject correctGo = Find(scene, "CorrectButton");
        if (canvas == null || correctGo == null)
        {
            return Fail("CoreGame sahnesinde Canvas veya CorrectButton bulunamadı.");
        }

        SetCamera(scene);
        SceneNavigator navigator = EnsureNavigator(scene);
        TMP_FontAsset font = FontOf(correctGo);

        // 2. Background: en arkada çizilsin diye Canvas'ın ilk çocuğu
        Image background = GetOrCreate(scene, "Background", canvas).GetOrAdd<Image>();
        background.color = PanelColor;
        Place(background.rectTransform, Center, Center, new Vector2(-250, -40), new Vector2(1100, 650));
        background.transform.SetAsFirstSibling();

        // 8. AttemptText: Background'un çocuğu, sağ üst köşe
        TextMeshProUGUI attempt = MakeText(scene, "AttemptText", background.transform, "Attempt 1", 40,
            MutedText, TextAlignmentOptions.TopRight, font);
        Place(attempt.rectTransform, TopRight, TopRight, new Vector2(-30, -20), new Vector2(300, 60));

        // 1. HeadingText: üst orta
        TextMeshProUGUI heading = MakeText(scene, "HeadingText", canvas,
            "Codebreaker <color=#3DFF8C>Fatih</color> is cracking your code...", 70,
            Color.white, TextAlignmentOptions.Center, font);
        Place(heading.rectTransform, TopCenter, TopCenter, new Vector2(0, -40), new Vector2(1600, 100));

        // 6. DialogueText: iki satırlık mesaj sığsın diye Auto Size
        TextMeshProUGUI dialogue = MakeText(scene, "DialogueText", canvas, "Is your secret code...", 60,
            Color.white, TextAlignmentOptions.Center, font);
        dialogue.enableAutoSizing = true;
        dialogue.fontSizeMin = 30;
        dialogue.fontSizeMax = 60;
        Place(dialogue.rectTransform, Center, Center, new Vector2(-250, 100), new Vector2(1000, 160));

        // 7. GuessBackground + GuessText (stretch: kutuyu tam doldurur)
        Image guessBox = GetOrCreate(scene, "GuessBackground", canvas).GetOrAdd<Image>();
        guessBox.color = GuessBoxColor;
        Place(guessBox.rectTransform, Center, Center, new Vector2(-250, -150), new Vector2(400, 200));

        TextMeshProUGUI guess = MakeText(scene, "GuessText", guessBox.transform, "????", 120,
            NeonGreen, TextAlignmentOptions.Center, font);
        Stretch(guess.rectTransform);

        // 3-5. Düğmeler
        Button correct = correctGo.GetComponent<Button>();
        NameButtonText(correct, "CorrectText", "CORRECT");
        StyleButton(correct, Hex("3DDC84"), Hex("31B56C"), Hex("278F56"), DarkText);
        Place((RectTransform)correct.transform, RightMiddle, RightMiddle, new Vector2(-150, 0), new Vector2(400, 120));

        Button higher = GetOrDuplicateButton(scene, correct, "HigherButton", "HigherText", "HIGHER");
        StyleButton(higher, Hex("2E86AB"), Hex("256E8C"), Hex("1D5770"), Color.white);
        Place((RectTransform)higher.transform, RightMiddle, RightMiddle, new Vector2(-150, 180), new Vector2(400, 120));

        Button lower = GetOrDuplicateButton(scene, correct, "LowerButton", "LowerText", "LOWER");
        StyleButton(lower, Hex("2E86AB"), Hex("256E8C"), Hex("1D5770"), Color.white);
        Place((RectTransform)lower.transform, RightMiddle, RightMiddle, new Vector2(-150, -180), new Vector2(400, 120));

        // Part 6: Gameplay nesnesi + Codebreaker
        GameObject gameplay = Find(scene, "Gameplay");
        if (gameplay == null)
        {
            gameplay = new GameObject("Gameplay");
            SceneManager.MoveGameObjectToScene(gameplay, scene);
        }
        gameplay.transform.SetParent(null);
        gameplay.transform.position = Vector3.zero;
        Codebreaker codebreaker = gameplay.GetOrAdd<Codebreaker>();

        // Part 7 + 9: Inspector alanları (private [SerializeField] alanlar SerializedObject ile)
        var so = new SerializedObject(codebreaker);
        so.FindProperty("lowestCode").intValue = 1;
        so.FindProperty("highestCode").intValue = 1000;
        so.FindProperty("guessText").objectReferenceValue = guess;
        so.FindProperty("dialogueText").objectReferenceValue = dialogue;
        so.FindProperty("attemptText").objectReferenceValue = attempt;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Part 4 + 7: On Click () bağlantıları
        Wire(correct, navigator.LoadNextScene);
        Wire(higher, codebreaker.OnPressHigher);
        Wire(lower, codebreaker.OnPressLower);

        return Save(scene);
    }

    // ---------------------------------------------------------------- Part 10

    static bool SetupStartMenu()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath("StartMenu"), OpenSceneMode.Single);
        Transform canvas = RequireCanvas(scene);
        GameObject startGo = Find(scene, "StartButton");
        if (canvas == null || startGo == null)
        {
            return Fail("StartMenu sahnesinde Canvas veya StartButton bulunamadı.");
        }

        SetCamera(scene);
        SceneNavigator navigator = EnsureNavigator(scene);
        TMP_FontAsset font = FontOf(startGo);

        Button start = startGo.GetComponent<Button>();
        NameButtonText(start, "StartText", "START");
        StyleButton(start, Hex("F2A541"), Hex("D98E2B"), Hex("B8741C"), DarkText);
        Place((RectTransform)start.transform, RightMiddle, RightMiddle, new Vector2(-150, 0), new Vector2(400, 120));
        Wire(start, navigator.LoadNextScene);

        // QUIT: START'tan küçük, altında, uyarı renginde
        Button quit = GetOrDuplicateButton(scene, start, "QuitButton", "QuitText", "QUIT");
        StyleButton(quit, Hex("E4572E"), Hex("C44A26"), Hex("A33D1F"), Color.white);
        Place((RectTransform)quit.transform, RightMiddle, RightMiddle, new Vector2(-200, -140), new Vector2(300, 90));
        quit.GetComponentInChildren<TextMeshProUGUI>(true).fontSize = 60;
        Wire(quit, navigator.QuitGame);

        TextMeshProUGUI intro = MakeText(scene, "IntroText", canvas,
            "<color=#3DFF8C>Welcome to Cipher Quest!</color>\n" +
            "<size=70%>Think of a secret code between 1 and 1000.\n" +
            "Codebreaker Fatih will crack it.</size>",
            70, Color.white, TextAlignmentOptions.Center, font);
        Place(intro.rectTransform, Center, Center, new Vector2(0, 300), new Vector2(1500, 260));

        return Save(scene);
    }

    static bool SetupWinScreen()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath("WinScreen"), OpenSceneMode.Single);
        Transform canvas = RequireCanvas(scene);
        GameObject playGo = Find(scene, "PlayAgainButton");
        if (canvas == null || playGo == null)
        {
            return Fail("WinScreen sahnesinde Canvas veya PlayAgainButton bulunamadı.");
        }

        SetCamera(scene);
        SceneNavigator navigator = EnsureNavigator(scene);
        TMP_FontAsset font = FontOf(playGo);

        Button playAgain = playGo.GetComponent<Button>();
        NameButtonText(playAgain, "PlayAgainText", "PLAY AGAIN");
        StyleButton(playAgain, Hex("F2A541"), Hex("D98E2B"), Hex("B8741C"), DarkText);
        Wire(playAgain, navigator.LoadStartScene);

        TextMeshProUGUI win = MakeText(scene, "WinText", canvas,
            "<color=#3DFF8C>Cipher cracked!</color>\n<size=70%>Want to challenge me again?</size>",
            80, Color.white, TextAlignmentOptions.Center, font);
        Place(win.rectTransform, Center, Center, new Vector2(-150, 150), new Vector2(1100, 260));

        return Save(scene);
    }

    // ---------------------------------------------------------------- yardımcılar

    static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
    static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);
    static readonly Vector2 TopRight = new Vector2(1f, 1f);
    static readonly Vector2 RightMiddle = new Vector2(1f, 0.5f);

    static string ScenePath(string name) => ScenesFolder + name + ".unity";

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }

    static bool Fail(string message)
    {
        EditorUtility.DisplayDialog("Cipher Quest", message, "Tamam");
        return false;
    }

    static bool Save(Scene scene)
    {
        // Koddan yapılan her değişikliği Unity'ye "değişti" olarak bildir, yoksa kayda girmeyebilir.
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

    static Transform RequireCanvas(Scene scene)
    {
        GameObject canvas = Find(scene, "Canvas");
        return canvas != null ? canvas.transform : null;
    }

    static T GetOrAdd<T>(this GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    // Adıyla bulur, yoksa UI nesnesi olarak oluşturur; her durumda parent'ı ayarlar.
    static GameObject GetOrCreate(Scene scene, string name, Transform parent)
    {
        GameObject go = Find(scene, name);
        if (go == null)
        {
            go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
        }
        go.transform.SetParent(parent, false);
        return go;
    }

    static TextMeshProUGUI MakeText(Scene scene, string name, Transform parent, string text, float size,
        Color color, TextAlignmentOptions alignment, TMP_FontAsset font)
    {
        TextMeshProUGUI tmp = GetOrCreate(scene, name, parent).GetOrAdd<TextMeshProUGUI>();
        if (font != null)
        {
            tmp.font = font;
        }
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.raycastTarget = false;  // yazılar tıklamayı düğmelerden çalmasın
        return tmp;
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        rt.localScale = Vector3.one;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = Center;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    static TMP_FontAsset FontOf(GameObject go)
    {
        TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>(true);
        return tmp != null && tmp.font != null ? tmp.font : TMP_Settings.defaultFontAsset;
    }

    static void NameButtonText(Button button, string textName, string label)
    {
        TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp == null)
        {
            return;
        }
        tmp.gameObject.name = textName;
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    static void StyleButton(Button button, Color normal, Color highlighted, Color pressed, Color textColor)
    {
        if (button.targetGraphic != null)
        {
            button.targetGraphic.color = Color.white;  // Color Tint, Image rengini bu renklerle çarpar
            button.targetGraphic.raycastTarget = true; // fare düğmeyi ancak bununla "görür"
        }
        ColorBlock colors = button.colors;
        colors.normalColor = normal;
        colors.highlightedColor = highlighted;
        colors.pressedColor = pressed;
        colors.selectedColor = normal;  // aksi halde tıkladıktan sonra "takılı" görünür
        button.colors = colors;

        TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp != null)
        {
            tmp.color = textColor;
        }
    }

    // Kaynak düğmeyi kopyalar (kılavuzdaki Ctrl+D); kopyanın On Click listesi temizlenir.
    static Button GetOrDuplicateButton(Scene scene, Button source, string name, string textName, string label)
    {
        GameObject go = Find(scene, name);
        if (go == null)
        {
            go = Object.Instantiate(source.gameObject, source.transform.parent);
            go.name = name;
        }
        Button button = go.GetComponent<Button>();
        ClearListeners(button);
        NameButtonText(button, textName, label);
        return button;
    }

    static void ClearListeners(Button button)
    {
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(button.onClick, i);
        }
    }

    // Inspector'daki On Click () + satırının koddaki karşılığı
    static void Wire(Button button, UnityAction action)
    {
        ClearListeners(button);
        UnityEventTools.AddPersistentListener(button.onClick, action);
        button.onClick.SetPersistentListenerState(0, UnityEventCallState.RuntimeOnly);
        EditorUtility.SetDirty(button);
    }

    static SceneNavigator EnsureNavigator(Scene scene)
    {
        GameObject go = Find(scene, "SceneNavigator");
        if (go == null)
        {
            go = new GameObject("SceneNavigator");
            SceneManager.MoveGameObjectToScene(go, scene);
        }
        return go.GetOrAdd<SceneNavigator>();
    }

    static void SetCamera(Scene scene)
    {
        GameObject go = Find(scene, "Main Camera");
        Camera camera = go != null ? go.GetComponent<Camera>() : null;
        if (camera == null)
        {
            return;
        }
        // URP'de Inspector'daki Environment > Background Type = Solid Color
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = CameraColor;
    }
}
