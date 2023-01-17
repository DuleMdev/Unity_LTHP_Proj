using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TabKeyControl : MonoBehaviour {

    public List<InputField> inputFieldList;

	// Use this for initialization
	void Start () {
		//inputFieldList[0].ac
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Tab))
            Next();
    }

    void Next()
    {
        // Megkeressük az Aktív komponenst
        int actIndex = 0;
        for (int i = 0; i < inputFieldList.Count; i++)
        {
            if (inputFieldList[i].isFocused)
            {
                // Meghatározzuk a következő komponens indexét
                i++;
                if (i >= inputFieldList.Count)
                    i = 0;

                // Bekapcsoljuk a következő komponenst
                inputFieldList[i].ActivateInputField();
                break;
            }
        }
    }
}
