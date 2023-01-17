using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleGameFinishScreen_v1 : HHHScreen
{
    static public CurriculumPathData curriculumPathData; // A karakter k�pek megszerz�s�hez
    static public Common.CallBack callBack; // Ha befejezt�k a j�t�kot mit h�vjon meg

    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Megnyomt�k a tov�bb gombot
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
