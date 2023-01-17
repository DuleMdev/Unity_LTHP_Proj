using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StarProgressBar : MonoBehaviour {

    List<PrefabStar> stars;

	// Use this for initialization
	void Awake () {
        // Megkeressük a csillagokat
        stars = new List<PrefabStar>();
        int index = 1;

        while (true)
        {
            GameObject go = gameObject.SearchChild("Star" + index);

            if (go == null)
                break;

            Common.ListAdd(stars, index - 1, go.GetComponent<PrefabStar>(), null);
            index++;
        }
	}

    public void SetProgressBarValue(float value)
    {
        value *= stars.Count; 

        for (int i = 0; i < stars.Count; i++)
        {
            stars[i].SetFillAmount(Mathf.Clamp01(value - i));
        }
    }
}
