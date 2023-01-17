using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarStriped : MonoBehaviour
{
    public Color strip1Color;
    public Color strip2Color;
    public Color backgroundColor;

    public float stripDistance;

    [Range(0, 1)]
    public float value;


    RectTransform progressBar;  // A progressBar méretének lekérdezéséhez

    GameObject stripes;         // Egy csík darabka. Ezt sokszorosítjuk

    RectTransform moveStripes;  // A létrehozott csíkok mozgatásához
    RectTransform moveLightCover;   // A csíkokon levő fény végének megszakításához ezt kell mozgatni

    Image lightStripColor1;
    Image lightBackground;

    Text textPercent;       // A százalék kiírásához







    List<GameObject> listOfStripes = new List<GameObject>();



    // Start is called before the first frame update
    void Awake()
    {
        progressBar = GetComponent<RectTransform>();

        stripes = gameObject.SearchChild("Stripes").gameObject;

        moveStripes = gameObject.SearchChild("MoveStripes").GetComponent<RectTransform>();
        moveLightCover = gameObject.SearchChild("MoveLightCover").GetComponent<RectTransform>();

        lightStripColor1 = gameObject.SearchChild("LightStripColor1").GetComponent<Image>();
        lightBackground = gameObject.SearchChild("LightBackground").GetComponent<Image>();

        textPercent = gameObject.SearchChild("TextPercent").GetComponent<Text>();

        Createstripes();
    }

    void Createstripes()
    {
        // Ha korábban már létrehoztunk vonalakat akkor azokat eltávolítjuk
        foreach (var item in listOfStripes)
            Destroy(item);

        listOfStripes = new List<GameObject>();

        // Létrehozzuk az új tartalmat
        float maxSize = progressBar.rect.width + 100;
        float actPos = 0;
        bool firstColor = true;
        while (Mathf.Abs(actPos) < maxSize)
        {
            GameObject newStripe = Instantiate(stripes, moveStripes);
            listOfStripes.Add(newStripe);

            // Beállítjuk a létrehozott sávnak a pozícióját
            newStripe.GetComponent<RectTransform>().anchoredPosition = new Vector2(actPos, 0);
            actPos -= stripDistance;

            // Beállítjuk a létrehozott sávnak a színét
            newStripe.GetComponent<Image>().color = firstColor ? strip1Color : strip2Color;
            firstColor = !firstColor;
        }

        // Színezzük a tartalmakat a beállításoknak megfelelően
        lightStripColor1.color = strip1Color;
        lightBackground.color = backgroundColor;
    }

    public void SetValue(float value)
    {
        textPercent.text = ((int)(value * 100)).ToString() + "%";

        float maxSize = progressBar.rect.width + 20;
        value = 1 - value;

        moveStripes.anchoredPosition = new Vector2(-maxSize * value - 24, 0);
        moveLightCover.anchoredPosition = new Vector2(-maxSize * value - 24, 0);
    }

    // Update is called once per frame
    void Update()
    {
        //SetValue(value);
    }
}
