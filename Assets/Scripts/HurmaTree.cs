using UnityEngine;

public class HurmaTree : MonoBehaviour
{
    public enum GrowthStage
    {
        Seed,
        Sapling,
        YoungTree,
        MatureTree,
        Flowering,
        Fruiting
    }

    [Header("State")]
    public GrowthStage CurrentStage = GrowthStage.Seed;
    public float AgeInDays = 0f;
    public float WaterLevel = 50f; // 0-100
    public float NitrogenLevel = 50f; // 0-100
    public int FruitCount = 0;

    [Header("Settings")]
    public float DaysToSapling = 1f;
    public float DaysToYoung = 2f;
    public float DaysToMature = 3f;
    public float WaterConsumptionPerDay = 1f; // Reduced from 10f to be more forgiving

    [Header("Visuals")]
    public GameObject SeedModel;
    public GameObject SaplingModel;
    public GameObject YoungTreeModel;
    public GameObject MatureTreeModel;

    [Header("Visual Effects")]
    public float WindStrength = 1.5f;
    public float WindSpeed = 1.0f;
    private Quaternion initialRotation;

    [Header("Audio")]
    public AudioClip GrowthSound;
    public AudioClip RustleSound;
    public AudioClip HarvestSound;

    private float rustleTimer = 0f;
    private float nextRustleTime = 10f;

    private void Start()
    {
        // FORCE TEST SETTINGS (Overrides Inspector)
        DaysToSapling = 1f;
        DaysToYoung = 2f;
        DaysToMature = 3f;
        WaterConsumptionPerDay = 0f; // Disable thirst for testing
        
        Debug.Log($"[FAST TEST MODE] Growth Days: {DaysToSapling}/{DaysToYoung}/{DaysToMature}, Water Cons: {WaterConsumptionPerDay}");

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayChanged += HandleDayChanged;
        }
        UpdateVisuals(true); // Instant on start
        initialRotation = transform.rotation;
        nextRustleTime = Random.Range(10f, 30f);
        
        // Auto-load Resources if missing
        if (RustleSound == null) RustleSound = Resources.Load<AudioClip>("Audio/MountainWind");
        if (HarvestSound == null) HarvestSound = Resources.Load<AudioClip>("Audio/HarvestJoy");
    }

    private void Update()
    {
        // Gentle Swaying (Zikr / Wind)
        if (CurrentStage >= GrowthStage.Sapling)
        {
            float noise = Mathf.PerlinNoise(Time.time * WindSpeed, 0f) * 2f - 1f; // -1 to 1
            float swayAngle = noise * WindStrength;
            
            transform.rotation = initialRotation * Quaternion.Euler(swayAngle * 0.5f, 0f, swayAngle);
            
            // Random Rustle (Zikr of nature)
            rustleTimer += Time.deltaTime;
            if (rustleTimer >= nextRustleTime)
            {
                if (GameAudioManager.Instance != null && RustleSound != null)
                {
                    GameAudioManager.Instance.PlaySFX(RustleSound, 0.3f); // Very quiet
                }
                rustleTimer = 0f;
                nextRustleTime = Random.Range(15f, 40f);
            }
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayChanged -= HandleDayChanged;
        }
    }

    private void HandleDayChanged(int day)
    {
        AgeInDays++;
        ConsumeResources();
        CheckGrowth();
    }

    public void Water(float amount)
    {
        WaterLevel = Mathf.Clamp(WaterLevel + amount, 0f, 100f);
        Debug.Log($"Watered tree. Current Level: {WaterLevel}");
    }

    private void ConsumeResources()
    {
        WaterLevel -= WaterConsumptionPerDay;
        if (WaterLevel < 0) WaterLevel = 0;
    }

    private void CheckGrowth()
    {
        if (WaterLevel <= 0) 
        {
            Debug.LogWarning("Tree is thirsty! (Susuzluktan büyüyemiyor)");
            return; // Stunted growth if no water
        }

        // Simple growth logic
        switch (CurrentStage)
        {
            case GrowthStage.Seed:
                if (AgeInDays >= DaysToSapling) AdvanceStage(GrowthStage.Sapling);
                break;
            case GrowthStage.Sapling:
                if (AgeInDays >= DaysToYoung) AdvanceStage(GrowthStage.YoungTree);
                break;
            case GrowthStage.YoungTree:
                if (AgeInDays >= DaysToMature) AdvanceStage(GrowthStage.MatureTree);
                break;
            case GrowthStage.MatureTree:
                // Check season/moon for flowering
                // FAST MODE: Bypass Moon Check if DaysToMature is small
                if (DaysToMature <= 3 || TimeManager.Instance.CurrentMoonPhase == TimeManager.MoonPhase.NewMoon)
                {
                    AdvanceStage(GrowthStage.Flowering);
                }
                break;
            case GrowthStage.Flowering:
                // Flower until Full Moon (approx 14 days), then bear fruit
                // FAST MODE: Bypass Moon Check if DaysToMature is small (wait only 1 day)
                if (DaysToMature <= 3 || TimeManager.Instance.CurrentMoonPhase == TimeManager.MoonPhase.FullMoon)
                {
                    FruitCount = Random.Range(3, 10); // Rizq is varying
                    AdvanceStage(GrowthStage.Fruiting);
                    Debug.Log($"Tree is now Fruiting with {FruitCount} dates.");
                }
                break;
        }
    }

    private void AdvanceStage(GrowthStage nextStage)
    {
        CurrentStage = nextStage;
        Debug.Log($"Hurma Tree grew to: {CurrentStage}");
        UpdateVisuals(false); // Smooth transition
        
        if (GameAudioManager.Instance != null && GrowthSound != null)
        {
            GameAudioManager.Instance.PlaySFX(GrowthSound, 0.6f);
        }
        
        if (NarrativeManager.Instance != null)
        {
            NarrativeManager.Instance.OnTreeStageChanged(nextStage);
        }
    }

    private void UpdateVisuals(bool instant = true)
    {
        // Logic to hide all and show current
        // But for smooth transition, we might fade in/out or scale up.
        // For prototype simplicity, let's cross-fade logic or simpler scale logic.
        
        StartCoroutine(TransitionToStage(CurrentStage, instant));
    }
    
    private System.Collections.IEnumerator TransitionToStage(GrowthStage stage, bool instant)
    {
        GameObject newModel = GetModelForStage(stage);
        GameObject oldModel = GetActiveModel();

        if (newModel == oldModel) yield break;

        if (instant)
        {
            if(oldModel) oldModel.SetActive(false);
            if(newModel) newModel.SetActive(true);
        }
        else
        {
            // Calm Transition: Wait a moment, then gentle scale up?
            // Or just appear and standard sway picks up.
            // Let's do: Scale Up New Model from 0
            
            if (newModel)
            {
                newModel.SetActive(true);
                newModel.transform.localScale = Vector3.zero;
                
                float timer = 0f;
                float duration = 2.0f; // Slow, contemplative growth
                
                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    float t = timer / duration;
                    // EaseOutQuad
                    t = t * (2 - t);
                    
                    newModel.transform.localScale = Vector3.one * t;
                    yield return null;
                }
                newModel.transform.localScale = Vector3.one;
            }
            
            if (oldModel) oldModel.SetActive(false);
        }
    }
    
    private GameObject GetActiveModel()
    {
        if (SeedModel && SeedModel.activeSelf) return SeedModel;
        if (SaplingModel && SaplingModel.activeSelf) return SaplingModel;
        if (YoungTreeModel && YoungTreeModel.activeSelf) return YoungTreeModel;
        if (MatureTreeModel && MatureTreeModel.activeSelf) return MatureTreeModel;
        return null;
    }

    private GameObject GetModelForStage(GrowthStage stage)
    {
        if (stage == GrowthStage.Seed) return SeedModel;
        if (stage == GrowthStage.Sapling) return SaplingModel;
        if (stage == GrowthStage.YoungTree) return YoungTreeModel;
        if (stage >= GrowthStage.MatureTree) return MatureTreeModel; 
        // Note: Flowering/Fruiting usually modify Mature model or are child objects.
        // For simplicity, we keep using MatureModel but text changes.
        return MatureTreeModel;
    }
    
    // Interaction for harvesting
    public void Harvest()
    {
        if (CurrentStage == GrowthStage.Fruiting)
        {
            Debug.Log($"Harvested {FruitCount} dates!");
            
            if (GameAudioManager.Instance != null && HarvestSound != null)
            {
                GameAudioManager.Instance.PlaySFX(HarvestSound, 0.7f);
            }
            
            // Add to Stewardship (Inventory)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddDates(FruitCount);
            }
            
            FruitCount = 0;
            CurrentStage = GrowthStage.MatureTree; // Reset cycle
        }
    }

    public TreeData GetTreeData()
    {
        TreeData data = new TreeData();
        data.StageName = CurrentStage.ToString();
        data.AgeInDays = AgeInDays;
        data.WaterLevel = WaterLevel;
        data.FruitCount = FruitCount;
        return data;
    }

    public void LoadTreeData(TreeData data)
    {
        if (System.Enum.TryParse(data.StageName, out GrowthStage stage))
        {
            CurrentStage = stage;
        }
        else
        {
            CurrentStage = GrowthStage.Seed;
        }

        AgeInDays = data.AgeInDays;
        WaterLevel = data.WaterLevel;
        FruitCount = data.FruitCount;
        
        UpdateVisuals(true); // Instant update on load
        Debug.Log("Tree Data Loaded.");
    }
}
