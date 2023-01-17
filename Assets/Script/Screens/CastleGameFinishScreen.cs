using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleGameFinishScreen : HHHScreen
{
    static public CurriculumPathData curriculumPathData; // A karakter képek megszerzéséhez
    static public Common.CallBack callBack; // Ha befejeztük a játékot mit hívjon meg

    public TextAsset getPlayableLearnRoutePathList;

    Image imageHero;
    Image imageEnemy;
    Image imageVictim;

    float secondCounter = 19f;

    // Start is called before the first frame update
    void Awake()
    {
        imageHero = gameObject.SearchChild("ImageHero").GetComponent<Image>();
        imageEnemy = gameObject.SearchChild("ImageEnemy").GetComponent<Image>();
        imageVictim = gameObject.SearchChild("ImageVictim").GetComponent<Image>();
    }

    override public IEnumerator ScreenShowStartCoroutine()
    {
        if (curriculumPathData == null)
            curriculumPathData = new CurriculumPathData(JSON.Parse(getPlayableLearnRoutePathList.text)[C.JSONKeys.answer][0]);

        // Letöltjük a képeket
        int ready = 0; // Számoljuk, hogy hány képet sikerült már letölteni
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

        // Amíg nem tölt?dött be a három kép addig várunk
        while (ready < 3) { yield return null; }

        yield break;
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
        CastleGameFinishScreen.curriculumPathData = curriculumPathData;
        CastleGameFinishScreen.callBack = callBack;

        Common.screenController.ChangeScreen(C.Screens.CastleGameFinishScreen);
    }
}
