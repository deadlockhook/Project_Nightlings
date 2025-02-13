using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasementHatch : MonoBehaviour
{
    private bool shouldReset = false;
    private bool resetAnimBegin = false;
    private float resetProgress = 0.0f;
    private float rotationZOnResetBeginForLeftDoor = 0.0f;
    private float rotationZOnResetBeginForRightDoor = 0.0f;
    private bool activityFinished = false;
    private bool inActivity = false;

    private SoundManager soundManager;
    private AudioSource triggerAudio1;
    private ActivityDirector.playedSoundAtTrigger[] soundTriggers;

    private Transform leftDoor;
    private Transform rightDoor;
    private void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();

        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 1)
        {
            triggerAudio1 = sources[0];
        }

        soundTriggers = new ActivityDirector.playedSoundAtTrigger[3];
        soundTriggers[0] = new ActivityDirector.playedSoundAtTrigger(0.25f, triggerAudio1);
        soundTriggers[1] = new ActivityDirector.playedSoundAtTrigger(0.50f, triggerAudio1);
        soundTriggers[2] = new ActivityDirector.playedSoundAtTrigger(0.75f, triggerAudio1);
        leftDoor = transform.Find("Left Door");
        rightDoor = transform.Find("Right Door");
    }

    private void PlayTriggerAudio()
    {
        soundManager.PlaySound("BasementKnock", triggerAudio1);
    }
    public void ResetActivity()
    {
        if (activityFinished)
            return;

        inActivity = false;
        shouldReset = true;
        resetAnimBegin = true;
        resetProgress = 0.0f;
        rotationZOnResetBeginForLeftDoor = leftDoor.localRotation.eulerAngles.z * -1.0f;
        rotationZOnResetBeginForRightDoor = rightDoor.localRotation.eulerAngles.z;
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

        leftDoor.localRotation = Quaternion.Euler(leftDoor.localRotation.eulerAngles.x, leftDoor.localRotation.eulerAngles.y, -90.0f * activityProgress);
        rightDoor.localRotation = Quaternion.Euler(rightDoor.localRotation.eulerAngles.x, rightDoor.localRotation.eulerAngles.y, 90.0f * activityProgress);

        return false;
    }
    public void ActivityTriggerEnd()
    {
        if (shouldReset)
            return;

        inActivity = false;
        activityFinished = true;
    }

    public void Update()
    {
        if (resetAnimBegin)
        {
            resetProgress += Time.deltaTime;

            if (resetProgress >= 1.0f)
            {
                resetProgress = 0.0f;
                resetAnimBegin = false;
                leftDoor.localRotation = Quaternion.Euler(leftDoor.localRotation.eulerAngles.x, leftDoor.localRotation.eulerAngles.y, 0);
                rightDoor.localRotation = Quaternion.Euler(rightDoor.localRotation.eulerAngles.x, rightDoor.localRotation.eulerAngles.y, 0);
            }
            else
            {
                rightDoor.localRotation = Quaternion.Euler(rightDoor.localRotation.eulerAngles.x, rightDoor.localRotation.eulerAngles.y, rotationZOnResetBeginForRightDoor - (rotationZOnResetBeginForRightDoor * resetProgress));
                leftDoor.localRotation = Quaternion.Euler(leftDoor.localRotation.eulerAngles.x, leftDoor.localRotation.eulerAngles.y, (rotationZOnResetBeginForRightDoor - (rotationZOnResetBeginForRightDoor * resetProgress)) * -1.0f);
            }
        }

    }
}
