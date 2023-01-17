using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class LThPOnOffButton : MonoBehaviour
{
    

    public GameObject turnLThPOn;

    public GameObject turnLThPOff;

    public SwitchToggle switchToggle;

    public GameObject lockButton;

    private int pinCode = 0;

    public AndroidCommunication androidCommunication;

    void Awake()
    {
        
    }
    // Update is called once per frame
    void Update()
    {

    }

    
    public void ButtonClick()
    {  
        
         /* Ebben van benne hogy van-e current game
        ClassYServerCommunication.instance.getCurrentHomeworks((bool success, JSONNode response) =>
        {
            Debug.Log("KKKKKKKKKKKKKKKKK");
            Debug.Log(response);
            //response[C.JSONKeys.answer][C.JSONKeys.parents][0][C.JSONKeys.unlockPin];
            
        });

        */
        if (switchToggle.GetSwitchIsActive() == false)
            turnLThPOn.SetActive(true);
        else
            turnLThPOff.SetActive(true);

    }

    public void ToggleOnOff(int input)
    {
        ClassYServerCommunication.instance.getFamilyConnections((bool success, JSONNode response) =>
        {
            pinCode = int.Parse(response[C.JSONKeys.answer][C.JSONKeys.parents][0][C.JSONKeys.unlockPin]);
            
            if (input == pinCode && switchToggle.GetSwitchIsActive() == false)
            {
                switchToggle.ActivateOnSwitch(true);
                lockButton.SetActive(true);
                AndroidJavaObject jo = new AndroidJavaObject("com.aycs.unitytoandroid.AndroidBlocking");
                int result = jo.Call<int>("Block","True");

            }
            else if (input == pinCode && switchToggle.GetSwitchIsActive() == true)
            {
                switchToggle.DeActivateOnSwitch(true);
                lockButton.SetActive(false);
                AndroidJavaObject jo = new AndroidJavaObject("com.aycs.unitytoandroid.AndroidBlocking");
                int result = jo.Call<int>("Block","False");
            }
            
        });
    }
}
