using UnityEngine;
using System.Collections;

public class CanvasScreenController : MonoBehaviour {

    CanvasScreenBase[] canvasScreens;        // A scene-n levő összes Canvas alapú képenyőt tárolja

    [Tooltip("Melyik képernyővel kezdődjön az alkalmazás")]
    public CanvasScreenBase startCanvasScreen;                   // Az inspector ablakban itt megadott képernyővel fog az alkalmazás indulni

    // Use this for initialization
    void Awake () {
        Common.canvasScreenController = this;

        canvasScreens = FindObjectsOfType<CanvasScreenBase>(); // Program indulásánál összegyűjtjük a HHHScreen szkripteket
        AllScreenOff(); // Majd mindet kikapcsoljuk
    }

    // Kikapcsolja az összes képernyőt
    void AllScreenOff()
    {
        foreach (CanvasScreenBase canvasScreen in canvasScreens)
            canvasScreen.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
