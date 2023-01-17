using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoofPath : MonoBehaviour
{
    public Transform prefabFootPrint;

    public float speed;

    [Tooltip("Lépések távolsága")]
    public float stepLength;
    [Tooltip("Lábnyomok szine")]
    public Color color1;
    [Tooltip("Megtett út szine")]
    public Color color2;




    BezierPath bezierPath;

    List<FootPrint> listOfFootPrint = new List<FootPrint>();
    int numberOfFootPrints;


    float aktPos;

    // Start is called before the first frame update
    void Awake()
    {
        bezierPath = GetComponentInChildren<BezierPath>();
        prefabFootPrint.gameObject.SetActive(false);
    }

    void Start()
    {
        // Létrehozzuk a lábnyomokat
        DrawFootPrints();



    }


    void DrawFootPrints()
    {
        // Töröljük a korábban létrehozott lábnyomokat
        for (int i = 0; i < listOfFootPrint.Count; i++)
            Destroy(listOfFootPrint[i].gameObject);

        // Létrehozzuk a lábnyomokat
        numberOfFootPrints = (int)(bezierPath.length / stepLength);
        if (numberOfFootPrints < 2)
            numberOfFootPrints = 2;

        float calculatedStepLength = bezierPath.length / (numberOfFootPrints - 1);

        for (int i = 0; i < numberOfFootPrints; i++)
        {
            FootPrint newFootPrint = Instantiate(prefabFootPrint, transform).GetComponent<FootPrint>();
            float distance = 1 * (i * 1f / (numberOfFootPrints - 1));

            newFootPrint.gameObject.SetActive(true);

            //newFootPrint.transform.position = bezierPath.GetPos(distance);
            //newFootPrint.transform.rotation = bezierPath.GetAngleZ(distance);

            newFootPrint.Initialize(bezierPath.GetPos(distance), bezierPath.GetAngleZ(distance), color1, i % 2 == 0);

            

            listOfFootPrint.Add(newFootPrint);
        }
    }

    public void DrawColor(float percent)
    {
        int numberOfFootPrint = (int)((listOfFootPrint.Count - 1) * percent);

        for (int i = 0; i < listOfFootPrint.Count; i++)
        {
            listOfFootPrint[i].SetColor(i <= numberOfFootPrint ? color2 : color1);
        }

    }

    // Update is called once per frame
    void Update()
    {
        aktPos += Time.deltaTime / speed;
        while (aktPos > 1)
        {
            aktPos -= 1;
        }

        DrawColor(aktPos);

        /*
        prefabFootPrint.position = bezierPath.GetPos(aktPos);
        prefabFootPrint.rotation = bezierPath.GetAngleZ(aktPos);
        */
    }
}
