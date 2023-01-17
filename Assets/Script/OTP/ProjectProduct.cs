using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A Provocent alkalmazásban a tanulói csoportok által készített termék megjelenítéséhez készült.
/// </summary>
public class ProjectProduct : MonoBehaviour
{
    Image imageShadow;
    ClassYIconControl iconControl;
    Text textMadeBy;                // Termék készítőjének cégneve
    GameObject investedSignal;      // Befektetésed jelző
    Text textInvested;

    public string buttonName { get { return iconControl.buttonName; } }

    // Use this for initialization
    void Awake()
    {
        imageShadow = gameObject.SearchChild("Shadow").GetComponent<Image>();
        iconControl = gameObject.SearchChild("ClassYIconControl").GetComponent<ClassYIconControl>();
        textMadeBy = gameObject.SearchChild("TextMadeBy").GetComponent<Text>();

        investedSignal = gameObject.SearchChild("InvestedSignal").gameObject;
        textInvested = gameObject.SearchChild("TextInvested").GetComponent<Text>();
    }

    public void Initialize(bool backgroundDark, ProjectProductData itemData, string buttonName, Common.CallBack_In_String buttonClick, bool? showIcon = null)
    {
        imageShadow.enabled = backgroundDark;

        textInvested.text = Common.languageController.Translate(C.Texts.OwnInvestment);

        iconControl.Initialize(itemData.productName, buttonName, buttonClick, itemData.investable ? ColorBuilder.GetColor(itemData.productName) : Color.gray, showIcon);
        textMadeBy.text = Common.languageController.Translate(C.Texts.Company) + ": " + itemData.companyName;

        investedSignal.SetActive(itemData.invested);
    }

    public void Empty(bool backgroundDark)
    {
        imageShadow.enabled = backgroundDark;

        iconControl.gameObject.SetActive(false);
    }

    public void SetActive(bool active)
    {
        iconControl.SetActive(active);
    }

    public class ProjectProductData
    {
        public string projectProductID { get; private set; }
        public string gameID { get; private set; }
        public string companyName { get; private set; }
        public string productName { get; private set; }
        public bool investable { get; private set; }
        public bool invested { get; private set; }

        public ProjectProductData(string projectProductID, string gameID, string companyName, string productName, bool investable, bool invested)
        {
            this.projectProductID = projectProductID;
            this.gameID = gameID;
            this.companyName = companyName;
            this.productName = productName;
            this.invested = invested;
            this.investable = investable;
        }

        public ProjectProductData(JSONNode jsonNode)
        {
            projectProductID = jsonNode[C.JSONKeys.id];
            gameID = jsonNode[C.JSONKeys.gameID];
            companyName = jsonNode[C.JSONKeys.observedGroupName];
            productName = jsonNode[C.JSONKeys.gameName];
            invested = jsonNode[C.JSONKeys.isOwnInvestment].AsBool;
            investable = jsonNode[C.JSONKeys.investable].AsBool;
        }
    }
}