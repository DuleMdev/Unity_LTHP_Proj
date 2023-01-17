using UnityEngine;
using System.Collections;

/*
A buborékos játéknál a lámpát villogtatja.
*/

public class LampLight : MonoBehaviour {

    public float MaxTime;
    public float MinTime;

    SpriteRenderer lampLight;
    bool enabledLight;               // A lámpa fel van-e kapcsolva

    float aktTime;

	// Use this for initialization
	void Awake () {
        lampLight = GetComponent<SpriteRenderer>();

        enabledLight = true;
    }
	
	// Update is called once per frame
	void Update () {
        aktTime -= Time.deltaTime;
        if (aktTime < 0) {
            aktTime = (float)Common.random.NextDouble() * (MaxTime - MinTime) + MinTime;

            if (enabledLight)
                lampLight.enabled = !lampLight.enabled;
        }
	}

    public void OnMouseDown()
    {
        // Mi történjen ha a lámpára kattintottak
        enabledLight = !enabledLight;
        lampLight.enabled = enabledLight;
        aktTime = MaxTime;
    }
}
