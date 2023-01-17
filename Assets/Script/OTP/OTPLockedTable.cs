using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;

public class OTPLockedTable : MonoBehaviour
{
    public GameObject LockedInfoPanel;
    public GameObject turnLThPOff;

    void Awake()
    {
        Debug.Log("Kikapcsolva az Info panel");
        LockedInfoPanel.SetActive(false);
    }

    public void Initialize(string groupMailListID, string pathMailListID, string text)
    {
    }

    public void Empty() {
        
    }

    public void ButtonClick()
    {
        turnLThPOff.SetActive(false);
        bool isActive = LockedInfoPanel.activeSelf;
        LockedInfoPanel.SetActive(!isActive);
        

    }
}