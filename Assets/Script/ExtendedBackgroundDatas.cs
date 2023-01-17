using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
A játék képernyők 16:9-be készültek.
Ha más képarányú volt a képernyő, akkor korábban ott feketeség volt látható a CanvasBorder_16_9 -es szkriptnek köszönhetően.
Viszont mostantól (2019.08.23) a kiterjesztett játék háttérnek kell ott látszódnia.

Ez az objektum tartalmazza a kiterjesztett képeket amiket a CanvasBorder_16_9 -es objektumnak meg kell jelenítenie.

Előfordul, hogy több képre is szükség van egy területen, ha például a színátmenetes hátterű játékokra gondolunk.
Tehát az egyik kép az alap amin van egy részben átlátszó második kép, esetleg más színnel.
*/
public class ExtendedBackgroundDatas : MonoBehaviour
{
    [System.Serializable]
    public class ExtendedBackgroundData
    {
        public Color color;
        public Sprite up;
        public Sprite down;
        public Sprite left;
        public Sprite right;
    }

    public List<ExtendedBackgroundData> extendedBackgroundDataList;
}
