using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerBusy : MonoBehaviour
{
    [Tooltip("Mennyi ideig vár míg elkezdődik az előtűnés.")]
    public float showWaitTime;
    [Tooltip("Mennyi idő alatt lesz látható.")]
    public float showAnimTime;
    [Tooltip("Mennyi idő alatt tűnik el.")]
    public float hideAnimTime;
    [Tooltip("A várakozási jel pörgési sebessége. A lépések közti idő.")]
    public float spinSpeed;

    static public ServerBusy instance;

    Image imageBackground;  // Háttér előtűnéséhez
    Image imageBusySignal;  // Signal jel előtűnéséhez
    RectTransform rectTransformBusySignal; // Signal jel forgatásához

    float initialTransparent;   // Mi volt a kezdeti láthatóság

    float remainTime;       // Mennyi idő maradt még az előtűnésig
    float actTransparent;   // Az aktuális láthatóság 0-1;

    bool hide;              // El kell tünnie

    // Use this for initialization
    void Awake()
    {
        instance = this;

        imageBackground = gameObject.SearchChild("Canvas").GetComponent<Image>();
        imageBusySignal = gameObject.SearchChild("BusySignal").GetComponent<Image>();
        rectTransformBusySignal = imageBusySignal.GetComponent<RectTransform>();

        initialTransparent = imageBackground.color.a;

        transform.position = Vector3.zero;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Megmutatja a panelt.
    /// </summary>
    public void Show()
    {
        hide = false;
        remainTime = showWaitTime;

        // Bekapcsoljuk a panelt, hogy takarjon
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Elrejti a panelt
    /// </summary>
    public void Hide()
    {
        hide = true;
    }

    void UpdateTransparent(float value)
    {
        imageBackground.color = imageBackground.color.SetA(value * initialTransparent);
        imageBusySignal.color = imageBusySignal.color.SetA(value);
    }

    public void ButtonClick()
    {
        remainTime = 0;
    }

    void Update()
    {
        if (actTransparent != 0 || remainTime <= 0)
        {
            actTransparent = Mathf.Clamp01(actTransparent + Time.deltaTime / ((hide) ? -hideAnimTime : showAnimTime));
            UpdateTransparent(actTransparent);

            if (hide && actTransparent == 0)
                gameObject.SetActive(false);
        }

        // Ha nem telt le a várakozási idő, akkor még várunk
        if (remainTime > 0)
            remainTime -= Time.deltaTime;

        // Pörgetjük a "homokórát"
        int phase = (int)(Time.realtimeSinceStartup / spinSpeed);
        rectTransformBusySignal.transform.eulerAngles = new Vector3(0, 0, phase * -36);
    }
}
