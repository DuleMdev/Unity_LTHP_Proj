using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/*
Minden óra őse.
*/

abstract public class Clock_Ancestor : MonoBehaviour
{
    protected enum Status
    {
        Stop,       // Az óra áll   
        Go,         // Az óra megy
    }

    // [Space(10)]
    [Tooltip("Legyen-e ketyegés az utolsó pár másodpercben")]
    public bool tickTockEnabled; // Az utolsó pár másodpercben legyen-e ketyegés
    [Tooltip("Az utolsó másodpercben is legyen ketyegés?")]
    public bool tickTockFinish;  // Legyen-e ketyegés ha lejárt az idő (nem kell ha valamilyen más hangot akarunk lejátszani)
    [Tooltip("Mennyi legyen az utolsó pár másodperc")]
    public int tickTockTime;

    //[Tooltip("Mennyi idő alatt járjon le az óra")]
    private float _timeInterval;
    public float timeInterval
    {
        get { return _timeInterval; }
        set
        {
            _timeInterval = value; // Ha nulla értéket adunk a time intervallumnak, akkor nem is fog látszódni az óra
            hide.SetActive(value > 0);
        }
    }

    protected GameObject hide;                            // Ha nincs szükség az órára, akkor ezzen a gameObject-en keresztűl el tudjuk tüntetni

    AudioSource audioSource;                    // Az óra ketyegése amikor lejár

    protected Status status;
    protected float actTime;               // A maradék idő

    int lastTick;                // Melyik másodpercben volt már ketyegés

    Common.CallBack timeExpireCallBack; // Meghívja a megadott függvényt ha az idő lejárt

    virtual public void Awake()
    {
        hide = Common.SearchGameObject(gameObject, "Hide").gameObject;
        audioSource = GetComponent<AudioSource>();
    }


    public void Init(float time = 0, Common.CallBack timeExpireCallBack = null)
    {
        if (time > 0)
            timeInterval = time;

        Reset(0);
        this.timeExpireCallBack = timeExpireCallBack;
    }

    // Az óra alaphelyzetbe állítása. Az órát alaphelyzetbe visszapörgeti a megadott idő alatt
    virtual public void Reset(float resetTime)
    {
        actTime = timeInterval;
        status = Status.Stop;
        lastTick = -1;
    }

    // Az óra indítása
    public void Go()
    {
        // Csak akkor indítjuk el az órát, ha még van hátra idő.
        if (actTime != 0)
            status = Status.Go;
    }

    // Az óra megállítása
    public void Stop()
    {
        status = Status.Stop;
    }

    /// <summary>
    /// Beállítja az órát a megadott értékre.
    /// </summary>
    /// <param name="time">Mire állítsuk be az órát.</param>
    public void SetTime(float time)
    {
        actTime = time;
    }

    // Update is called once per frame
    void Update()
    {
        if (true) // status == Status.Go && timeInterval > 0)
        {
            if (status == Status.Go)
            {
                actTime -= Time.deltaTime;

                if (tickTockEnabled && actTime < tickTockTime)
                {
                    int second = (int)actTime;

                    if (second != lastTick)
                    {
                        lastTick = second;
                        Debug.Log("lastTick : " + lastTick);
                        audioSource.Play();
                    }
                }
            }

            if (actTime < 0)
                actTime = 0;

            SetClockTime(actTime);
            /*
            float angle = 360 / timeInterval * actTime;

            clockBackground.fillAmount = actTime / timeInterval;
            pointer.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            pointerShadow.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            */
            if (actTime == 0 && status == Status.Go)
            {
                if (timeExpireCallBack != null)
                    timeExpireCallBack();

                Debug.Log("Finish time");

                if (tickTockEnabled)
                    audioSource.Play();
                //Common.audioController.SFXPlay("ticktock");

                status = Status.Stop;
            }
        }
    }

    // beállítja a mutatót a megadott értéknek megfelelően
    abstract protected void SetClockTime(float time);

    // Ezt majd el lehet távolítani, ha már minden játékban ki van cserélve a több design-t tartalmazó óra egy design-t tartalmazóra
    virtual public void SetPictures()
    {
    }

}
