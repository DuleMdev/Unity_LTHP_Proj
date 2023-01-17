using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward : MonoBehaviour
{
    string s;
    Common.CallBack_In_String callBack;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize(string s, Common.CallBack_In_String callBack)
    {
        this.s = s;
        this.callBack = callBack;
    }
    public void AnimEnd()
    {
        Debug.Log("AnimEnd");

        if (callBack != null)
            callBack(s);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
