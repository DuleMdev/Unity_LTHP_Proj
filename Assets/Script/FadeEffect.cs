using UnityEngine;
using System.Collections;

/*

Ez az objektum valósítja meg a fade Effect-et

Az elmélete az, hogy a kamera és az aktuális képernyő közé egy képet tesz, ami kitölti a teljes képernyőt,
majd a képet fokozatosan a teljesen átlátszóból láthatóvá tesszük minek következtében fokozatosan egyre jobban
kitakarja az alatta levő képet.

A kitakarásra használt kép lehet egyszínű és lehet egy konkrét kép is.
FadeIn() - Ha nem adunk meg semmit, akkor az inspector ablakban a fadeColor változóba beállított színnel fogja a kitakarást elvégezni.
FadeIn(color) - Ha megadunk egy színt, akkor azzal a színnel lesz kitakarva a teljes képernyő.
FadeIn(index) - Ha egy számot adunk meg, akkor az inspector ablakban a transitionPictures tömbben tárolt képek közül az indexedikkel fogja kitakarni a képernyőt.

FadeOut() - A kitakaró kép vagy szín eltűnik.

Mindegyik metódusnál opcionálisan meg lehet adni egy paraméternélküli függvényt, amit a Fade metódus lefutása után vissza hív, így tudatva, hogy befejezte a működését.

*/

public class FadeEffect : MonoBehaviour {

    [Tooltip("Milyen gyors legyen az effect (másodpercben)")]
    public float effectSpeed;               // Az effect sebessége

    [Tooltip("Az alapértelmezett fade effekt színe")]
    public Color fadeColor;

    [Tooltip("Az átmenethez használt képek")]
    public Sprite[] transitionPictures;     // képernyő váltások között ezekből a képekből lehet választani a kép indexének segítségével

    public delegate void CallBackEmpty();   // Egy paramétert nem váró függvényt

    SpriteRenderer spriteRenderer;          // 

    //CallBackEmpty callBack;                 // Ha a fadeIn vagy fadeOut befejeződött, akkor meghívjuk a megadott függvényt ha megadták

    void Awake() {
        //Common.fadeEffect = this;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void FadeInColor(Color? color = null, float? effectSpeed = null, Common.CallBack callBack = null)
    {
        if (color == null) color = fadeColor;
        if (effectSpeed == null) effectSpeed = this.effectSpeed;

        SetPicture(0);
        spriteRenderer.color = color.Value;

        StartCoroutine(FadeInCoroutine(effectSpeed.Value, callBack));
    }

    public void FadeInPicture(int pictureIndex, float? effectSpeed = null, Common.CallBack callBack = null)
    {
        if (effectSpeed == null) effectSpeed = this.effectSpeed;

        SetPicture(pictureIndex);

        StartCoroutine(FadeInCoroutine(effectSpeed.Value, callBack));
    }

    IEnumerator FadeInCoroutine(float effectSpeed, Common.CallBack callBack = null)
    {
        ColorUpdate(0);

        iTween.ValueTo(gameObject, iTween.Hash("from", 0, "to", 1, "time", effectSpeed, "easetype", iTween.EaseType.linear, "onupdate", "ColorUpdate", "onupdatetarget", gameObject));

        yield return new WaitForSeconds(effectSpeed);

        if (callBack != null)
            callBack();
    }

    // iTween ValueTo metódusa hívja meg, amikor változtatni kell az szín értékét
    void ColorUpdate(float value)
    {
        Color oldColor = spriteRenderer.color;
        spriteRenderer.color = new Color(oldColor.r, oldColor.g, oldColor.b, value);
    }

    // Beállítjuk, hogy a megadott indexű kép teljesen lefedje a képernyőt
    void SetPicture(int pictureIndex) {
        Sprite sprite = transitionPictures[pictureIndex]; // Kiválasztjuk, hogy melyik képet kell megjeleníteni

        // A FadeEffect szkriptet tartalmazó GameObject-et úgy skálázzuk, hogy a rajta levő kép teljesen kitöltse a képernyőt
        transform.localScale = new Vector3(Camera.main.aspect / sprite.bounds.size.x * 2, 2 / sprite.bounds.size.y);

        spriteRenderer.sprite = sprite;     // A képet megjelenítjük
        spriteRenderer.color = Color.white; // A kép színét fehérre állítjuk (Lehet, hogy korábban a FadeColor-ral valamilyen másik színt állítottunk be, tehát azt semlegesíti ez az utasítás)
    }

    // A képernyőről eltávolítja a takarást a megadott sebességgel
    public void FadeOut(float? effectSpeed = null, Common.CallBack callBack = null)
    {
        if (effectSpeed == null) effectSpeed = this.effectSpeed;

        StartCoroutine(FadeOutCoroutine(effectSpeed.Value, callBack));
    }

    // A képernyőt takaró kép eltünése
    IEnumerator FadeOutCoroutine(float effectSpeed, Common.CallBack callBack = null)
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", 1, "to", 0, "time", effectSpeed, "easetype", iTween.EaseType.linear, "onupdate", "ColorUpdate", "onupdatetarget", gameObject));

        yield return new WaitForSeconds(effectSpeed);

        if (callBack != null)
            callBack();
    }
}
