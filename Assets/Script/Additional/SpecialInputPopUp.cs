using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialInputPopUp : MonoBehaviour
{
    public enum InputType
    {
        plainText,
        fraction,
    }

    FractionInput fractionInput;

    InputType inputType;
    Common.CallBack_In_String callBack;

    // Start is called before the first frame update
    void Awake()
    {
        fractionInput = gameObject.SearchChild("FractionInput").GetComponent<FractionInput>();
    }

    static public InputType getSpecialInputType(string s)
    {
        if (FractionInput.IsFraction(s).Success)
            return InputType.fraction;

        return InputType.plainText;
    }

    public string GetInitText(string s)
    {
        switch (getSpecialInputType(s))
        {
            case InputType.fraction: return fractionInput.GetInitText(s); break;
            default: return null; break;
        }
    }

    public void Show(string initText, string actText, Common.CallBack_In_String callBack)
    {
        this.callBack = callBack;

        gameObject.SetActive(true);

        inputType = getSpecialInputType(initText);

        switch (inputType)
        {
            case InputType.fraction: fractionInput.Initialize(initText, actText); break;
        }

        fractionInput.gameObject.SetActive(inputType == InputType.fraction);
    }

    public string GetResult()
    {
        string result = "";
        switch (inputType)
        {
            case InputType.fraction:
                result = fractionInput.GetResult();
                break;
            default:
                break;
        }

        return result;
    }

    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Ok":
                gameObject.SetActive(false);
                callBack(GetResult());
                break;

            case "Exit":
                gameObject.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s1">Amit várunk válasznak</param>
    /// <param name="s2">Amit válaszoltak</param>
    /// <returns></returns>
    static public bool IsEqual(string s1, string s2)
    {
        InputType inputType = getSpecialInputType(s1);

        switch (inputType)
        {
            case InputType.fraction:  return FractionInput.IsEqual(s1, s2); break;
        }

        // Ha számokat tartalmaz a válasz akkor legyen egyenlő a 5.0 == 5 == 5,0 is
        float f1;
        float f2;
        if (Common.tryNumberParse(s1.Remove(" "), out f1) && Common.tryNumberParse(s2.Remove(" "), out f2))
        {
            return f1 == f2;
        }

        return s1 == s2;
    }
}
