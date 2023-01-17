using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPath : MonoBehaviour
{
    public int segmentCount;
    public bool refresh;

    public float length;

    float _length;

    LineRenderer lineRenderer;

    List<Vector3> list = new List<Vector3>();

    List<float> listOfStartPos; // A szegmensek kezdetét tárolja a vonal hosszán
    

    // Start is called before the first frame update
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        RefreshLine();
    }

    void RefreshLine()
    {
        if (segmentCount < 1)
            segmentCount = 1;

        list = new List<Vector3>();

        if (transform.childCount > 1)
        {
            for (int i = 1; i < transform.childCount; i++)
            {
                list.AddRange(GetSegment(
                    transform.GetChild(i - 1).position,
                    transform.GetChild(i - 1).Find("next").position,
                    transform.GetChild(i).position,
                    transform.GetChild(i).Find("prev").position,
                    segmentCount));
            }

            listOfStartPos = new List<float>();
            listOfStartPos.Add(0);
            _length = 0;
            for (int i = 1; i < list.Count; i++)
            {
                _length += Vector3.Distance(list[i - 1], list[i]);
                listOfStartPos.Add(_length);
            }
        }

        length = _length;

        lineRenderer.positionCount = list.Count;
        lineRenderer.SetPositions(list.ToArray());
    }

    public Vector3 GetPos(float percent)
    {
        float pos = percent * _length;
        int segment = GetSegment(pos);

        if (segment == listOfStartPos.Count - 1)
            return list[segment];
        else
            return Vector3.Lerp(list[segment], list[segment + 1], (pos - listOfStartPos[segment]) / (listOfStartPos[segment + 1] - listOfStartPos[segment]));
    }

    public  Quaternion GetAngleZ(float percent)
    {
        float pos = percent * _length;
        int segment = GetSegment(pos);

        if (segment == listOfStartPos.Count - 1)
            segment--;

        float distance = Vector3.Distance(list[segment], list[segment + 1]);
        float xdiff = list[segment].x - list[segment + 1].x;

        float angle = Mathf.Asin(xdiff / distance); // A szög radiánban
        angle = (float)(angle / (2 * Math.PI) * 360); // A szög fokokban

        if (list[segment + 1].y < list[segment].y)
            angle = 180 - angle;

        return Quaternion.Euler(new Vector3(0, 0, angle));
    }

    int GetSegment(float length)
    {
        int min = 0;
        int max = listOfStartPos.Count - 1;

        while (min != max)
        {
            int akt = (min + max + 1) / 2;

            if (listOfStartPos[akt] <= length)
                min = akt;

            if (listOfStartPos[akt] > length)
                max = akt - 1;
        }

        return min;
    }

    List<Vector3> GetSegment(Vector3 pos1, Vector3 controlPoint1, Vector3 pos2, Vector3 controlPoint2, int segmentCount)
    {
        List<Vector3> list = new List<Vector3>();
        for (int i = 0; i <= segmentCount; i++)
        {
            list.Add(GetPoint(new List<Vector3> { pos1, controlPoint1, controlPoint2, pos2 }, 1f / segmentCount * i));
        }

        return list;
    }

    Vector3 GetPoint(List<Vector3> list, float percent)
    {
        if (list.Count == 1)
            return list[0];

        List<Vector3> newList = new List<Vector3>();
        for (int i = 1; i < list.Count; i++)
            newList.Add(Vector3.Lerp(list[i - 1], list[i], percent));

        return GetPoint(newList, percent);
    }

    // Update is called once per frame
    void Update()
    {
        if (refresh)
        {
            RefreshLine();
        }
    }
    
}
