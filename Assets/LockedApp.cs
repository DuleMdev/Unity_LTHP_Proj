using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedApp : MonoBehaviour
{

    public GameObject button;
    void Awake()
    {
        button.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
