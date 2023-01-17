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
        // Meg kell nézni, hogy van-e már berakott elem
        if (transform.childCount > 0)
        {
            // Ha van, akkor a helyére kell küldeni
            for (; transform.childCount > 0;)
            {
                transform.GetChild(0).GetComponent<CharacterSelectDrag>().GotoInitialPos();
            }
        }

        // Elhelyezzük az új elemet
        dragItem.transform.SetParent(transform);
        dragItem.transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
