using UnityEngine;
using System.Collections;

public class UIProgressBarEvaluation : MonoBehaviour {

    Transform transformProgressBar;

    float targetProgressBarValue;   // Mit kell mutatnia a progressBarnak

    float actValue;                 // Mit mutat aktuálisan a progressBar

    public bool progressBarIsEnd { get { return actValue < 0.01; }  }

    float ratio = 0.96f;
    float timeStep = 0.02f;

    float lastTime;

    void Awake () {
        transformProgressBar = gameObject.SearchChild("imageProgressBar").transform;
    }

    /// <summary>
    /// Beállítjuk a progressBar-t a megadott értéknek megfelelően, ami nulla és egy közötti érték lehet.
    /// A 0 érték jelenti a semmi sem látszik a sárga csíkból, az 1 pedig az egész sárga csík látszik.
    /// </summary>
    /// <param name="value"></param>
    public void SetProgressBar(float value) {
        targetProgressBarValue = 1 - value; // Mathf.Clamp01(1 - value);
        actValue = targetProgressBarValue;

        transformProgressBar.localScale = Vector3.one.SetX(targetProgressBarValue);
    }

    /*
    /// <summary>
    /// Beállítjuk a progressBar-t a megadott értéknek megfelelően, ami nulla és egy közötti érték lehet.
    /// A 0 érték jelenti a semmi sem látszik a sárga csíkból, az 1 pedig az egész sárga csík látszik.
    /// </summary>
    /// <param name="value"></param>
    void SetProgressBarByiTween(float value)
    {
        //targetProgressBarValue = Mathf.Clamp01(value);
        transformProgressBar.localScale = Vector3.one.SetX(targetProgressBarValue);
    }
    */

    public void SetProgressBarAnim(float value, float time)
    {
        targetProgressBarValue = 1 - value; // Mathf.Clamp01(1 - value);

        //iTween.ValueTo(gameObject, iTween.Hash("from", transformProgressBar.localScale.x, "to", 1 - value, "time", time, "easetype", iTween.EaseType.easeOutCubic, "onupdate", "SetProgressBarByiTween", "onupdatetarget", gameObject));
    }

    void Update() {
        // Hány lépést kell tenni a célfelé

        float timeDifferent = Time.time - lastTime;

        float step = timeDifferent / timeStep;

        float distance = 1 - Mathf.Pow(ratio, step);

        actValue = (targetProgressBarValue - actValue) * distance + actValue;

        actValue = Mathf.Clamp01(actValue);

        transformProgressBar.localScale = Vector3.one.SetX(actValue);

        lastTime = Time.time;
    }



}