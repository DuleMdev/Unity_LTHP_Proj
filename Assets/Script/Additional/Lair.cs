using UnityEngine;
using System.Collections;

public class Lair : MonoBehaviour {

    [Tooltip("Minimum mennyit kell várni, hogy kinézzenek az oduból")]
    public float lookingMin; // Minimum mennyit kell várni, hogy az odu lakó kinézzen az oduból
    [Tooltip("Maximum mennyit kell várni, hogy kinézzenek az oduból")]
    public float lookingMax;
    [Tooltip("Mennyi ideig néz ki az állat az oduból")]
    public float lookingTime;
    [Tooltip("Mennyi ideig tart az előbukkanás és elbújás animációja")]
    public float lookingAnimSpeed;

    SpriteRenderer lairCover;   // Az oduba bújást segítő képecske
    GameObject animals;         // A mókus és a bagoly mozgatásának GameObject-je
    SpriteRenderer squirrel;        // A mókus képét tartalmazó GameObject
    SpriteRenderer owl;             // A bagoly képét tartalmazó GameObject
    GameObject showPos;         // Az előbukkanás pozíciója
    GameObject hidePos;         // Az elbújás pozíciója

    float aktLookingTime;       // Mennyi idő van még hátra a következő előbukkanásig
    bool justShow;              // Éppen előbukkant az állat

    // Use this for initialization
    void Awake () {
        lairCover = GetComponent<SpriteRenderer>();
        animals = Common.SearchGameObject(gameObject, "Animals");
        squirrel = Common.SearchGameObject(gameObject, "squirrel").GetComponent<SpriteRenderer>();
        owl = Common.SearchGameObject(gameObject, "owl").GetComponent<SpriteRenderer>();
        showPos = Common.SearchGameObject(gameObject, "ShowPos");
        hidePos = Common.SearchGameObject(gameObject, "HidePos");

        // Eldöntjük, hogy a bagoly vagy a mókus lakjon az oduba
        int decision = Common.random.Next(2);
        squirrel.gameObject.SetActive(decision == 0);
        owl.gameObject.SetActive(decision == 1);

        aktLookingTime = (float)Common.random.NextDouble() * (lookingMax - lookingMin) + lookingMin;
    }

    public void SetPictures(Sprite lairCover, Sprite animal1, Sprite animal2) {
        this.lairCover.sprite = lairCover;
        squirrel.sprite = animal1;
        owl.sprite = animal2;
    }

	// Update is called once per frame
	void Update () {

        aktLookingTime -= Time.deltaTime;
        if (aktLookingTime < 0)
        {
            aktLookingTime = (float)Common.random.NextDouble() * (lookingMax - lookingMin) + lookingMin;
            if (!justShow) // Csak akkor bukkan fel, ha éppen nincs előbukkanva
                StartCoroutine(AnimalLookingOutFromTheLair());

            aktLookingTime = (float)Common.random.NextDouble() * (lookingMax - lookingMin) + lookingMin;
        }

    }

    // Az állat kinéz az oduból
    IEnumerator AnimalLookingOutFromTheLair()
    {
        justShow = true;
        iTween.Stop(animals);
        iTween.MoveTo(animals, iTween.Hash("isLocal", true, "y", showPos.transform.localPosition.y, "time", lookingAnimSpeed, "easeType", iTween.EaseType.easeInOutCubic, "delay", 0.001f));
        yield return new WaitForSeconds(lookingTime + lookingAnimSpeed);
        justShow = false;
        iTween.MoveTo(animals, iTween.Hash("isLocal", true, "y", hidePos.transform.localPosition.y, "time", lookingAnimSpeed, "easeType", iTween.EaseType.easeInOutCubic));
    }

    // Mi történjen ha az odura kattintottak
    public void OnMouseDown()
    {
        if (!justShow) {
            StartCoroutine(AnimalLookingOutFromTheLair());
        }
    }
}
