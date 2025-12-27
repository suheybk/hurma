using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string saveFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "hurma_save.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // 1. Collect Time Data
        if (TimeManager.Instance != null)
        {
            data.CurrentDay = TimeManager.Instance.CurrentDay;
            data.TimeOfDay = TimeManager.Instance.TimeOfDay;
        }

        // 2. Collect Game/Stewardship Data
        if (GameManager.Instance != null)
        {
            data.DatesHarvested = GameManager.Instance.DatesHarvested;
        }

        // 3. Collect Tree Data
        // For simplicity in this prototype, we find the active tree in the scene.
        // In a complex game, we'd have a list of trees.
        HurmaTree activeTree = FindFirstObjectByType<HurmaTree>();
        if (activeTree != null)
        {
            data.TreeState = activeTree.GetTreeData();
            data.TreeState.IsPlanted = true;
        }
        else
        {
            // If no tree script is found, check if Soil has one? 
            // Or assume not planted if not active.
            data.TreeState = new TreeData(); 
            data.TreeState.IsPlanted = false;
            
            // Check Soil specifically if it has a reference but maybe disabled?
            // Usually tree object is instantiated. If null, it's not there.
            Soil soil = FindFirstObjectByType<Soil>();
            if (soil != null && soil.PlantedTree != null)
            {
                 data.TreeState = soil.PlantedTree.GetTreeData();
                 data.TreeState.IsPlanted = true;
            }
        }

        // Write to file
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Game Saved to {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("No save file found.");
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // 1. Restore Time
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.LoadTimeData(data.CurrentDay, data.TimeOfDay);
            }

            // 2. Restore Stewardship
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadStewardshipData(data.DatesHarvested);
            }

            // 3. Restore Tree
            // Needs to handle if tree exists or needs planting
            if (data.TreeState != null && data.TreeState.IsPlanted)
            {
                Soil soil = FindFirstObjectByType<Soil>();
                if (soil != null)
                {
                    // If not occupied, plant it first
                    if (!soil.IsOccupied)
                    {
                        soil.PlantSeed(true); // true = silent/loading mode
                    }
                    
                    if (soil.PlantedTree != null)
                    {
                        soil.PlantedTree.LoadTreeData(data.TreeState);
                    }
                }
            }

            Debug.Log("Game Loaded Successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
    
    // Optional: Save on pause/focus lost (mobile friendly)
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGame();
        }
    }
}
