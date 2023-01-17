using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPinCodeReq : MonoBehaviour
{
    public GameObject newPinInfoPanel;
    public GameObject turnLThP;
    
    // Start is called before the first frame update
    void Awake()
    {
        turnLThP.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // Itt eltüntetjük azt a panelt ahova be tudja írni a felhasználó az új pinkódot, és előhozzuk hogy elkültük az új pin kódodat az email címedre
    public void ButtonClick(){
        turnLThP.SetActive(false);
        newPinInfoPanel.SetActive(true);
        
    }

}
