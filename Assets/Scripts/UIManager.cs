using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD References")]
    public Text DayText;
    public Text MoonText;
    public Text SoilText;

    [Header("Wisdom References")]
    public GameObject WisdomPanel;
    public Text QuoteText;

    [Header("Controls")]
    public Slider TimeSpeedSlider;
    public Button NextDayButton;

    private float quoteTimer = 0f;
    private Soil activeSoil;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Auto-find references logic
        InitializeReferences();
        
        // Cache Soil - Find the one in the scene properly
        GameObject soilObj = GameObject.Find("Soil");
        if (soilObj != null) activeSoil = soilObj.GetComponent<Soil>();
        if (activeSoil == null) activeSoil = FindFirstObjectByType<Soil>(); // Fallback
        
        // Setup Listeners
        if (NextDayButton != null) NextDayButton.onClick.AddListener(OnNextDayClicked);
        if (TimeSpeedSlider != null) 
        {
             TimeSpeedSlider.onValueChanged.AddListener(OnSpeedChanged);
             // Set initial slider value to current time scale
             TimeSpeedSlider.value = Time.timeScale;
        }

        // Initial State
        if (WisdomPanel) WisdomPanel.SetActive(false);
    }

    private void InitializeReferences()
    {
        if (DayText == null) DayText = GameFinder.FindComponent<Text>("DayText");
        if (MoonText == null) MoonText = GameFinder.FindComponent<Text>("MoonText");
        if (SoilText == null) SoilText = GameFinder.FindComponent<Text>("SoilText");
        
        if (WisdomPanel == null) WisdomPanel = GameObject.Find("WisdomPanel");
        if (WisdomPanel != null && QuoteText == null) QuoteText = WisdomPanel.GetComponentInChildren<Text>();

        if (NextDayButton == null) NextDayButton = GameFinder.FindComponent<Button>("NextDayButton");
        if (TimeSpeedSlider == null) TimeSpeedSlider = GameFinder.FindComponent<Slider>("SpeedSlider");
        
        if (InventoryText == null) InventoryText = GameFinder.FindComponent<Text>("InventoryText"); 
        // If not found, create one? For now just try to find.
        if (InventoryText == null)
        {
             // Fallback: Try finding by tag or just type if unique
             InventoryText = GameFinder.FindComponent<Text>("RizqText");
        }
    }

    private void OnNextDayClicked()
    {
        Debug.Log("Next Day Button Clicked!");
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.CurrentDay++;
            TimeManager.Instance.TimeOfDay = 0.28f; // Reset to morning
        }
        UpdateHUD();
    }

    private void OnSpeedChanged(float val)
    {
        Time.timeScale = val;
    }

    private void Update()
    {
        UpdateHUD();
        UpdateInventoryDisplay(); // Keep UI fresh
        UpdateQuoteTimer();
    }

    private void UpdateHUD()
    {
        if (TimeManager.Instance != null && DayText != null)
        {
            DayText.text = $"Day: {TimeManager.Instance.CurrentDay}";
            if (MoonText != null) MoonText.text = $"Moon: {TimeManager.Instance.CurrentMoonPhase}";
        }

        if (SoilText != null && activeSoil != null)
        {
             SoilText.text = activeSoil.IsOccupied 
                 ? (activeSoil.PlantedTree != null ? $"Status: {activeSoil.PlantedTree.CurrentStage}" : "Status: Seeded") 
                 : "Status: Empty (Click Soil!)";
        }
        
        // Stewardship / Inventory Display
        // Assuming we might have a text for this, or reuse an existing unused one, or creating a dynamic one.
        // For now, let's log it or display it if we had a dedicated text. 
        // NOTE: The user's scene structure might not have this extra text field yet.
        // I will add a method to dynamically find or we assume the user will set it up.
        // Let's add resilience.
        
        if (GameManager.Instance != null && SoilText != null)
        {
             // Temporary: Append to SoilText or use a new logic if finding components
             // Better: Create a new Text variable in UIManager script for "InventoryText"
        }
    }
    
    // Adding field first, then using it
    [Header("Stewardship")]
    public Text InventoryText; // User needs to link this, or we find it.

    public void UpdateInventoryDisplay()
    {
        if (InventoryText != null && GameManager.Instance != null)
        {
            InventoryText.text = $"Rızık: {GameManager.Instance.DatesHarvested} Hurma";
        }
    }

    public void ShowWisdom(string quote, float duration)
    {
        if (WisdomPanel != null && QuoteText != null)
        {
            WisdomPanel.SetActive(true);
            QuoteText.text = quote;
            quoteTimer = duration;
        }
    }

    private void UpdateQuoteTimer()
    {
        if (quoteTimer > 0)
        {
            quoteTimer -= Time.deltaTime;
            if (quoteTimer <= 0)
            {
                if (WisdomPanel != null) WisdomPanel.SetActive(false);
            }
        }
    }
}
