using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MarketPlace2021
{
    public class MarketPlace2021_Serious : HHHScreen
    {
        MainMenu menu;
        Fade fade;

        GameObject goPersonalProfilPage;
        GameObject goStartUpperPage;
        GameObject goInvestorPage;
        GameObject goProfitPage;
        GameObject goCertificatePage;

        PersonalProfilPage personalProfilPage;
        StartUpperPage startUpperPage;
        InvestorPage investorPage;
        ProfitPage profitPage;
        CertificatePage certificatePage;

        string actPageName; // Az aktuális page

        //bool initialized; // Volt már inicializálva?

        void Awake()
        {
            menu = gameObject.GetComponentInChildren<MainMenu>();
            fade = gameObject.SearchChild("Cover").GetComponent<Fade>();

            goPersonalProfilPage = gameObject.SearchChild("PersonalProfilPage");
            goStartUpperPage = gameObject.SearchChild("StartUpperPage");
            goInvestorPage = gameObject.SearchChild("InvestorPage");
            goProfitPage = gameObject.SearchChild("ProfitPage");
            goCertificatePage = gameObject.SearchChild("CertificatePage");

            personalProfilPage = GetComponentInChildren<PersonalProfilPage>(true);
            startUpperPage = GetComponentInChildren<StartUpperPage>(true);
            investorPage = GetComponentInChildren<InvestorPage>(true);
            profitPage = GetComponentInChildren<ProfitPage>(true);
            certificatePage = GetComponentInChildren<CertificatePage>(true);
        }

        override public IEnumerator InitCoroutine()
        {
            // Ha az initializáslá már megtörtént, akkor másodjára csak kilépünk
            //if (initialized) yield break;

            DrawDatas();

            ChangePage(C.Texts.PersonalProfil);

            //menu.Initialize(ButtonClick);
            fade.ShowImmediatelly();

            //initialized = true;

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

        public void DrawDatas()
        {
            menu.Initialize(ButtonClick);

            goPersonalProfilPage.SetActive(true);
            goStartUpperPage.SetActive(true);
            goInvestorPage.SetActive(true);

            personalProfilPage.Initialize(OTPMain.instance.marketPlaceData);
            startUpperPage.Initialize(OTPMain.instance.marketPlaceData);
            investorPage.Initialize(OTPMain.instance.marketPlaceData);

            // Opcionálisan elérhető oldalok
            // Ha van webshop, akkor inicializáljuk
            if (!OTPMain.instance.marketPlaceData[C.JSONKeys.webshop][C.JSONKeys.error].AsBool)
            {
                goProfitPage.SetActive(true);
                profitPage.Initialize(OTPMain.instance.marketPlaceData);
            }

            // Ha van bizonyítvány, akkor inicializáljuk
            if (!OTPMain.instance.marketPlaceData[C.JSONKeys.achievements][C.JSONKeys.error].AsBool)
            {
                goCertificatePage.SetActive(true);
                certificatePage.Initialize(OTPMain.instance.marketPlaceData);
            }
        }

        public void setPageImmediatelly(string pageName)
        {
            goPersonalProfilPage.SetActive(C.Texts.PersonalProfil == pageName);
            goStartUpperPage.SetActive(C.Texts.StartUpperPage == pageName);
            goInvestorPage.SetActive(C.Texts.InvestorPage == pageName);
            goProfitPage.SetActive(C.Texts.ProfitPage == pageName);
            goCertificatePage.SetActive(C.Texts.CertificatePage == pageName);

            menu.Selected(pageName);
        }

        public void ChangePage(string pageName)
        {
            StartCoroutine(ChangePageCoroutine(pageName));
        }

        IEnumerator ChangePageCoroutine(string pageName)
        {
            actPageName = pageName;

            // Lekérdezzük a szükséges adatoket a netről
            bool dataArrived = false;
            switch (pageName)
            {
                case C.Texts.PersonalProfil:
                    break;
                case C.Texts.StartUpperPage:
                    break;
                case C.Texts.InvestorPage:
                    break;
                case C.Texts.ProfitPage:

                    break;
                case C.Texts.CertificatePage:

                    break;
            }
            dataArrived = true;

            fade.Show();

            // Várunk amíg megjön a netről az adat és a cover teljesen megjelent
            while (!fade.isFadeFullyShow || !dataArrived) { yield return null; }

            // Beállítjuk a megérkezett adatoknak megfelelően a tartalmat
            setPageImmediatelly(pageName);

            // Eltüntetjük a Cover-t
            fade.Hide();
        }

        public void ButtonClick(string buttonName)
        {
            Debug.Log(buttonName);

            switch (buttonName)
            {
                case "back":
                    Common.screenController.ChangeScreen(C.Screens.OTPMain);
                    break;
                default:
                    ChangePage(buttonName);
                    break;
            }
        }
    }
}
