using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public AudioSource Walk, Run;

    void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Walk.enabled = false;
                Run.enabled = true;
            }
            else
            {
                Walk.enabled = true;
                Run.enabled = false;
            }
        }
        else
        {
            Walk.enabled = false;
            Run.enabled = false;
        }
    }
}