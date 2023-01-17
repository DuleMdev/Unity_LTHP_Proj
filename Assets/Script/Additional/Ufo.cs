using UnityEngine;
using System.Collections;

public class Ufo : MonoBehaviour {

    [Tooltip("Minimum mennyit kell várni, hogy az alien újra kinézzen az ablakon")]
    public float lookingMin; // Minimum mennyit kell várni, hogy az ufó kinézzen az ablakon
    [Tooltip("Maximum mennyit kell várni, hogy az alien újra kinézzen az ablakon")]
    public float lookingMax;
    [Tooltip("Mennyi ideig néz ki az alien az ablakon")]
    public float lookingTime;
    [Tooltip("Mennyi ideig tart míg teljesen előbukkan az alien")]
    public float lookingAnimSpeed;

    [Tooltip("Milyen mértékkel lebeg az ufó")]
    public float floatSize;
    [Tooltip("Milyen frekvenciával lebeg 1 = 1 másodperc")]
    public float floatTime;

    GameObject move;
    GameObject alien;

    float aktLookingTime;       // Mennyi idő van még hátra a következő előbukkanásig
    float aktFloatTime;         // A lebegéshez

    float nextClickRemainTime;  // A következő kattintás érzékelésig mennyi idő van hátra (nem érzékeljük az ufóra kattintást ha ez nagyobb mint nulla)

    // Use this for initialization
    void Awake () {
        move = transform.Find("move").gameObject;
        alien = move.transform.Find("alien").gameObject;

        aktLookingTime = (float)Common.random.NextDouble() * (lookingMax - lookingMin) + lookingMin;
    }

    IEnumerator AlienLookingOutTheWindow() {
        iTween.MoveTo(alien, iTween.Hash("isLocal", true, "y", 0.34f, "time", lookingAnimSpeed, "easeType", iTween.EaseType.easeInOutCubic));
        yield return new WaitForSeconds(lookingTime + lookingAnimSpeed);
        iTween.MoveTo(alien, iTween.Hash("isLocal", true, "y", 0.16f, "time", lookingAnimSpeed, "easeType", iTween.EaseType.easeInOutCubic));
    }

    // Update is called once per frame
    void Update () {
        nextClickRemainTime -= Time.deltaTime;

        aktLookingTime -= Time.deltaTime;
        if (aktLookingTime < 0)
        {
            aktLookingTime = (float)Common.random.NextDouble() * (lookingMax - lookingMin) + lookingMin;
            StartCoroutine(AlienLookingOutTheWindow());
        }

        // kiszámoljuk az aktuális lebegés értéket

        aktFloatTime += Time.deltaTime / floatTime * Mathf.PI;
        float yPos = Mathf.Sin(aktFloatTime);

        move.transform.localPosition = new Vector2(move.transform.localPosition.x, yPos * floatSize);
    }

    public void OnMouseDown() {
        // Mi történjen ha az ufóra kattintottak
        if (nextClickRemainTime <= 0)
        {
            nextClickRemainTime = Common.audioController.GetAudioClip("ufo").length; // A következő kattintás akkor lehetséges, ha az ufo már lejátszotta a hangot
            Common.audioController.SFXPlay("ufo");
        }
    }
}
