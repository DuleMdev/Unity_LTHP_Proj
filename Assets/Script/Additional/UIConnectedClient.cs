using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIConnectedClient : MonoBehaviour {

    [Tooltip("Az alapértelmezett panel szín megadása, akkor lesz használva ha nincs csoportba sorolva az aktuális kliens.")]
    public Color defaultPanelColor;

    Image imagePanel;   // A panel színének megváltoztatásához

    Text textRowNumber; // A sor számának kiírásához
    Text textClientID;  // A kiliens azonosítójának kiírásához
    Text textUserName;  // A felhasználói név kiírásához

	// Use this for initialization
	void Awake () {
        imagePanel = GetComponent<Image>();
        textRowNumber = Common.SearchGameObject(gameObject, "TextRowNumber").GetComponent<Text>();
        textClientID = Common.SearchGameObject(gameObject, "TextClientID").GetComponent<Text>();
        textUserName = Common.SearchGameObject(gameObject, "TextUserName").GetComponent<Text>();

        defaultPanelColor = imagePanel.color;
    }

    public void SetTexts(string rowNumber = null, string clientID = null, string userName = null) {
        if (rowNumber != null) textRowNumber.text = rowNumber;
        if (clientID != null) textClientID.text = clientID;
        if (userName != null) textUserName.text = userName;
    }

    public void SetPanelColor(Color? color) {
        imagePanel.color = (color == null) ? defaultPanelColor : color.Value;
    }
}
