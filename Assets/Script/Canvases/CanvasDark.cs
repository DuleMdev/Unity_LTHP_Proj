using UnityEngine;
using System.Collections;

public class CanvasDark : MonoBehaviour {

    /*
    Besötétíti a képernyőt ha nem a játékos jön lépéssel.

    Ennek az objektumnak most persze nem sok értelme van, de lehetséges, hogy a sötétítés
    később animálva lesz megoldva és akkor már lesz értelme.

    Amikor nem a játékos jön lépéssel, akkor a képernyőn fogaskerekek forognak körbe jelezve, hogy más lép éppen.
    */

    Transform gearLittle;
    Transform gearMedium;
    Transform gearBig;

    public float speed = 3;    // Mennyi idő alatt fordul körbe a kis fogaskerék

    // Use this for initialization
    void Awake () {
        Common.canvasDark = this;

        gearLittle = Common.SearchGameObject(gameObject, "GearLittle").transform;
        gearMedium = Common.SearchGameObject(gameObject, "GearMedium").transform;
        gearBig = Common.SearchGameObject(gameObject, "GearBig").transform;

        Dark(false);
	}

    public void Dark(bool dark) {
        gameObject.SetActive(dark);
    }

    void Update() {
        gearLittle.Rotate(Vector3.back * 360 / speed * Time.deltaTime);
        gearMedium.Rotate(Vector3.back * 360 / speed * Time.deltaTime / 2);
        gearBig.Rotate(Vector3.forward * 360 / speed * Time.deltaTime / 3);
    }
}
