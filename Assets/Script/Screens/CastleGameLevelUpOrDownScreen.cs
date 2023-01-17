using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleGameLevelUpOrDownScreen : HHHScreen
{
    
    static public CurriculumPathData _curriculumPathData; // A karakter képek megszerzéséhez
    static public bool _levelUp;
    static public Common.CallBack _callBack; // Ha befejeztük a játékot mit hívjon meg

    Text text;
    Text textCounter;

    float secondCounter = 2.9f;

    // Start is called before the first frame update
    void Awake()
    {
        text = gameObject.SearchChild("Text").GetComponent<Text>();
        textCounter = gameObject.SearchChild("TextCounter").GetComponent<Text>();

        text.text = _levelUp ? "LevelUp animáció" : "LevelDown animáció";
    }

    // Update is called once per frame
    void Update()
    {
        secondCounter -= Time.deltaTime;

        if (secondCounter < 0)
        {
            _callBack();
            secondCounter = float.MaxValue;
        }
        else
            textCounter.text = ((int)secondCounter).ToString();
    }

    static public void Load(bool levelUp, CurriculumPathData curriculumPathData, Common.CallBack callBack)
    {
        _levelUp = levelUp;
        _curriculumPathData = curriculumPathData;
        _callBack = callBack;

        Common.screenController.ChangeScreen(C.Screens.CastleGameLevelUpOrDownScreen);
    }
}
