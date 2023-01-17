using UnityEngine;
using System.Collections;

/*

A Canvas típusú képernyők hátterét szolgáltatja.
Csak akkor kell bekapcsolva lennie ha ilyen típusú képernyő látható.

Bekapcsolása a Common objektumon keresztűl lehetséges.
Common.canvasBackground.Show(true);

Kikapcsolása
Common.canvasBackground.Show(false);

*/

public class CanvasBackground : MonoBehaviour {

    GameObject canvas;

	// Use this for initialization
	void Awake () {
        Common.canvasBackground = this;

        canvas = Common.SearchGameObject(gameObject, "Canvas").gameObject;
	}

    public void Show(bool show) {
        canvas.SetActive(show);
    }
}
