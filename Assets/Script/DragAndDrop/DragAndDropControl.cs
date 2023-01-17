using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using SimpleJSON;
using System.Collections.Generic;

public class DragAndDropControl : MonoBehaviour {

    public delegate void DragItemCallBack(DragItem dragItem);   // Egy DragItem paramétert váró és semmit sem vissza adó függvényt deklarálunk
    public delegate void DragItemAndDragTargetCallBack(DragItem dragItem, DragTarget dragTarget);   // Egy DragItem és egy DragTarget paramétert váró és semmit sem vissza adó függvényt deklarálunk

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
    /// A megfogott elem rétegsorrendje a mozgatás során
    /// </summary>
    [Tooltip("A megfogott elem rétegsorrendje a mozgatás során")]
    public int dragOrderInLayer;
    /// <summary>
    /// A milyen rétegsorrendre állítsuk az elemet ha elengedték
    /// </summary>
    [Tooltip("Mi legyen a rétegsorrend amíg vissza megy az elem a helyére elengedés után")]
    public int dragReleaseOrderInLayer;

    [Tooltip("Kattintás figyelés engedélyezve van-e a megfogható elemen. Ha igen, akkor a megfogható elemet nem csak vonszolni, hanem kattintani is lehet és ekkor kiváltja a itemClick eseményt.")]
    public bool clickIsEnabled;

    /// <summary>
    /// A megfogott DragItem, ha ez null, akkor nincs megfogott elem
    /// </summary>
    DragItem dragging;
    /// <summary>
    /// Hol volt a DragItem a megfogás pillanatában
    /// </summary>
    Vector3 draggingPosInWorldSpace;
    /// <summary>
    /// Hol volt az egér a megfogás pillanatában
    /// </summary>
    Vector3 dragMousePosInWorldSpace;

    public DragItemCallBack itemClick;                                  // Kattintottak egy elemen
    public DragItemCallBack itemDrag;                                   // Megfogtak egy elemet
    public DragItemCallBack itemReleased;                               // Elengedtük az elemet
    public DragItemAndDragTargetCallBack itemReleasedOverADragTarget;   // Elengedtük az elemet egy célpont felett, még nem tudjuk, hogy jó helyen vagy rossz helyen
    public DragItemAndDragTargetCallBack itemPutWrongPlace;             // Az elemet rossz helyre helyeztük
    public DragItemAndDragTargetCallBack itemPutGoodPlace;              // Az elemet jó helyre helyeztük

    //public Common.CallBack_In_JSONNode sendMessage;     // Eseményt küldünk

    /// <summary>
    /// Ebben a listában található objektumokba lehet dobni valamit
    /// </summary>
    [HideInInspector]
    public List<DragTarget> ListOfDragTarget = null;

    // Multiplayer esetén a többi klienshez is el kell küldeni, hogy éppen hová mozgatta a megfogott elemet a felhasználó
    /// <summary>
    /// Mikor küldhetjük a következő mozgatási eseményt
    /// </summary>
    float nextMoveSendTime;
    /// <summary>
    /// Milyen sűrűn küldjünk mozgási eseményt
    /// </summary>
    float moveSendTime = 0.5f;
    /// <summary>
    /// Milyen messziről vonza be a DragTarget a DragItem-et
    /// </summary>
    float snapDistance = 0.25f;

    /// <summary>
    /// Melyik DragItem-en történt az egér lenyomás
    /// </summary>
    DragItem dragItem;
    /// <summary>
    /// A változó tárolja az egér lenyomásának pillanatát
    /// </summary>
    float clickTime;
    /// <summary>
    /// Jelzi, hogy még nem tudja a rendszer, hogy klikkelés történt-e (még nem engedték el az egeret a megadott idő alatt)
    /// </summary>
    bool maybeClick;
    /// <summary>
    /// Milyen messzire távolodhat el az egér az egér lenyomás helyétől, hogy még kattintásnak érzékelhesse
    /// </summary>
    float maxClickDistance = 0.1f;
    /// <summary>
    /// Minnyi idő telhet el maximum az egér lenyomástól az egér felengedéséig, hogy kattintásnak érzékelje
    /// </summary>
    float maxClickTime = 0.3f;

    /// <summary>
    /// Itt kezeljük a Drag&Drop-ot
    /// </summary>
    void Update() {

        // Ha valamelyik fade actív, akkor nem reagálunk az egérre
        if (Common.fadeActive)
            return;

        // Ha megnyomták a bal egér gombot és engedélyezve van a Drag&Drop
        if (Input.GetMouseButtonDown(0) && dragAndDropEnabled)
        {
            // Még nem tudjuk, hogy kattintani akarnak az elemen vagy mozgatni
            // Ha 0.3 másodperc alatt elengedték és nem mozgatták nagyobb távolságra mint 0.1 egység, akkor kattintani
            // más esetekben mozgatni
            dragItem = (DragItem)Common.GetComponentInPos(Camera.main.ScreenToWorldPoint(Input.mousePosition), "DragItem");

            // Ha éppen villogás animáció fut az elemen, akkor letiltjuk az újbóli megfogását
            if (dragItem && dragItem.animRun)
                dragItem = null;

            dragMousePosInWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Rögzítjük a megfogás pillanatában az egér pozíciót

            // Ha van megfogott item, akkor lehet, hogy ráklikkeltek
            maybeClick = dragItem;
            clickTime = Time.time;

            // Ha nincs engedélyezve a klikkelés, akkor úgy csinálunk mintha már letelt volna az idő, így átvált grab üzemmódba
            if (!clickIsEnabled)
                clickTime -= maxClickTime * 2;

            // Ha van megfogott elem, akkor leállítjuk a dragItem-en az esetleg működő iTween animációt
            if (dragItem)
                dragItem.SetDragPos(dragItem.MoveTransform.position); 
        }


        // Ha az egér lenyomásása helyétől kellőképen eltávolodtunk vagy letelt a klikkelésre szánt idő, akkor átváltunk grab üzemmódba
        if (maybeClick && (((Camera.main.ScreenToWorldPoint(Input.mousePosition) - dragMousePosInWorldSpace).magnitude > maxClickDistance) || clickTime + maxClickTime <= Time.time))

        // Elküldjük a megfogás eseményt
        // ha megnyomták a bal egér gombot és engedélyezve van a Drag&Drop
        //if (Input.GetMouseButtonDown(0) && dragAndDropEnabled)
        {
            maybeClick = false;

            // Megvizsgáljuk, hogy melyik objektumot fogta meg a felhasználó
            //DragItem dragItem = (DragItem)GetComponentInPos(Camera.main.ScreenToWorldPoint(Input.mousePosition), "DragItem");

            // Ha egy megfogható objektumra kattintott és az az objektum engedélyezve van megfogásra
            if (dragItem != null && dragItem.enabledGrab)
            {
                // Elküldjük a megfogás eseményt
                JSONClass jsonClass = new JSONClass();
                jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.drag;
                jsonClass[C.JSONKeys.selectedAnswer].AsInt = dragItem.answerIndex;

                Common.taskController.SendMessageToServer(jsonClass);

                //dragMousePosInWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Rögzítjük a megfogás pillanatában az egér pozíciót
            }
        }

        // Elküldjük a halmaz elem mozgatását
        // ha le van nyomva az egér és van megfogott elem és engedélyezett a mozgatás
        if (Input.GetMouseButton(0) && dragging != null && dragAndDropEnabled)
        {
            // Kiszámoljuk a mozgatás különbségét
            Vector3 differentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - dragMousePosInWorldSpace;
            Vector3 actPos = draggingPosInWorldSpace + differentMousePos;

            // Megnézzük, hogy van-e a közelben egy target elem
            actPos = GetNearestTargetPos(actPos);

            // Ha eltelt fél másodperc az utolsó mozgatás küldés között, akkor elküldjük az eseményt
            if (nextMoveSendTime < Time.time) {
                // Elküldjük a mozgatás eseményt
                JSONClass jsonClass = new JSONClass();
                jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.dragMove;
                jsonClass[C.JSONKeys.selectedAnswer].AsInt = dragging.answerIndex;
                jsonClass[C.JSONKeys.dragPosX].AsFloat = actPos.x;
                jsonClass[C.JSONKeys.dragPosY].AsFloat = actPos.y;

                Common.taskController.SendMessageToServer(jsonClass);

                nextMoveSendTime = Time.time + moveSendTime;
            }

            // Beállítjuk a halmaz elemet az új pozícióba
            dragging.SetDragPos(actPos); // draggingPosInWorldSpace + differentMousePos);
        }

        // Elküldjük a halmaz elem elengedésének eseményét
        // ha nincs lenyomva az egér gomb vagy nem engedélyezett már a drag&drop illetve ha van megfogott elem
        if (Input.GetMouseButtonUp(0) || !dragAndDropEnabled)  //((Input.GetMouseButtonUp(0) || !dragAndDropEnabled) && dragging != null)
        {
            // Ha még lehetséges a click, akkor klikk-nek vesszük
            if (maybeClick)
            {
                if (itemClick != null)
                    itemClick(dragItem);
                maybeClick = false;
            }
            else if (dragging != null)
            {
                dragging.SetOrderInLayer(dragReleaseOrderInLayer);
                dragging.grabbed = false;

                // Megvizsgáljuk, hogy a célpont fölött engedték-e el
                DragTarget dragTarget = (DragTarget)Common.GetComponentInPos((dropPosIsCursorPos) ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : dragging.GetDropPos(), "DragTarget");
                // Ha célpont fölött engedték el és a célpontba engedélyezett a bedobás és engedélyezett a drab&drop
                if (dragTarget != null && dragTarget.enabledDrop && dragAndDropEnabled)
                {
                    // Elküldjük az elengedés eseményt a szervernek ami majd kiértékeli azt
                    JSONClass jsonClass = new JSONClass();
                    jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                    jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.answer;
                    jsonClass[C.JSONKeys.selectedQuestion].AsInt = dragTarget.questionIndex;
                    jsonClass[C.JSONKeys.selectedSubQuestion].AsInt = dragTarget.subQuestionIndex;
                    jsonClass[C.JSONKeys.selectedAnswer].AsInt = dragging.answerIndex;
                    jsonClass[C.JSONKeys.dragPosX].AsFloat = dragTarget.transform.position.x;
                    jsonClass[C.JSONKeys.dragPosY].AsFloat = dragTarget.transform.position.y;

                    Common.taskController.SendMessageToServer(jsonClass);
                }
                else {
                    // Egyébként egy olyan eseményt küldünk mintha nem célpont fölött engedték volna el
                    JSONClass jsonClass = new JSONClass();
                    jsonClass[C.JSONKeys.dataContent] = C.JSONValues.gameEvent;
                    jsonClass[C.JSONKeys.gameEventType] = C.JSONValues.dragReleased;
                    jsonClass[C.JSONKeys.selectedAnswer].AsInt = dragging.answerIndex;

                    Common.taskController.SendMessageToServer(jsonClass);
                }
            }

            dragging = null;
        }
    }

    Vector3 GetNearestTargetPos(Vector3 actPos) {
        // Megvizsgáljuk, hogy melyik target-hez van a legközelebb és az elég közel van-e, hogy bevonza
        if (ListOfDragTarget != null && ListOfDragTarget.Count > 0)
        {
            float minDistance = float.MaxValue;
            Vector3? minPos = null;

            for (int i = 0; i < ListOfDragTarget.Count; i++)
            {
                if (ListOfDragTarget[i].enabledDrop)
                {
                    float actDistance = (ListOfDragTarget[i].transform.position - actPos).magnitude;
                    if (actDistance < minDistance)
                    {
                        minDistance = actDistance;
                        minPos = ListOfDragTarget[i].transform.position;
                    }
                }
            }

            if (minPos != null && minDistance <= snapDistance)
                actPos = minPos.Value;
        }

        return actPos;
    }

    public void MessageArrived(DragTarget dragTarget, DragItem dragItem, JSONNode jsonNodeMessage)
    {
        switch (jsonNodeMessage[C.JSONKeys.gameEventType])
        {
            case C.JSONValues.drag: // Megfogták az elemet
                Debug.Log("Drag");

                // Visszejelzést küldünk a megfogás eseményéről
                if (itemDrag != null)
                    itemDrag(dragItem);

                dragging = dragItem;
                dragItem.SetOrderInLayer(dragOrderInLayer);
                draggingPosInWorldSpace = dragItem.MoveTransform.position;
                //dragMousePosInWorldSpace = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                dragItem.grabbed = true;

                dragItem.SetDragPos(draggingPosInWorldSpace); // Ez leállítja a halmaz elemen esetleg működő iTween animációt

                break;

            case C.JSONValues.dragMove: // Mozgatják az elemet
                Debug.Log("Move");

                // Csak az inaktív játékosoknál állítjuk be a pozíciót
                if (!Common.taskController.playerActive)
                    dragItem.SetDragPosWithAnim(new Vector3(jsonNodeMessage[C.JSONKeys.dragPosX].AsFloat, jsonNodeMessage[C.JSONKeys.dragPosY].AsFloat));

                break;

            case C.JSONValues.dragReleased: // Az elemet elengedték de nem egy célpont felett
                if (itemReleased != null)
                    itemReleased(dragItem);

                break;

            case C.JSONValues.answer: // Az elem kiértékelésre került (elengedték egy dragTarget felett és a válasz helyessége az evaluateAnswer-ben található
                dragItem.dragTarget = dragTarget;

                // Az elemet elengedték egy célpont felett
                if (itemReleasedOverADragTarget != null)
                    itemReleasedOverADragTarget(dragItem, dragTarget);

                if (Common.configurationController.answerFeedback != ConfigurationController.AnswerFeedback.Immediately)
                {
                    dragTarget.PutItemInTarget(dragItem);

                    // Tudatjuk a felíratkozott függvényel, hogy egy elemet jó helyre tettünk
                    if (itemPutGoodPlace != null)
                        itemPutGoodPlace(dragItem, dragTarget);
                }
                else
                {
                    StartCoroutine(answerFeedBackInmediatelly(dragTarget, dragItem, jsonNodeMessage));
                }

                break;
        }
    }

    IEnumerator answerFeedBackInmediatelly(DragTarget dragTarget, DragItem dragItem, JSONNode jsonNodeMessage)
    {
        if (jsonNodeMessage.ContainsKey(C.JSONKeys.replayMode))
        {
            // A bedobási pozícióba igazítjuk az elemet
            dragItem.SetDragPosWithAnim(dragTarget.GetDropPos(dragItem));

            while (dragItem.animationRun) yield return null;
        }

        // Megvizsgáljuk, hogy a megfelelő célpont felett lett-e elengedve
        if (jsonNodeMessage[C.JSONKeys.evaluateAnswer].Value == C.JSONValues.evaluateIsTrue)
        {
            // Igen a megfelelő célpont felett engedték el

            // Ha van pozíció megadva, akkor beállítjuk azt (csak halmazos játék esetén van pozíció megadva)
            if (jsonNodeMessage.ContainsKey(C.JSONKeys.dragPosX))
            {
                //Vector3 releasedPos = new Vector3(jsonNodeMessage[C.JSONKeys.dragPosX].AsFloat, jsonNodeMessage[C.JSONKeys.dragPosY].AsFloat);
                //dragItem.SetDragPos(releasedPos);
                //dragItem.SetBasePos(releasedPos);
            }

            dragTarget.PutItemInTarget(dragItem);

            Common.audioController.SFXPlay("positive");
            dragItem.FlashingPositive();

            // Tudatjuk a felíratkozott függvényel, hogy egy elemet jó helyre tettünk
            if (itemPutGoodPlace != null)
                itemPutGoodPlace(dragItem, dragTarget);
        }

        if (jsonNodeMessage[C.JSONKeys.evaluateAnswer].Value == C.JSONValues.evaluateIsFalse)
        {
            // Nem a megfelelő halmazba került
            if (itemPutWrongPlace != null)
                itemPutWrongPlace(dragItem, dragTarget);

            Common.audioController.SFXPlay("negative");
            dragItem.FlashingNegative();
        }

        if (jsonNodeMessage[C.JSONKeys.evaluateAnswer].Value == C.JSONValues.evaluateIsIgnore)
        {
            // Nem a megfelelő halmazba került
            if (itemPutWrongPlace != null)
                itemPutWrongPlace(dragItem, dragTarget);
        }
        //if (jsonNodeMessage[C.JSONKeys.evaluateAnswer].Value == C.JSONValues.evaluateIsSilent)
        //{
        //    dragTarget.PutItemInTarget(dragItem);
        //
        //    // Tudatjuk a felíratkozott függvényel, hogy egy elemet jó helyre tettünk
        //    if (itemPutGoodPlace != null)
        //        itemPutGoodPlace(dragItem, dragTarget);
        //}
        //break;
    }

    IEnumerator WaitWhileMove(DragItem dragItem, Common.CallBack callBack)
    {
        while (dragItem.animationRun) yield return null;

        callBack();
    }
}
