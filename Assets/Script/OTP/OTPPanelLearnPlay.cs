using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;

public class OTPPanelLearnPlay : MonoBehaviour
{
    OTPHomework homework;

    GameObject prefabLabel;
    RectTransform content;

    bool getEmailGroupListSuccess; 
    public EmailGroupLists emailGroupLists;

    List<GameObject> madeGameObjects = new List<GameObject>();
    // Start is called before the first frame update
    void Awake()
    {
        prefabLabel = gameObject.SearchChild("PanelLabel").gameObject;
        content = gameObject.SearchChild("Content").GetComponent<RectTransform>();

        prefabLabel.SetActive(false);

    }

    private void Start()
    {
        DrawList();
    }

    public async void DrawList()
    {
        // Kit�r�lj�k a kor�bban l�trehozott GameObject-eket
        for (int i = 0; i < madeGameObjects.Count; i++)
            Destroy(madeGameObjects[i]);

        madeGameObjects.Clear();

        // L�trehozzuk az �j tartalmat
        float posY = 0;
        RectTransform labelTransform;

        ClassYServerCommunication.instance.getMyGroupEditEmailLists((bool success, JSONNode response) => {
                if (success)
                {
                    for (int i = 0; i < response[C.JSONKeys.answer][0][C.JSONKeys.answer].Count; i++)
                    {
                        string groupMailListID = response[C.JSONKeys.answer][0][C.JSONKeys.answer][i][C.JSONKeys.mailListID];
                        ClassYServerCommunication.instance.GetHomeworks(response[C.JSONKeys.answer][0][C.JSONKeys.answer][i][C.JSONKeys.mailListID], (bool success, JSONNode response) => {
                            // Válasz feldolgozása
                            for(int j = 0; j < response[C.JSONKeys.answer].Count; j++) {
                                if(DateTime.Parse(response[C.JSONKeys.answer][j][C.JSONKeys.endTime]) > DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) && DateTime.Parse(response[C.JSONKeys.answer][j][C.JSONKeys.startTime]) <= DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))) {
                                    Debug.Log("AKTÍV");
                                    Debug.Log(response[C.JSONKeys.answer]);
                                    labelTransform = CreateLabel(posY, response[C.JSONKeys.answer][j]["mailList"][C.JSONKeys.name] + " (#" + response[C.JSONKeys.answer][j]["id"] + ")", groupMailListID, response[C.JSONKeys.answer][j]["learnRoutePathID"]);
                                    posY -= labelTransform.sizeDelta.y;
                                }
                            }
                        });
                    }
                }
        });

        ClassYServerCommunication.instance.getMyGroupEditEmailLists((bool success, JSONNode response) => {
            if (success)
            {
                for (int i = 0; i < response[C.JSONKeys.answer][0][C.JSONKeys.answer].Count; i++)
                {
                    string groupMailListID = response[C.JSONKeys.answer][0][C.JSONKeys.answer][i][C.JSONKeys.mailListID];
                    ClassYServerCommunication.instance.GetHomeworks(response[C.JSONKeys.answer][0][C.JSONKeys.answer][i][C.JSONKeys.mailListID], (bool success, JSONNode response) => {
                        for(int j = 0; j < response[C.JSONKeys.answer].Count; j++) {
                            if(DateTime.Parse(response[C.JSONKeys.answer][j][C.JSONKeys.endTime]) <= DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))) {
                                Debug.Log("PASSZÍV");
                                Debug.Log(response[C.JSONKeys.answer]);
                                labelTransform = CreateLabel(posY, response[C.JSONKeys.answer][j]["mailList"][C.JSONKeys.name] + " (#" + response[C.JSONKeys.answer][j]["id"] + ")", groupMailListID, response[C.JSONKeys.answer][j]["learnRoutePathID"]);
                                posY -= labelTransform.sizeDelta.y;
                            }
                        }
                    });
                }
            }
        });

    }

    private RectTransform CreateLabel(float posy, string textId, string groupMailListID, string pathMailListID)
    {
        GameObject newPrefabLabel = Instantiate(prefabLabel, content);
        newPrefabLabel.SetActive(true);
        madeGameObjects.Add(newPrefabLabel);

        //newPrefabLabel.GetComponentInChildren<SetLanguageText>().SetTextID(textId);
        newPrefabLabel.GetComponent<Image>().color = new Color(1.0f,1.0f,1.0f,0.0f);

        newPrefabLabel.GetComponent<OTPHomework>().Initialize(groupMailListID, pathMailListID, textId);

        RectTransform newPrefabLabelRectTransform = newPrefabLabel.GetComponent<RectTransform>();
        newPrefabLabelRectTransform.anchoredPosition = new Vector2(0, posy);
        return newPrefabLabelRectTransform;
    }

    IEnumerator GetEmailGroupList()
    {
        bool ready = false;

        Common.CallBack_In_Bool_JSONNode answerProcessor = (bool success, JSONNode response) =>
        {
            getEmailGroupListSuccess = success;

            // Válasz feldolgozása
            if (success)
            {
                emailGroupLists = new EmailGroupLists(response[C.JSONKeys.answer]);
            }

            ready = true;
        };

        Debug.Log(getEmailGroupListSuccess);

        // Várunk amíg a lekérdezés megjön és feldolgozódik
        while (!ready) { yield return null; }

        // Ha a lekérdezés sikeres volt, akkor kirajzoljuk a listát
        if (getEmailGroupListSuccess) {
            
            DrawList();
        }
    }
}
