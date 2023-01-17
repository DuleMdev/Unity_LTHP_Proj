using UnityEngine;
using System.Collections;

public class GameScreen2 : HHHScreen
{
    public Texture2D[] pictures;

    Background background;

    float time;

    System.Random random;

    // Use this for initialization
    new void Awake()
    {
        //((HHHScreen)this).Awake(); // Meghívjuk az ős osztály Awake metódusát
        random = new System.Random();
    }

    override public IEnumerator InitCoroutine()
    {
        // Kitaláljuk, hogy melyik képet kéne megjeleníteni
        background.ChangeBackground(pictures[random.Next(pictures.Length)]);

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Common.screenController.changeScreenInProgress)
        {
            time += Time.deltaTime;

            if (time > 3)
            {
                // Kitaláljuk, hogy milyen képváltást alkalmazzunk
                ScreenController.ScreenTransition screenTransition = (ScreenController.ScreenTransition)random.Next(4);

                screenTransition = ScreenController.ScreenTransition.Slide;

                iTween.EaseType easeType = iTween.EaseType.easeOutBack;

                switch (random.Next(5))
                {
                    case 0:
                        easeType = iTween.EaseType.easeOutBack;
                        break;
                    case 1:
                        easeType = iTween.EaseType.easeOutBounce;
                        break;
                    case 2:
                        easeType = iTween.EaseType.spring;
                        break;
                    case 3:
                        easeType = iTween.EaseType.linear;
                        break;
                    case 4:
                        easeType = iTween.EaseType.easeOutExpo;
                        break;
                }

                Debug.Log("GameScreen2 : " + screenTransition + " - " + easeType);

                Common.screenController.ChangeScreenFull("GameScreen", screenTransition, null, 5, (ScreenController.Direction)random.Next(4), easeType, 1);
            }
        }
    }

    void OnEnable()
    {
        time = 0;
    }

    void OnDisable()
    {
        time = 0;
    }
}