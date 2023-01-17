using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PrefabStar : MonoBehaviour {

    Image imageStarFill;

	// Use this for initialization
	void Awake () {
        imageStarFill = gameObject.SearchChild("ImageStarFill").GetComponent<Image>();
	}

    public void SetFillAmount(float fillAmount) {
        imageStarFill.fillAmount = fillAmount;
    }
}
