using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCoinsPiece : MonoBehaviour
{
    List<GameObject> listOfCoins;

    // Start is called before the first frame update
    void Awake()
    {
        // Összegyűjtjük az itemeket
        listOfCoins = new List<GameObject>();

        int i = 1;
        Transform t;
        do
        {
            t = transform.Find("Item" + i);
            if (t)
                listOfCoins.Add(t.gameObject);
            i++;
        } while (t != null);
    }

    public void SetCount(int count)
    {
        for (int i = 0; i < listOfCoins.Count; i++)
        {
            listOfCoins[i].SetActive(i < count);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
