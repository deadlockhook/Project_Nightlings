using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowsActivity : MonoBehaviour
{
    private bool shouldReset = false;
    public void ResetWindow()
    {
        shouldReset = true;
    }
    public void ActivityTriggerStart()
    {
        shouldReset = false;
    }
    public bool OnActivityUpdate(float activityProgress)
    {
        if (shouldReset)
        {
            //reset window trigger animation
            shouldReset = false;
            return true;
        }

        //progress trigger animation
        Debug.Log(activityProgress);

        return false;
    }
    public void ActivityTriggerEnd()
    {
        //trigger monster event as window has been opened
    }

}
