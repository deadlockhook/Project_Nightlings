using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkylightActivity : MonoBehaviour
{
    // Start is called before the first frame update
    private bool shouldReset = false;
    private bool resetAnimBegin = false;
    private float resetProgress = 0.0f;
    private float positionYOnResetBegin = 0.0f;
    private bool activityFinished = false;
    private bool inActivity = false;

    private float endYPosition = 0.0f;
    private float startYPosition = 0.0f;
    private float lastActivityProgress = 0.0f;

    private AudioSource triggerAudio1;
    private SoundManager soundManager;

    private ActivityDirector.playedSoundAtTrigger[] soundTriggers;
    private void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();

        startYPosition = transform.localPosition.y;
        endYPosition = startYPosition - 0.00909f;

        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 1)
        {
            triggerAudio1 = sources[0];
        }

        soundTriggers = new ActivityDirector.playedSoundAtTrigger[3];
        soundTriggers[0] = new ActivityDirector.playedSoundAtTrigger(0.25f, triggerAudio1);
        soundTriggers[1] = new ActivityDirector.playedSoundAtTrigger(0.50f, triggerAudio1);
        soundTriggers[2] = new ActivityDirector.playedSoundAtTrigger(0.75f, triggerAudio1);
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
        positionYOnResetBegin = transform.localPosition.y;
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

        transform.localPosition = new Vector3(transform.localPosition.x, startYPosition - (0.00909f * activityProgress), transform.localPosition.z);
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

    public void Update()
    {
        if (resetAnimBegin)
        {
            resetProgress += Time.deltaTime;
            resetProgress = Mathf.Clamp(resetProgress, 0.0f, 1.0f);

            if (resetProgress >= 1.0f)
            {
                resetProgress = 0.0f;
                resetAnimBegin = false;
                transform.localPosition = new Vector3(transform.localPosition.x, startYPosition, transform.localPosition.z);
                SoundManager.Instance.PlaySound("WindowShut", triggerAudio1);
            }
            else
            {
                transform.localPosition = new Vector3(transform.localPosition.x, endYPosition + (0.009409f * resetProgress), transform.localPosition.z);
            }
        }

    }
}
