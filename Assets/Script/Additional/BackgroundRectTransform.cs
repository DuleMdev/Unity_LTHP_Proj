using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundRectTransform : MonoBehaviour {

    public bool extension = true;

    RectTransform rectTransform;

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
        Refresh();
    }

	// Use this for initialization
	void Start () {

	}
	
    /// <summary>
    /// Beállítja a megadott skálázási módnak megfelelően a gameObject-et
    /// </summary>
    /// <param name="requestedSize">A vektorral azt a méretet adhatjuk meg amelyik méretre a képet skálázni kell.
    /// Ha nem adunk meg méretet, akkor a képernyőt teljesen kifogja tölteni 16:9 méretarányban.</param>
    public void Refresh(Vector3? requestedSize = null)
    {
        if (requestedSize == null)
            requestedSize = Common.fitSize_16_9();

        rectTransform.localScale = new Vector3(
            requestedSize.Value.x / rectTransform.sizeDelta.x,
            requestedSize.Value.y / rectTransform.sizeDelta.y,
            1);

        // Extension - Kibővítés : Kibővítjük a Canvas sizeDelta értrékét úgy, hogy a teljes képernyőt eltakarja
        if (extension)
        {
            Vector3 v3 = GetScaleToScreenRatio(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x * v3.x, rectTransform.sizeDelta.y * v3.y);
        }

        /*
        if (requestedSize == null)
            requestedSize = Common.fitSize_16_9();

        Vector3 actLocalScale = rectTransform.localScale;
        Vector2 actSize = new Vector2(rectTransform.sizeDelta.x * actLocalScale.x, rectTransform.sizeDelta.y * actLocalScale.y);

        rectTransform.localScale = new Vector3(
            actLocalScale.x / actSize.x * requestedSize.Value.x,
            actLocalScale.y / actSize.y * requestedSize.Value.y,
            1);

        // Extension - Kibővítés : Kibővítjük a Canvas sizeDelta értrékét úgy, hogy a teljes képernyőt eltakarja
        if (extension)
        {
            Vector3 v3 = GetScaleToScreenRatio(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x * v3.x, rectTransform.sizeDelta.y * v3.y);
        }
        */
    }

    /// <summary>
    /// Vissza adja a képernyő méretét unit-okban.
    /// </summary>
    /// <returns></returns>
    static Vector3 GetScreenSize()
    {
        return new Vector3(Camera.main.orthographicSize * Camera.main.aspect * 2, Camera.main.orthographicSize * 2);
    }

    /// <summary>
    /// Kiszámolja, hogy egy adott méretű ablak hány unit-ot foglalhet el vízszintesen és függőlegesen a képernyőn maximum úgy, hogy teljes egészében látható maradjon.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    static Vector3 GetWindowBestFitSize(float width, float height)
    {
        Vector3 ScreenSize = GetScreenSize();

        float ratioScreen = ScreenSize.x / ScreenSize.y;
        float ratioWindow = width / height;

        // Ha az ablak szélességenek aránya nagyobb mint a képernyő szélességének aránya, akkor az ablak szélessége a képernyő szélességével fog megegyezni, de a magasságából csökkenteni kell, hogy az aránya megmaradjon
        if (ratioWindow > ratioScreen)
        {
            return new Vector3(ScreenSize.x, ScreenSize.x / ratioWindow);
        }
        else // ha viszont a magassága nagyobb, akkora magassága lesz a képernyő magasságával egyező lenni és a szélességét kell csökkenteni
        {
            return new Vector3(ScreenSize.y * ratioWindow , ScreenSize.y);
        }
    }

    /// <summary>
    /// Kiszámolja, hogy mennyivel kell megszorozni a szélesség és a magasság értéket, hogy ugyan az a méret arány jöjjön létre ami a képernyőnek van. Ebből kifolyólag az egyik skála érték mindig 1.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    static Vector3 GetScaleToScreenRatio(float width, float height)
    {
        Vector3 ScreenSize = GetScreenSize();

        float ratioScreen = ScreenSize.x / ScreenSize.y;
        float ratioWindow = width / height;

        // Ha az ablak szélességenek aránya nagyobb mint a képernyő szélességének aránya, akkor az ablak szélessége a képernyő szélességével fog megegyezni, de a magasságát növelni kell, hogy az aránya azonos legyen a képernyőével
        if (ratioWindow > ratioScreen)
        {
            return new Vector3(1, ratioWindow / ratioScreen);
        }
        else // ha viszont a magassága nagyobb, akkora magassága lesz a képernyő magasságával egyező lenni és a szélességét kell növelni
        {
            return new Vector3(ratioScreen / ratioWindow, 1);
        }
    }
}
