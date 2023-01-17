using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmptyScreen : HHHScreen
{
    public static EmptyScreen instance;

    static Color? backgroundColor;
    static Common.CallBack callBack;

    Image backgroundImage;

    void Awake()
    {
        instance = this;

        backgroundImage = GetComponentInChildren<Image>();
    }

    override public IEnumerator InitCoroutine()
    {
        backgroundImage.color = backgroundColor != null ? backgroundColor.Value : Color.black;

        yield break;
    }

    override public IEnumerator ScreenShowFinishCoroutine()
    {
        if (callBack != null)
            callBack();

        yield break;
    }

    /// <summary>
    /// cLassy2008
    /// cLassay2008
    /// </summary>
    /// <param name="backgroundColor"></param>
    /// <param name="callBack"></param>
    public static void Load(Color? backgroundColor = null, Common.CallBack callBack = null)
    {
        EmptyScreen.backgroundColor = backgroundColor;
        EmptyScreen.callBack = callBack;

        Common.screenController.ChangeScreen(C.Screens.EmptyScreen);
    }
}
