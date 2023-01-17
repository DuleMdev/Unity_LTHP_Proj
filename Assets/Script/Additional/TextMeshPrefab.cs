using UnityEngine;
using System.Collections;
using System;

public class TextMeshPrefab : MonoBehaviour, IWidthHeight {

    TextMesh textMesh;
    TEXDraw texDraw;

    void Awake() {
        textMesh = Common.SearchGameObject(gameObject, "TextMesh").GetComponent<TextMesh>();
        texDraw = Common.SearchGameObject(gameObject, "TEXDraw").GetComponent<TEXDraw>();
    }

    public void Initialize(string text) {
        textMesh.text = text;
        texDraw.text = text;

        // Beállítjuk a texDraw méretét a preferált mérethez
        texDraw.GetComponent<RectTransform>().sizeDelta = new Vector2(texDraw.preferredWidth + 1, texDraw.preferredHeight + 1);

        textMesh.GetComponent<MeshRenderer>().enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text;
        texDraw.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.TEXDraw;
    }

    public float GetHeight()
    {
        // A méretet attól függően határozzuk meg, hogy sima textMesh vagy TEXDraw komponenst használunk
        return Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text ?
            textMesh.GetComponent<Renderer>().bounds.size.y :
            texDraw.GetComponent<RectTransform>().rect.height * texDraw.GetComponent<RectTransform>().lossyScale.y;

        //texDraw.GetComponent<Renderer>().bounds.size.y; // preferredWidth * texDraw.transform.localScale.y;
        //return textMesh.GetComponent<Renderer>().bounds.size.y / textMesh.transform.lossyScale.y;
    }

    public float GetWidth()
    {
        // A méretet attól függően határozzuk meg, hogy sima textMesh vagy TEXDraw komponenst használunk
        return Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text ?
            textMesh.GetComponent<Renderer>().bounds.size.x :
            texDraw.GetComponent<RectTransform>().rect.width * texDraw.GetComponent<RectTransform>().lossyScale.x;
            //texDraw.GetComponent<Renderer>().bounds.size.x; // preferredHeight * texDraw.transform.localScale.x;
        //return textMesh.GetComponent<Renderer>().bounds.size.x / textMesh.transform.lossyScale.x;
    }
}
