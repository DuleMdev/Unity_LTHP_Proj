using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketPlace2021_Infantile : HHHScreen
{

    public Color studentPageColor;
    public Color startUpperPageColor;
    public Color entrepreneurPageColor;
    public Color profitPageColor;

    enum PageType
    {
        student,
        startUpper,
        entrepreneur,   // Vállalkozói
        profit,
    }

    Fade fade;
    Image canvasImage;


    PageType pageType;

    void Awake()
    {

    }

    override public IEnumerator InitCoroutine()
    {
        fade = gameObject.SearchChild("Cover").GetComponent<Fade>();
        canvasImage = gameObject.SearchChild("Canvas").GetComponent<Image>();

        fade.ShowImmediatelly();
        StartCoroutine(ChangePage(PageType.student));

        yield return null;
    }

    /// <summary>
    /// Amikor az új képernyő megjelenítése elkezdődik akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator ScreenShowStartCoroutine()
    {
        yield return null;
    }

    /// <summary>
    /// Amikor az új képernyő megjelenítése befejeződik, akkor ezt meghívja a ScreenController 
    /// </summary>
    /// <returns></returns>
    override public IEnumerator ScreenShowFinishCoroutine()
    {
        yield return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ChangePage(PageType pageType)
    {
        this.pageType = pageType;

        // Lekérdezzük a szükséges adatoket a netről
        bool dataArrived = false;
        switch (pageType)
        {
            case PageType.student:
                break;
            case PageType.startUpper:
                break;
            case PageType.entrepreneur:
                break;
            case PageType.profit:
                break;
        }

        dataArrived = true;

        // A Cover-t a előhozzuk a kívánt lap szinével
        Color pageColor = Color.white;
        switch (pageType)
        {
            case PageType.student:      pageColor = studentPageColor;       break;
            case PageType.startUpper:   pageColor = startUpperPageColor;    break;
            case PageType.entrepreneur: pageColor = entrepreneurPageColor;  break;
            case PageType.profit:       pageColor = profitPageColor;        break;
        }

        fade.gameObject.GetComponent<Image>().color = pageColor;
        fade.Show();

        // Várunk amíg megjön a netről az adat és a cover teljesen megjelent
        while (!fade.isFadeFullyShow || !dataArrived) { yield return null; }

        // Beállítjuk a megérkezett adatoknak megfelelően a tartalmat
        canvasImage.color = pageColor;


        // Eltüntetjük a Cover-t
        fade.Hide();
    }

    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "next":
                if (pageType < PageType.profit)
                    pageType++;
                else
                    pageType = 0; // PageType.student;
                StartCoroutine(ChangePage(pageType));
                break;

            case "back":
                Common.screenController.ChangeScreen(C.Screens.OTPMain);
                break;
        }
    }
}
