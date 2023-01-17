using UnityEngine;
using System.Collections;

using UnityEngine.UI;

public class CanvasNetworkHUD : MonoBehaviour {

    class FadeController {
        float fadeTime;
        Color color;        // Másodlagos szín az elsődleges a fehér

        bool activate;      // Volt-e esemény
        float remainTime;   // Mennyi idő maradt még a teljes eltűnésig

        Color actColor;

        public FadeController(ref float fadeTime, ref Color color) {
            this.fadeTime = fadeTime;
            this.color = color;
        }

        public void Activate() {
            activate = true;
        }

        public Color GetColor() {
            if (actColor == Color.white)
            {
                remainTime = fadeTime;
                actColor = color;
            }
            else if (activate)
            {
                activate = false;
                actColor = Color.white;
            }
            else {
                remainTime -= Time.deltaTime;
                actColor = new Color(color.r, color.g, color.b, Mathf.InverseLerp(0, fadeTime, remainTime));
            }

            return actColor;
        }
    }

    [Tooltip("Hálózati forgalom mutatásának engedélyezése")]
    public bool HUDEnabled;     // Engedélyezve van a hálózati forgalom mutatása?

    [Tooltip("Hálózati forgalom mutatásának engedélyezése")]
    public bool FPSEnabled;     // Engedélyezve van az FPS mutatása?

    [Tooltip("Mennyi idő alatt tűnjön el a felvillanó ikon")]
    public float fadeTime;

    [Tooltip("Felvillanás után milyen legyen a színe")]
    public Color color;

    Canvas canvas;

    Image imageConnect;
    Image imageData;
    Image imageDisconnect;
    Image imageGroupColor;

    Text text;              // A felhasználó nevének és azonosítójának kiírásához
    Text textShadow;        // A fenti szöveg árnyékának, hogy bármilyen hátteren látszódjon

    Text textGameName;          // Ide írjuk ki az éppen játszott játék azonosítóját vagy nevét

    Text textFPS;           // FPS megjelenítéséhez
    
    FadeController fadeControllerConnect;
    FadeController fadeControllerData;
    FadeController fadeControllerDisconnect;

    GameObject imageDisabled;

    int lastSecond;         // Utoljára melyik másodpercben lett frissítve a számláló
    int fpsCounter;         // A frame-eket számolja

    [HideInInspector]
    public string gameName;

	// Use this for initialization
	void Awake () {
        Common.canvasNetworkHUD = this;

        canvas = Common.SearchGameObject(gameObject, "Canvas").GetComponent<Canvas>();

        imageConnect = Common.SearchGameObject(gameObject, "ImageConnect").GetComponent<Image>();
        imageData = Common.SearchGameObject(gameObject, "ImageData").GetComponent<Image>();
        imageDisconnect = Common.SearchGameObject(gameObject, "ImageDisconnect").GetComponent<Image>();
        imageDisabled = Common.SearchGameObject(gameObject, "ImageDisabled").gameObject;

        imageGroupColor = Common.SearchGameObject(gameObject, "ImageGroupColor").GetComponent<Image>();

        text = Common.SearchGameObject(gameObject, "Text").GetComponent<Text>();
        textShadow = Common.SearchGameObject(gameObject, "TextShadow").GetComponent<Text>();
        textGameName = gameObject.SearchChild("TextGameName").GetComponent<Text>();

        textFPS = gameObject.SearchChild("TextFPS").GetComponent<Text>();

        fadeControllerConnect = new FadeController(ref fadeTime, ref color);
        fadeControllerData = new FadeController(ref fadeTime, ref color);
        fadeControllerDisconnect = new FadeController(ref fadeTime, ref color);

        Connect();
        Data();
        Disconnect();
    }

    public void SetEnabled(bool enabled) {
        canvas.enabled = enabled;
    }

    // Kapcsolat létrejött
    public void Connect() {
        if (HUDEnabled)
            fadeControllerConnect.Activate();
    }

    // Adat érkezett
    public void Data() {
        if (HUDEnabled)
            fadeControllerData.Activate();
    }

    // Megszakadt a kapcsolat
    public void Disconnect() {
        if (HUDEnabled)
            fadeControllerDisconnect.Activate();
    }

    public void SetGroupColor() {
        imageGroupColor.color = (Common.configurationController.userGroup == -1) ? Color.white * 0 : Common.configurationController.groupColors[Common.configurationController.userGroup];
    }

    public void SetText(string IDandName) {
        this.text.text = IDandName;
        textShadow.text = IDandName;
    }

    // Update is called once per frame
    void Update () {
        //Disconnect();
        imageDisabled.SetActive(!Common.HHHnetwork.messageProcessingEnabled && HUDEnabled);

        imageConnect.color = fadeControllerConnect.GetColor();
        imageConnect.gameObject.SetActive(imageConnect.color.a != 0);

        imageData.color = fadeControllerData.GetColor();
        imageData.gameObject.SetActive(imageData.color.a != 0);

        imageDisconnect.color = fadeControllerDisconnect.GetColor();
        imageDisconnect.gameObject.SetActive(imageDisconnect.color.a != 0);

        SetGroupColor();

        // Ha játékban vagyunk, akkor kiírjuk a játék azonosítóját
        if (Common.screenController.actScreen != null)
            textGameName.text = (Common.screenController.actScreen.name.Contains("Game")) ? gameName : "";

        // Kiírjuk az FPS értéket
        if (FPSEnabled)
        {
            fpsCounter++;
            int actSecond = System.DateTime.Now.Second;

            if (lastSecond != actSecond)
            {
                textFPS.text = "FPS : " + fpsCounter;
                lastSecond = actSecond;
                fpsCounter = 0;
            }
        }

    }
}
