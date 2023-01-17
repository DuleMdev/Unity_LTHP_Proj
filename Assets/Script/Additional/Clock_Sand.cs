using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock_Sand : Clock_Ancestor
{
//    struct SandShape
//    {
//        /// <summary>
//        /// A pozícióban mennyi a szélesség
//        /// </summary>
//        float posWidth; 
//
//        /// <summary>
//        /// A pozíció milyen magasságban található
//        /// </summary>
//        int posHeight;
//    }

    [Header("Színátmenet")]
    [Space(10)]
    public Color colorStart;
    public Color colorEnd;

    [Space(10)]
    [Tooltip("Mennyi idő alatt ér le az első homokszem (homok csorgás sebessége) ")]
    public float sandFlowTime;

    System.Drawing.Point[] sandUpperShape = {
        new System.Drawing.Point(0, 0),
        new System.Drawing.Point(0, 34),
        new System.Drawing.Point(7, 42),
        new System.Drawing.Point(20, 45),
        new System.Drawing.Point(29, 54),
        new System.Drawing.Point(31, 63),
    };

    System.Drawing.Point[] sandBottomShape = {
        new System.Drawing.Point(35, 0),
        new System.Drawing.Point(0, 21),
        new System.Drawing.Point(0, 50),
    };

    int sandHourInnerWidth = 70;

    Image imageUpperSandContainer;
    Image imageSandFlow;
    Image imageBottomSandContainer;

    RectTransform rectTransformUpperSandContainer;
    RectTransform rectTransformSandFlow;
    RectTransform rectTransformBottomSandContainer;

    Transform transformHourGlassRotate;

    float[] sandUpperLevels; // Homok mennyisége a homokóra felső részében
    float[] sandBottomLevels; // Homok mennyisége a homokóra alsó részében

    float allSandVolume;

    override public void Awake()
    {
        base.Awake();

        // Begyűjtjük a referenciákat
        imageUpperSandContainer = gameObject.SearchChild("ImageSandUpper").GetComponent<Image>();
        imageSandFlow = gameObject.SearchChild("ImageSandFlow").GetComponent<Image>();
        imageBottomSandContainer = gameObject.SearchChild("ImageSandBottom").GetComponent<Image>();

        rectTransformUpperSandContainer = imageUpperSandContainer.GetComponent<RectTransform>();
        rectTransformSandFlow = imageSandFlow.GetComponent<RectTransform>();
        rectTransformBottomSandContainer= imageBottomSandContainer.GetComponent<RectTransform>();

        transformHourGlassRotate = gameObject.SearchChild("HourGlassRotate").GetComponent<Transform>();

        // Kiszámoljuk a homok formában elférő homok mennyiségét
        sandUpperLevels = CalculateSandValume(sandUpperShape);
        sandBottomLevels = CalculateSandValume(sandBottomShape);

        Debug.Log("Upper volume : " + CalculateAllSandVolume(sandUpperLevels));
        Debug.Log("Bottom volume : " + CalculateAllSandVolume(sandBottomLevels));

        allSandVolume = CalculateAllSandVolume(sandBottomLevels);
    }

    // Az óra alaphelyzetbe állítása. Az órát alaphelyzetbe visszapörgeti a megadott idő alatt
    override public void Reset(float resetTime)
    {
        base.Reset(resetTime);

        SetClockTime(timeInterval);

        Tween.TweenAnimation animation = new Tween.TweenAnimation(
            startPos: 0f,
            endPos: 360f,
            easeType: Tween.EaseType.easeOutElastic,
            time: resetTime * 2,
            onUpdate: (object o) => {
                float value = (float)o;

                while (value > 360) { value -= 360; }

                transformHourGlassRotate.eulerAngles = new Vector3(0, 0, value);
            }
        );

        Tween.StartAnimation(animation);
    }

    // beállítja a mutatót a megadott értéknek megfelelően
    override protected void SetClockTime(float time)
    {
        //Debug.Log("Time : " + time);

        // Kiszámoljuk minek hol kell lennie

        float sandVolume = allSandVolume / timeInterval * (timeInterval - time); // Ennyi homok perget már le

        imageUpperSandContainer.fillAmount = 1f / rectTransformUpperSandContainer.sizeDelta.y * CalculateSandHeight(sandUpperLevels, allSandVolume - sandVolume, false);

        //rectTransformSandFlow.gameObject.SetActive(time != 0 && time != timeInterval); // Ha megy az óra, akkor pereg a homok
        rectTransformSandFlow.gameObject.SetActive(status == Status.Go); // Ha megy az óra, akkor pereg a homok

        rectTransformBottomSandContainer.anchoredPosition = new Vector2(0, -132 + CalculateSandHeight(sandBottomLevels, sandVolume, true));

        // Színátmenet elkészítése
        Color newColor = Color.Lerp(colorStart, colorEnd, (timeInterval - time) / timeInterval);

        imageUpperSandContainer.color = newColor;
        imageSandFlow.color = newColor;
        imageBottomSandContainer.color = newColor;
    }

    void SetUpperPartSandVolume(float value)
    {

    }

    void SetBottomPartSandVolume(float value)
    {

    }

    void SetSandflow()
    {

    }

    float[] CalculateSandValume(System.Drawing.Point[] sandShape)
    {
        float[] sandVolume = new float[sandShape[sandShape.Length - 1].Y + 1];

        for (int i = 0; i < sandShape.Length - 1; i++)
        {
            int linePiece = sandShape[i + 1].Y - sandShape[i].Y;
            for (int j = 0; j <= linePiece; j++)
            {
                float actWidth = (sandShape[i].X * (linePiece - j) + sandShape[i + 1].X * j) / (float)linePiece;
                actWidth -= sandHourInnerWidth / 2;
                sandVolume[sandShape[i].Y + j] = actWidth * actWidth * Mathf.PI;
            }
        }

        return sandVolume;
    }

    float CalculateAllSandVolume(float[] sandVolume)
    {
        float sumVolume = 0;

        for (int i = 0; i < sandVolume.Length; i++)
            sumVolume += sandVolume[i];

        return sumVolume;
    }

    float CalculateSandHeight(float[] sandLevels, float sandVolume, bool fromTopToBottom)
    {
        float levelValue = 0;

        int from = fromTopToBottom ? 0 : sandLevels.Length - 1;
        int to = fromTopToBottom ? sandLevels.Length : -1;
        int step = fromTopToBottom ? 1 : -1;

        float sumSandLevels = 0;
        for (int i = from; i != to; i += step)
        {
            if (sumSandLevels + sandLevels[i] <= sandVolume)
            {
                levelValue++;
                sumSandLevels += sandLevels[i];
                continue;
            }
            else
            {
                levelValue += (sandVolume - sumSandLevels) / sandLevels[i];
                break;
            }
        }

        return levelValue;
    }

}
