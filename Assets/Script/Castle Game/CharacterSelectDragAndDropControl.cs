using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectDragAndDropControl : MonoBehaviour
{
    /// <summary>
    /// Enged�lyezve van a dragAndDrop?
    /// </summary>
    [HideInInspector]
    public bool dragAndDropEnabled;

    /// <summary>
    /// A kurzor poz�ci�ja hat�rozza meg a bedob�si pontot vagy a megfogott elem
    /// </summary>
    [Tooltip("A kurzor poz�ci�ja hat�rozza meg a bedob�si pontot vagy a megfogott elem")]
    public bool dropPosIsCursorPos = true;

    /// <summary>
    /// A megfogott DragItem, ha ez null, akkor nincs megfogott elem
    /// </summary>
    CharacterSelectDrag dragItem;
    /// <summary>
    /// Hol volt a DragItem a megfog�s pillanat�ban
    /// </summary>
    Vector3 draggingPosInWorldSpace;
    /// <summary>
    /// Hol volt az eg�r a megfog�s pillanat�ban
    /// </summary>
    Vector3 dragMousePosInWorldSpace;

    // Start is called before the first frame update
    void Start()
    {
        dragAndDropEnabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Ha megnyomt�k a bal eg�r gombot �s enged�lyezve van a Drag&Drop
        if (Input.GetMouseButtonDown(0) && dragAndDropEnabled)
        {
            // Megn�zz�k, hogy egy mozgathat� elemen nyomt�k-e le az egeret
            dragItem = (CharacterSelectDrag)Common.GetComponentInPos(Camera.main.ScreenToWorldPoint(Input.mousePosition), "CharacterSelectDrag");

            if (dragItem && dragItem.dragEnabled)
            {
                dragMousePosInWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition); // R�gz�tj�k a megfog�s pillanat�ban az eg�r poz�ci�t
                draggingPosInWorldSpace = dragItem.transform.position;
            }
            else
                dragItem = null;
        }

        // Elk�ldj�k a halmaz elem mozgat�s�t
        // ha le van nyomva az eg�r �s van megfogott elem �s enged�lyezett a mozgat�s
        if (Input.GetMouseButton(0) && dragItem && dragAndDropEnabled)
        {
            // Kisz�moljuk a mozgat�s k�l�nbs�g�t
            Vector3 differentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - dragMousePosInWorldSpace;
            Vector3 actPos = draggingPosInWorldSpace + differentMousePos;

            // Megn�zz�k, hogy van-e a k�zelben egy target elem
            //actPos = GetNearestTargetPos(actPos);

            // Be�ll�tjuk a halmaz elemet az �j poz�ci�ba
            dragItem.SetDragPos(actPos); // draggingPosInWorldSpace + differentMousePos);
        }

        // Elk�ldj�k a halmaz elem elenged�s�nek esem�ny�t
        // ha nincs lenyomva az eg�r gomb vagy nem enged�lyezett m�r a drag&drop illetve ha van megfogott elem
        if (Input.GetMouseButtonUp(0) || !dragAndDropEnabled)  //((Input.GetMouseButtonUp(0) || !dragAndDropEnabled) && dragging != null)
        {
            if (dragItem)
            {
                // Megvizsg�ljuk, hogy a c�lpont f�l�tt engedt�k-e el
                CharacterSelectDrop dropItem = (CharacterSelectDrop)Common.GetComponentInPos((dropPosIsCursorPos) ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : dragItem.transform.position, "CharacterSelectDrop");
                // Ha c�lpont f�l�tt engedt�k el �s a c�lpontba enged�lyezett a bedob�s �s enged�lyezett a drab&drop
                if (dropItem != null && dragAndDropEnabled)
                {
                    // Betessz�k a dropItem-be a dragItem-et
                    dropItem.putDragItem(dragItem);
                }
                else
                {
                    // Vissza tessz�k a hely�re a megfogott elemet
                    dragItem.GotoInitialPos();
                }
            }

            dragItem = null;
        }
    }
}
