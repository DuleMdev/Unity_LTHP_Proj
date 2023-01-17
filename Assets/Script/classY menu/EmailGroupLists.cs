using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmailGroupLists 
{
    public List<EmailGroupList> list;
    
    public EmailGroupLists(JSONNode json)
    {
        list = new List<EmailGroupList>();
    
        for (int i = 0; i < json.Count; i++)
            list.Add(new EmailGroupList(json[i]));
    }

    public EmailGroup GetEmailGroup(int listIndex, string groupID)
    {
        for (int i = 0; i < list[listIndex].list.Count; i++)
            if (list[listIndex].list[i].id == groupID)
                return list[listIndex].list[i];

        return null;
     }
}
