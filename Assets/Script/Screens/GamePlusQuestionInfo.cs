using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlusQuestionInfo : MonoBehaviour
{
    public static GamePlusQuestionInfo instance;

    [Tooltip("Mennyi idő alatt lesz látható.")]
    public float showAnimTime;
    [Tooltip("Mennyi idő alatt tűnik el.")]
    public float hideAnimTime;

    CanvasGroup canvasGroup;
    TextyGameTextAndPictureInScroll textAndPictureInScroll;

    /// <summary>
    /// Mutatni kell a tartalmat.
    /// </summary>
    public bool show { get; private set; }

    /// <summary>
    /// Aktuálisan mennyire látszik a panel
    /// </summary>
    float actTransparent;

    void Awake()
    {
        instance = this;

        canvasGroup = gameObject.SearchChild("Canvas").GetComponent<CanvasGroup>();
        textAndPictureInScroll = GetComponentInChildren<TextyGameTextAndPictureInScroll>();

        transform.position = Vector3.zero;

        show = false;
        canvasGroup.alpha = 0;
        //gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        Hide();
    }

    public void Initialize(string plusInfo, Common.CallBack_In_String_Out_Sprite getSprite, Zoomer zoomer)
    {
        textAndPictureInScroll.Initialize(plusInfo, getSprite, zoomer);
    }

    public void Show()
    {
        // Megmutatjuk a panelt
        show = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        // Eltüntetjük a panelt
        show = false;
        //gameObject.SetActive(false);
    }

    public void Reset()
    {
        show = false;
        actTransparent = 0;

        // Ezeket elintézi az Update
        //canvasGroup.alpha = 0;
        //gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        actTransparent = Mathf.Clamp01(actTransparent + Time.deltaTime / ((show) ? showAnimTime : -hideAnimTime));
        canvasGroup.alpha = actTransparent;

        if (!show && actTransparent == 0)
            gameObject.SetActive(false);
    }

    public void ExitButton()
    {
        Hide();
    }
}
