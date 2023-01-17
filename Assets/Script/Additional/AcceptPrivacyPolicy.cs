using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcceptPrivacyPolicy : MonoBehaviour
{
    public GameObject gameObject;
    
    public void close() {
        gameObject.transform.gameObject.SetActive(false);
    }
}
