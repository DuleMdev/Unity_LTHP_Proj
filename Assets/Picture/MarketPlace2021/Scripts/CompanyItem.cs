using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompanyItem : MonoBehaviour
{

    public Color colorInvestable;
    public Color colorNoInvestable;
    public Color colorInvested;

    Image imageCompanyLogoCircle;
    Image imageCompanyLogoCube;

    Text textCompanyNameLogo;
    Text textCompanyName;

    Common.CallBack_In_String callBack;
    string buttonName;

    // Start is called before the first frame update
    void Awake()
    {
        imageCompanyLogoCircle = gameObject.SearchChild("ImageCompanyLogo").GetComponent<Image>();
        imageCompanyLogoCube = gameObject.SearchChild("ImageCommonCompanyLogo").GetComponent<Image>();

        textCompanyNameLogo = gameObject.SearchChild("TextCompanyNameLogo").GetComponent<Text>();
        textCompanyName = gameObject.SearchChild("TextCompanyName").GetComponent<Text>();
    }

    public void Initialize(JSONNode data, string buttonName, Common.CallBack_In_String callBack)
    {
        this.callBack = callBack;
        this.buttonName = buttonName;

        textCompanyNameLogo.text = data[C.JSONKeys.observedGroupName];
        textCompanyName.text = data[C.JSONKeys.observedGroupName];

        Color color = colorNoInvestable;
        if (data[C.JSONKeys.investable].AsBool) color = colorInvestable;
        if (data[C.JSONKeys.isOwnInvestment].AsBool) color = colorInvested;

        imageCompanyLogoCircle.color = color;
        imageCompanyLogoCube.color = color;
    }

    public void ButtonClick()
    {
        callBack(buttonName);
    }
}
