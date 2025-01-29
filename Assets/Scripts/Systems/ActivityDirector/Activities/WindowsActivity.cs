using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowsActivity : MonoBehaviour
{
    private bool shouldReset = false;
    private bool resetAnimBegin = false;
    private float resetProgress = 0.0f;
    private float rotationXOnResetBegin = 0.0f;
    private bool activityFinished = false;

    private float endYPosition = 0.0f;

    private void Start()
    {
        endYPosition =  transform.position.y + 0.8972201f;
    }
    public void ResetActivity()
    {
        if (activityFinished)
            return;
        shouldReset = true;
        resetAnimBegin = true;
        resetProgress = 0.0f;
        rotationXOnResetBegin = transform.eulerAngles.x;
    }
    public void ActivityTriggerStart()
    {
        shouldReset = false;
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

        Quaternion target = Quaternion.Euler(90.0f * activityProgress, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        transform.rotation = target;

        //progress trigger animation

        return false;
    }
    public void ActivityTriggerEnd()
    {
        if (shouldReset)
            return;

        activityFinished = true;
        //trigger monster event as window has been opened
    }

    public void Update()
    {
        if (resetAnimBegin)
        {
            resetProgress += Time.deltaTime;

            Debug.Log(resetProgress);

            if (resetProgress >= 1.0f)
            {
                resetProgress = 0.0f;
                resetAnimBegin = false;
            }
            else
            {
                Quaternion target = Quaternion.Euler(rotationXOnResetBegin - (rotationXOnResetBegin * resetProgress), transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                transform.rotation = target;
            }
        }

    }

}
