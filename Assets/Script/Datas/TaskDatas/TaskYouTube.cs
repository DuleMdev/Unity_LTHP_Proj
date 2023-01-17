using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

    {
        "link" : "link a videóra pl. http:\\blabla.com\valami.avi"
    }

*/

public class TaskYouTube : TaskAncestor {

    public string link;
    public SrtProcessor srtProcessor;

    // A megadott JSON alapján inicializálja a változóit
    public TaskYouTube(JSONNode jsonNode)
    {
        taskType = TaskType.YouTube;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode node)
    {
        error = false;

        id = "-1";
        time = 0;
        link = node[C.JSONKeys.link].Value;
        if (node.ContainsKey(C.JSONKeys.subtitle))
            srtProcessor = new SrtProcessor(Common.Base64Decode(node[C.JSONKeys.subtitle].Value));
    }
}
