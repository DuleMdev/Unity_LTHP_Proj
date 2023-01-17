using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClearInputs : MonoBehaviour
{
    public InputField inputField;

    public TurnLThPOk turnLThPOk;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnEnable()
    {
        turnLThPOk.SetInput(0);
        inputField.text = "";
    }
}
