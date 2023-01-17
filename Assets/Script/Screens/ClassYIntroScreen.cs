using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassYIntroScreen : HHHScreen {

    static public ClassYIntroScreen instance;

    [Tooltip("Melyik képernyő jöjjön az introscreen után")]
    public HHHScreen nextScreen;

    [Tooltip("Mennyi ideig látszódjon az intro képernyő")]
    public float showIntroTime;

    GameObject testServer;

    float time;

    // Use this for initialization
    void Awake()
    {
        instance = this;

        testServer = gameObject.SearchChild("TextTestServer").gameObject;

        CanvasBorder_16_9.instance.SetBorderColor(Common.MakeColor("#F08621"));
    }

    void Start()
    {
        testServer.SetActive(
            Common.configurationController.link == ConfigurationController.Link.TestLink ||
            Common.configurationController.link == ConfigurationController.Link.Server2020DuckLink);
    }

    // Update is called once per frame
    void Update () {
        // Ha már teljesen megjelent a képernyő, akkor elkezdjük mérni az időt
        if (!Common.screenController.changeScreenInProgress)
        {
            time += Time.deltaTime;

            // Ha letelt az idő, akkor átváltunk a megadott képernyőre
            if (time > showIntroTime)
            {
                time = 0;

                Common.screenController.LoadScreenAfterIntro();
            }
        }
    }
}
