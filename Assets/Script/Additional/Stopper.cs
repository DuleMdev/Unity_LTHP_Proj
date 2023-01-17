using UnityEngine;
using System.Collections;

public class Stopper : MonoBehaviour {

    static Stopper instance;
    static float startTime;
    static float targetTime;

	// Use this for initialization
	void Awake () {
        instance = new Stopper();
	}

    static void Start(float targetTime = 0) {
        startTime = Time.time;
        Stopper.targetTime = targetTime;
    }

    static void SetTime(float targetTime) {
        Stopper.targetTime = targetTime;
    }

    static void AddTime(float time) {
        targetTime += time;
    }

    

}

