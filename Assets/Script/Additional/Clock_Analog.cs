using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock_Analog : Clock_Ancestor
{
    Image imageClockBackground;                 // Az óra hátterét tartalmazó Image, ami eltűnik ahogy a mutató halad
    SpriteRenderer spriteRendererPointer;       // Az óra mutatója
    SpriteRenderer spriteRendererPointerShadow; // Az óra mutatójának árnyéka

    override public void Awake()
    {
        base.Awake();

        imageClockBackground = GetComponentInChildren<Image>();
        spriteRendererPointer = Common.SearchGameObject(hide, "pointer").GetComponent<SpriteRenderer>();
        spriteRendererPointerShadow = Common.SearchGameObject(hide, "pointerShadow").GetComponent<SpriteRenderer>();
    }

    // Az óra alaphelyzetbe állítása. Az órát alaphelyzetbe visszapörgeti a megadott idő alatt
    override public void Reset(float resetTime)
    {
        base.Reset(resetTime);

        iTween.Stop(gameObject);
        iTween.ValueTo(gameObject, iTween.Hash("from", actTime, "to", timeInterval, "time", resetTime, "easetype", iTween.EaseType.easeOutQuad, "delay", 0.001f, "onupdate", "SetClockTime", "onupdatetarget", gameObject));
    }

    // beállítja a mutatót a megadott értéknek megfelelően
    override protected void SetClockTime(float time)
    {
        float angle;

        if (timeInterval > 0)
        {
            time = Mathf.Clamp(time, 0, timeInterval);
            angle = 360 / timeInterval * time;
        }
        else {
            angle = 360;
        }

        imageClockBackground.fillAmount = time / timeInterval;
        spriteRendererPointer.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        spriteRendererPointerShadow.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
