using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    private static GameAudioManager _instance;
    public static GameAudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameAudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameAudioManager");
                    _instance = go.AddComponent<GameAudioManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Sources")]
    public AudioSource AmbienceSource;
    public AudioSource SFXSource;

    [Header("Settings")]
    public bool IsMuted = false;
    [Range(0f, 1f)]
    public float MasterVolume = 1.0f;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSources();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSources()
    {
        if (AmbienceSource == null)
        {
            AmbienceSource = gameObject.AddComponent<AudioSource>();
            AmbienceSource.loop = true;
            AmbienceSource.playOnAwake = false;
        }

        if (SFXSource == null)
        {
            SFXSource = gameObject.AddComponent<AudioSource>();
            SFXSource.loop = false;
            SFXSource.playOnAwake = false;
        }
    }

    public void PlayAmbience(AudioClip clip, float volume = 0.5f)
    {
        if (IsMuted) return;

        if (AmbienceSource.clip == clip && AmbienceSource.isPlaying) return;

        AmbienceSource.clip = clip;
        AmbienceSource.volume = volume * MasterVolume;
        AmbienceSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (IsMuted || clip == null) return;

        SFXSource.PlayOneShot(clip, volume * MasterVolume);
    }

    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        AmbienceSource.mute = IsMuted;
        SFXSource.mute = IsMuted;
        
        Debug.Log($"Audio Muted: {IsMuted}");
    }
}
