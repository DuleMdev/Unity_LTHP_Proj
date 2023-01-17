using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
Ennek a szkriptnek a feladata, hogy ha egy játékban van egy kérdés amihez tartozik egy kép, akkor ez megjelenítse a képet, illetve ha rákattintottak a képre, akkor nagyítsa ki és vissza.
Ez két játékban fordul elő:
1. Millionaire
2. Bubble
*/
public class PictureZoomer : MonoBehaviour
{
    PictureZoomer instance;

    public bool QuestionPictureZoomable;



    Image questionImage;
    Image questionImageBorder;

    Image answerImage;
    Image answerImageBorder;

    GameObject cover;

    Tween.TweenAnimation questionAnimation;

    Vector3 questionPicturePos; // Hol jelenik meg a kérdés képe, ha nincs kinagyítva

    Vector3 answerPicturePos;   // A válasz képe hol jelenik meg, ha nincs kinagyítva

    float animationStartSize;
    float animationEndSize;
    Vector3 animationStartPos;
    Vector3 animaationEndPos;

    bool zoomed;    // Ki van-e nagyítva a kép

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        questionImage = gameObject.SearchChild("ImageQuestion").GetComponent<Image>();
        questionImageBorder = gameObject.SearchChild("ImageQuestionBorder").GetComponent<Image>();

        answerImage = gameObject.SearchChild("ImageAnswer").GetComponent<Image>();
        answerImageBorder = gameObject.SearchChild("ImageAnswerBorder").GetComponent<Image>();

        cover = gameObject.SearchChild("Cover").gameObject;

        questionAnimation = new Tween.TweenAnimation(easeType: Tween.EaseType.easeOutCubic);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Beállítjuk a question képet.
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="picturePos"></param>
    /// <param name="pictureSize"></param>
    void SetQuestionPicture(Sprite sprite, Vector2 picturePos, Vector2 pictureSize, bool isBorderNeed = true)
    {
        questionImage.sprite = sprite;
        questionImageBorder.enabled = isBorderNeed;

        questionAnimation = new Tween.TweenAnimation(
            startPos: Vector3.one * 0.001f,
            endPos: Vector3.one,
            easeType: Tween.EaseType.easeOutCubic,
            time: 1,
            onUpdate: SetQuestionPictureSize
            );

        Tween.StartAnimation(questionAnimation);

        //
        //questionAnimation.startPos = Vector3.one * 0.001f;
        //questionAnimation.endPos = Vector3.one;
        //questionAnimation.easeType = Tween.EaseType.easeInElastic;
        //questionAnimation.onUpdate = SetQuestionPictureSize;
        //questionAnimation.time = 1;
        //
        //

    }

    void SetQuestionPictureSize(object o)
    {
        float value = (float)o;

        questionImage.transform.localScale = new Vector3(value, value, value);
    }


    void SetQuestionPicturePosition(object o)
    {
        float value = (float)o;



    }

    /// <summary>
    /// A főképet megjelenítjük
    /// </summary>
    void ShowPicture()
    {

    }

    /// <summary>
    /// A főképet kinagyítjuk
    /// </summary>
    void MainPictureZoom()
    {

    }

    /// <summary>
    /// Kinagyítunk item képet.
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="picturePos"></param>
    /// <param name="pictureSize"></param>
    void ZoomAnswerPicture(Sprite sprite, Vector2 picturePos, Vector2 pictureSize)
    {

    }

    /// <summary>
    /// A question képet kinagyítja vagy vissza kicsinyíti, attól függően, hogy milyen állapotban van éppen
    /// </summary>
    void ZoomQuestionPictureWithAnim()
    {

    }

    /// <summary>
    /// A kinagyított elemnek vissza kell mennie a helyére.
    /// </summary>
    public void ButtonClick(string buttonName)
    {
        // Csak akkor foglalkozunk a gomb nyomásokkal, ha az animáció már lefutott
        if (questionAnimation.status == Tween.TweenAnimation.AnimationState.finished)
        {
            switch (buttonName)
            {
                case "questionPicture":
                    break;
                case "answerPicture":
                    break;
            }
        }
    }

}
