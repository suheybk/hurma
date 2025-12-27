using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InventoryUISetup : MonoBehaviour
{
    // This script can be run to ensure UI is up to date
    public static void CreateInventoryUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        GameObject existingText = GameObject.Find("InventoryText");
        if (existingText != null)
        {
            Debug.Log("Inventory Text already exists.");
            return;
        }

        // Create Text Object
        GameObject textObj = new GameObject("InventoryText");
        textObj.transform.SetParent(canvas.transform, false);
        
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 20;
        text.color = new Color(0.2f, 0.4f, 0.2f); // Earthy Green
        text.alignment = TextAnchor.MiddleRight;
        
        // Positioning (Top Right, slightly below Moon text)
        RectTransform rt = text.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -100); // Below status area
        rt.sizeDelta = new Vector2(300, 40);

        Debug.Log("Created Inventory UI Text.");
    }
}
