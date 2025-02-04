using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetDoorActivity : MonoBehaviour
{
    private bool shouldReset = false;
    private bool resetAnimBegin = false;
    private float resetProgress = 0.0f;
    private float rotationXOnResetBegin = 0.0f;
    private bool activityFinished = false;
    private bool inActivity = false;

    private SoundManager soundManager;
    private AudioSource triggerAudio;
    private ActivityDirector.playedSoundAtTrigger[] soundTriggers;
    private void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();
        triggerAudio = GetComponent<AudioSource>();
        soundTriggers = new ActivityDirector.playedSoundAtTrigger[3];
        soundTriggers[0] = new ActivityDirector.playedSoundAtTrigger(0.25f, triggerAudio);
        soundTriggers[1] = new ActivityDirector.playedSoundAtTrigger(0.50f, triggerAudio);
        soundTriggers[2] = new ActivityDirector.playedSoundAtTrigger(0.75f, triggerAudio);
    }

    private void PlayTriggerAudio()
    {
        soundManager.PlaySound("DoorBell", triggerAudio);
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (activityFinished || !inActivity)
            return;

        if (collision.gameObject.tag == "Interactable_Toy")
        {
            ResetActivity();
            Destroy(collision.gameObject);
        }
    }

    public void ResetActivity()
    {
        if (activityFinished)
            return;

        inActivity = false;
        shouldReset = true;
        resetAnimBegin = true;
        resetProgress = 0.0f;
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
        //  transform.rotation = Quaternion.Slerp(transform.rotation, target, activityProgress * Time.deltaTime);

        if (activityFinished)
            return true;

        if (shouldReset)
        {
            //reset window trigger animation
            shouldReset = false;
            return true;
        }

        foreach (var trigger in soundTriggers)
        {
            if (trigger.ShouldPlay(activityProgress))
                PlayTriggerAudio();
        }

        Quaternion target = Quaternion.Euler(90.0f * activityProgress, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);

        transform.localRotation = target;

        //progress trigger animation

        return false;
    }
    public void ActivityTriggerEnd()
    {
        if (shouldReset)
            return;

        inActivity = false;
        activityFinished = true;
        //trigger monster event as window has been opened
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
            }
            else
            {
                Quaternion target = Quaternion.Euler(rotationXOnResetBegin - (rotationXOnResetBegin * resetProgress), transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);

                transform.localRotation = target;
            }
        }

    }
}
