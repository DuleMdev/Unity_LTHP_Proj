using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStaticObject : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Debug.Log(TestStaticObjectA.instance.value);
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 100), "Kill object"))
        {
            Destroy(GameObject.Find("A"));
        }
    }
}
