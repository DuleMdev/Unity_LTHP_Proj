using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleGameLevelDownScreen : HHHScreen
{
    static public CurriculumPathData curriculumPathData; // A karakter k�pek megszerz�s�hez
    static JSONNode statusData; // Az aktu�lis szint megszerz�s�hez
    static public Common.CallBack callBack; // Ha befejezt�k a j�t�kot mit h�vjon meg

    public TextAsset statusDataAsset;
    public TextAsset getPlayableLearnRoutePathList;

    TextMesh textMeshLevelForward1;
    TextMesh textMeshLevelForward2;
    TextMesh textMeshLevelUp;
    TextMesh textMeshLevelDown;

    Image imageHero;
    Image imageEnemy;
    Image imageVictim;

    float secondCounter = 9f;

    // Start is called before the first frame update
    void Awake()
    {
        textMeshLevelForward1 = gameObject.SearchChild("TextMeshLevelForward1").GetComponent<TextMesh>();
        textMeshLevelForward2 = gameObject.SearchChild("TextMeshLevelForward2").GetComponent<TextMesh>();
        textMeshLevelUp = gameObject.SearchChild("TextMeshLevelUp").GetComponent<TextMesh>();
        textMeshLevelDown = gameObject.SearchChild("TextMeshLevelDown").GetComponent<TextMesh>();

        imageHero = gameObject.SearchChild("ImageHero").GetComponent<Image>();
        imageEnemy = gameObject.SearchChild("ImageEnemy").GetComponent<Image>();
        imageVictim = gameObject.SearchChild("ImageVictim").GetComponent<Image>();
    }

    override public IEnumerator ScreenShowStartCoroutine()
    {
        if (statusData == null)
            statusData = JSON.Parse(statusDataAsset.text);

        if (curriculumPathData == null)
            curriculumPathData = new CurriculumPathData(JSON.Parse(getPlayableLearnRoutePathList.text)[C.JSONKeys.answer][0]);

        int actLevel = statusData[C.JSONKeys.currentLevelNumber].AsInt;
        textMeshLevelUp.text = "" + (actLevel + 2);
        textMeshLevelForward1.text = "" + (actLevel + 1);
        textMeshLevelForward2.text = "" + (actLevel + 1);
        textMeshLevelDown.text = "" + actLevel;

        // Let�ltj�k a k�peket
        int ready = 0; // Sz�moljuk, hogy h�ny k�pet siker�lt m�r let�lteni
        EmailGroupPictureController.instance.GetPictureFromUploadsDir(curriculumPathData.heroData.image,
            (Sprite sprite) =>
            {
                imageHero.sprite = sprite;
                imageHero.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
                ready++;
            }
        );

        EmailGroupPictureController.instance.GetPictureFromUploadsDir(curriculumPathData.enemyData.image,
            (Sprite sprite) =>
            {
                imageEnemy.sprite = sprite;
                imageEnemy.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
                ready++;
            }
        );

        EmailGroupPictureController.instance.GetPictureFromUploadsDir(curriculumPathData.victimData.image,
            (Sprite sprite) =>
            {
                imageVictim.sprite = sprite;
                imageVictim.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
                ready++;
            }
        );

        // Am�g nem t�lt?d�tt be a h�rom k�p addig v�runk
        while (ready < 3) { yield return null; }

        yield break;
    }

    // Update is called once per frame
    void Update()
    {
        secondCounter -= Time.deltaTime;

        if (secondCounter < 0)
        {
            secondCounter = float.MaxValue;

            //EmptyScreen.Load(callBack: callBack);

            callBack();
        }
    }

    public void ButtonClick()
    {
        secondCounter = 0;
    }

    static public void Load(CurriculumPathData curriculumPathData, JSONNode statusData, Common.CallBack callBack)
    {
        CastleGameLevelDownScreen.curriculumPathData = curriculumPathData;
        CastleGameLevelDownScreen.statusData = statusData;
        CastleGameLevelDownScreen.callBack = callBack;

        Common.screenController.ChangeScreen(C.Screens.CastleGameLevelDownScreen);
    }
}
