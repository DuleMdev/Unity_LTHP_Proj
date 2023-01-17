using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketPlaceMenuStripe : MonoBehaviour
{
    MarketPlaceMenuItem projectProducts;
    MarketPlaceMenuItem companiesProfiles;
    MarketPlaceMenuItem ownProfile;

    // Use this for initialization
    void Awake () {
        projectProducts = gameObject.SearchChild("ProjectProducts").GetComponent<MarketPlaceMenuItem>();
        companiesProfiles = gameObject.SearchChild("CompaniesProfiles").GetComponent<MarketPlaceMenuItem>();
        ownProfile = gameObject.SearchChild("OwnProfile").GetComponent<MarketPlaceMenuItem>();
    }

    public void Initialize()
    {
        projectProducts.Initialize(Common.languageController.Translate(C.Texts.ProjectProducts));
        companiesProfiles.Initialize(Common.languageController.Translate(C.Texts.CompaniesProfiles));
        ownProfile.Initialize(Common.languageController.Translate(C.Texts.OwnProfile));
    }

    public void Selected(string selectedMenuItem)
    {
        projectProducts.Selected(selectedMenuItem == C.Texts.ProjectProducts + "Panel");
        companiesProfiles.Selected(selectedMenuItem == C.Texts.CompaniesProfiles + "Panel");
        ownProfile.Selected(selectedMenuItem == C.Texts.OwnProfile + "Panel");
    }
}
