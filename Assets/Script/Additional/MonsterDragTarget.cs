using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MonsterDragTarget : DragTarget {

    SpriteRenderer questionMarkPicture;
    DragItem dragItem;  // A berakott DragItem

    override public void Awake()
    {
        base.Awake();

        questionMarkPicture = Common.SearchChild(gameObject, "Picture").GetComponent<SpriteRenderer>();
    }

    public override void PutItemInTarget(DragItem dragItem)
    {
        base.PutItemInTarget(dragItem);

        this.dragItem = dragItem;

        // Mostantól a bedobott elem fogja meghatározni a DragTarget szélességét
        //itemRenderer = dragItem.itemRenderer;

        // Kikapcsoljuk a befogaadó elemet
        questionMarkPicture.enabled = false;
    }

    /*
    public override float GetHeight()
    {


        return (dragItem != null) ? //dragItem.GetHeight()
            0: //(dragItem.GetHeight() * (scale / dragItem.BaseTransform.localScale.y)) :
            base.GetHeight();
    }
    */

    public override float GetWidth()
    {
        return (dragItem != null) ?
            (dragItem.GetWidth() * (scale / dragItem.BaseTransform.localScale.x)) :
            //(dragItem.BaseTransform.localScale.x / dragItem.GetWidth() * scale) :
            base.GetWidth();
    }

    public void SetPicture(Sprite picture) {
        questionMarkPicture.sprite = picture;
    }
}
