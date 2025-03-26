using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowsActivity : MonoBehaviour
{
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
    private playedSoundAtTrigger[] soundTriggers;

    [Header("Nightling Settings")]
    public GameObject creature;
    public float hiddenOffset = -0.5f;
    public float riseTime = 0.5f;
    public float lowerTime = 0.5f;

    private Vector3 creaturePeekPos;
    private Vector3 creatureHiddenPos;

    private void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();
        startYPosition = transform.position.y;
        endYPosition = startYPosition + 0.8972201f;

        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 1)
        {
            triggerAudio1 = sources[0];
        }

        soundTriggers = new playedSoundAtTrigger[3];
        soundTriggers[0] = new playedSoundAtTrigger(0.25f, triggerAudio1);
        soundTriggers[1] = new playedSoundAtTrigger(0.50f, triggerAudio1);
        soundTriggers[2] = new playedSoundAtTrigger(0.75f, triggerAudio1);

        if (creature != null)
        {
            creaturePeekPos = creature.transform.position;
            creatureHiddenPos = new Vector3(creaturePeekPos.x, creaturePeekPos.y + hiddenOffset, creaturePeekPos.z);
            creature.transform.position = creatureHiddenPos;
            creature.SetActive(false);
        }
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
        positionYOnResetBegin = transform.position.y;

        if (creature != null && creature.activeSelf)
        {
            StartCoroutine(LowerCreature());
        }
    }

    public void ActivityTriggerStart()
    {
        if (activityFinished)
            return;

        inActivity = true;
        shouldReset = false;
        PlayTriggerAudio();

        if (creature != null)
        {
            creature.SetActive(true);
            StopCoroutine("LowerCreature");
            StartCoroutine(RiseCreature());
        }
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

        Vector3 target = new Vector3(transform.position.x, startYPosition + (0.8972201f * activityProgress), transform.position.z);
        transform.position = target;
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

        if (creature != null)
        {
            StartCoroutine(LowerCreature());
        }
    }

    private IEnumerator RiseCreature()
    {
        float elapsed = 0f;
        Vector3 startPos = creature.transform.position;
        while (elapsed < riseTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseTime);
            creature.transform.position = Vector3.Lerp(startPos, creaturePeekPos, t);
            yield return null;
        }
        creature.transform.position = creaturePeekPos;
    }

    private IEnumerator LowerCreature()
    {
        float elapsed = 0f;
        Vector3 startPos = creature.transform.position;
        while (elapsed < lowerTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lowerTime);
            creature.transform.position = Vector3.Lerp(startPos, creatureHiddenPos, t);
            yield return null;
        }
        creature.transform.position = creatureHiddenPos;
        creature.SetActive(false);
    }

    private bool windowShutPlayed = false;

    private void Update()
    {
        if (resetAnimBegin)
        {
            resetProgress += Time.deltaTime;
            resetProgress = Mathf.Clamp(resetProgress, 0.0f, 1.0f);

            if (!windowShutPlayed && resetProgress >= 0.9f)
            {
                windowShutPlayed = true;
                SoundManager.Instance.PlaySound("WindowShut", triggerAudio1);
            }

            if (resetProgress >= 1.0f)
            {
                resetProgress = 0.0f;
                resetAnimBegin = false;
                transform.position = new Vector3(transform.position.x, startYPosition, transform.position.z);
                windowShutPlayed = false;
                SoundManager.Instance.PlaySound("WindowShut", triggerAudio1);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, endYPosition - (0.8972201f * resetProgress), transform.position.z);
            }
        }
    }
}
