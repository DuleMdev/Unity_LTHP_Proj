using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuInformation : MonoBehaviour {

    /*
    enum InfoPanelType
    {
        AutoGroupNumber,
        SelectGroupNumber,
        AutoStartNextLessonMosaic,
        ExitFromLessonPlan,
        PauseLessonPlan,
        SureStartLessonPlan,
    }
    */

    [Tooltip("Mennyi idő alatt ugorjon elő az infopanel.")]
    public float fadeInTime;

    [Tooltip("Mennyi idő alatt tünjön el az infopanel.")]
    public float fadeOutTime;

    Image imageBackground;                  // Szürke elsőtétítés
    GameObject backgroundPanel;             // A panel képe ami előugrik középről

    GameObject actInfoPanel;                // Éppen melyik infoPanel-t mutatjuk

    public bool show { get; private set; }  // Látszik-e valamilyen panel teljesen (fogadhat a panel gombnyomásokat)

    bool animEnd;   // Befejeződött az eltüntetés animáció?

	// Use this for initialization
	void Awake () {
        Common.menuInformation = this;

        // Bekapcsoljuk a fő Canvas-t, mert előfordulhat, hogy az editorban ki van kapcsolva, hogy ne zavarjon
        //Common.SearchGameObject(gameObject, "Canvas").GetComponent<Canvas>().enabled = true;

        imageBackground = Common.SearchGameObject(gameObject, "BackgroundFade").GetComponent<Image>();
        backgroundPanel = Common.SearchGameObject(gameObject, "BackgroundPanel").gameObject;

        imageBackground.gameObject.SetActive(true);
    }

    void Start() {
        imageBackground.gameObject.SetActive(false);

        show = false;
    }

    /// <summary>
    /// Az infoPanelek hívják ezt a metódust ha meg kell mutatniuk magukat.
    /// </summary>
    /// <param name="infoPanel"></param>
    public void Show(GameObject infoPanel) {
        show = false;
        StartCoroutine(ShowCoroutine(infoPanel));
    }

    IEnumerator ShowCoroutine(GameObject infoPanel) {
        animEnd = false;

        // Ha van régi infoPanel, amit éppen mutatunk, akkor azt kikapcsoljuk
        if (actInfoPanel != null)
        {
            actInfoPanel.SetActive(false);
        }
        else { // Ha nincs akkor elszürkítjük a hátteret
            imageBackground.gameObject.SetActive(true);
            iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 0.5, "time", fadeInTime / 4, "easetype", iTween.EaseType.linear, "onupdate", "ImageFadeUpdate", "onupdatetarget", gameObject));
        }

        actInfoPanel = infoPanel;

        // Közben előúgrik az új panel
        backgroundPanel.transform.localScale = Vector3.one * 0.001f;
        infoPanel.SetActive(true);
        iTween.ScaleTo(backgroundPanel, iTween.Hash("islocal", true, "scale", Vector3.one, "time", fadeInTime, "easeType", iTween.EaseType.easeOutElastic, "oncomplete", "iTweenAnimationComplete", "oncompletetarget", gameObject));

        while (!animEnd) { yield return null; }


        //yield return new WaitForSeconds(fadeInTime);

        show = true;
    }

    public void Hide(Common.CallBack callBack = null) {
        show = false;

        StartCoroutine(HideCoroutine(callBack));
    }

    public IEnumerator WaitShowFinish() {
        while (!show)
            yield return null;
    }

    IEnumerator HideCoroutine(Common.CallBack callBack) {
        // Csak akkor tüntetjük el, ha valami látszik
        if (actInfoPanel != null)
        {
            animEnd = false;

            // Ha a panel előbukkanása után gyorsan alakarjuk tüntetni, akkor kelhet leállítani az előbukkanó animációt
            iTween.Stop(gameObject);
            iTween.Stop(backgroundPanel);
            yield return null;

            // Eltüntetjük a hátteret
            iTween.ValueTo(gameObject, iTween.Hash("from", 0.5, "to", 0, "time", fadeOutTime, "easetype", iTween.EaseType.linear, "onupdate", "ImageFadeUpdate", "onupdatetarget", gameObject));

            // Közben összezsugorítjuk a panelt
            backgroundPanel.transform.localScale = Vector3.one;
            iTween.ScaleTo(backgroundPanel, iTween.Hash("islocal", true, "scale", Vector3.zero * 0.001f, "time", fadeOutTime, "easeType", iTween.EaseType.linear, "oncomplete", "iTweenAnimationComplete", "oncompletetarget", gameObject));

            while (!animEnd) { yield return null; }

            actInfoPanel.SetActive(false); // Összezsugorítás után kikapcsoljuk a panelt
            actInfoPanel = null;

            imageBackground.gameObject.SetActive(false); // Kikapcsoljuk a hátteret
        }

        if (callBack != null)
            callBack();
    }

    void iTweenAnimationComplete() {
        animEnd = true;
    }

    /// <summary>
    /// iTween ValueTo metódusa hívja meg, amikor változtatni kell az szín értékét 
    /// </summary>
    /// <param name="value"></param>
    void ImageFadeUpdate(float value)
    {
        imageBackground.color = imageBackground.color.SetA(value);
    }
}
