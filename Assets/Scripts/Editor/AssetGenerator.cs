using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetGenerator : EditorWindow
{
    [MenuItem("Hurma/Generate Art Assets")]
    public static void GenerateAssets()
    {
        EnsureFoldersExist();

        // 1. Create Materials
        Material sandMat = CreateMaterial("SandMat", new Color(0.9f, 0.8f, 0.6f)); // Beige
        Material barkMat = CreateMaterial("BarkMat", new Color(0.4f, 0.25f, 0.1f)); // Brown
        Material leafMat = CreateMaterial("LeafMat", new Color(0.2f, 0.6f, 0.1f)); // Green
        Material fruitMat = CreateMaterial("FruitMat", new Color(0.3f, 0.1f, 0.05f)); // Dark Date Color

        // 2. Create Tree Prefabs
        CreateSeedPrefab(barkMat); // Use bark mat for seed for now
        CreateSaplingPrefab(leafMat);
        CreateYoungTreePrefab(barkMat, leafMat);
        CreateMatureTreePrefab(barkMat, leafMat, fruitMat);
        CreateSoilPrefab(sandMat);

        // 3. Update Soil Material
        GameObject soil = GameObject.Find("Soil");
        if (soil != null)
        {
            // The renderer might be on the child if it's our new mound prefab
            Renderer r = soil.GetComponent<Renderer>();
            if (r == null) r = soil.GetComponentInChildren<Renderer>();
            
            if (r != null) r.sharedMaterial = sandMat;
        }

        Debug.Log("Art Assets Generated Successfully!");
    }

    private static void EnsureFoldersExist()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); // Default URP Shader
            if (mat.shader == null) mat = new Material(Shader.Find("Standard")); // Fallback
            mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.color = color;
        }
        return mat;
    }

    private static void CreateSaplingPrefab(Material leafMat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "SaplingModel";
        go.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
        go.GetComponent<Renderer>().sharedMaterial = leafMat;
        
        SaveAsPrefab("SaplingPrefab", go);
        DestroyImmediate(go);
    }

    private static void CreateYoungTreePrefab(Material barkMat, Material leafMat)
    {
        GameObject root = new GameObject("YoungTreeModel");
        
        // Trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.parent = root.transform;
        trunk.transform.localPosition = new Vector3(0, 1f, 0);
        trunk.transform.localScale = new Vector3(0.3f, 1f, 0.3f);
        trunk.GetComponent<Renderer>().sharedMaterial = barkMat;

        // Leaves
        GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaves.transform.parent = root.transform;
        leaves.transform.localPosition = new Vector3(0, 2f, 0);
        leaves.transform.localScale = new Vector3(1.2f, 0.8f, 1.2f);
        leaves.GetComponent<Renderer>().sharedMaterial = leafMat;

        SaveAsPrefab("YoungTreePrefab", root);
        DestroyImmediate(root);
    }

    private static void CreateMatureTreePrefab(Material barkMat, Material leafMat, Material fruitMat)
    {
        GameObject root = new GameObject("MatureTreeModel");
        
        // Trunk - Taller
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.parent = root.transform;
        trunk.transform.localPosition = new Vector3(0, 2f, 0);
        trunk.transform.localScale = new Vector3(0.5f, 2f, 0.5f);
        trunk.GetComponent<Renderer>().sharedMaterial = barkMat;

        // Canopy
        for(int i=0; i<5; i++)
        {
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leaf.transform.parent = root.transform;
            leaf.transform.localPosition = new Vector3(0, 4f, 0) + (Quaternion.Euler(0, i * 72, 0) * new Vector3(1f, 0, 0));
            leaf.transform.localRotation = Quaternion.Euler(0, i * 72, 30);
            leaf.transform.localScale = new Vector3(1.5f, 0.2f, 0.5f);
            leaf.GetComponent<Renderer>().sharedMaterial = leafMat;
        }

        // Fruits (Hidden by default, can be toggled via script)
        GameObject fruits = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fruits.name = "Fruits";
        fruits.transform.parent = root.transform;
        fruits.transform.localPosition = new Vector3(0, 3.8f, 0.3f);
        fruits.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        fruits.GetComponent<Renderer>().sharedMaterial = fruitMat;

        SaveAsPrefab("MatureTreePrefab", root);
        DestroyImmediate(root);
    }

    private static void SaveAsPrefab(string name, GameObject go)
    {
        string path = $"Assets/Prefabs/{name}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
    }

    private static void CreateSoilPrefab(Material sandMat)
    {
        // Create a mound using a flattened Sphere
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "SoilModel";
        // Flatten it to look like a heap of sand
        go.transform.localScale = new Vector3(1.5f, 0.3f, 1.5f);
        
        // Offset pivot adjustment (hacky: put it in a parent)
        GameObject root = new GameObject("SoilPrefab");
        go.transform.parent = root.transform;
        go.transform.localPosition = new Vector3(0, 0.15f, 0); // Move up so pivot is at bottom
        
        go.GetComponent<Renderer>().sharedMaterial = sandMat;

        // Remove collider from visual model so it doesn't interfere (Soil parent has the box collider)
        // actually, if we keep it, our new Soil.cs RaycastChildOf logic works fine.
        // But for cleanliness:
        GameObject c = go.GetComponent<Collider>().gameObject; // Primitive has collider on itself
        DestroyImmediate(go.GetComponent<Collider>()); 

        SaveAsPrefab("SoilPrefab", root);
        DestroyImmediate(root);
    }
    private static void CreateSeedPrefab(Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "SeedModel";
        go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); // Tiny seed
        go.GetComponent<Renderer>().sharedMaterial = mat;
        
        DestroyImmediate(go.GetComponent<Collider>()); // No collider on visual seed

        SaveAsPrefab("SeedPrefab", go);
        DestroyImmediate(go);
    }
}
