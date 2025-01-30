using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// To play 2D sounds Ex: UI
// SoundManager.Instance.PlaySound("Sound Name");

// To play sound on a specific object Ex: Anything 3D
// SoundManager.Instance.PlaySound("Sound Name", AudioSource);

// To play background music
// SoundManager.Instance.PlayMusic("Music Name");

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    public List<Sound> sounds;
    private Dictionary<string, AudioClip> soundDictionary;
    private Dictionary<string, AudioSource> audioSources;
    private AudioSource musicSource;

    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;

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
            return;
        }

        soundDictionary = new Dictionary<string, AudioClip>();
        foreach (var sound in sounds)
        {
            soundDictionary[sound.name] = sound.clip;
        }

        audioSources = new Dictionary<string, AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume * masterVolume;
    }

    private void Start()
    {
        masterVolumeSlider.value = masterVolume;
        sfxVolumeSlider.value = sfxVolume;
        musicVolumeSlider.value = musicVolume;

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
    }


    public void PlaySound(string soundName, AudioSource source = null)
    {
        if (!soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            Debug.LogWarning($"Sound '{soundName}' not found");
            return;
        }

        if (source != null)
        {
            if (source.isPlaying && source.clip == clip)
                return;

            source.clip = clip;
            source.volume = sfxVolume * masterVolume;
            source.Play();
        }
        else
        {
            Play2DSound(clip);
        }
    }


    private void Play2DSound(AudioClip clip)
    {
        AudioSource source2D;

        if (!audioSources.ContainsKey("2D"))
        {
            GameObject obj = new GameObject("2DAudioSource");
            source2D = obj.AddComponent<AudioSource>();
            source2D.spatialBlend = 0;
            DontDestroyOnLoad(obj);
            audioSources["2D"] = source2D;
        }
        else
        {
            source2D = audioSources["2D"];
        }

        source2D.volume = sfxVolume * masterVolume;
        source2D.PlayOneShot(clip);
    }

    public void PlayMusic(string musicName)
    {
        if (!soundDictionary.TryGetValue(musicName, out AudioClip clip))
        {
            Debug.LogWarning("Music not found");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        UpdateVolumes();
        musicSource.Play();
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        musicSource.volume = musicVolume * masterVolume;

        foreach (var source in audioSources.Values)
        {
            source.volume = sfxVolume * masterVolume;
        }
    }
}

