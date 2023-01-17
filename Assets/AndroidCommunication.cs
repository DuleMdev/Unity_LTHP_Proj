using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidCommunication : MonoBehaviour
{
    AndroidJavaObject AJO = null;

    void Start()
    {
        Debug.Log("JJJJJJJJJJJJJJJJJJJJ");
        SetPreferenceString("playerName", "John Doe");
        Debug.Log(GetPlayerName());
    }
    public void SetPreferenceString(string prefKey, string prefValue)
    {
        if (AJO == null)
            AJO = new AndroidJavaObject("com.unity3d.player.UnityPlayer", new object[0]);

        AJO.Call("setPreferenceString", new object[] { prefKey, prefValue });
    }

    public string GetPreferenceString(string prefKey)
    {
        if (AJO == null)
            AJO = new AndroidJavaObject("com.unity3d.player.UnityPlayer", new object[0]);

        if (AJO == null)
            return string.Empty;
        return AJO.Call<string>("getPreferenceString", new object[] { prefKey });
    }

    public string GetPlayerName()
    {
        return GetPreferenceString("playerName");
    }
}
