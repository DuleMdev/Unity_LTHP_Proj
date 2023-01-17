using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SetLanguageText : MonoBehaviour
{
    public string textID;

    public bool stop;

    public Common.CallBack_In_String_Out_String dataProvider;

    Text text;
    TextHelper textHelper;

    Image image;

    Dictionary<string, string> dicParams;

    bool start = false; // Jelzi, hogy elindult-e már a script

	// Use this for initialization
	void Awake () {
        text = GetComponent<Text>();
        textHelper = GetComponent<TextHelper>();

        image = GetComponent<Image>();

        if (stop)
            Debug.Log("Stop");

        IDataProvider iDataProvider = Common.SearchClass<IDataProvider>(transform);

        if (iDataProvider != null)
            dataProvider = iDataProvider.DataProvider;

        //dataProvider = Common.SearchClass<IDataProvider>(gameObject).DataProvider;
    }

    void Start()
    {
        Refresh();
        start = true;
    }

    public void SetParams(Dictionary<string, string> dicParams)
    {
        this.dicParams = dicParams;

        Refresh();
    }

    public void AddParams(params string[] keyValuePairs)
    {
        if (dicParams == null)
            dicParams = new Dictionary<string, string>();

        for (int i = 0; i < keyValuePairs.Length - 1; i = i + 2)
        {
            if (!dicParams.ContainsKey(keyValuePairs[i]))
                dicParams.Add(keyValuePairs[i], keyValuePairs[i + 1]);
            else
                dicParams[keyValuePairs[i]] = keyValuePairs[i + 1];
        }

        Refresh();
    }

    public void SetTextID(string textID)
    {
        this.textID = textID;

        Refresh();
    }

    public void Refresh()
    {
        if (Common.languageController)
        {
            if (stop)
                Debug.Log("Stop");

            string translatedText = Common.languageController.Translate(textID, dicParams: dicParams, dataProvider: dataProvider);

            if (textHelper)
            {
                textHelper.SetText(translatedText);

                Debug.Log("SetLanguageText : " + Common.GetGameObjectHierarchy(gameObject));

            }
            else if (text)
            {
                text.text = translatedText;
            }

            if (image)
                image.sprite = Common.languageController.GetSprite(textID);
        }
    }

    void OnEnable()
    {
        if (start)
            Refresh();
    }
}
