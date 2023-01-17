using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleGameFinishScreen : HHHScreen
{
    static public CurriculumPathData curriculumPathData; // A karakter k�pek megszerz�s�hez
    static public Common.CallBack callBack; // Ha befejezt�k a j�t�kot mit h�vjon meg

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

    }

    // Megnyomt�k a tov�bb gombot
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
