/*
 * A kastély játékon belül a inventory-ban felbukkanó ablak tartalmaz sugarakat mintha a nap sütne.
 * Ez azért lett létrehozva, hogy ne nekem kelljen egyessével a sugarakat létrehozni és beforgatni.
 * Tehát csak megadjuk a sugarak számát és létrejönnek a megadott számban arányosan elosztva a körön.
*/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rays : MonoBehaviour
{
    public int count;           // Sugarak száma
    public float raythickness;  // Sugár vastagság
    public float rayLength;     // Sugár hossz
    public float rotateSpeed;   // Pörgési sebeesség

    GameObject rayPrefab;

    List<GameObject> ListOfMadeRays = new List<GameObject>();

    float adjustedThickness;
    float adjustedLength;

    // Start is called before the first frame update
    void Awake()
    {
        rayPrefab = gameObject.SearchChild("RayPrefab").gameObject;
    }

    void MakeItems()
    {
        // Kitöröljük a korábban létrehozott sugarakat
        for (int i = 0; i < ListOfMadeRays.Count; i++)
            Destroy(ListOfMadeRays[i]);

        ListOfMadeRays.Clear();

        // Kiszámoljuk hogy hány fokonként kell egy sugár
        float angleBetweenRays = 360 / count;

        // Létrehozzuk a sugarakat
        for (int i = 0; i < count; i++)
        {
            GameObject newRay = Instantiate(rayPrefab, rayPrefab.transform.parent);
            newRay.SetActive(true);
            newRay.transform.eulerAngles = new Vector3(0, 0, angleBetweenRays * i);

            ListOfMadeRays.Add(newRay.gameObject);
        }
    }

    private void OnEnable()
    {
        // Ha engedélyezve van, akkor azonnal beállítjuk a sugarak forgatását, mert különben ugrás látható bennük
        Update();
    }

    void SetImageSize()
    {
        for (int i = 0; i < ListOfMadeRays.Count; i++)
        {
            ((RectTransform)(ListOfMadeRays[i].transform)).sizeDelta = new Vector2(raythickness, rayLength);
        }

        adjustedThickness = raythickness;
        adjustedLength = rayLength;
    }

    // Update is called once per frame
    void Update()
    {
        if (count != ListOfMadeRays.Count)
            MakeItems();

        if (adjustedThickness != raythickness ||
            adjustedLength != rayLength)
            SetImageSize();

        rayPrefab.transform.parent.eulerAngles = new Vector3(0, 0, Time.realtimeSinceStartup / rotateSpeed * 360);
    }
}
