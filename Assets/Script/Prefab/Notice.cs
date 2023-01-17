using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Notice : MonoBehaviour {

    Text text;
    Image image;

	// Use this for initialization
	void Awake () {
        try
        {
            image = gameObject.SearchChild("ImageNotice").GetComponent<Image>();
            text = gameObject.SearchChild("TextNotice").GetComponent<Text>();
        }
        catch (System.Exception e)
        {
            Debug.LogError(Common.GetGameObjectHierarchy(gameObject) + "\n" + e.Message);
        }
	}

    public void Initialize(string text) {
        this.text.text = text;
        image.gameObject.SetActive(text != "0");
    }
}
