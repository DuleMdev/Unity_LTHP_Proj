using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class LocalStorageWebGL
{
    public static void DeleteKey(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RemoveFromLocalStorage(key: key);
#endif
    }

    public static bool HasKey(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return (HasKeyInLocalStorage(key) == 1);
#endif
        return false;
    }

    public static string GetString(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return LoadFromLocalStorage(key: key);
#endif
        return null;

    }

    public static void SetString(string key, string value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SaveToLocalStorage(key: key, value: value);
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
      private static extern void SaveToLocalStorage(string key, string value);

      [DllImport("__Internal")]
      private static extern string LoadFromLocalStorage(string key);

      [DllImport("__Internal")]
      private static extern void RemoveFromLocalStorage(string key);

      [DllImport("__Internal")]
      private static extern int HasKeyInLocalStorage(string key);
#endif
}
