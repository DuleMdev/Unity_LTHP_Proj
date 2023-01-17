using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Tooltip("Milyen magasra ugorhat fel a labda")]
    float jumpHeight;

    [Tooltip("Hol tart a pattogásában a labda")]
    float x;

    [Tooltip("Mennyi idő alatt forduljon meg a labda")]
    public float turnSpeed = 0.2f;


    RectTransform ball;
    AudioSource audioSource;

    public bool stop;

    float lastSound;

    bool turnIsInProcess;
    float turnTime;
    float startAngle;
    float targetAngle;

    // Start is called before the first frame update
    void Start()
    {
        ball = gameObject.SearchChild("BallRed").GetComponent<RectTransform>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Refresh(float value, float jumpHeight)
    {
        // A pattogás hang egy kicsit késik, ezért van hozzá adva a 0.1 tizedmásodperc
        float i = Mathf.Floor(value + 0.1f);

        if (i != lastSound)
        {
            audioSource.Play();
            lastSound = i;
        }

        // Az érték törtrészével határozzuk meg a labda pattogásának pozícióját
        if (value < 0)
            value *= -1;
        value -= Mathf.Floor(value);

        value = Mathf.Sin(value * Mathf.PI);

        ball.anchoredPosition = new Vector3(0, value * jumpHeight);
    }
    
    /// <summary>
    /// Utasítjuk a labdát fordulásra.
    /// </summary>
    public void Turn()
    {
        if (!turnIsInProcess)
        {
            turnIsInProcess = true;

            turnTime = 0;
            startAngle = ball.localRotation.eulerAngles.z;
            targetAngle = RedIsBottom() ? 0 : -180;
        }
    }

    /// <summary>
    /// Igaz értéket ad visssza ha a piros szín van alul.
    /// </summary>
    /// <returns></returns>
    public bool RedIsBottom()
    {
        return Mathf.Abs(ball.localRotation.eulerAngles.z - 180) < 90;
    }

    // Update is called once per frame
    void Update()
    {
        if (stop) return;

        if (turnIsInProcess)
        {
            turnTime += Time.deltaTime;

            ball.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(startAngle, targetAngle, turnTime /  turnSpeed));

            //Debug.Log("Ball rotation : " + ball.localRotation.eulerAngles.z + " - Red is bottom : " + RedIsBottom());

            if (turnTime > turnSpeed)
                turnIsInProcess = false;
            //ball.localRotation.eulerAngles = new Vector3(0, 0, 0);
        }
    }
}
