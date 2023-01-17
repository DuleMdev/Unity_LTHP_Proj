using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Notifications : MonoBehaviour
{
    static public Notifications instance;

    public string pushProvider = null;
    public string userPushID = null;

	// Use this for initialization
	void Awake ()
    {
        instance = this;
    }

    void Start()
    {
     //   if (Common.configurationController.appID == ConfigurationController.AppID.PROVOCENT)
     //   {
     //       UTNotifications.Manager.Instance.OnSendRegistrationId +=
     //       (string pushProvider, string userPushID) =>
     //       {
     //           Debug.Log(pushProvider + " registrationID : " + userPushID);
     //           this.pushProvider = pushProvider;
     //           this.userPushID = userPushID;
     //       };
     //
     //       //Debug.LogWarning("FirebaseSenderID" + UTNotifications.Settings.Instance.FirebaseSenderID);
     //
     //       UTNotifications.Settings.Instance.SetFirebaseSenderID("190355336861");
     //
     //       //UTNotifications.Settings.Instance.FirebaseSenderID = "190355336861";
     //
     //       //Debug.LogWarning("FirebaseSenderID" + UTNotifications.Settings.Instance.FirebaseSenderID);
     //
     //       UTNotifications.Manager.Instance.Initialize(false);
     //   }
    }

	// Update is called once per frame
	void Update () {
		
	}
}
