using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class EmailDisplayOnPinInfoPanel : MonoBehaviour
{
    public GameObject newPinInfoPanel;
    public GameObject emailDisplay;
    
    // Start is called before the first frame update

    void Awake(){
        newPinInfoPanel.SetActive(false);
    }
    void Start()
    {
        string email;
        ClassYServerCommunication.instance.getUserProfile((bool success, JSONNode response) => {
            
            email = response[C.JSONKeys.answer][C.JSONKeys.userEmail];
            emailDisplay.GetComponent<Text>().text = email;
            
        });
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
