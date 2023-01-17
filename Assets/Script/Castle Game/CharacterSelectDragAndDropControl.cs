using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectDragAndDropControl : MonoBehaviour
{
    /// <summary>
    /// Engedélyezve van a dragAndDrop?
    /// </summary>
    [HideInInspector]
    public bool dragAndDropEnabled;

    /// <summary>
    /// A kurzor pozíciója határozza meg a bedobási pontot vagy a megfogott elem
    /// </summary>
    [Tooltip("A kurzor pozíciója határozza meg a bedobási pontot vagy a megfogott elem")]
    public bool dropPosIsCursorPos = true;

    /// <summary>
    /// A megfogott DragItem, ha ez null, akkor nincs megfogott elem
    /// </summary>
    CharacterSelectDrag dragItem;
    /// <summary>
    /// Hol volt a DragItem a megfogás pillanatában
    /// </summary>
    Vector3 draggingPosInWorldSpace;
    /// <summary>
    /// Hol volt az egér a megfogás pillanatában
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
        // Ha megnyomták a bal egér gombot és engedélyezve van a Drag&Drop
        if (Input.GetMouseButtonDown(0) && dragAndDropEnabled)
        {
            // Megnézzük, hogy egy mozgatható elemen nyomták-e le az egeret
            dragItem = (CharacterSelectDrag)Common.GetComponentInPos(Camera.main.ScreenToWorldPoint(Input.mousePosition), "CharacterSelectDrag");

            if (dragItem && dragItem.dragEnabled)
            {
                dragMousePosInWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Rögzítjük a megfogás pillanatában az egér pozíciót
                draggingPosInWorldSpace = dragItem.transform.position;
            }
            else
                dragItem = null;
        }

        // Elküldjük a halmaz elem mozgatását
        // ha le van nyomva az egér és van megfogott elem és engedélyezett a mozgatás
        if (Input.GetMouseButton(0) && dragItem && dragAndDropEnabled)
        {
            // Kiszámoljuk a mozgatás különbségét
            Vector3 differentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - dragMousePosInWorldSpace;
            Vector3 actPos = draggingPosInWorldSpace + differentMousePos;

            // Megnézzük, hogy van-e a közelben egy target elem
            //actPos = GetNearestTargetPos(actPos);

            // Beállítjuk a halmaz elemet az új pozícióba
            dragItem.SetDragPos(actPos); // draggingPosInWorldSpace + differentMousePos);
        }

        // Elküldjük a halmaz elem elengedésének eseményét
        // ha nincs lenyomva az egér gomb vagy nem engedélyezett már a drag&drop illetve ha van megfogott elem
        if (Input.GetMouseButtonUp(0) || !dragAndDropEnabled)  //((Input.GetMouseButtonUp(0) || !dragAndDropEnabled) && dragging != null)
        {
            if (dragItem)
            {
                // Megvizsgáljuk, hogy a célpont fölött engedték-e el
                CharacterSelectDrop dropItem = (CharacterSelectDrop)Common.GetComponentInPos((dropPosIsCursorPos) ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : dragItem.transform.position, "CharacterSelectDrop");
                // Ha célpont fölött engedték el és a célpontba engedélyezett a bedobás és engedélyezett a drab&drop
                if (dropItem != null && dragAndDropEnabled)
                {
                    // Betesszük a dropItem-be a dragItem-et
                    dropItem.putDragItem(dragItem);
                }
                else
                {
                    // Vissza tesszük a helyére a megfogott elemet
                    dragItem.GotoInitialPos();
                }
            }

            dragItem = null;
        }
    }
}
