using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectDrop : MonoBehaviour
{
    CharacterSelectDrag dragItem;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void putDragItem(CharacterSelectDrag dragItem)
    {
        // Meg kell n�zni, hogy van-e m�r berakott elem
        if (transform.childCount > 0)
        {
            // Ha van, akkor a hely�re kell k�ldeni
            for (; transform.childCount > 0;)
            {
                transform.GetChild(0).GetComponent<CharacterSelectDrag>().GotoInitialPos();
            }
        }

        // Elhelyezz�k az �j elemet
        dragItem.transform.SetParent(transform);
        dragItem.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
