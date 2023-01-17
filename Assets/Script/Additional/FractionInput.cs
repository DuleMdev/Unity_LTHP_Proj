using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class FractionInput : MonoBehaviour
{
    class SplitInput
    {
        public bool success;

        public string numerator;
        public int? int_numerator;
        public string denominator;
        public int? int_denominator;

        public bool fixNumerator;
        public bool fixDenumerator;

        public SplitInput(Match match)
        {
            success = match.Success;

            if (match.Success)
            {
                numerator = match.Groups[3].Captures[0].Value.Trim();
                denominator = match.Groups[6].Captures[0].Value.Trim();

                int i;
                if (!int.TryParse(numerator, out i))
                    int_numerator = i;
                if (!int.TryParse(denominator, out i))
                    int_denominator = i;

                fixNumerator = match.Groups[2].Captures.Count > 0;
                fixDenumerator = match.Groups[5].Captures.Count > 0;
            }
        }
    }

    InputField numeratorInputField;
    InputField denominatorInputField;

    // Start is called before the first frame update
    void Awake()
    {
        numeratorInputField = gameObject.SearchChild("InputFieldNumerator").GetComponent<InputField>();
        denominatorInputField = gameObject.SearchChild("InputFieldDenominator").GetComponent<InputField>();
    }

    static public Match IsFraction(string text)
    {
        return Regex.Match(text, @"\s*\\frac\s*{((\s*fix:)?([^}]*))}\s*{((\s*fix:)?([^}]*))}");
    }

    /// <summary>
    /// Text valami ilyesmi : \fract {1}{2}
    /// </summary>
    /// <param name="text"></param>
    public void Initialize(string initText, string actText)
    {
        Match matchInit = IsFraction(initText);
        Match matchAct = IsFraction(actText); // Regex.Match(text, @"\s*\\frac\s*{\s*(\d+)\s*}\s*{\s*(\d+)\s*}");

        if (matchInit.Success)
        {
            /*
            string result = string.Format($"Match: {match.Value} | Index : {match.Index} | Length : {match.Length} \n");
            for (int groupCtr = 0; groupCtr < match.Groups.Count; groupCtr++)
            {
                Group group = match.Groups[groupCtr];
                result += string.Format($"   Group {groupCtr}: {group.Value} | Index : {group.Index}| Length : {group.Length} \n");
                for (int captureCtr = 0; captureCtr < group.Captures.Count; captureCtr++)
                    result += string.Format($"      Capture {captureCtr}: {group.Captures[captureCtr].Value} | Index : {group.Captures[captureCtr].Index}| Length : {group.Captures[captureCtr].Length} \n");
            }

            Debug.Log(result);
            */

            int i;
            numeratorInputField.text = "";
            denominatorInputField.text = "";

            numeratorInputField.text = intParse(matchAct.Groups[3].Captures[0].Value);
            numeratorInputField.enabled = matchInit.Groups[2].Captures.Count == 0;

            denominatorInputField.text = intParse(matchAct.Groups[6].Captures[0].Value);
            denominatorInputField.enabled = matchInit.Groups[5].Captures.Count == 0;
            /*
            if (!int.TryParse(match.Groups[1].Captures[0].Value, out i))
                numeratorInputField.text = i.ToString();
            if (!int.TryParse(match.Groups[2].Captures[0].Value, out i))
                denominatorInputField.text = i.ToString();
                */
        }
    }

    static public bool IsEqual(string s1, string s2)
    {
        Match match1 = IsFraction(s1);
        Match match2 = IsFraction(s2);

        if (match1.Success && match2.Success)
        {
            string numerator1 = match1.Groups[3].Captures[0].Value.Trim();
            string denominator1 = match1.Groups[6].Captures[0].Value.Trim();
            string numerator2 = match2.Groups[3].Captures[0].Value.Trim();
            string denominator2 = match2.Groups[6].Captures[0].Value.Trim();

            if (numerator1 == numerator2 && denominator1 == denominator2)
            {
                return true;
            }
            else
            {
                int int_numerator1;
                int int_denominator1;
                int int_numerator2;
                int int_denominator2;

                // Ha mindegyik felismerhető számként
                if (int.TryParse(numerator1, out int_numerator1) && int.TryParse(denominator1, out int_denominator1) &&
                    int.TryParse(numerator2, out int_numerator2) && int.TryParse(denominator2, out int_denominator2))
                {
                    // Nincs a nevezőben nulla
                    if (int_denominator1 != 0 && int_denominator2 != 0)
                    {
                        if (Mathf.Abs((float)(int_numerator1) / int_denominator1 - (float)(int_numerator2) / int_denominator2) <= float.Epsilon) // 0.000001)
                            return true;
                    }
                }
            }
        }

        return false;
    }


    static string intParse(string s)
    {
        string result;

        try
        {
            int i = int.Parse(s);
            result = i.ToString();
        }
        catch (System.Exception)
        {
            result = "";
        }

        return result;
    }

    public string GetInitText(string text)
    {
        SplitInput splitInput = new SplitInput(IsFraction(text));

        return "\\frac{" + (splitInput.fixNumerator ? splitInput.numerator : "?") + "}{" + (splitInput.fixDenumerator ? splitInput.denominator : "?") + "}";

        //return @"\frac{?}{?}";
    }

    public string GetResult()
    {
        return "\\frac{" + (string.IsNullOrWhiteSpace(numeratorInputField.text) ? "?" : numeratorInputField.text) + "}{" + 
                           (string.IsNullOrWhiteSpace(denominatorInputField.text) ? "?" : denominatorInputField.text) + "}";
    }
}
