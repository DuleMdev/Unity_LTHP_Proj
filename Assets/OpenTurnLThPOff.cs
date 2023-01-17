using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenTurnLThPOff : MonoBehaviour
{
    public GameObject turnLThPOff;
    public GameObject lockedInfoPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ButtonClick(){
        lockedInfoPanel.SetActive(false);
        turnLThPOff.SetActive(true);
    }
}
