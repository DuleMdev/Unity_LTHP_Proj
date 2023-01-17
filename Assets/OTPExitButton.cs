using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OTPExitButton : MonoBehaviour
{

    //Ezt a kódot törölni kell mert nem használtam, ha ezt olvasod elfelejtettem és töröld légyszi
    public GameObject LockedInfoPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonClick()
    {
        Debug.Log("Ittvagyok!!");
        
        bool isActive = LockedInfoPanel.activeSelf;
        Debug.Log("Megjelentem!!");
        LockedInfoPanel.SetActive(!isActive);
        

    }
}
