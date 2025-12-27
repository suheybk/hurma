using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Duration of a full day in real-time seconds")]
    [SerializeField] private float dayDurationInSeconds = 2f;
    
    [Header("State")]
    public int CurrentDay = 1;
    [Range(0f, 1f)]
    public float TimeOfDay = 0.28f; // Start at morning (Light visible)
    
    [Header("Audio")]
    public AudioClip DayAmbience;
    public AudioClip NightAmbience;
    private bool isNightAudio = false;

    public event Action<int> OnDayChanged;
    public event Action<MoonPhase> OnMoonPhaseChanged;

    public MoonPhase CurrentMoonPhase { get; private set; }

    public enum MoonPhase
    {
        NewMoon = 0,
        WaxingCrescent,
        FirstQuarter,
        WaxingGibbous,
        FullMoon,
        WaningGibbous,
        LastQuarter,
        WaningCrescent
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateMoonPhase();
        // Auto-load Resources
        DayAmbience = Resources.Load<AudioClip>("Audio/DesertWind");
        NightAmbience = Resources.Load<AudioClip>("Audio/NightCrickets");
        
        // Initial Audio
        UpdateAmbience();
    }

    private void Update()
    {
        // Advance time
        TimeOfDay += (Time.deltaTime / dayDurationInSeconds);

        if (TimeOfDay >= 1f)
        {
            TimeOfDay = 0f;
            CurrentDay++;
            UpdateMoonPhase();
            OnDayChanged?.Invoke(CurrentDay);
            
            // Auto-save on new day
            if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
        }
        
        // Update Sun/Moon Rotation
        Light sun = FindFirstObjectByType<Light>(); // Hacky but works for prototype
        if (sun != null && sun.type == LightType.Directional)
        {
            float angle = (TimeOfDay * 360f) - 90f;

            sun.transform.rotation = Quaternion.Euler(angle, 170f, 0f);
        }
        
        UpdateAmbience();
    }
    
    private void UpdateAmbience()
    {
        // Simple day/night check for audio
        // Try to match visual night
        bool night = (TimeOfDay > 0.75f || TimeOfDay < 0.25f); // Rough estimate
        
        if (night != isNightAudio)
        {
            isNightAudio = night;
            // Play if GameAudioManager exists
            if (GameAudioManager.Instance != null)
            {
                GameAudioManager.Instance.PlayAmbience(isNightAudio ? NightAmbience : DayAmbience, 0.4f);
            }
        }
    }

    private void UpdateMoonPhase()
    {
        // Simple 29-day lunar cycle
        int cycleDay = (CurrentDay - 1) % 29;
        
        // Map 29 days to 8 phases
        MoonPhase prevPhase = CurrentMoonPhase;

        if (cycleDay <= 1) CurrentMoonPhase = MoonPhase.NewMoon;
        else if (cycleDay <= 5) CurrentMoonPhase = MoonPhase.WaxingCrescent;
        else if (cycleDay <= 9) CurrentMoonPhase = MoonPhase.FirstQuarter;
        else if (cycleDay <= 13) CurrentMoonPhase = MoonPhase.WaxingGibbous;
        else if (cycleDay == 14) CurrentMoonPhase = MoonPhase.FullMoon; // Day 15 is technically full
        else if (cycleDay <= 18) CurrentMoonPhase = MoonPhase.WaningGibbous;
        else if (cycleDay <= 22) CurrentMoonPhase = MoonPhase.LastQuarter;
        else CurrentMoonPhase = MoonPhase.WaningCrescent;

        if (prevPhase != CurrentMoonPhase)
        {
            OnMoonPhaseChanged?.Invoke(CurrentMoonPhase);
        }
    }

    public void LoadTimeData(int day, float time)
    {
        CurrentDay = day;
        TimeOfDay = time;
        UpdateMoonPhase();
        Debug.Log($"Time Loaded: Day {CurrentDay}, Time {TimeOfDay:F2}");
    }
}
