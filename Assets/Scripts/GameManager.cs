using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Stewardship Data (formerly 'DatesCollected')
    public int DatesHarvested = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load game on start
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
        }
    }

    public void AddDates(int amount)
    {
        DatesHarvested += amount;
        Debug.Log($"Rizq Harvested: {DatesHarvested}");
        
        // Auto-save after harvest
        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
    }

    public void LoadStewardshipData(int harvested)
    {
        DatesHarvested = harvested;
        Debug.Log($"Stewardship Loaded: {DatesHarvested} Dates");
    }
}
