using UnityEngine;
using System.Collections;

public class DragItem : MonoBehaviour, IWidthHeight {

    public float animSpeed;     // Az animáció sebessége (milyen gyorsan menjen az elem a bázis pozícióba)

    public int OrderInLayer;    // Az objektum alap rétegsorredje

    [HideInInspector]
    public bool enabledGrab;

    [HideInInspector]
    public bool grabbed;        // Az elemet megfogták, ilyenkor lekéne állítani bizonyos dolgokat

    /// <summary>
    /// Éppen fut a flashing animáció. Ilyenkor nem szabad engedni, hogy megfogják az elemet.
    /// </summary>
    public bool animRun;

    public Transform BaseTransform { get; protected set; } // Az elem gyökér gameObject-jének a transformja
    public Transform MoveTransform { get; protected set; } // Ezt mozgatjuk míg a bázis változatlan marad, így tudjuk hova kell visszarakni az elemet ha elengedik vagy ha rossz helyre rakják
    public string itemName { get; protected set; }         // Az eleme neve

    public bool itemInPlace;    // A buborékot a helyére tettük, ha pozítiv villogás történik, akkor ez true-ra változik

    [HideInInspector]
    public DragTarget dragTarget;   // melyik célpontba dobták vagy próbálták bedobni az elemet

    public Renderer itemRenderer;                  // Az elem szélességét meghatározó renderer

    public int answerIndex;           // A megfogható elemek közül a hányadik

    public bool animationRun;    // Fut-e az animálva mozgatás
    protected Vector3 animationTarget;  // Hova kell eljuttatnia az animációnak az elemet

    public virtual void Awake() {
        enabledGrab = true;
    }

    // Beállítjuk a rétegsorrendjét az elemnek
    public virtual void SetOrderInLayer(int order)
    {

    }

    /// <summary>
    /// Beállítja az elem bázis pozícióját 
    /// </summary>
    /// <param name="newPos">Az elem új bázis pozíciója.</param>
    /// <param name="delay">Mennyit várakozzon míg elindul az új pozícióba.</param>
    /// <returns>Igaz értéket ad vissza ha a pozíció egy új pozíció, hamisat ha már a megadott pozícióban volt.</returns>
    public virtual bool SetBasePos(Vector3 newPos, float delay = 0)
    {
        bool returnValue = BaseTransform.position != newPos;

        // A bázis átrakásánál a moveTransform is megváltoztatja a pozícióját hiszen a bázis gyereke
        // ezért a bázis átrakása előtt megjegyezzük a moveTransform pozícióját, majd átrakás után beállítjuk az átrakás előtti pozícióba
        // hogy a bázis áthelyezése ne befolyásolja az eleme pillanatnyi pozícióját
        Vector3 originalMovePos = MoveTransform.position;
        BaseTransform.position = newPos;
        MoveTransform.position = originalMovePos;

        // Az új bázis pozícióba mozgatjuk a moveTransform-ot
        MoveBasePos(delay);

        return returnValue;
    }

    // Az elemet a bázis pozíciójába mozgatja a megadott idő letelte után
    public virtual void MoveBasePos(float delay = 0)
    {
        animationRun = false;
        if (delay <= 0) delay = 0.001f; // Ha leállítjuk az iTween animációt egy gameObjecten, akkor rögtön nem tudunk egy másikat elindítani rajta, ezért van itt ez a minimális késleltetés
        // iTween animációval az új pozícióba mozgatjuk az elemet
        iTween.Stop(MoveTransform.gameObject); // Leállítjuk az esetlegesen már működő iTween animációkat
        iTween.MoveTo(MoveTransform.gameObject, iTween.Hash("position", Vector3.zero, "islocal", true, "easetype", iTween.EaseType.easeOutCubic, "time", 1, "delay", delay, "oncompletetarget", gameObject, "oncomplete", "MoveBasePosEnd"));
    }

    // Ez a metódust az iTween objektum hívja meg ha a bázis pozícióra mozgatta a move gameObject-et
    public virtual void MoveBasePosEnd()
    {
        SetOrderInLayer(OrderInLayer);
    }

    // Ha megfogták az elemet, akkor ezzel az eljárással lehet mozgatni
    // dragPos      - word koordináta
    public virtual void SetDragPos(Vector3 grabWorldPos)
    {
        iTween.Stop(MoveTransform.gameObject); // Leállítjuk az esetlegesen már működő iTween animációkat pl. azt amelyik a bázis pozícióba vissza mozgatja az elemet, ha rossz helyre akarták rakni
        MoveTransform.position = grabWorldPos;
    }

    public virtual void SetDragPosWithAnim(Vector3 grabWorldPos) {
        animationTarget = grabWorldPos;
        animationRun = true;
    }

    // vissza adja, hogy melyik pontot kell figyelni a célpontba dobásnál
    // Nem biztos, hogy a megfogott elem root GameObject pozíciója határozza meg az elem helyét. lásd Toldalékos játéknál például a tábla bal szélének csúcsa.
    public virtual Vector3 GetDropPos()
    {
        return MoveTransform.position;
    }

    // Villogtatja az elemet
    public virtual void FlashingPositive()
    {
        animRun = true;
        StartCoroutine(FlashingCoroutine());
    }

    public virtual void FlashingNegative()
    {
        animRun = true;
        StartCoroutine(FlashingCoroutine());
    }

    public virtual IEnumerator FlashingCoroutine()
    {
        yield return null;
    }

    public virtual float GetHeight()
    {
        //return Common.FindGameObject(gameObject, "Picture").GetComponent<Renderer>().bounds.size.y;
        return itemRenderer.bounds.size.y;
    }

    public virtual float GetWidth()
    {
        //return Common.FindGameObject(gameObject, "Picture").GetComponent<Renderer>().bounds.size.x;
        return itemRenderer.bounds.size.x;
    }

    // MultiPlayer esetben a többi játékosnál is mutatni kell, hogy mit mozgat az aktuális játékos
    // Ezt a mozgatást ebben az Update-ben valósítom meg
    public void Update() {
        if (animationRun) {
            // Kiszámoljuk a különbséget a két vektornak
            Vector3 different = animationTarget - MoveTransform.position;

            // Ha már kellőképpen megközelítettük a célt, akkor a célba tesszük az elemet és leállítjuk az animációt
            if (Mathf.Abs(different.x) < 0.01f && Mathf.Abs(different.y) < 0.01f)
            {
                SetDragPos(animationTarget);
                animationRun = false;
            }
            else
            {
                SetDragPos(MoveTransform.position + different * 0.2f);
            }
        }
    }
}
