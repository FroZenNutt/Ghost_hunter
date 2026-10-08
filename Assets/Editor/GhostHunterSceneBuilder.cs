using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class GhostHunterSceneBuilder
{
    const string ScenePath = "Assets/Scenes/Game.unity";
    const string ConfigPath = "Assets/ScriptableObjects/GameConfig.asset";
    const string PrefabPath = "Assets/Prefabs/Ghost.prefab";
    const string PlayerMaterialPath = "Assets/Materials/PlayerMat.mat";
    const string GhostMaterialPath = "Assets/Materials/GhostMat.mat";

    static GhostHunterSceneBuilder() => EditorApplication.delayCall += EnsureBuilt;

    static void EnsureBuilt()
    {
        if (AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath) != null && AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) return;
        Build();
    }

    [MenuItem("Ghost Hunter/Rebuild Greybox Scene")]
    public static void Build()
    {
        EnsureFolder("Assets/ScriptableObjects");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Materials");
        EnsureFolder("Assets/Scenes");

        GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
        }
        Material playerMaterial = CreateMaterial(PlayerMaterialPath, new Color(124f / 255f, 1f, 212f / 255f));
        Material ghostMaterial = CreateMaterial(GhostMaterialPath, new Color(1f, 119f / 255f, 168f / 255f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateLight();
        CreateArena();
        Player player = CreatePlayer(config, playerMaterial);
        GameObject ghostPrefab = CreateGhostPrefab(config, ghostMaterial);
        CreateManagers(config, ghostPrefab);
        CreateCanvas(config, player);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        Debug.Log("Ghost Hunter greybox rebuild complete. Open Assets/Scenes/Game and press Play.");
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    static Material CreateMaterial(string path, Color color)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        return material;
    }

    static void CreateCamera()
    {
        GameObject go = new GameObject("Main Camera", typeof(Camera));
        go.tag = "MainCamera";
        go.transform.SetPositionAndRotation(new Vector3(0, 20, 0), Quaternion.Euler(90, 0, 0));
        Camera camera = go.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 12f;
        camera.backgroundColor = new Color(7f / 255f, 10f / 255f, 18f / 255f);
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;
    }

    static void CreateLight()
    {
        GameObject go = new GameObject("Directional Light", typeof(Light));
        go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        go.GetComponent<Light>().type = LightType.Directional;
        go.GetComponent<Light>().intensity = 1.1f;
    }

    static void CreateArena()
    {
        GameObject arena = GameObject.CreatePrimitive(PrimitiveType.Plane);
        arena.name = "Arena";
        arena.transform.localScale = new Vector3(4f, 1f, 2.2f);
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        material.color = new Color(0.055f, 0.075f, 0.12f);
        arena.GetComponent<Renderer>().sharedMaterial = material;
    }

    static Player CreatePlayer(GameConfig config, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Player";
        go.tag = "Player";
        go.transform.position = new Vector3(0, 0.5f, 0);
        go.transform.localScale = new Vector3(1.4f, 0.7f, 1.4f);
        go.GetComponent<Renderer>().sharedMaterial = material;
        Rigidbody body = go.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
        body.constraints = RigidbodyConstraints.FreezePositionY;
        Player player = go.AddComponent<Player>();
        player.config = config;
        return player;
    }

    static GameObject CreateGhostPrefab(GameConfig config, Material material)
    {
        GameObject ghost = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ghost.name = "Ghost";
        ghost.tag = "Ghost";
        ghost.transform.position = new Vector3(5, 0.5f, 0);
        ghost.transform.localScale = Vector3.one * 2.2f;
        ghost.GetComponent<Renderer>().sharedMaterial = material;
        ghost.GetComponent<SphereCollider>().isTrigger = true;
        Ghost ghostScript = ghost.AddComponent<Ghost>();
        ghostScript.config = config;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(ghost, PrefabPath);
        Object.DestroyImmediate(ghost);
        return prefab;
    }

    static void CreateManagers(GameConfig config, GameObject ghostPrefab)
    {
        GameObject managers = new GameObject("Managers");
        managers.AddComponent<GameFeel>().config = config;
        GameObject gmObject = new GameObject("GameManager");
        gmObject.transform.SetParent(managers.transform);
        gmObject.AddComponent<GameManager>().config = config;
        GameObject spawnerObject = new GameObject("Spawner");
        spawnerObject.transform.SetParent(managers.transform);
        GameObject container = new GameObject("GhostContainer");
        Spawner spawner = spawnerObject.AddComponent<Spawner>();
        spawner.config = config;
        spawner.ghostPrefab = ghostPrefab;
        spawner.ghostContainer = container.transform;
    }

    static void CreateCanvas(GameConfig config, Player player)
    {
        if (Object.FindFirstObjectByType<EventSystem>() == null) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MenuController));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject hud = Panel("HUDPanel", canvasObject.transform, new Color(0, 0, 0, 0));
        TextMeshProUGUI timer = Text("TimerText", hud.transform, "30", 72, new Color(124f / 255f, 1f, 212f / 255f), TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(300, 100));
        TextMeshProUGUI kills = Text("KillsText", hud.transform, "× 0", 48, Color.white, TextAlignmentOptions.Left, new Vector2(0, 1), new Vector2(0, 1), new Vector2(50, -60), new Vector2(300, 90));
        TextMeshProUGUI combo = Text("BestComboText", hud.transform, "0×", 40, new Color(124f / 255f, 1f, 212f / 255f), TextAlignmentOptions.Right, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-50, -60), new Vector2(250, 80));
        Image dash = Bar("DashCooldownBar", hud.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(230, -70), new Color(1f, .45f, .65f));
        Image milestone = Bar("MilestoneProgressBar", hud.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-230, -70), new Color(.9f, .8f, .25f));
        HUD hudScript = hud.AddComponent<HUD>();
        hudScript.config = config; hudScript.timerText = timer; hudScript.killsText = kills; hudScript.bestComboText = combo;
        hudScript.dashCooldownBar = dash; hudScript.milestoneProgressBar = milestone; hudScript.player = player;

        GameObject start = Panel("StartScreen", canvasObject.transform, new Color(7f / 255f, 10f / 255f, 18f / 255f, .92f));
        Text("Title", start.transform, "GHOST HUNTER", 108, new Color(124f / 255f, 1f, 212f / 255f), TextAlignmentOptions.Center, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 130), new Vector2(1000, 150));
        Text("Subtitle", start.transform, "TAP TO DASH THROUGH GHOSTS", 35, Color.white, TextAlignmentOptions.Center, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 30), new Vector2(1000, 70));
        Button play = Button("PlayButton", start.transform, "PLAY", new Vector2(0, -150));

        GameObject over = Panel("GameOverScreen", canvasObject.transform, new Color(7f / 255f, 10f / 255f, 18f / 255f, .92f));
        Text("GameOverTitle", over.transform, "GAME OVER", 100, new Color(1f, .45f, .65f), TextAlignmentOptions.Center, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 150), new Vector2(900, 140));
        TextMeshProUGUI score = Text("FinalScoreText", over.transform, "KILLS: 0", 52, Color.white, TextAlignmentOptions.Center, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 30), new Vector2(600, 100));
        Button restart = Button("RestartButton", over.transform, "RESTART", new Vector2(0, -150));

        MenuController menu = canvasObject.GetComponent<MenuController>();
        menu.startScreen = start; menu.gameOverScreen = over; menu.hudPanel = hud; menu.playButton = play; menu.restartButton = restart; menu.finalScoreText = score;
        hud.SetActive(false); over.SetActive(false);
    }

    static GameObject Panel(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return go;
    }

    static TextMeshProUGUI Text(string name, Transform parent, string value, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max, Vector2 position, Vector2 dimensions)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = min; rect.anchorMax = max; rect.anchoredPosition = position; rect.sizeDelta = dimensions;
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset; text.text = value; text.fontSize = size; text.color = color; text.alignment = alignment;
        return text;
    }

    static Image Bar(string name, Transform parent, Vector2 min, Vector2 max, Vector2 position, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = min; rect.anchorMax = max; rect.anchoredPosition = position; rect.sizeDelta = new Vector2(170, 18);
        Image image = go.GetComponent<Image>(); image.type = Image.Type.Filled; image.fillMethod = Image.FillMethod.Horizontal; image.color = color;
        return image;
    }

    static Button Button(string name, Transform parent, string label, Vector2 position)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(300, 100);
        Image image = go.GetComponent<Image>(); image.color = new Color(124f / 255f, 1f, 212f / 255f);
        Text("Label", go.transform, label, 42, new Color(.03f, .08f, .10f), TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return go.GetComponent<Button>();
    }
}
