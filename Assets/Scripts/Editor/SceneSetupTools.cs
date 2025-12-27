using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SceneSetupTools : EditorWindow
{
    [MenuItem("Hurma/Auto Setup Scene")]
    public static void SetupScene()
    {
        // 1. Create Managers
        GameObject managers = GameObject.Find("Managers");
        if (managers == null)
        {
            managers = new GameObject("Managers");
            Undo.RegisterCreatedObjectUndo(managers, "Create Managers");
        }

        if (managers.GetComponent<TimeManager>() == null) managers.AddComponent<TimeManager>();
        if (managers.GetComponent<GameManager>() == null) managers.AddComponent<GameManager>();
        if (managers.GetComponent<TimeManager>() == null) managers.AddComponent<TimeManager>();
        if (managers.GetComponent<GameManager>() == null) managers.AddComponent<GameManager>();
        if (managers.GetComponent<NarrativeManager>() == null) managers.AddComponent<NarrativeManager>();
        


        // 2. Create Tree Prefab
        string prefabPath = "Assets/Prefabs/TreePlaceholder.prefab";
        
        // FORCE DELETE old prefab to ensure new Art Assets are applied
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }

        GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (treePrefab == null)
        {
            // Create a container
            GameObject tempTree = new GameObject("TreeContainer");
            
            // Add component
            if (tempTree.GetComponent<HurmaTree>() == null) tempTree.AddComponent<HurmaTree>();
            
            // Assign Models if they exist
            HurmaTree treeScript = tempTree.GetComponent<HurmaTree>();
            
            // Automation:
            AddChildPrefab(tempTree, "Assets/Prefabs/SeedPrefab.prefab", ref treeScript.SeedModel);
            AddChildPrefab(tempTree, "Assets/Prefabs/SaplingPrefab.prefab", ref treeScript.SaplingModel);
            AddChildPrefab(tempTree, "Assets/Prefabs/YoungTreePrefab.prefab", ref treeScript.YoungTreeModel);
            AddChildPrefab(tempTree, "Assets/Prefabs/MatureTreePrefab.prefab", ref treeScript.MatureTreeModel);
            
            // Ensure Prefabs folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            // Save as Prefab
            treePrefab = PrefabUtility.SaveAsPrefabAsset(tempTree, prefabPath);
            GameObject.DestroyImmediate(tempTree);
            Debug.Log("Created Tree Placeholder Prefab.");
        }

        // 3. Create Soil
        GameObject soil = GameObject.Find("Soil");
        if (soil != null)
        {
            // Check if it's the old cube, if so destroy it to replace
            MeshFilter mf = soil.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && mf.sharedMesh.name == "Cube")
            {
                DestroyImmediate(soil);
                soil = null;
            }
        }

        if (soil == null)
        {
            GameObject soilPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SoilPrefab.prefab");
            if (soilPrefab != null)
            {
                soil = (GameObject)PrefabUtility.InstantiatePrefab(soilPrefab);
                soil.name = "Soil";
                soil.transform.position = Vector3.zero;
            }
            else
            {
                // Fallback
                soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
                soil.name = "Soil";
                soil.transform.position = Vector3.zero;
            }
            Undo.RegisterCreatedObjectUndo(soil, "Create Soil");
        }
        
        // Ensure it has a collider (Prefabs made from primitives keeping colloiders inside children might be tricky for mouse events on parent)
        // Let's add a BoxCollider to the root Soil object if it doesn't have one
        if (soil.GetComponent<Collider>() == null)
        {
             BoxCollider bc = soil.AddComponent<BoxCollider>();
             bc.center = new Vector3(0, 0.15f, 0);
             bc.size = new Vector3(1.5f, 0.3f, 1.5f);
        }

        Soil soilScript = soil.GetComponent<Soil>();
        if (soilScript == null) soilScript = soil.AddComponent<Soil>();

        // Link the prefab directly
        soilScript.TreePrefab = treePrefab;
        EditorUtility.SetDirty(soilScript);

        Debug.Log("Scene Setup Complete! Press Play to test.");
        
        // 4. Auto Setup UI
        UISetupTools.CreateUI();
        
        // 5. HARD LINK REFERENCES (To prevent "Start" finding issues)
        if (managers.GetComponent<UIManager>() == null) managers.AddComponent<UIManager>();
        UIManager uiMgr = managers.GetComponent<UIManager>();
        
        // Find UI elements we just created
        GameObject canvas = GameObject.Find("MainCanvas");
        if (canvas != null)
        {
             // Helper local function to find deep child
             Transform FindDeep(Transform parent, string name) {
                 foreach(Transform child in parent) {
                     if(child.name == name) return child;
                     var result = FindDeep(child, name);
                     if (result != null) return result;
                 }
                 return null;
             }
             
             Transform btnTr = FindDeep(canvas.transform, "NextDayButton");
             if (btnTr != null) uiMgr.NextDayButton = btnTr.GetComponent<Button>();
             
             Transform sliderTr = FindDeep(canvas.transform, "SpeedSlider");
             if (sliderTr != null) uiMgr.TimeSpeedSlider = sliderTr.GetComponent<Slider>();
             
             Transform hud = FindDeep(canvas.transform, "HUD");
             if (hud != null)
             {
                 uiMgr.DayText = FindDeep(hud, "DayText")?.GetComponent<Text>();
                 uiMgr.MoonText = FindDeep(hud, "MoonText")?.GetComponent<Text>();
                 uiMgr.SoilText = FindDeep(hud, "SoilText")?.GetComponent<Text>();
             }
             
             Transform wisdom = FindDeep(canvas.transform, "WisdomPanel");
             if (wisdom != null)
             {
                 uiMgr.WisdomPanel = wisdom.gameObject;
                 uiMgr.QuoteText = FindDeep(wisdom, "QuoteText")?.GetComponent<Text>();
             }
             
             EditorUtility.SetDirty(uiMgr);
        }
    }
    
    private static void AddChildPrefab(GameObject parent, string path, ref GameObject fieldToAssign)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            GameObject child = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            child.SetActive(false); // Hide by default
            fieldToAssign = child;
        }
    }
}
