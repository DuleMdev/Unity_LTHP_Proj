using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStaticObjectA : MonoBehaviour
{
    static public TestStaticObjectA instance;

    public int value = 100;

    void Awake()
    {
        instance = this;
    }
}
