using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FireplaceActivity : MonoBehaviour
{
    private bool shouldReset = false;
    private bool activityFinished = false;
    private bool inActivity = false;
    private bool hintDisplayed = false;

    public GameObject fireVFX;

    private SoundManager soundManager;
    private AudioSource triggerAudio1;
    private AudioSource triggerAudio2;
    private playedSoundAtTrigger[] soundTriggers;

    private timedActivity activityReference;
    private FireplaceSoundHandler fireSoundHandler;

    private void Start()
    {
        fireSoundHandler = GetComponent<FireplaceSoundHandler>();
        soundManager = FindObjectOfType<SoundManager>();
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            triggerAudio1 = sources[0];
            triggerAudio2 = sources[1];
        }
        triggerAudio1.loop = true;
        triggerAudio1.Play();
        triggerAudio1.spatialBlend = 1f;
        soundTriggers = new playedSoundAtTrigger[3];
        soundTriggers[0] = new playedSoundAtTrigger(0.25f, triggerAudio1);
        soundTriggers[1] = new playedSoundAtTrigger(0.50f, triggerAudio1);
        soundTriggers[2] = new playedSoundAtTrigger(0.75f, triggerAudio1);
    }

    private void PlayTriggerAudio()
    {
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (activityFinished || !inActivity)
            return;

        if (collision.gameObject.tag == "Interactable_Plank")
        {
            UpdateActivityProgress(1.0f);
            Destroy(collision.gameObject);
            SoundManager.Instance.PlaySound("RefuelFire", triggerAudio2);
        }

        if (collision.gameObject.tag == "Interactable_Toy")
        {
            UpdateActivityProgress(0.25f);
            Destroy(collision.gameObject);
            SoundManager.Instance.PlaySound("RefuelFire", triggerAudio2);
        }

        if (collision.gameObject.tag == "Interactable_Blocks")
        {
            UpdateActivityProgress(0.25f);

            GameObject parent = collision.gameObject.transform.parent.gameObject;
            if (parent)
            {
                Rigidbody[] blocks = parent.GetComponentsInChildren<Rigidbody>();
                for (int i = 0; i < blocks.Length; i++)
                {
                    Rigidbody blockRigidBody = blocks[i];
                    if (blockRigidBody)
                    {
                        Destroy(blockRigidBody.gameObject);
                    }
                }
            }
        }
    }

    public void UpdateActivityProgress(float removeProgressPercentage)
    {
        activityReference.RemoveProgressPercentage(removeProgressPercentage);
    }

    public void ActivityTriggerStart(timedActivity activity)
    {
        activityReference = activity;
        if (activityFinished)
            return;

        inActivity = true;
        shouldReset = false;
        hintDisplayed = false;
        PlayTriggerAudio();
    }

    public bool OnActivityUpdate(float activityProgress)
    {
        if (activityFinished)
            return true;

        fireSoundHandler.SetFireIntensity(1.0f - activityProgress);

        if (shouldReset)
        {
            shouldReset = false;
            return true;
        }

        foreach (var trigger in soundTriggers)
        {
            if (trigger.ShouldPlay(activityProgress))
                PlayTriggerAudio();
        }

        if (!hintDisplayed && activityProgress >= 0.5f)
        {
            HintManager.Instance.DisplayGameHint(HintType.Fireplace);
            hintDisplayed = true;
        }

        if (fireVFX != null)
        {
            fireVFX.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, activityProgress);
        }

        return false;
    }

    public void ActivityTriggerEnd()
    {
        if (shouldReset)
            return;

        inActivity = false;
        activityFinished = true;
    }
}
