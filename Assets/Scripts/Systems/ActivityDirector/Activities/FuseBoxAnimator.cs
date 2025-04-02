using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuseBoxAnimator : MonoBehaviour
{

    private bool bCanAnimate = true;

    private float xAnimEndPos = 0.0f;
    private float xAnimStartPos = 0.0f;

    private float xCurrentAnimPos = 0.0f;

    private float singleAnimTime = 0.5f;

    private float currentTime = 0.0f;
    public void TriggerAnimation()
    {
        if (!bCanAnimate)
            return;

        xCurrentAnimPos = xAnimStartPos;
        bCanAnimate = false;
        currentTime = 0.0f;
    }

    void Start()
    {
        xAnimEndPos =0.0f;
        xAnimStartPos = transform.localPosition.x;
    }

    void Update()
    {
        if (!bCanAnimate)
        {
            currentTime += Time.deltaTime;

            float progressTime = currentTime / singleAnimTime;

            if (currentTime > singleAnimTime)
            {
                progressTime = (currentTime - singleAnimTime) / singleAnimTime;
                progressTime = 1.0f - progressTime;
            }

            if (progressTime < 0.0f)
            {
                bCanAnimate = true;
            }
            else
            {
                 transform.localRotation =  Quaternion.Euler(Mathf.Clamp(1.0f - progressTime, 0f, 1.0f) * -89.0f, transform.localPosition.y, transform.localPosition.z);
            }

        }

    }
}
