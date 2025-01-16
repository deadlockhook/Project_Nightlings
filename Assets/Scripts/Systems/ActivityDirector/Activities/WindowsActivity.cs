using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowsActivity : MonoBehaviour
{
    private ActivityDirector activityDirector;

    private List<ActivityDirector.timedActivity> sixWindows = new List<ActivityDirector.timedActivity>();
    void Start()
    {
        activityDirector = FindObjectOfType<ActivityDirector>();

        sixWindows = new List<ActivityDirector.timedActivity>();

        for (int i = 0; i < 6; i++)
            sixWindows.Add(new ActivityDirector.timedActivity(10, i, TriggerWindowActivity));
    }
    void TriggerWindowActivity(int triggerIndex)
    {

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
