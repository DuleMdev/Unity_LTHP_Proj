using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmailGroupListRequestData
{
    public string requestName;
    public string color;
    public bool bigItems;

    public EmailGroupListRequestData(string requestName, string color = "", bool bigItems = false)
    {
        this.requestName = requestName;
        this.color = color;
        this.bigItems = bigItems;
    }
}
