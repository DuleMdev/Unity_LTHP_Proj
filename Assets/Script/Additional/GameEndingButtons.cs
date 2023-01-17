using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ez az osztály a játékokban kezeli a játék végeknél megjelenő gombokat
/// Ez Teszt módban megjelenő Újra és Tovább gombok.
/// </summary>

public class GameEndingButtons : MonoBehaviour
{
    Text nextOrSelfVerifyButtonText;   // Az önellenőrzés gomb felíratának módosításához
    Text againButtonText;              // Újra gomb felíratának módosításához

    Common.CallBack_In_String buttonClick;

    public void Awake()
    {
        try
        {
            if (gameObject.SearchChild("VerifyButtonText") != null)
                nextOrSelfVerifyButtonText = gameObject.SearchChild("VerifyButtonText").GetComponent<Text>();
            if (gameObject.SearchChild("AgainButtonText") != null)
                againButtonText = gameObject.SearchChild("AgainButtonText").GetComponent<Text>();
        }
        catch (System.Exception e)
        {
            Debug.LogError(Common.GetGameObjectHierarchy(gameObject) + "\n" + e.Message + "\n" + e.StackTrace);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // A verify gomb szövegének beállítása nyelvfüggően
        if (nextOrSelfVerifyButtonText != null) // Ha van Verify gomb a scene-ben
        {
            if (Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.NoFeedback &&
                nextOrSelfVerifyButtonText != null)
                nextOrSelfVerifyButtonText.text = Common.languageController.Translate(C.Texts.Next);
            if (Common.configurationController.answerFeedback == ConfigurationController.AnswerFeedback.ShelfCheckButton)
                nextOrSelfVerifyButtonText.text = Common.languageController.Translate(C.Texts.Verify);
            nextOrSelfVerifyButtonText.transform.parent.gameObject.SetActive(Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately && Common.taskController.task.differentAnswerFeedbackEnabled);

            // Ha replay módban vagyunk, akkor legyen next gomb és természetesen látszódjon is
            if (Common.taskController.task.replayMode)
            {
                nextOrSelfVerifyButtonText.text = Common.languageController.Translate(C.Texts.Next);
                nextOrSelfVerifyButtonText.transform.parent.gameObject.SetActive(true);
            }
        }

        // Again gomb szöveegéneke beállítása nyelvfüggően
        if (againButtonText != null)
        {
            againButtonText.text = Common.languageController.Translate(C.Texts.Again);
            againButtonText.transform.parent.gameObject.SetActive(Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately && Common.taskController.task.differentAnswerFeedbackEnabled);
        }
    }

    public void Initialize(Common.CallBack_In_String buttonClick)
    {
        this.buttonClick = buttonClick;
    }

    public void ButtonClick(string buttonName)
    {
        if (buttonClick != null)
            buttonClick(buttonName);
    }
}
