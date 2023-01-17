using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    [Tooltip("A megadott image-ből fogja venni a fade szinét")]
    public Image getColor;

    [Tooltip("Mennyi idő alatt jelenjen meg a takarás")]
    public float fadeOnTime = 0.5f;
    [Tooltip("Mennyi idő alatt tünjön el a takrarás")]
    public float fadeOffTime = 0.5f;

    Image imageCover;           // Takarás
    float initialTransparent;   // Mi volt a kezdeti láthatóság

    // Látszódjon-e a fade
    bool show;
    float actTransparent; // Az aktuális láthatóság

    public bool isFadeFullyShow {
        get {
            return actTransparent == 1;
        }
    }

    public bool isFadeFullyHide {
        get
        {
            return actTransparent == 0;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        imageCover = GetComponent<Image>();
        initialTransparent = imageCover.color.a;
    }

    /// <summary>
    /// Elindítjuk a fade effektet.
    /// </summary>
    public void Show()
    {
        show = true;
        // Bekapcsoljuk a panelt, hogy takarjon
        gameObject.SetActive(true);
        if (fadeOnTime == 0)
            actTransparent = 1;

        UpdateTransparent(actTransparent);
    }

    // Azonnal teljesen láthatóvá tesszük
    public void ShowImmediatelly()
    {
        actTransparent = 1;
        Show();
    }

    /// <summary>
    /// Eltüntetjük a fade effektet.
    /// </summary>
    public void Hide()
    {
        show = false;
    }

    void UpdateTransparent(float value)
    {
        imageCover.color = imageCover.color.SetA(value * initialTransparent);
    }

    // Update is called once per frame
    void Update()
    {
        if (getColor != null)
            imageCover.color = getColor.color;

        actTransparent = Mathf.Clamp01(actTransparent + Time.deltaTime / ((show) ? fadeOnTime : -fadeOffTime));
        UpdateTransparent(actTransparent);

        if (!show && actTransparent == 0)
            gameObject.SetActive(false);

    }
}
