using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireplaceActivity : MonoBehaviour
{
    private bool shouldReset = false;
    private bool activityFinished = false;
    private bool inActivity = false;

    private SoundManager soundManager;
    private AudioSource triggerAudio;
    private ActivityDirector.playedSoundAtTrigger[] soundTriggers;

    private ActivityDirector.timedActivity activityReference;
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

        if (collision.gameObject.tag == "Interactable_Plank")
        {
            UpdateActivityProgress(1.0f);
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.tag == "Interactable_Toy")
        {
            UpdateActivityProgress(0.25f);
            Destroy(collision.gameObject);
        }
    }
    public void UpdateActivityProgress(float removeProgressPercentage)
    {
        activityReference.RemoveProgressPercentage(removeProgressPercentage);
    }

    public void ActivityTriggerStart(ActivityDirector.timedActivity activity)
    {
        activityReference = activity;

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

        this.GetComponent<Renderer>().material.color = Color.Lerp(Color.white, Color.red, activityProgress);

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
