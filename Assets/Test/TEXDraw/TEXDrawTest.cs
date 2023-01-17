using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEXDrawTest : MonoBehaviour
{
    TEXDraw texDraw;

    void Awake()
    {
        texDraw = GetComponent<TEXDraw>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(texDraw.GetComponent<RectTransform>().sizeDelta);
    }
}
