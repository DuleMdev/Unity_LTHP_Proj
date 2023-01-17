using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PrefabTwoTriangle : MonoBehaviour {

    Image leftTriangle;
    Image rightTriangle;

	// Use this for initialization
	void Awake () {
        leftTriangle = gameObject.SearchChild("ImageLeftTriangle").GetComponent<Image>();
        rightTriangle = gameObject.SearchChild("ImageRightTriangle").GetComponent<Image>();
    }

    public void SetTriangleColor(Color leftTriangleColor, Color rightTriangleColor) {
        leftTriangle.color = leftTriangleColor;
        rightTriangle.color = rightTriangleColor;
    }
}
