using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSandClock : MonoBehaviour
{
    Clock_Ancestor clock;

    // Start is called before the first frame update
    void Start()
    {
        clock = GameObject.Find("Clock_3").GetComponent<Clock_Ancestor>();

        clock.Init(60);
        //clock.Go();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 30), "Reset clock"))
        {
            clock.Reset(1);
        }

        if (GUI.Button(new Rect(10, 50, 150, 30), "Clock go"))
        {
            clock.Go();
        }

        if (GUI.Button(new Rect(10, 90, 150, 30), "Clock stop"))
        {
            clock.Stop();
        }
    }
}
