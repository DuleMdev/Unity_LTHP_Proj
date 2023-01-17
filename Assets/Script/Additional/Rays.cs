/*
 * A kast�ly j�t�kon bel�l a inventory-ban felbukkan� ablak tartalmaz sugarakat mintha a nap s�tne.
 * Ez az�rt lett l�trehozva, hogy ne nekem kelljen egyess�vel a sugarakat l�trehozni �s beforgatni.
 * Teh�t csak megadjuk a sugarak sz�m�t �s l�trej�nnek a megadott sz�mban ar�nyosan elosztva a k�r�n.
*/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rays : MonoBehaviour
{
    public int count;           // Sugarak sz�ma
    public float raythickness;  // Sug�r vastags�g
    public float rayLength;     // Sug�r hossz
    public float rotateSpeed;   // P�rg�si sebeess�g

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
        // Kit�r�lj�k a kor�bban l�trehozott sugarakat
        for (int i = 0; i < ListOfMadeRays.Count; i++)
            Destroy(ListOfMadeRays[i]);

        ListOfMadeRays.Clear();

        // Kisz�moljuk hogy h�ny fokonk�nt kell egy sug�r
        float angleBetweenRays = 360 / count;

        // L�trehozzuk a sugarakat
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
        // Ha enged�lyezve van, akkor azonnal be�ll�tjuk a sugarak forgat�s�t, mert k�l�nben ugr�s l�that� benn�k
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
