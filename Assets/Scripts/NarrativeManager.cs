using UnityEngine;
using System.Collections.Generic;

public class NarrativeManager : MonoBehaviour
{
    public static NarrativeManager Instance { get; private set; }

    [Header("UI Reference")]
    public string CurrentMessage = "";
    private float messageTimer = 0f;
    private float messageDuration = 5f;

    private Dictionary<HurmaTree.GrowthStage, string> growthQuotes;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializeQuotes();
    }

    private void Start()
    {
        // Subscribe to tree events if tree exists, or let tree subscribe to us
        // Better: Make HurmaTree notify us via Instance
        ShowMessage("Bismillah. (Toprağa tıkla ve tohumu ek)");
    }

    private void Update()
    {
        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0)
            {
                CurrentMessage = ""; // Clear message
            }
        }
    }

    [Header("Audio")]
    public AudioClip WisdomChime;

    public void ShowMessage(string msg)
    {
        CurrentMessage = msg;
        messageTimer = messageDuration;
        Debug.Log($"Narrative: {msg}");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWisdom(msg, messageDuration);
        }
        
        if (GameAudioManager.Instance != null && WisdomChime != null)
        {
            GameAudioManager.Instance.PlaySFX(WisdomChime, 0.4f); // Gentle chime
        }
    }

    public void OnTreeStageChanged(HurmaTree.GrowthStage stage)
    {
        if (growthQuotes.ContainsKey(stage))
        {
            ShowMessage(growthQuotes[stage]);
        }
    }

    private void InitializeQuotes()
    {
        growthQuotes = new Dictionary<HurmaTree.GrowthStage, string>()
        {
            { HurmaTree.GrowthStage.Seed, "Her tohum, sabırla yeşermeyi bekleyen bir duadır." },
            { HurmaTree.GrowthStage.Sapling, "Kökleri sağlam olanın, dalları göğe yükselir." },
            { HurmaTree.GrowthStage.YoungTree, "Acele şeytandan, teenni (sabırla hareket) Rahman'dandır." },
            { HurmaTree.GrowthStage.MatureTree, "Güzel söz, kökü sabit, dalları gökte olan güzel bir ağaç gibidir. (İbrahim Suresi: 24)" },
            { HurmaTree.GrowthStage.Flowering, "Çiçekler şükrün tebessümüdür. Rızkını bekle." },
            { HurmaTree.GrowthStage.Fruiting, "Rızkı veren Allah'tır. İnfak ettiğin seninle kalır." }
        };
        
        // Additional Pool of randomness we can pull from?
        // For now, let's keep it tied to stages but enrich the text.
    }
}
