using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour {

    RectTransform rectTransformProgressBar;
    RectTransform rectTransformProgressBarStripe;

	// Use this for initialization
	void Awake () {
        rectTransformProgressBar = GetComponent<RectTransform>();
        rectTransformProgressBarStripe = gameObject.SearchChild("ImageStripe").GetComponent<RectTransform>();		
	}

    /// <summary>
    /// Beállítja a progressBar értékét 0 - 1 (min - max) érték között.
    /// </summary>
    /// <param name="value"></param>
    public void SetValue(float value)
    {
        float max = rectTransformProgressBar.sizeDelta.x; // - rectTransformProgressBar.sizeDelta.y;
        rectTransformProgressBarStripe.sizeDelta = new Vector2(max * (-1 + Mathf.Clamp01(value)), 0);
    }
}
