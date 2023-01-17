using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class UIPanelGameResult : MonoBehaviour {

    Image imageColor;
    Image imageLightening;

    Text textFirstRow;
    Text textSecondRow;

	// Use this for initialization
	void Awake () {
        imageColor = GetComponent<Image>();
        imageLightening = Common.SearchGameObject(gameObject, "PanelLightening").GetComponent<Image>();

        textFirstRow = Common.SearchGameObject(gameObject, "TextFirstRow").GetComponent<Text>();
        textSecondRow = Common.SearchGameObject(gameObject, "TextSecondRow").GetComponent<Text>();
    }

    public void SetText(string firstRow = null, string secondRow = null) {
        if (firstRow != null)
            textFirstRow.text = firstRow;

        if (secondRow != null)
            textSecondRow.text = secondRow;
    }

    public void SetColor(Color? color = null, float lightening = -1) {
        if (color != null)
            imageColor.color = color.Value;

        if (lightening != -1)
            imageLightening.color = Color.white * lightening;
    }
}
