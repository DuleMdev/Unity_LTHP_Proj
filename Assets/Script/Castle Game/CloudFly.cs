using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudFly : MonoBehaviour
{
    [Tooltip("pixel/másodperc")]
    public float speed = 100;

    RectTransform CanvasRect;
    RectTransform ownRect;

    // Start is called before the first frame update
    void Awake()
    {
        CanvasRect = GetComponentInParent<Canvas>(true).GetComponent<RectTransform>();
        ownRect = (RectTransform)transform;
    }

    // Update is called once per frame
    void Update()
    {
        float width = CanvasRect.sizeDelta.x + ownRect.sizeDelta.x * ownRect.localScale.x;

        float maxPos = width / 2;
        float minPos = -maxPos;

        // haladás
        float going = Time.deltaTime * speed;

        float newPos = ownRect.localPosition.x + going;

        ownRect.localPosition = new Vector3(
            (newPos < maxPos) ? newPos : minPos,
            ownRect.localPosition.y,
            ownRect.localPosition.z
            );
    }
}
