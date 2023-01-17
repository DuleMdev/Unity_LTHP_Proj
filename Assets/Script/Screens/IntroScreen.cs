using UnityEngine;
using System.Collections;

public class IntroScreen : HHHScreen {

    [Tooltip("Melyik képernyő jöjjön az introscreen után")]
    public HHHScreen nextScreen;

    [Tooltip("Mennyi ideig látszódjon az intro képernyő")]
    public float showIntro;

    [Tooltip("A háttér zene indexe")]
    public int backgroundMusicIndex;

    public Texture2D[] backgorunds;

    Background background;

    float time;
    int backgroundIndex;

	// Use this for initialization
	new void Awake () {
        background = GetComponentInChildren<Background>();
        backgroundIndex = 2;
	}

    override public IEnumerator InitCoroutine()
    {
        Common.audioController.SetBackgroundMusic(backgroundMusicIndex, 0.3f, 1, false);

        yield return null;
    }


    // Update is called once per frame
    void Update () {
        if (!Common.screenController.changeScreenInProgress) {
            time += Time.deltaTime;

            if (time > showIntro) {
                time = 0;
                if (backgroundIndex < backgorunds.Length)
                {
                    // Kicseréljük a háttérképet
                    Common.fadeEffect.FadeInColor(Color.white, callBack: () =>
                    {
                        background.ChangeBackground(backgorunds[backgroundIndex]);
                        Common.fadeEffect.FadeOut(callBack: () =>
                        {
                            time = 0;
                            backgroundIndex++;
                        });
                    });
                }
                else {

                    Common.screenController.ChangeScreen("MenuClassYLogin");
                    //Common.configurationController.CheckUpdate();

                    //Common.screenController.ChangeScreenColor(nextScreen.name); //    Slide(nextScreen.name, ScreenController.Direction.Down, iTween.EaseType.easeOutBounce, 1);
                }
            }
        }
	}

    void OnDisable()
    {
        time = 0;
    }
}
