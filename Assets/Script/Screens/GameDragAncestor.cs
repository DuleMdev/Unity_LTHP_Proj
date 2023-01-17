using UnityEngine;
using System.Collections;

public class GameDragAncestor : TrueGameAncestor {

    protected DragAndDropControl dragAndDropControl;

    override public void Awake() {
        base.Awake();

        dragAndDropControl = gameObject.GetComponent<DragAndDropControl>();
    }
	
	// Update is called once per frame
	new void Update () {
        base.Update();

        // Meghatározzuk, hogy létezik-e a zoomer komponens és éppen ki van-e nagyítva egy kép
        // mert ha igen, akkor le kell tiltani a dragAndDrop rendszert
        bool dragBlockByZoomer = false;
        if (zoomer)
            dragBlockByZoomer = zoomer.active;

        dragAndDropControl.dragAndDropEnabled = (status == Status.Play && !paused && !dragBlockByZoomer);
    }
}
