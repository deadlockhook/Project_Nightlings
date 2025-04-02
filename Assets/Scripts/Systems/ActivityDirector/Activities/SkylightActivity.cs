using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkylightActivity : MonoBehaviour
{
    // Start is called before the first frame update
    private bool shouldReset = false;
    private bool resetAnimBegin = false;
    private float resetProgress = 0.0f;
    private float positionZOnResetBegin = 0.0f;
    private bool activityFinished = false;
    private bool inActivity = false;

    private float endZPosition = 0.0f;
    private float startZPosition = 0.0f;
    private float lastActivityProgress = 0.0f;

    private AudioSource triggerAudio1;
    private SoundManager soundManager;

    private playedSoundAtTrigger[] soundTriggers;

    private float posDelta = 2.729924f;
    private void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();

        startZPosition = transform.localPosition.z;
        endZPosition = startZPosition - posDelta;

        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 1)
        {
            triggerAudio1 = sources[0];
        }

        soundTriggers = new playedSoundAtTrigger[3];
        soundTriggers[0] = new playedSoundAtTrigger(0.25f, triggerAudio1);
        soundTriggers[1] = new playedSoundAtTrigger(0.50f, triggerAudio1);
        soundTriggers[2] = new playedSoundAtTrigger(0.75f, triggerAudio1);
    }
    private void PlayTriggerAudio()
    {
        soundManager.PlaySound("Creak2", triggerAudio1);
    }
    public void ResetActivity()
    {
        if (activityFinished || !inActivity)
            return;

        inActivity = false;
        shouldReset = true;
        resetAnimBegin = true;
        resetProgress = 1.0f - lastActivityProgress;
        positionZOnResetBegin = transform.localPosition.z;
    }
    public void ActivityTriggerStart()
    {
        if (activityFinished)
            return;

        inActivity = true;
        shouldReset = false;
        PlayTriggerAudio();
    }
    public bool OnActivityUpdate(float activityProgress)
    {
        if (activityFinished)
            return true;

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

        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, startZPosition - (posDelta * activityProgress));
        lastActivityProgress = activityProgress;

        return false;
    }
    public void ActivityTriggerEnd()
    {
        if (shouldReset)
            return;

        inActivity = false;
        activityFinished = true;
        PlayTriggerAudio();
    }

    private bool skylightShutPlayed = false;
    public void Update()
    {
        if (resetAnimBegin)
        {
            resetProgress += Time.deltaTime;
            resetProgress = Mathf.Clamp(resetProgress, 0.0f, 1.0f);

            if (!skylightShutPlayed && resetProgress >= 0.9f)
            {
                skylightShutPlayed = true;
                SoundManager.Instance.PlaySound("WindowShut", triggerAudio1);
            }

            if (resetProgress >= 1.0f)
            {
                resetProgress = 0.0f;
                resetAnimBegin = false;
                skylightShutPlayed = false;
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, startZPosition);
                SoundManager.Instance.PlaySound("WindowShut", triggerAudio1);
            }
            else
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, endZPosition + (posDelta * resetProgress));
            }
        }

    }
}
