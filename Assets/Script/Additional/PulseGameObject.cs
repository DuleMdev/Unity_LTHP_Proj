using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseGameObject : MonoBehaviour
{
    GameObject go;             // A gameobject amit pulzálni kell
    bool buttonPulseIsEnabled = true;   // Engedélyezi a gameObject pulzálását
    bool buttonPulseBig;                // A gameObject mérete éppen nagy
    [Tooltip("Mekkora legyen a kisebb méret az eredetihez képest")]
    public float buttonPulseLittleSize = 0.8f; // Mekkora legyen a kis gomb mérete az eredetihez képest
    [Tooltip("Milyen időközönként vesz fel másik méretet")]
    public float buttonPulseFrequency = 1.2f;  // Milyen sebességgel pulzáljon a gameObject
    [Tooltip("Mennyi idő alatt veszi fel a következő méretét")]
    public float animSpeed = 1; // Az animáció sebessége

    float remainingTime = 0f;         // Mennyi idő maradt a következő pulzálásig
    Vector3 originalSize;

    // Start is called before the first frame update
    void Awake()
    {
        originalSize = transform.localScale;
    }

    /// <summary>
    /// A gomb elkezd pulzálni
    /// </summary>
    public void Pulse()
    {
        buttonPulseIsEnabled = true;
        remainingTime = 0;
    }

    /// <summary>
    /// A pulzálás befejeződik
    /// </summary>
    public void Stop()
    {
        buttonPulseIsEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // gameObject pulzálása
        remainingTime -= Time.deltaTime;

        if (buttonPulseBig && !buttonPulseIsEnabled)
            remainingTime = float.MaxValue;

        if (remainingTime < 0)
        {
            buttonPulseBig = !buttonPulseBig;
            iTween.ScaleTo(gameObject, iTween.Hash("islocal", true, "scale", originalSize * ((buttonPulseBig) ? 1 : buttonPulseLittleSize), "time", animSpeed, "easeType", iTween.EaseType.easeOutCubic));

            remainingTime = buttonPulseFrequency;
        }
    }


}
