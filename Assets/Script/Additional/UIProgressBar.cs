using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class UIProgressBar : MonoBehaviour {

    Image imageBackground;
    Image imageProgressBar;
    Image imageBorder;

	// Use this for initialization
	void Awake () {
        imageBackground = Common.SearchGameObject(gameObject, "ImageBackground").GetComponent<Image>();
        imageProgressBar = Common.SearchGameObject(gameObject, "ImageProgressBar").GetComponent<Image>();
        imageBorder = Common.SearchGameObject(gameObject, "ImageBorder").GetComponent<Image>();

        SetProgressValue(0);
    }

    /// <summary>
    /// A progressBar értékét adhatjuk meg.
    /// A nulla a 0%, az 1 pedig a 100%
    /// </summary>
    public void SetProgressValue(float value) {

        Debug.Log("Progress value : " + value);

        value = Mathf.Clamp(value, 0, 1);

        //imageProgressBar.rectTransform.rect.width

        imageProgressBar.rectTransform.sizeDelta = new Vector2(
            imageBackground.rectTransform.rect.width * value,
            imageProgressBar.rectTransform.sizeDelta.y
            );
    }
}
