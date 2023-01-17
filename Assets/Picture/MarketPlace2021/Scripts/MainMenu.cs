using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarketPlace2021
{
    public class MainMenu : MonoBehaviour
    {
        GameObject prefabMenuItem;

        List<MainMenuItem> listOfMenuItem = new List<MainMenuItem>();

        // Start is called before the first frame update
        void Awake()
        {
            prefabMenuItem = gameObject.SearchChild("MenuItem").gameObject;
            prefabMenuItem.SetActive(false);
        }

        public void Initialize(Common.CallBack_In_String callBackButton)
        {
            // Töröljük az esetlegesen már létező menü elemeket
            for (int i = 0; i < listOfMenuItem.Count; i++)
                Destroy(listOfMenuItem[i].gameObject);
            listOfMenuItem.Clear();

            MakeMenuItem(C.Texts.PersonalProfil, callBackButton);
            MakeMenuItem(C.Texts.StartUpperPage, callBackButton);
            MakeMenuItem(C.Texts.InvestorPage, callBackButton);
            if (!OTPMain.instance.marketPlaceData[C.JSONKeys.webshop][C.JSONKeys.error].AsBool)
                MakeMenuItem(C.Texts.ProfitPage, callBackButton);

            if (!OTPMain.instance.marketPlaceData[C.JSONKeys.achievements][C.JSONKeys.error].AsBool)
                MakeMenuItem(C.Texts.CertificatePage, callBackButton);

            //MakeMenuItem(C.Texts.ProfitPage, callBackButton);
            //MakeMenuItem(C.Texts.CertificatePage, callBackButton);
        }

        void MakeMenuItem(string name, Common.CallBack_In_String callBackButton)
        {
            MainMenuItem menuItem = Instantiate(prefabMenuItem, gameObject.transform).GetComponent<MainMenuItem>();
            menuItem.gameObject.SetActive(true);
            menuItem.Initialize(name, callBackButton);

            listOfMenuItem.Add(menuItem);
        }

        public void Selected(string buttonName)
        {
            for (int i = 0; i < listOfMenuItem.Count; i++)
                listOfMenuItem[i].Selected(buttonName);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
