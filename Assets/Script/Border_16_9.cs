using UnityEngine;
using System.Collections;

/*
Ez a szkript a képernyők köré egy fekete keretet készít 16:9 arányban
*/

public class Border_16_9 : MonoBehaviour {

	// Use this for initialization
	void Awake () {

        Transform up = gameObject.SearchChild("Up").GetComponent<Transform>();
        Transform down = gameObject.SearchChild("Down").GetComponent<Transform>();
        Transform left = gameObject.SearchChild("Left").GetComponent<Transform>();
        Transform right = gameObject.SearchChild("Right").GetComponent<Transform>();

        Vector3 v = up.gameObject.GetComponent<Renderer>().bounds.size;
        Vector3 screenSize = Common.fitSize_16_9();

        // Hányszorosára kell nyújtani a határoló elemeket
        Vector3 borderSize = new Vector3(screenSize.x / v.x, screenSize.y / v.y);

        // Beállítjuk a határoló Sprite-ok méretét
        up.localScale = borderSize;
        down.localScale = borderSize;
        left.localScale = new Vector3(borderSize.y, borderSize.x);
        right.localScale = new Vector3(borderSize.y, borderSize.x);

        // Beállítjuk a Sprite-ok pozícióját
        up.localPosition = up.localPosition.SetY(screenSize.y / 2);
        down.localPosition = down.localPosition.SetY(- screenSize.y / 2);
        left.localPosition = left.localPosition.SetX(- screenSize.x / 2);
        right.localPosition = right.localPosition.SetX(screenSize.x / 2);

        transform.position = Vector3.zero;
    }
}
