using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*

*/

public class Clock_With_Layout : Clock_Ancestor
{
    LayoutManager layoutManager;    // A különböző layoutokhoz tartozó képeket tartalmazza

    Image imageClockBackground;                 // Az óra hátterét tartalmazó Image, ami eltűnik ahogy a mutató halad
    SpriteRenderer spriteRendererPointer;       // Az óra mutatója
    SpriteRenderer spriteRendererPointerShadow; // Az óra mutatójának árnyéka
    SpriteRenderer spriteRendererBorder;        // Az óra kerete

    override public void Awake()
    {
        base.Awake();

        layoutManager = GetComponentInChildren<LayoutManager>();

        imageClockBackground = GetComponentInChildren<Image>();
        spriteRendererPointer = Common.SearchGameObject(hide, "pointer").GetComponent<SpriteRenderer>();
        spriteRendererPointerShadow = Common.SearchGameObject(hide, "pointerShadow").GetComponent<SpriteRenderer>();
        spriteRendererBorder = Common.SearchGameObject(hide, "clockBorder").GetComponent<SpriteRenderer>();
    }

    // Az óra alaphelyzetbe állítása. Az órát alaphelyzetbe visszapörgeti a megadott idő alatt
    override public void Reset(float resetTime)
    {
        base.Reset(resetTime);

        iTween.Stop(gameObject);
        iTween.ValueTo(gameObject, iTween.Hash("from", actTime, "to", timeInterval, "time", resetTime, "easetype", iTween.EaseType.easeOutQuad, "delay", 0.001f, "onupdate", "SetClockFace", "onupdatetarget", gameObject));
    }

    //// iTween ValueTo call this
    //public void iTweenValueTo(float value)
    //{
    //    SetClockFace(value);
    //}

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

    override public void SetPictures()
    {
        layoutManager.SetLayoutSet(LayoutManager.actLayoutSetName);

        imageClockBackground.sprite = layoutManager.GetSprite("background");
        spriteRendererPointer.sprite = layoutManager.GetSprite("pointer");
        spriteRendererPointerShadow.sprite = layoutManager.GetSprite("pointerShadow");
        spriteRendererBorder.sprite = layoutManager.GetSprite("border");
    }
}

//public class Clock_With_Layout : MonoBehaviour
//{
//
//    enum Status
//    {
//        Stop,       // Az óra áll   
//        Go,         // Az óra megy
//    }
//
//    // [Space(10)]
//    [Tooltip("Legyen-e ketyegés az utolsó pár másodpercben")]
//    public bool tickTockEnabled; // Az utolsó pár másodpercben legyen-e ketyegés
//    [Tooltip("Az utolsó másodpercben is legyen ketyegés?")]
//    public bool tickTockFinish;  // Legyen-e ketyegés ha lejárt az idő (nem kell ha valamilyen más hangot akarunk lejátszani)
//    [Tooltip("Mennyi legyen az utolsó pár másodperc")]
//    public int tickTockTime;
//
//    //[Tooltip("Mennyi idő alatt járjon le az óra")]
//    private float _timeInterval;
//    public float timeInterval
//    {
//        get { return _timeInterval; }
//        set
//        {
//            _timeInterval = value; // Ha nulla értéket adunk a time intervallumnak, akkor nem is fog látszódni az óra
//            hide.SetActive(value > 0);
//        }
//    }
//
//    LayoutManager layoutManager;    // A különböző layoutokhoz tartozó képeket tartalmazza
//
//    GameObject hide;                            // Ha nincs szükség az órára, akkor ezzen a gameObject-en keresztűl el tudjuk tüntetni
//
//    Image imageClockBackground;                 // Az óra hátterét tartalmazó Image, ami eltűnik ahogy a mutató halad
//    SpriteRenderer spriteRendererPointer;       // Az óra mutatója
//    SpriteRenderer spriteRendererPointerShadow; // Az óra mutatójának árnyéka
//    SpriteRenderer spriteRendererBorder;        // Az óra kerete
//
//    AudioSource audioSource;                    // Az óra ketyegése amikor lejár
//
//    Status status;
//    float actTime;               // Az óra aktuális állása
//
//    int lastTick;                // Melyik másodpercben volt az utolsó ketyegés
//
//    Common.CallBack timeExpireCallBack; // Meghívja a megadott függvényt ha az idő lejárt
//
//    void Awake()
//    {
//        layoutManager = GetComponentInChildren<LayoutManager>();
//
//        hide = Common.SearchGameObject(gameObject, "Hide").gameObject;
//        imageClockBackground = GetComponentInChildren<Image>();
//        spriteRendererPointer = Common.SearchGameObject(hide, "pointer").GetComponent<SpriteRenderer>();
//        spriteRendererPointerShadow = Common.SearchGameObject(hide, "pointerShadow").GetComponent<SpriteRenderer>();
//        spriteRendererBorder = Common.SearchGameObject(hide, "clockBorder").GetComponent<SpriteRenderer>();
//
//        audioSource = GetComponent<AudioSource>();
//
//        //Reset(0);
//        //Go();
//    }
//
//    public void Init(float time = 0, Common.CallBack timeExpireCallBack = null)
//    {
//        if (time > 0)
//            this.timeInterval = time;
//
//        Reset(0);
//        this.timeExpireCallBack = timeExpireCallBack;
//    }
//
//    // Az óra alaphelyzetbe állítása. Az órát alaphelyzetbe visszapörgeti a megadott idő alatt
//    public void Reset(float resetTime)
//    {
//
//        iTween.Stop(gameObject);
//        iTween.ValueTo(gameObject, iTween.Hash("from", actTime, "to", timeInterval, "time", resetTime, "easetype", iTween.EaseType.easeOutQuad, "delay", 0.001f, "onupdate", "SetPointer", "onupdatetarget", gameObject));
//
//        actTime = timeInterval;
//        status = Status.Stop;
//        lastTick = -1;
//    }
//
//    // iTween ValueTo call this
//    public void iTweenValueTo(float value)
//    {
//        SetPointer(value);
//    }
//
//    // Az óra indítása
//    public void Go()
//    {
//        // Csak akkor indítjuk el az órát, ha még van hátra idő.
//        if (actTime != 0)
//            status = Status.Go;
//    }
//
//    // Az óra megállítása
//    public void Stop()
//    {
//        status = Status.Stop;
//    }
//
//    // Update is called once per frame
//    void Update()
//    {
//
//
//        if (status == Status.Go && timeInterval > 0)
//        {
//            actTime -= Time.deltaTime;
//
//            if (tickTockEnabled && actTime < tickTockTime)
//            {
//                int second = (int)actTime;
//
//                if (second != lastTick)
//                {
//                    lastTick = second;
//                    Debug.Log("lastTick : " + lastTick);
//                    audioSource.Play();
//                }
//            }
//
//            if (actTime < 0)
//                actTime = 0;
//
//            SetPointer(actTime);
//            /*
//            float angle = 360 / timeInterval * actTime;
//
//            clockBackground.fillAmount = actTime / timeInterval;
//            pointer.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
//            pointerShadow.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
//            */
//            if (actTime == 0 && status == Status.Go)
//            {
//                if (timeExpireCallBack != null)
//                    timeExpireCallBack();
//
//                Debug.Log("Finish time");
//
//                if (tickTockEnabled)
//                    audioSource.Play();
//                //Common.audioController.SFXPlay("ticktock");
//
//                status = Status.Stop;
//            }
//        }
//    }
//
//    /// <summary>
//    /// Beállítja az órát a megadott értékre.
//    /// </summary>
//    /// <param name="time">Mire állítsuk be az órát.</param>
//    public void SetTime(float time)
//    {
//        actTime = time;
//    }
//
//    // beállítja a mutatót a megadott értéknek megfelelően
//    void SetPointer(float time)
//    {
//
//        float angle;
//
//        if (timeInterval > 0)
//        {
//            time = Mathf.Clamp(time, 0, timeInterval);
//            angle = 360 / timeInterval * time;
//        }
//        else {
//            angle = 360;
//        }
//
//        imageClockBackground.fillAmount = time / timeInterval;
//        spriteRendererPointer.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
//        spriteRendererPointerShadow.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
//    }
//
//    public void SetPictures()
//    {
//        layoutManager.SetLayoutSet(LayoutManager.actLayoutSetName);
//
//        imageClockBackground.sprite = layoutManager.GetSprite("background");
//        spriteRendererPointer.sprite = layoutManager.GetSprite("pointer");
//        spriteRendererPointerShadow.sprite = layoutManager.GetSprite("pointerShadow");
//        spriteRendererBorder.sprite = layoutManager.GetSprite("border");
//    }
//}

