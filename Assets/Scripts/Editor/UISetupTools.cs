using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class UISetupTools : EditorWindow
{
    [MenuItem("Hurma/Setup UI")]
    public static void CreateUI()
    {
        // 1. Create Canvas
        GameObject canvasObj = GameObject.Find("MainCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("MainCanvas");
            Canvas c = canvasObj.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }
        else
        {
            // Ensure scaler settings are correct even if canvas exists
             CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
             if (scaler != null)
             {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
             }
        }

        // 2. Create HUD (Top Left Stats)
        CreateHUD(canvasObj.transform);

        // 3. Create Wisdom Panel (Center/Bottom)
        CreateWisdomPanel(canvasObj.transform);

        // 4. Create Controls Panel (Top Right)
        CreateControlsPanel(canvasObj.transform);

        // 5. Setup Event System (Critical for Input)
        SetupEventSystem();

        Debug.Log("UI Setup Complete!");
    }

    private static void SetupEventSystem()
    {
        UnityEngine.EventSystems.EventSystem es = UnityEngine.Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        
        if (es == null)
        {
             GameObject esObj = new GameObject("EventSystem");
             es = esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
             Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
        }

        // Check for old Input Module
        var oldModule = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (oldModule != null) DestroyImmediate(oldModule);

        // Ensure New Input Module
        var newModule = es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        if (newModule == null)
        {
            es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    private static void CreateControlsPanel(Transform parent)
    {
        GameObject panel = FindChild(parent, "ControlsPanel");
        if (panel != null) DestroyImmediate(panel);

        // Pivot Top-Right (1, 1)
        panel = CreatePanel(parent, "ControlsPanel", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(250, 100), new Vector2(1, 1));
        
        // Semi-transparent background (Dark to see white text)
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f); 

        // Next Day Button
        GameObject btnObj = CreateButton(panel.transform, "NextDayButton", "Next Day (+1)", new Vector2(0, -10), new Vector2(200, 30));
        
        // Speed Slider
        CreateSlider(panel.transform, "SpeedSlider", new Vector2(0, -50), new Vector2(200, 20));
        CreateText(panel.transform, "SpeedLabel", "Game Speed", new Vector2(0, -75), 14, Color.white);
    }
    
    private static void CreateHUD(Transform parent)
    {
        GameObject hud = FindChild(parent, "HUD");
        if (hud != null) DestroyImmediate(hud);

        // Pivot Top-Left (0, 1)
        hud = CreatePanel(parent, "HUD", new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(300, 150), new Vector2(0, 1));
        // Semi-transparent background
        hud.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        
        // Stats Text
        CreateText(hud.transform, "DayText", "Day: 1", new Vector2(10, -10), 20, Color.white);
        CreateText(hud.transform, "MoonText", "Moon: New", new Vector2(10, -40), 18, Color.yellow);
        CreateText(hud.transform, "SoilText", "Soil: Empty", new Vector2(10, -70), 18, Color.green);
    }

    private static void CreateWisdomPanel(Transform parent)
    {
        GameObject panel = FindChild(parent, "WisdomPanel");
        if (panel != null) DestroyImmediate(panel);

        // Center bottom, Pivot (0.5, 0)
        panel = CreatePanel(parent, "WisdomPanel", new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), Vector2.zero, new Vector2(800, 150), new Vector2(0.5f, 0));
        Image img = panel.GetComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Dark bg
            
            // Decorative Border (optional, simple outline)
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.8f, 0.6f, 0.2f); // Gold
            outline.effectDistance = new Vector2(3, 3);

            // Quote Text
            GameObject txtObj = CreateText(panel.transform, "QuoteText", "Bismillahirrahmanirrahim", new Vector2(0, 0), 24, Color.white);
            Text txt = txtObj.GetComponent<Text>();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = new Vector2(20, 20);
            txt.rectTransform.offsetMax = new Vector2(-20, -20);
            
            // Hide initially? Or logic handles it.
            // panel.SetActive(false);

    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Vector2 pivot)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        
        Image img = panel.AddComponent<Image>();
        // Assign default background sprite so color tint works properly
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        img.type = Image.Type.Sliced;
        
        return panel;
    }

    private static GameObject CreateText(Transform parent, string name, string content, Vector2 pos, int fontSize, Color color)
    {
        GameObject txtObj = new GameObject(name);
        txtObj.transform.SetParent(parent, false);
        
        RectTransform rt = txtObj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(280, 40); // Default width
        
        Text txt = txtObj.AddComponent<Text>();
        txt.text = content;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.color = color;
        
        return txtObj;
    }

    private static GameObject CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        
        Image img = btnObj.AddComponent<Image>();
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        
        Button btn = btnObj.AddComponent<Button>();
        
        // Label
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        
        Text txt = textObj.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Fixed font
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.black;
        
        return btnObj;
    }

    private static GameObject CreateSlider(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        // Standard UI Slider structure: Root -> Background -> Fill Area -> Fill, Handle Slide Area -> Handle
        // Using built-in creation is hard from script without prefabs.
        // Let's make a simplified one or try to load default.
        // Actually, simplest is to create a GameObject and add Slider, but it needs sub-objects to work visually.
        // For this prototype, let's look for a default resource or build minimal.
        
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        
        Slider slider = root.AddComponent<Slider>();
        
        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(root.transform, false);
        RectTransform bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.25f);
        bgRt.anchorMax = new Vector2(1, 0.75f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.gray;
        slider.targetGraphic = bgImg; // Just to have something
        
        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(root.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1, 0.75f);
        fillAreaRt.offsetMin = new Vector2(5, 0); // Padding
        fillAreaRt.offsetMax = new Vector2(-5, 0);
        
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.sizeDelta = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.yellow;
        
        slider.fillRect = fillRt;
        
        // Logic
        slider.minValue = 1;
        slider.maxValue = 20;
        slider.value = 1;
        
        return root;
    }

    private static GameObject FindChild(Transform parent, string name)
    {
        foreach(Transform t in parent)
        {
            if (t.name == name) return t.gameObject;
        }
        return null;
    }
}
