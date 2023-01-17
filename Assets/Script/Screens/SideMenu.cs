using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A három csíkos menügomb megnyomására megjelenik bal oldalt egy menü, miközben az aktuális képernyőt jobbra részben kitolja a képernyőből.
/// 
/// Show
/// A metódusnak meg kell adni az aktuális képernyő gameObjectjét amit ki kell tolnia a képernyőből, helyet csinálva a menünek.
/// A cover egy takaró ami letakarja a korábbi ablakot, hogy ne lehessen rákattintani.
/// </summary>

public class SideMenu : MonoBehaviour
{
    static public SideMenu instance;

    RectTransform rectTransform;    // A menüt mozgató transform
    Transform transformActScreen;   // Az aktuális képernyőt mozgató transform
    Image coverImage;

    Tween.TweenAnimation tweenAnimation;    // A mozgatást végző animáció
    Common.CallBack hideCallBack;   // Az oldal menü eltüntetése után mit hívjon meg

    float showPos; // Melyik pozícióba kell mozgatni az ablakokat, hogy a menü láthatóvá váljon

	// Use this for initialization
	void Awake () {
        instance = this;

        rectTransform = GetComponent<RectTransform>();
        coverImage = gameObject.SearchChild("Cover").GetComponent<Image>();

        tweenAnimation = new Tween.TweenAnimation();
        tweenAnimation.onUpdate = SetXPos;

        Vector3 screenSizeInUnit = Common.fitSize_16_9();
        showPos = screenSizeInUnit.x / 2048 * gameObject.SearchChild("Background").GetComponent<RectTransform>().sizeDelta.x;
    }

    public void Show(GameObject actScreenGameObject)
    {
        transformActScreen = actScreenGameObject.transform;

        coverImage.enabled = true;

        tweenAnimation.actTime = 0;
        tweenAnimation.time = 1;
        tweenAnimation.startPos = rectTransform.position.x;
        tweenAnimation.endPos = showPos;
        tweenAnimation.easeType = Tween.EaseType.easeOutCubic;
        tweenAnimation.onComplete = null;

        Tween.StartAnimation(tweenAnimation);
    }

    public void Hide(Common.CallBack hideCallBack = null)
    {
        this.hideCallBack = hideCallBack;

        tweenAnimation.actTime = 0;
        tweenAnimation.time = 0.5f;
        tweenAnimation.startPos = rectTransform.position.x;
        tweenAnimation.endPos = 0f;
        tweenAnimation.easeType = Tween.EaseType.easeOutCubic;
        tweenAnimation.onComplete = HideComplete;

        Tween.StartAnimation(tweenAnimation);
    }

    void HideComplete()
    {
        coverImage.enabled = false;

        if (hideCallBack != null)
            hideCallBack();
    }

    public void SetXPos(object o)
    {
        float value = (float)o;
        rectTransform.position = rectTransform.position.SetX(value);
        transformActScreen.position = transformActScreen.position.SetX(value);
    }

    public void ButtonClick(string button)
    {
        string[] buttonNameSplitted = button.Split(':');

        switch (buttonNameSplitted[0])
        {
            case "cover":
                Hide();
                break;

            case "classyCuriculums":
                Hide(() => { Common.screenController.ChangeScreen(C.Screens.OTPMain); } );
                break;

            case "ClassroomPlay":
                ClassroomScreens.instance.ChangeScreen(ClassroomScreens.ClassroomScreensEnum.TeacherStudentChoice);
                Hide(() => { Common.screenController.ChangeScreen(C.Screens.ClassroomScreens); });
                break;

            case "SetLanguageOfApplication":

                // Létrehozzuk a nyelvi adatokat a megjelenítéshez

                List<LanguageData> list = new List<LanguageData>();

                List<string> listOfLangName = Common.languageController.GetLanguages();
    
                for (int i = 0; i < listOfLangName.Count; i++)
                {
                    LanguageData languageData = new LanguageData(
                        listOfLangName[i],
                        listOfLangName[i],
                        listOfLangName[i]);

                    list.Add(languageData);
                }

                ClassYLanguageSelector.instance.Initialize(list, C.Program.LanguageSelector);
                ClassYLanguageSelector.instance.Show(Common.configurationController.applicationLangName, C.Texts.SelectApplicationLanguage, ButtonClick);
                break;

            case C.Program.LanguageSelector: // A nyelvválasztó panelen kiválasztottak egy zászlót

                // Kiválasztottak egy nyelvet
                // Ha más nyelvet választottak mint korábban volt, csak akkor csinálunk valamit
                if (Common.configurationController.applicationLangName != buttonNameSplitted[1])
                {
                    Common.configurationController.applicationLangName = buttonNameSplitted[1];
                    Common.configurationController.Save();

                    Common.languageController.LanguageDetermination();
                }

                ClassYLanguageSelector.instance.Hide(() => Hide());
                break;

            case C.Program.FadeClickLanguageSelector: // A fade panelre kattintottak, mikor a nyelvválasztó panel volt látható
                // Eltüntetjük a felbukkanó ablakot
                ClassYLanguageSelector.instance.Hide(() => Hide());
                break;

            case "Settings":

                // Lekérdezzük a szervertől az adatokat
                ClassYServerCommunication.instance.getUserProfile(
                    (bool success, JSONNode response) => {
                        // Ha sikeres volt a lekérdezés, akkor megmutatjuk a Settings ablakot
                        if (success)
                        {
                            Hide(() => {
                                Settings.instance.Show();
                            });
                        }
                    }
                );
                break;

            case "Exit":
                ErrorPanel.instance.Show(Common.languageController.Translate(C.Texts.ExitConfirmation), Common.languageController.Translate(C.Texts.Ok), button2Text: Common.languageController.Translate(C.Texts.Cancel), callBack: (string bName) =>
                {
                    ErrorPanel.instance.Hide(() => {
                        if (bName == "button1")
                        {
                            // ha kilépünk az OTPMain képernyőből a bejelentkező képernyőre, akkor 
                            Hide(() => {
                                ClassYServerCommunication.instance.sessionToken = "";
                                Common.configurationController.Save(); // Elmentjük, hogy töröltük a session token-t
                                Common.screenController.LoadScreenAfterIntro();
                            });
                        }
                    });
                });

                //Hide(() => { Common.screenController.ChangeScreen(C.Screens.OTPLogin); });
                break;

            default:
                break;
        }

        Debug.Log(button);
    }
}
