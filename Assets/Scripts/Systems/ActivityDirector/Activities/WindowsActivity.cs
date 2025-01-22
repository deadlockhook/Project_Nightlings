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

    }
    void TriggerWindowActivity(int triggerIndex)
    {
              
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
