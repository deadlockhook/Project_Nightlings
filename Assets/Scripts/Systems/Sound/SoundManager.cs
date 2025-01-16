using System.Collections.Generic;
using UnityEngine;

// To play 2D sounds Ex: UI
// SoundManager.Instance.PlaySound("Sound Name");

// To play sound on a specific object Ex: Anything 3D
//SoundManager.Instance.PlaySound("Sound Name", AudioSource);

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    public List<Sound> sounds;
    private Dictionary<string, AudioClip> soundDictionary;
    private Dictionary<string, AudioSource> audioSources;

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
    }

    public void PlaySound(string soundName, AudioSource source = null)
    {
        if (!soundDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            Debug.LogWarning("Sound not found");
            return;
        }

        if (source != null)
        {
            source.PlayOneShot(clip);
        }
        else
        {
            Play2DSound(clip);
        }
    }

    private void Play2DSound(AudioClip clip)
    {
        if (!audioSources.TryGetValue("2D", out AudioSource source2D))
        {
            GameObject obj = new GameObject("2DAudioSource");
            source2D = obj.AddComponent<AudioSource>();
            source2D.spatialBlend = 0;
            DontDestroyOnLoad(obj);
            audioSources["2D"] = source2D;
        }

        source2D.PlayOneShot(clip);
    }
}
