using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleGameFinishScreen_v1 : HHHScreen
{
    static public CurriculumPathData curriculumPathData; // A karakter képek megszerzéséhez
    static public Common.CallBack callBack; // Ha befejeztük a játékot mit hívjon meg

    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Megnyomták a tovább gombot
    public void ButtonClick()
    {
        callBack();
    }

    static public void Load(CurriculumPathData curriculumPathData, Common.CallBack callBack)
    {
        CastleGameFinishScreen_v1.curriculumPathData = curriculumPathData;
        CastleGameFinishScreen_v1.callBack = callBack;

        Common.screenController.ChangeScreen(C.Screens.CastleGameFinishScreen);
    }
}
