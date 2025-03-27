using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireplaceSoundHandler : MonoBehaviour
{
    private AudioSource fireAudio;
    private float fireIntensity = 1.0f;
    private bool isPlaying = false;

    private void Start()
    {
        fireAudio = gameObject.AddComponent<AudioSource>();

        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManager not found");
            return;
        }

        SoundManager.Instance.PlaySound("FireBurning", fireAudio);
        fireAudio.loop = true;
        fireAudio.spatialBlend = 1f;
        isPlaying = true;
    }

    private void Update()
    {
        if (fireAudio == null || SoundManager.Instance == null)
            return;

        fireAudio.volume = SoundManager.Instance.sfxVolume * SoundManager.Instance.masterVolume * fireIntensity;
    }

    public void SetFireIntensity(float intensity)
    {
        fireIntensity = Mathf.Clamp01(intensity);

        if (fireIntensity <= 0f && isPlaying)
        {
            fireAudio.Stop();
            isPlaying = false;
        }
        else if (fireIntensity > 0f && !isPlaying)
        {
            SoundManager.Instance.PlaySound("FireBurning", fireAudio);
            isPlaying = true;
        }
    }
}
