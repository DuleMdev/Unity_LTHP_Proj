using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnLThPOk : MonoBehaviour
{
    public GameObject turnLThp;

    private int input = 0;

    public LThPOnOffButton lThPOnOffButton;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ButtonClick(){
        lThPOnOffButton.ToggleOnOff(input);
        turnLThp.SetActive(false);
    }
    public void ReadStringInput(string s){
        input = int.Parse(s);
    }

    public int GetInput(){
        return input;
    }

    public void SetInput(int newInput){
        input = newInput;
    }

}
