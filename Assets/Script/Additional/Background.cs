using UnityEngine;
using System.Collections;

/*
Ez az objektum egy háttérképet tesz ki a képernyőre amit különböző módszerekkel tud a képernyőhöz igazítani.

scale        : A képet úgy torzítja, hogy az teljesen kitöltse a képernyőt.
               Ha nagy arányú a torzítás, akkor a szabályos kör alakú objektumok oválissá válhatnak. 
scaleToFit   : A képet olyan nagyra skálázza amilyen nagyra lehet, úgy, hogy a kép minden része látszódjon torzítás mentesen
               Ekkor előfordulhat, hogy a kép két szélén vagy alul felül látható a háttér (vagy egy fekete csík)
scaleToFill  : A képet torzításmentesen úgy skálázza, hogy az teljesen kitöltse a teljes képernyőt.
               Ekkor előfordulhat, hogy a kép széle vagy az alsó, felső része kilóg a képernyőből.

Ha scaleToFill módszert választjuk, akkor a megadott képből kivág egy részt és azt fogja kirajzolni, hogy a képernyőből kilógó részek ne is létezzenek.
Ez a képernyők közötti váltásnál fontos, ha valamilyen csúszó kép váltást állítunk be pl. SlideUp.

Az objektum létrejöttekor az inspectorban megadott háttérképet helyezi a képernyőre automatikusan.
Ha a program futása során szükségessé válik a háttérkép csere, akkor a ChangeBackground meghívásával
megváltoztathatjuk azt.

Ha több előre beállított háttérképünk van amit az inspectorban a BackgroundArray-ban megadtunk, azok közül
a ChangeBackgroundIndex metódussal választhatunk.
*/

public class Background : MonoBehaviour {

    public enum ScaleMethod
    {
        scale,          // Torzítja és teljesen kitölti
        scaleToFit,     // Nem torzítja és nem tölti ki teljesen
        scaleToFill,    // Nem torzítja és kitölti teljesen
    }

    public ScaleMethod scaleMethod; // Milyen módon skálázza a háttérképet
    public Texture2D background;    // A SpriteRendererből kiolvasott default háttérkép

    public Texture2D[] backgroundArray; // Több háttérképet ebbe kell megadni

    Texture2D workTexture;          // Ez a kivágott képrész amit a backgroundból vágunk ki scaleToFill-nél, más esetben a background referenciáját tartalmazza

    SpriteRenderer spriteRenderer;

    bool itWasRefresh; // Mutatja, hogy volt-e már refresh

    public void Awake() {
        if (name == "MenuBackground")
            Common.menuBackground = this;

        spriteRenderer = GetComponent<SpriteRenderer>();
        Start();
        //background = spriteRenderer.sprite.texture;
    }

	// Use this for initialization
	public void Start () {
        if (!itWasRefresh && spriteRenderer.sprite != null)
            Refresh();
	}

    /// <summary>
    /// Kicseréli a háttérképet a megadott Texture2D-re
    /// </summary>
    /// <param name="newBackground">Az új háttérkép</param>
    public void ChangeBackground(Texture2D newBackground) {

        if (newBackground != null) 
            spriteRenderer.sprite = Sprite.Create(newBackground, new Rect(0, 0, newBackground.width, newBackground.height), new Vector2(0.5f, 0.5f));

        // background = newBackground;

        // A sprite csak akkor látszik, ha van megadva háttérkép
        spriteRenderer.enabled = newBackground != null;

        // Ha van háttérkép, akkor hozzá igazítjuk a képernyő méretéhez
        if (newBackground != null) {
            Refresh();
        }
    }

    /// <summary>
    /// Kicseréli a háttérképet a megadott indexű képre, amit a backgroundArray-ban lehet
    /// beállítani az inspector ablakban.
    /// </summary>
    /// <remarks>
    /// Ha olyan idexet adunk meg ami nem létezik, akkor a háttér képe kikapcsolásra kerül.
    /// </remarks>
    /// <param name="index">Az új háttérkép indexe a backgroundArray-ban.</param>
    public void ChangeBackgroundIndex(int index) {

        ChangeBackground((index < 0 || index >= backgroundArray.Length) ? null : backgroundArray[index]);
    }



    /// <summary>
    /// Beállítja a megadott skálázási módnak megfelelően a gameObject-et
    /// </summary>
    /// <param name="vector">A vektorral azt a méretet adhatjuk meg amelyik méretre a képet skálázni kell.
    /// Ha nem adunk meg méretet, akkor a képernyőt teljesen kifogja tölteni 16:9 méretarányban.</param>
    public void Refresh(Vector3? vector = null) {

        Debug.Log("Background refresh");

        if (vector == null)
        {
            vector = Common.fitSize_16_9();

            /*
            vector = new Vector3(2 * Camera.main.aspect, 2 * Camera.main.orthographicSize);

            // 16:9 -es legyen a kép mindenféleképpen

            // Kiszámoljuk a magassághoz tartozó 16:9 -es szélességet
            vector = new Vector3(2 * Camera.main.orthographicSize / 9 * 16, 2 * Camera.main.orthographicSize);

            // Ha a magassághoz kiszámol szélesség túl széles, tehát kilóg a képernyőből
            if (vector.Value.x > 2 * Camera.main.aspect) {
                // akkor inkább a szélességhez számoljuk ki a magasságot
                vector = new Vector3(2 * Camera.main.aspect, 2 * Camera.main.aspect /16 * 9);
            }
            */
            //float aspect1 = Camera.main.orthographicSize / 9 * 16;
        }

        // Ha scaleToFill-t választottuk, akkor a background képből kivágunk egy megfelelő méretű képet, 
        // amit úgy lehet a képernyőre kifeszíteni, hogy a méretaránya ne változzon
        // A kivágot kép a workTexture-ba kerül
        if (scaleMethod == ScaleMethod.scaleToFill) {
            //workTexture = background;
            background = spriteRenderer.sprite.texture;

            float ratioX = (float)Screen.width / workTexture.width;
            float ratioY = (float)Screen.height / workTexture.height;
            float ratio = Mathf.Max(ratioX, ratioY);

            int newPictureWidth = (int)(Screen.width / ratio);
            int newPictureHeight = (int)(Screen.height / ratio);

            workTexture = new Texture2D(newPictureWidth, newPictureHeight);
            Color[] pixels = background.GetPixels(
                (background.width - newPictureWidth) / 2,
                (background.height - newPictureHeight) / 2,
                newPictureWidth,
                newPictureHeight);

            workTexture.SetPixels(pixels);
            workTexture.Apply();

            ratioX = Screen.width / workTexture.width;
            ratioY = Screen.height / workTexture.height;

            // A workTexture-ból készítünk egy új sprite-ot és elhelyezzük a SpriteRenderer-ben
            Sprite sprite = Sprite.Create(workTexture, new Rect(0, 0, workTexture.width, workTexture.height), new Vector2(0.5f, 0.5f));
            spriteRenderer.sprite = sprite;
        }

        switch (scaleMethod)
        {
            case ScaleMethod.scale:
            case ScaleMethod.scaleToFill:
                transform.localScale = new Vector3(vector.Value.x / spriteRenderer.sprite.bounds.size.x, vector.Value.y / spriteRenderer.sprite.bounds.size.y, 1);
                break;
            case ScaleMethod.scaleToFit:
                float ratioMin = Mathf.Min(
                    vector.Value.x / spriteRenderer.sprite.bounds.size.x, // Az x méret aránya
                    vector.Value.y / spriteRenderer.sprite.bounds.size.y); // Az Y arány

                transform.localScale = new Vector3(ratioMin, ratioMin, 1); // Méretarányokat megtartva skálázunk
                break;
        }

        itWasRefresh = true;
    }








}
