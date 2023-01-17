using UnityEngine;
using System.Collections;

/*
Ezt az interface-t implementáló objektumoknak implementálni kell az alábbi metódusokat.
Így meg kell tudniuk adni, hogy mennyi az elem globális szélessége, illetve magassága
*/

public interface IWidthHeight {
    float GetHeight();
    float GetWidth();
}
