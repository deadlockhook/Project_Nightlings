using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityDirector : MonoBehaviour
{
    private ActivityDirector directorInstance = null;
    private void Awake()
    {
        if (directorInstance == null)
        {
            directorInstance = this;
            DontDestroyOnLoad(directorInstance);
        }
        else
            Destroy(this);
    }

    public delegate void timedActivityTrigger(int val);
    public struct timedActivity
    {
        public timedActivity(float _triggerTimeMilliSeconds, int _triggerIndex, timedActivityTrigger _action)
        {
            currentTime = 0;
            triggerTime = _triggerTimeMilliSeconds;
            action = _action;
            triggerIndex = _triggerIndex;
        }

        void OnUpdate()
        {
            currentTime += Time.deltaTime;
            if (currentTime >= triggerTime)
            {
                action(triggerIndex);
                Reset();
            }
        }

        void Reset()
        {
            currentTime = 0;
        }

        float GetProgress()
        {
            return currentTime / triggerTime;
        }

        private int triggerIndex;
        private float currentTime;
        private float triggerTime;
        private timedActivityTrigger action;
    }
    void Start()
    {

    }

    void Update()
    {
        
    }
}
