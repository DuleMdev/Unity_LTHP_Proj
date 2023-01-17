using UnityEngine;
using System.Collections;

/*

Ez az objektum az iTween komponens segítségével mozgatja a szkriptet tartalmazó objektumot




*/

public class iTweenMove : MonoBehaviour {

    Common.CallBack_In_Object callBack;

    public void MoveLocal(Vector3 targetPos, iTween.EaseType easeTypeX, iTween.EaseType easeTypeY, float time, Common.CallBack_In_Object callBack) {
        this.callBack = callBack;

        iTween.ValueTo(gameObject, iTween.Hash("from", gameObject.transform.localPosition.x, "to", targetPos.x, "time", time, "easetype", easeTypeX, "onupdate", "ChangeXValue", "onupdatetarget", gameObject,
            "oncomplete", "AnimEnd", "oncompletetarget", gameObject));
        iTween.ValueTo(gameObject, iTween.Hash("from", gameObject.transform.localPosition.y, "to", targetPos.y, "time", time, "easetype", easeTypeY, "onupdate", "ChangeYValue", "onupdatetarget", gameObject));
    }

    void ChangeXValue(float value)
    {
        gameObject.transform.localPosition = gameObject.transform.localPosition.SetX(value);
    }

    void ChangeYValue(float value)
    {
        gameObject.transform.localPosition = gameObject.transform.localPosition.SetY(value);
    }

    void AnimEnd() {
        if (callBack != null)
            callBack(this.gameObject);
    }

}
