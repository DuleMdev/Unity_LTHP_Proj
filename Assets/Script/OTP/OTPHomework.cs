using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;

public class OTPHomework : MonoBehaviour
{

    List<CurriculumPathData> listOfCurriculumPath;
    string groupMailListID;
    string pathMailListID;
    Text text;

    void Awake()
    {
        text = gameObject.SearchChild("Text").GetComponent<Text>();
    }

    public void Initialize(string groupMailListID, string pathMailListID, string text)
    {
        this.groupMailListID = groupMailListID;
        this.pathMailListID = pathMailListID;
        this.text.text = text;
    }

    public void Empty() {
        
    }

    public void ButtonClick()
    {

        ClassYServerCommunication.instance.GetPlayableLearnRoutePathList(groupMailListID, (bool success, JSONNode response) =>
        {
            // Válasz feldolgozása
            if (success)
            {
                listOfCurriculumPath = CurriculumPathData.JsonArrayToList(response[C.JSONKeys.answer]);

                for(int i = 0; i < listOfCurriculumPath.Count; i++) {
                    Debug.Log("HAHAHAHAHHAHAHA");
                    Debug.Log(listOfCurriculumPath[i].ID);
                    Debug.Log(pathMailListID);
                    if(listOfCurriculumPath[i].ID == pathMailListID) {
                        Debug.Log("MATCH");

                        Common.CallBack end = () =>
                        {
                            Common.screenController.ChangeScreen(C.Screens.OTPMain);
                        };

                        ServerPlay.instance.PlayLearnPathByNextGameMode(listOfCurriculumPath[i], end);
                    }
                }
            }
        });
    }
}
