using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineOrOfflineMenu : MonoBehaviour
{
    public void ButtonClick(string buttonName)
    {
        switch (buttonName)
        {
            case "Back":
                ClassroomScreens.instance.ChangeScreenWithAnimation(ClassroomScreens.ClassroomScreensEnum.TeacherStudentChoice);
                break;
            case "OnlineStart":
                ClassroomScreens.instance.ChangeScreenWithAnimation(ClassroomScreens.ClassroomScreensEnum.ClassroomGroupList);
                break;
            case "PathDownload":
                break;
            case "ClassroomGroupsDownload":
                break;
            case "OfflineStart":
                break;
            case "OfflineDatasUpload":
                break;
        }

        Debug.Log(buttonName);
    }
}
