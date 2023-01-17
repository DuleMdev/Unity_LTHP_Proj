using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketPlacePanel : MonoBehaviour
{
    MarketPlaceMenuStripe menuStripe;

    MarketPlaceProjectProductsPanel projectProductsPanel;
    MarketPlaceCompaniesProfilePanel companiesProfilePanel;
    MarketPlaceOwnProfilePanel ownProfilePanel;

    JSONNode jsonData;
    string lang;        // Az utolsó Initialize-nél milyen nyelvű volt az app, ha változik újra kell inicializálni

    GameObject showPanel;   // Melyik panelt mutatjuk éppen

    static public DateTime actPeriodEnd; // Az aktuális periódnak mikor van vége
    static public bool timeCountDown;   // Az aktuális periódnak működik-e a visszaszámlálója
    static public string hours;         // Hány óra van hátra
    static public string remainTime;    // Mennyi idő maradt az aktuális periódból, ha órák, akkor órában írja ki, ha kevesebb mint 1 óra akkor percben és másodpercben
    static public bool lastHour;        // Ha az utolsó órában vagyunk, akkor ez true

    // Use this for initialization
    void Awake ()
    {
        menuStripe = gameObject.SearchChild("MenuStripe").GetComponent<MarketPlaceMenuStripe>();

        projectProductsPanel = gameObject.SearchChild("ProjectProductsPanel").GetComponent<MarketPlaceProjectProductsPanel>();
        companiesProfilePanel = gameObject.SearchChild("CompaniesProfilesPanel").GetComponent<MarketPlaceCompaniesProfilePanel>();
        ownProfilePanel = gameObject.SearchChild("OwnProfilePanel").GetComponent<MarketPlaceOwnProfilePanel>();
    }

    void Start()
    {
        PreInitialize();
        ShowPanel(projectProductsPanel.gameObject);
    }

    // Előzetesen feltöltjük az értékeket valamilyen alap értékekkel
    public void PreInitialize()
    {
        menuStripe.Initialize();

        GameObject go = showPanel;
        ShowPanel(null);
        projectProductsPanel.Initialize();
        companiesProfilePanel.Initialize("-", "0", "0", "0", "0", "0", "0");
        ownProfilePanel.Initialize("-", "0", "0", "0", "0", "0", "0", "0");
        ShowPanel(go);
    }

    // Ha megjött a szervertől a válasz, akkor frissítjük az adatokat a szerver válaszának megfelelően
    public void Initialize(JSONNode json)
    {
        jsonData = json;
        lang = Common.configurationController.applicationLangName;

        menuStripe.Initialize();

        GameObject go = showPanel;
        ShowPanel(null);
        projectProductsPanel.Initialize(json, json[C.JSONKeys.ownProfile][C.JSONKeys.investable]);
        companiesProfilePanel.Initialize(json[C.JSONKeys.companyProfile]);
        ownProfilePanel.Initialize(json[C.JSONKeys.ownProfile]);
        ShowPanel(go);
    }

    /// <summary>
    /// Bekapcsolja a megadott GameObjectű panelt. Ha null-t adunk meg, akkor mindegyiket bekapcsolja.
    /// </summary>
    /// <param name="go"></param>
    void ShowPanel(GameObject go)
    {
        if (go != null)
            menuStripe.Selected(go.name);

        projectProductsPanel.gameObject.SetActive(projectProductsPanel.gameObject == go || go == null);
        companiesProfilePanel.gameObject.SetActive(companiesProfilePanel.gameObject == go || go == null);
        ownProfilePanel.gameObject.SetActive(ownProfilePanel.gameObject == go || go == null);

        showPanel = go;
    }

    void Update()
    {
        if (lang != Common.configurationController.applicationLangName && jsonData != null)
            Initialize(jsonData);

        if (timeCountDown)
        {
            TimeSpan timeSpan = actPeriodEnd - DateTime.Now;

            hours = ((long)(timeSpan.TotalHours)).ToString();
            remainTime = hours;

            lastHour = false;
            if ((long)(timeSpan.TotalHours) < 1)
            {
                lastHour = true;
                remainTime = (timeSpan.Minutes.ToString()).PadLeft(2, '0') + ":" + (timeSpan.Seconds.ToString()).PadLeft(2, '0');
            }

            if (timeSpan.Ticks < 0)
            {
                // Frissítjük a Piac tér adatait
                OTPMain.instance.Refresh();

                timeCountDown = false;
            }
        }
    }

    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case C.Texts.ProjectProducts:
                ShowPanel(projectProductsPanel.gameObject);
                break;
            case C.Texts.CompaniesProfiles:
                ShowPanel(companiesProfilePanel.gameObject);
                break;
            case C.Texts.OwnProfile:
                ShowPanel(ownProfilePanel.gameObject);
                break;
        }
    }
}
