using UnityEngine;
using System.Collections;

public class DragItemBasic : DragItem {

    /*
    // Beállítjuk a rétegsorrendjét a halmaz elemnek
    public override void SetOrderInLayer(int order)
    {

    }*/

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

    /*
    // Az elemet a bázis pozíciójába mozgatja a megadott idő letelte után
    public override void MoveBasePos(float delay = 0)
    {
        if (delay == 0) delay = 0.001f; // Ha leállítjuk az iTween animációt egy gameObjecten, akkor rögtön nem tudunk egy másikat elindítani rajta, ezért van itt ez a minimális késleltetés
        // iTween animációval az új pozícióba mozgatjuk az elemet
        iTween.Stop(MoveTransform.gameObject); // Leállítjuk az esetlegesen már működő iTween animációkat
        iTween.MoveTo(MoveTransform.gameObject, iTween.Hash("position", Vector3.zero, "islocal", true, "easetype", iTween.EaseType.easeOutCubic, "time", 1, "delay", delay, "oncompletetarget", gameObject, "oncomplete", "MoveBasePosEnd"));
    }*/

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

    /*
    // Villogtatja az elemet
    public override void FlashingPositive()
    {
        StartCoroutine(FlashingCoroutine());
    }*/

    /*
    public override void FlashingNegative()
    {
        StartCoroutine(FlashingCoroutine());
    }*/

    /*
    public override IEnumerator FlashingCoroutine()
    {
        yield return null;
    }*/

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
}
