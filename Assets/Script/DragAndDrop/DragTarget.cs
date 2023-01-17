using UnityEngine;
using System.Collections.Generic;

public class DragTarget : MonoBehaviour, IWidthHeight {

    // Engedélyezve van az elemek bedobása?
    bool _enabledDrop = true;
    [HideInInspector]
    public bool enabledDrop
    { 
        get { return _enabledDrop; }
        set { if (!value || !(items.Count == 0)) _enabledDrop = value; }
    }    

    public float animTime;
    public float scale;
    public Vector3 rotate;

    public int maxItem { get; set; }        // Hány elemet lehet beledobni maximum
    int itemNumber;                         // Hány elemet tettek már bele

    protected List<string> items;           // Milyen nevű elemeket lehet beledobni

    SpriteRenderer spriteRendererItem;      // 
    protected Renderer itemRenderer;                  // Az elem szélességét meghatározó renderer

    public int questionIndex;               // A kérdés indexe
    public int subQuestionIndex;            // Az all kérdés indexe

    public virtual void Awake() {
        // Egy Picture nevű gameObjecten található renderer határozza meg az elem méreteit
        itemRenderer = Common.SearchGameObject(gameObject, "Picture").GetComponent<Renderer>();
        spriteRendererItem = Common.SearchGameObject(gameObject, "Picture").GetComponent<SpriteRenderer>();
    }

    public virtual void Initialize(List<string> items)
    {
        this.items = new List<string>();

        foreach (string item in items)
            this.items.Add(item);
    }

    // Megviszgálja, hogy a megadott halmaz elem ebbe a halmazba tartozik-e
    // True értéket ad vissza ha igen
    public virtual bool IsItemAcceptable(DragItem dragItem)
    {
        return items.Contains(dragItem.itemName);
    }

    // A megadott elemet elhelyezi a célpontba
    public virtual void PutItemInTarget(DragItem dragItem)
    {
        dragItem.BaseTransform.SetParent(transform);  // Mostantól a target lesz az item szülője

        dragItem.itemInPlace = true;       // Az elem a helyén van
        dragItem.enabledGrab = false;      // Az elem a helyén van, letiltjuk a megfogást

        // Beállítjuk a skálázást és a forgatást
        dragItem.SetBasePos(GetDropPos(dragItem));
        GameObject go = dragItem.BaseTransform.gameObject;

        //iTween.ScaleTo(go.gameObject, iTween.Hash("scale", go.transform.localScale * scale * transform.lossyScale.x, "easetype", iTween.EaseType.easeOutCubic, "time", animTime));
        iTween.ScaleTo(go.gameObject, iTween.Hash("scale", GetDropScale(dragItem), "easetype", iTween.EaseType.easeOutCubic, "time", animTime));
        iTween.RotateTo(go.gameObject, iTween.Hash("rotation", GetDropRotate(dragItem), "islocal", true, "easetype", iTween.EaseType.easeOutCubic, "time", animTime));

        // Az elemet eltávolítjuk a listából, hogy más hasonló elemet ne lehessen már ide tenni
        items.Remove(dragItem.itemName);
        itemNumber++;
        if (items.Count == 0 || maxItem == itemNumber)
            enabledDrop = false;
    }

    // Vissza adja, hogy a bedobott elemet hova kell a térben elhelyezni
    public virtual Vector3 GetDropPos(DragItem dragItem) {
        return transform.position;
    }

    // Vissza adja, hogy a bedobott elemet mekkorára kell skálázni
    public virtual Vector3 GetDropScale(DragItem dragItem) {
        return Vector3.one * scale;
        //return dragItem.BaseTransform.localScale / dragItem.GetWidth() * GetWidth();
    }

    // Vissza adja, hogy a bedobott elemet hogyan kell elforgatni
    public virtual Vector3 GetDropRotate(DragItem dragItem) {
        return rotate;
    }

    public void SetPicture(Sprite picture) {
        spriteRendererItem.sprite = picture;
    }


    public virtual float GetHeight()
    {
        //return Common.FindGameObject(gameObject, "Picture").GetComponent<Renderer>().bounds.size.y;
        return itemRenderer.bounds.size.y;
        return itemRenderer.bounds.size.y / itemRenderer.transform.lossyScale.y;
    }

    public virtual float GetWidth()
    {
        //return Common.FindGameObject(gameObject, "Picture").GetComponent<Renderer>().bounds.size.x;
        return itemRenderer.bounds.size.x;
        return itemRenderer.bounds.size.x / itemRenderer.transform.lossyScale.x;
    }
}
