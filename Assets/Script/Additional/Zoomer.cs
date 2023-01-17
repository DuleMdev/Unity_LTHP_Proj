using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
Ennek a szkriptnek a feladata, hogy képeket nagyítson ki, illetve ha rákattintottak a kinagyított képree, akkor tegye vissza az eredeti helyére.
Ez négy játékban fordul elő:
1. Millionaire
2. Bubble
3. Halmazos, ahol a halmaz elemeket lehet kinagyatani a segítségével
4. Texty

A zoom metódus meghívásával lehet a nagyítást elvégezni.
Ott meg kell adni a
goMagnify - Ami az a gameOject amit nagyítani kell.
A nagyítást kétféleképpen lehet elvégezni
Skálázással
RectTransform esetén a deltaSize értékének megváltoztatásával.
Ha a goMagnify tartalmaz egy RectTransformot, akkor az utóbbi fogja használni egyébként az előbbit.
Továbbá
lehet egy fix méretszeresére
Vagy a képernyőhöz képest a teljes kitöltöttséghez képest.
A fix méretes nagyításhoz meg kell adni a zoomFactor-t.
Ha ez nulla, akkor a képernyőhöz képest történik a nagyítás.
Ebben az esetben tudni kell, hogy mekkora a nagyítandó terület, ezért meg kell adni a goPicture paramétert is ami segítségével
a nagyítandó objektum méretét fogjuk meghatározni.
Ha ez nincs megadva, akkor a goMagnify paraamétert fogjuk erre a célra is használni.
továbbá meg lehet adni, hogy a nagyításkor helyben hagyja az objektumot vagy helyezze át a zoomer objektumra.
Bizonyos esetekben át kell helyezni. Például akkor amikor a Texty játékban a scrollBox-ban levő képet kell nagyítani, mivel ha 
nem tennénk át, akkor a scrollBox-ba nagyítódna ki, viszont a scrollBox levágja azokat a részeket amik kilógnának belőle.

*/
public class Zoomer : MonoBehaviour
{
    public delegate void ZoomBackCallBack();    // Egy DragItem paramétert váró és semmit sem vissza adó függvényt deklarálunk

    static public Zoomer instance;

    public ZoomBackCallBack beforeZoomBackStartCallBack;    // Mielőtt a kinagyított elem vissza menne a helyére ez meghívódik
    public ZoomBackCallBack zoomBackFinishCallBack;         // Mikor befejeződött a helyre vivő animáció

    /*
    static public bool active {
        get {
            return instance && (instance.imageCover.enabled == true);
        }
    }
    */

    public bool active
    {
        get
        {
            return imageCover.activeSelf;
        }
    }

    GameObject imageCover;   // Ez akadályozza meg, hogy máshová is kattintani lehessen, ha a nagyítás éppen aktív

    GameObject pictureHolding;  // Erre a gameObject-re helyezzük át a képet, ha át kell helyezni

    Tween.TweenAnimation tweenAnimation;

    /// <summary>
    /// A GameObject amit mozgatni és nagyítani kell
    /// </summary>
    [HideInInspector]
    public GameObject goMagnify;

    /// <summary>
    /// Milyen mértékben kell a nagyítást végrehajtani
    /// </summary>
    float zoomFactor;

    bool scaleable;

    /// <summary>
    /// A nagyítandó objektumot át kell-e helyezni.
    /// </summary>
    bool moveable;

    /// <summary>
    /// Ha a nagyítandó elemet át kell helyezni, akkor tudni kell, hogy hová kell vissza tenni. Ezt tárolja ez a változó.
    /// </summary>
    Transform goMagnifyOriginalParentTransform;

    /// <summary>
    /// A GameObject kezdeti globális pozíciója
    /// </summary>
    [HideInInspector]
    public Vector3 goMagnifyOriginalPos;

    /// <summary>
    /// A GameObject cél pozíciója
    /// </summary>
    [HideInInspector]
    public Vector3 itemZoomedPos;

    /// <summary>
    /// A kép kezdeti mérete
    /// </summary>
    Vector3 originalSize;

    /// <summary>
    /// Vissza zsugorításnál a pozíciót lokális vagy globálisan kell beállítani
    /// </summary>
    [HideInInspector]
    public bool animSetLocalPos;

    [Tooltip("A nagyítás maximális mérete a képernyőhöz képest. Ha ez 1 akkor a képernyő teljes szélességét vagy magasságát kitöltheti a kinagyított kép ")]
    public float maxSize = 1;

    /// <summary>
    /// A nagyítandó elem RectTransformot tartalmaz-e, mert akkor nem a scale tulajdonságával fogjuk nagyítani, hanem a sizeDelta tulajdonságával
    /// </summary>
    RectTransform canvasRectTransform;

    // Start is called before the first frame update
    void Awake()
    {
        imageCover = gameObject.SearchChild("Cover").gameObject;
        pictureHolding = gameObject.SearchChild("PictureHolding");

        if (transform.parent.GetComponent<ScreenController>()) // !=  null)
            instance = this;

        tweenAnimation = new Tween.TweenAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// A megadott gameObject-et a képernyő közeépre helyezi és felnagyítja a zoomFactor szerinti mérték szerint.
    /// Ha rectTransform meg van adva, akkor az alapján nagyít a maxSize szerint. Ilyenkor a zoomFactor figyelmen kívűl van hagyva ha esetleg az is meg lenne adva.
    /// </summary>
    /// <param name="goMagnify">A GameObject amit nagyítani kell</param>
    /// <param name="zoomFactor">Milyen mértékben kell a nagyítást végre hajtani</param>
    /// <param name="scale">Skálázzással vagy deltaSize-al végezze a nagyítást</param>
    /// <param name="goPicture">A rectTranstorm ami alapján nagyítani kell</param>
    /// <remarks>
    /// 
    /// </remarks>
    public void Zoom(GameObject goMagnify, float zoomFactor = 0, bool scaleable = false, GameObject goPicture = null, bool moveable = false)
    {
        this.goMagnify = goMagnify;
        this.zoomFactor = zoomFactor;
        this.scaleable = scaleable;
        this.moveable = moveable;

        goMagnifyOriginalParentTransform = goMagnify.transform.parent;

        // A méretet a goPicture vagy ha az nincs megadva, akkor a goMagnify objektumból határozzuk meg
        GameObject go = goPicture != null ? goPicture : goMagnify;

        canvasRectTransform = go.GetComponent<RectTransform>(); ;
        //RectTransform rectTransform = go.GetComponent<RectTransform>();

        // Ha nincs megadva a nagyítási méret, akkor meghatározzuk
        if (zoomFactor == 0)
        {
            // Meghatározzuk a kép méretét
            float pictureSizeX;
            float pictureSizeY;

            if (canvasRectTransform)
            {
                // Lekérdezzük a megadott rectTransform sarokpontjait. Óramutató járással azonosan következnek az első a bal alsó, aztán a bal felső stb.
                Vector3[] v = new Vector3[4];
                canvasRectTransform.GetWorldCorners(v);
                //go.GetComponent<RectTransform>().GetWorldCorners(v);

                // Kiszámoljuk a képet tartalmazó rectTransform méretét
                pictureSizeX = Mathf.Abs(v[0].x - v[2].x);
                pictureSizeY = Mathf.Abs(v[0].y - v[2].y);
            }
            else
            {
                Bounds bounds = go.transform.GetComponent<Renderer>().bounds;
                Vector3 v3 = bounds.size;

                pictureSizeX = v3.x;
                pictureSizeY = v3.y;
            }

            // Kiszámoljuk a maaximális lehetséges méretét a nagyítandó képnek
            Vector2 screenSize = /* Common.fitSize_16_9() * maxSize; */  Common.ScreenSizeInUnit() * maxSize;

            // Kiszámoljuk mekkorára kell nagyítani a képet, hogy maximális legyen a mérete
            float xRatio = screenSize.x / pictureSizeX;
            float yRatio = screenSize.y / pictureSizeY;

            // A kiszámolt nagyítási értékek közül a kisebbet választjuk
            this.zoomFactor = Mathf.Min(xRatio, yRatio);
        }

        // Ha a megadott gameObject-en Canvas van, akkor a méret változtatást nem a scale property-n keresztűl kell megvalósítani, hanem a RectTransform width és Height property-jén keresztűl
        //canvasRectTransform = null;
        //if (goMagnify.GetComponent<Canvas>())
        //    canvasRectTransform = goMagnify.GetComponent<RectTransform>();

        // Elmentjük a korábbi értékeket
        goMagnifyOriginalPos = goMagnify.transform.position;

        //originalSize = scaleable ? goMagnify.transform.localScale : canvasRectTransform.sizeDelta;

        if (scaleable)
            originalSize = goMagnify.transform.localScale;
        else
            originalSize = canvasRectTransform.sizeDelta;

        // Felkészülés a nagyításra
        animSetLocalPos = false;   // Kinagyításnál mindig globális pozíciót használunk
        itemZoomedPos = Vector3.zero; // Célpozíció beállítása. (A képernyő közepére nagyítsa)

        // Beállítjuk az animáció paramétereit
        tweenAnimation.time = 1;
        tweenAnimation.startPos = 0f;
        tweenAnimation.endPos = 1f;
        tweenAnimation.easeType = Tween.EaseType.easeOutCubic;
        tweenAnimation.onUpdate = SetAnim;
        tweenAnimation.onComplete = null;

        // Ha engedélyezve van az áthelyezés, akkor áthelyezzük
        if (moveable)
            goMagnify.transform.SetParent(pictureHolding.transform); //   parent = pictureHolding.transform;

        // Elindítjuk az animációt
        Tween.StartAnimation(tweenAnimation);

        // "Láthatóvá" tesszük a zoomer canvas-t, hogy takarja a játék elemeket és rálehessen kattintani a vissza kicsinyítéshez
        imageCover.SetActive(true);
    }

    void SetAnim(object o)
    {
        float value = (float)o;

        // Pozíció beállítása
        if (animSetLocalPos)
        {
            goMagnify.transform.localPosition = Vector3.Lerp(goMagnifyOriginalPos, itemZoomedPos, value);
        }
        else
        {
            goMagnify.transform.position = Vector3.Lerp(goMagnifyOriginalPos, itemZoomedPos, value);
        }

        // Nagyítás mértékének beállítása
        float zoomValue = Mathf.Lerp(1, zoomFactor, value);
        if (scaleable)
            goMagnify.transform.localScale = originalSize * zoomValue;
        else
            canvasRectTransform.sizeDelta = originalSize * zoomValue;

        //float zoomValue = Mathf.Lerp(startSize, startSize * zoomFactor, value);
        //if (canvasRectTransform)
        //    canvasRectTransform.sizeDelta = new Vector2(zoomValue, zoomValue) * 3;
        //else
        //    go.transform.localScale = Vector3.one * zoomValue;
    }

    // Ha van kinagyított elem, akkor azt azonnal vissza kell tenni a helyére, illetve ha animáció megy, akkor azt le kell állítani
    public void Reset()
    {
        // Csak akkorr csinálunk valamit, ha ki van nagyítva egy kép
        if (imageCover.activeInHierarchy)
        {
            Tween.StopAnimation(tweenAnimation);
            SetAnim(0f); // A képet vissza állítjuk a kiinduló állapotába
            ZoomBackComplete();
        }
    }

    /// <summary>
    /// A kinagyított gameObject-et vissza viszi az eredeti helyére.
    /// </summary>
    void ZoomBack()
    {
        // Meghívjuk a callBack eseményt mielőtt megkezdődne a visszakicsinyítés ha beállították
        if (beforeZoomBackStartCallBack != null)
            beforeZoomBackStartCallBack();

        tweenAnimation.time = 0.5f;
        tweenAnimation.startPos = 1f;
        tweenAnimation.endPos = 0f;
        tweenAnimation.onComplete = ZoomBackComplete;
        /*
        tweenAnimation.onComplete = () => {
            imageCover.SetActive(false);

            // Ha engedélyezve van az áthelyezés, akkor vissza helyezzük
            if (moveable)
                goMagnify.transform.SetParent(goMagnifyOriginalParentTransform); // parent = goMagnifyOriginalParentTransform;

            // Meghívjuk a callback esemény miután befejeződött az animáció ha beállították
            if (zoomBackFinishCallBack != null)
                zoomBackFinishCallBack();
        };
        */

        // Elindítjuk az animációt
        Tween.StartAnimation(tweenAnimation);
    }

    void ZoomBackComplete()
    {
        imageCover.SetActive(false);

        // Ha engedélyezve van az áthelyezés, akkor vissza helyezzük
        if (moveable)
            goMagnify.transform.SetParent(goMagnifyOriginalParentTransform); // parent = goMagnifyOriginalParentTransform;

        // Meghívjuk a callback esemény miután befejeződött az animáció ha beállították
        if (zoomBackFinishCallBack != null)
            zoomBackFinishCallBack();
    }

    // Akkor hívódik meg ha a cover aktív és rákattintottak.
    public void ButtonClick()
    {
        // Ha a nagyító animáció lefutott, akkor lehet csak vissza kicsinyíteni
        if (tweenAnimation.status == Tween.TweenAnimation.AnimationState.finished)
        {
            ZoomBack();
        }
    }
}
