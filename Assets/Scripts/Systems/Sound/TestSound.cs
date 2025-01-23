using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSound : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SoundManager.Instance.PlayMusic("Test");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
