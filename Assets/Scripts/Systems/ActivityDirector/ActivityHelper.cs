using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void timedActivityTrigger(int val);
public class playedSoundAtTrigger
{
    public playedSoundAtTrigger(float _percentage, AudioSource _source)
    {
        percentage = _percentage;
        played = false;
        source = _source;
    }

    public bool ShouldPlay(float _percentage)
    {
        if (played)
            return false;

        if (_percentage >= percentage)
        {
            played = true;
            return true;
        }

        return false;
    }

    private AudioSource source;
    private bool played;
    private float percentage;
}
public class timedActivity
{
    public timedActivity(float _triggerTimeSeconds, int _triggerIndex, timedActivityTrigger _actionStart, timedActivityTrigger _actionEnd, timedActivityTrigger _actionOnUpdate)
    {
        currentTime = 0;
        triggerTime = _triggerTimeSeconds;
        actionStart = _actionStart;
        actionEnd = _actionEnd;
        actionOnUpdate = _actionOnUpdate;
        triggerIndex = _triggerIndex;
        active = false;
    }

    public void RemoveProgressPercentage(float progressToRemove)
    {
        currentTime -= triggerTime * progressToRemove;

        if (currentTime < 0)
            currentTime = 0;
    }
    public void OnUpdate()
    {
        if (!active)
            return;

        currentTime += Time.deltaTime;

        if (actionOnUpdate != null)
            actionOnUpdate(triggerIndex);

        if (currentTime >= triggerTime)
        {
            if (actionEnd != null)
                actionEnd(triggerIndex);

            Reset();
        }
    }
    public void Activate(List<timedActivity> activites)
    {
        activites.Add(this);
        active = true;

        currentTime = 0;

        if (actionStart != null)
            actionStart(triggerIndex);
    }
    public void Deactivate(List<timedActivity> activites)
    {
        active = false;
        activites.Remove(this);
    }

    public void Reset() {  currentTime = 0; }
    public float GetProgress()  { return currentTime / triggerTime; }
    public bool IsActive() { return active; }
    public int GetTriggerIndex() { return triggerIndex; }

    private bool active;
    private int triggerIndex;
    private float currentTime;
    private float triggerTime;
    private timedActivityTrigger actionStart;
    private timedActivityTrigger actionEnd;
    private timedActivityTrigger actionOnUpdate;
}
public class activityTrigger
{
    public activityTrigger(GameObject _gameObj, float triggerTimeSeconds, int triggerIndex, timedActivityTrigger actionStart, timedActivityTrigger actionEnd, timedActivityTrigger actionOnUpdate)
    {
        gameObj = _gameObj;
        eventTime = new timedActivity(triggerTimeSeconds, triggerIndex, actionStart, actionEnd, actionOnUpdate);
    }

    public GameObject gameObj;
    public timedActivity eventTime;
}

public struct timeLimits
{
    public timeLimits(float _rangeStart, float _rangeEnd, float _timeLimit)
    {
        rangeStart = _rangeStart;
        rangeEnd = _rangeEnd;
        timeLimit = _timeLimit;
        lastSelectedTime = 0.0f;
        lastUpdateTime = 0.0f;
        selectedTimeRange = rangeEnd;
        finished = false;
    }

    public float rangeStart;
    public float rangeEnd;
    public float timeLimit;
    public float lastSelectedTime;
    public float lastUpdateTime;
    public float selectedTimeRange;
    public bool finished;

    public void SelectRange() 
    { 
        selectedTimeRange = Random.Range(rangeStart, rangeEnd);
    }
}

public class timeManager
{
    public float currentTime;

    public void OnUpdate()
    {
        currentTime += Time.deltaTime;
    }

    public bool IsInLimit(timeLimits timeLimit)
    {
        return currentTime - timeLimit.lastUpdateTime > timeLimit.selectedTimeRange;
    }
}

