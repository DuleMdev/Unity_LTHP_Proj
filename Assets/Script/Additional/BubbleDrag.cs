using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*
Ez az objektum a húzható buborékot valósítja meg





*/
public class BubbleDrag : DragItem
{

    public int glowRelativeSortingOrder = 3;

    [Tooltip("Bolyongás mértéke")]
    public float rambleMeasure = 0.2f;             // A bolyongás mértéke
    [Tooltip("Bolyongás minimum sebessége")]
    public float minSpeed = 0.1f;
    [Tooltip("Bolyongás maximum sebessége")]
    public float maxSpeed = 2f;

    Transform staticMoveTransform;          // A buborék bolyongásának Transformja

    SpriteRenderer bubbleSpriteRenderer;    // buborék
    SpriteRenderer gleamSpriteRenderer;     // Csillogás
    SpriteRenderer glowSpriteRenderer;      // ragyogás
    Canvas canvas;
    Text text;
    TEXDraw texDraw;
    //TextMesh textMesh;                      // A jó válaszok ellenőrzésénél szükséges tudni, hogy milyen érték van a buborékon, ez ezért publikus

    Color bubbleColor;                      // A buborék színe

    float rambleSpeed;                      // A bolyongás sebessége
    float ramblePos;

    // Use this for initialization
    public override void Awake()
    {
        base.Awake();

        MoveTransform = transform;
        staticMoveTransform = transform.parent;
        BaseTransform = transform.parent.parent;
        bubbleSpriteRenderer = Common.SearchGameObject(gameObject, "DragMove").GetComponent<SpriteRenderer>();
        itemRenderer = bubbleSpriteRenderer.GetComponent<Renderer>();
        gleamSpriteRenderer = Common.SearchGameObject(gameObject, "Gleam").GetComponent<SpriteRenderer>();
        glowSpriteRenderer = Common.SearchGameObject(gameObject, "Glow").GetComponent<SpriteRenderer>();
        //textMesh = GetComponentInChildren<TextMesh>();
        canvas = gameObject.SearchChild("Canvas").GetComponent<Canvas>();
        text = gameObject.SearchChild("Text").GetComponent<Text>();
        texDraw = gameObject.SearchChild("TEXDraw").GetComponent<TEXDraw>();

        SetOrderInLayer(OrderInLayer);

        rambleSpeed = (float)(Common.random.NextDouble() * (maxSpeed - minSpeed) + minSpeed); // Meghatározzuk a bolyongás sebességét
        if (Common.random.Next(2) == 1) // Meghatározzuk az irányát
            rambleSpeed *= -1;
    }

    // Beállítja a halmaz elemen megjenő képet és a keret színét
    // A itemName a halmaz elem neve, amivel be tudjuk majd azonosítani
    public void Initialize(string itemName, Color? color, int answerIndex)
    {
        this.itemName = itemName;
        bubbleColor = (color == null) ? bubbleSpriteRenderer.color : color.Value;
        this.answerIndex = answerIndex;

        bubbleSpriteRenderer.color = bubbleColor; // color;
        //textMesh.text = itemName;
        text.text = itemName;
        texDraw.text = itemName;
        text.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.Text;
        texDraw.enabled = Common.configurationController.textComponentType == ConfigurationController.TextComponentType.TEXDraw;
    }

    // Beállítjuk a rétegsorrendjét a halmaz elemnek
    public override void SetOrderInLayer(int order)
    {
        bubbleSpriteRenderer.sortingOrder = order;
        gleamSpriteRenderer.sortingOrder = order + 1;
        glowSpriteRenderer.sortingOrder = order + glowRelativeSortingOrder;

        canvas.sortingOrder = order + 2;
        //textMesh.GetComponent<Renderer>().sortingOrder = order + 2;


        /*
        SortingLayerExposed sortingLayerExposed = textMesh.GetComponent<SortingLayerExposed>();
        sortingLayerExposed.renderer.sortingorder = order + 2;
        */
    }

    /*
    // Beállítja az elem bázis pozícióját
    // newPos       - A halmaz elem új pozíciója
    // delay        - mennyit várakozzon míg elindul az új pozícióba
    // Igaz értéket ad vissza ha a pozíció egy új pozíció, hamisat ha már a megadott pozícióban volt
    public override bool SetBasePos(Vector3 newPos, float delay = 0)
    {
        bool returnValue = BaseTransform.position != newPos;

        Vector3 originalMovePos = MoveTransform.position;
        BaseTransform.position = newPos;
        MoveTransform.position = originalMovePos;

        MoveBasePos(delay);

        return returnValue;
    }*/

    
    // Az elemet a bázis pozíciójába mozgatja a megadott idő letelte után
    public override void MoveBasePos(float delay = 0)
    {
        // Ha az eleme a helyén van, akkor a bolyongást nullázzuk
        if (itemInPlace) {
            Vector3 originalMovePos = MoveTransform.position;
            staticMoveTransform.transform.localPosition = Vector3.zero;
            MoveTransform.position = originalMovePos;
        }

            if (delay == 0) delay = 0.001f; // Ha leállítjuk az iTween animációt egy gameObjecten, akkor rögtön nem tudunk egy másikat elindítani rajta, ezért van itt ez a minimális késleltetés
        // iTween animációval az új pozícióba mozgatjuk az elemet
        iTween.Stop(MoveTransform.gameObject); // Leállítjuk az esetlegesen már működő iTween animációkat
        iTween.MoveTo(MoveTransform.gameObject, iTween.Hash("position", Vector3.zero, "islocal", true, "easetype", iTween.EaseType.easeOutCubic, "time", 1, "delay", delay, "oncompletetarget", gameObject, "oncomplete", "MoveBasePosEnd"));
    }

    /*
    // Ha megfogták az elemet, akkor ezzel az eljárással lehet mozgatni
    // dragPos      - word koordináta
    public override void SetDragPos(Vector3 grabWorldPos)
    {
        MoveTransform.position = grabWorldPos;
    }*/

    /*
    public override Vector3 GetDragPos()
    {
        return MoveTransform.position;
    }*/

    
    // Villogtatja az elemet
    public override void FlashingPositive()
    {
        glowSpriteRenderer.color = Color.green;

        base.FlashingPositive();
    }

    
    public override void FlashingNegative()
    {
        glowSpriteRenderer.color = Color.red;

        base.FlashingNegative();
        //StartCoroutine(FlashingCoroutine());
    }

    
    public override IEnumerator FlashingCoroutine()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.2f);
            glowSpriteRenderer.enabled = true;

            yield return new WaitForSeconds(0.2f);
            glowSpriteRenderer.enabled = false;
        }

        animRun = false;
        //yield return null;
    }

    /*
    // Vissza adja az elem globális szélességét
    public override float GetGlobalWidth()
    {
        return 0;
    }*/

    /*
    // Vissza adja az elem globális magasságát
    public override float GetGlobalHeight()
    {
        return 0;
    }*/

    // Update is called once per frame
    void Update () {
        // Buborék bolyongása
        if (!itemInPlace && Common.configurationController.playAnimation)
        {
            ramblePos += rambleSpeed * Time.deltaTime;
            staticMoveTransform.localPosition = new Vector3(Mathf.Sin(ramblePos) * rambleMeasure, Mathf.Cos(ramblePos) * rambleMeasure, 0);
        }
    }

    public void SetPicture(Sprite bubble, Sprite gleam, Color gleamColor) {
        bubbleSpriteRenderer.sprite = bubble;
        gleamSpriteRenderer.sprite = gleam;
        gleamSpriteRenderer.color = gleamColor;
    }
}
