using UnityEngine;
using System.Collections;
using SimpleJSON;

public class TaskFreePlayData : TaskAncestor {

    public TaskFreePlayData() {

    }

    public TaskFreePlayData(JSONNode jsonNode) {
        taskType = TaskType.FreePlay;
        InitDatas(jsonNode);
    }

    override public void InitDatas(JSONNode jsonNode)
    {

    }
}
