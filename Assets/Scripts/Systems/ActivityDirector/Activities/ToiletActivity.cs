using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToiletActivity : MonoBehaviour
{
    private bool shouldReset = false;
    private bool resetAnimBegin = false;
    private float resetProgress = 0.0f;
    private float rotationXOnResetBegin = 0.0f;
    private bool activityFinished = false;
    private bool inActivity = false;

    private float endYPosition = 0.0f;
    private float startYPosition = 0.0f;
    private float lastActivityProgress = 0.0f;

    private AudioSource triggerAudio;
    private SoundManager soundManager;

    private ActivityDirector.playedSoundAtTrigger[] soundTriggers;
    private void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();

        startYPosition = transform.localPosition.y;
        endYPosition = startYPosition - 0.00909f;

        triggerAudio = GetComponent<AudioSource>();
        soundTriggers = new ActivityDirector.playedSoundAtTrigger[3];
        soundTriggers[0] = new ActivityDirector.playedSoundAtTrigger(0.25f, triggerAudio);
        soundTriggers[1] = new ActivityDirector.playedSoundAtTrigger(0.50f, triggerAudio);
        soundTriggers[2] = new ActivityDirector.playedSoundAtTrigger(0.75f, triggerAudio);
    }
    private void PlayTriggerAudio()
    {
        soundManager.PlaySound("Creak2", triggerAudio);
    }
    public void ResetActivity()
    {
        if (activityFinished || !inActivity)
            return;

        inActivity = false;
        shouldReset = true;
        resetAnimBegin = true;
        resetProgress = 1.0f - lastActivityProgress;
        rotationXOnResetBegin = transform.localRotation.eulerAngles.x;
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

        transform.localRotation = Quaternion.Euler(-90.0f * activityProgress, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
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
            }
            else
            {
                transform.localRotation = Quaternion.Euler(-90.0f * (1.0f - resetProgress), transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);
            }
        }

    }
}
