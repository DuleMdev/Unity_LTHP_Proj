using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YoutubeTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LinkReady(string link)
    {
        Debug.Log("Link:\n" + link);
    }
}
