using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITextyInput
{
    int GetQuestionIndex();
    int GetSubQuestionIndex();
    string GetText();

    void SetText(string text);

    bool WasAnswer();

    void Interactable(bool interactable);
    float Flashing(bool positive);


}
