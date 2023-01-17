using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

static public class URLParameterReader
{
    //static string URL = "https://my.domain.name/myPath.html?myParameter=Hello%20World";

    static Dictionary<string, string> parameters;

    static URLParameterReader()
    {
        Calculate(Application.absoluteURL, unEscape: true);
    }

    static public void AddURL(string URL, bool unEscape)
    {
        Calculate(URL, unEscape);
    }

    static public string GetParameter(string parameterName)
    {
        string value;
        if (parameters.TryGetValue(parameterName, out value))
            return value;

        return null;
    }

    static public void WriteParameters()
    {
        foreach (string item in parameters.Keys)
        {
            Debug.Log("Key : " + item + " | Value : " + parameters[item]);
        }
    }

    static public bool KeyIsExists(string keyName)
    {
        bool result = parameters.ContainsKey(keyName);
        return result;
        //return parameters.ContainsKey(keyName);
    }

    static void Calculate(string URL, bool unEscape)
    {
        Debug.Log("URLParameterReader Calculate start");

        Debug.Log("URL : " + URL);

        parameters = new Dictionary<string, string>();
        URL = (unEscape) ? WWW.UnEscapeURL(URL) : URL;

        Debug.Log("URL : " + URL);

        // [^?]* először minden karaktert feldolgozunk ami nem kérdőjel.
        // \? Ha meg van a kérdőjel, akkor feldolgozzuk a kérdőjelet. A kérdőjelet semlegesíteni kell a speciális jelentésétől, ami az, hogy az előtte álló dolog vagy van vagy nincs.
        // (([^=]+)=([^?]+)&?)* ha kérdőjelet találtunk, akkor utána van egy kulcs érték pár = jellel elválsztva pl. ?firstParameter=apple.
        //      ([^=]+) Minden karakter megfelelő ami nem egyenlőségjel.
        //      = ezután egy egyenlőség jel következik.
        //      ([^&]+) végül mindent feldolgozunk addig amíg nem találunk egy & jelet.
        //      &? ezután feldolgozzuk az & jelet, ami vagy van vagy nincs ha csak egy paraméter van.
        //      )* az előző kifejezést többször megpróbáljuk megkeresni, lehet, hogy több paraméter is van.
        // Kulcs érték párokból ugyan annyi fog keletkezni.
        // A kulcs a második csoportban az érték a harmadik csoportban fog keletkezni.
        string reg = @"[^?]*\?(([^=]+)=([^&]+)&?)*";

        Match match = Regex.Match(URL, reg);
        if (match.Success)
            for (int i = 0; i < match.Groups[2].Captures.Count; i++)
                parameters.Add(match.Groups[2].Captures[i].Value, match.Groups[3].Captures[i].Value);

        // Test
        string s = "Linkben talált paraméterek:\n";
        foreach (KeyValuePair<string, string> entry in parameters)
            s += entry.Key + " = " + entry.Value + "\n";
        Debug.Log(s);

        Debug.Log("URLParameterReader Calculate end");
    }
}
