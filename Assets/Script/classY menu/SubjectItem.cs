using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SubjectItem : MonoBehaviour
{
    Image imageSubjectIcon;
    Text textSubjectName;

	// Use this for initialization
	void Awake () {
        imageSubjectIcon = gameObject.SearchChild("ImageSubjectIcon").GetComponent<Image>();
        textSubjectName = gameObject.SearchChild("ImageSubjectName").GetComponent<Text>();
    }

    public void Initialize(Sprite subjectIcon, string subjectName) {
        imageSubjectIcon.sprite = subjectIcon;
        textSubjectName.text = subjectName;
    }
}