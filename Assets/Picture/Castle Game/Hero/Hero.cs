using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    Animator animator;
    bool walk;

    Common.CallBack callBack;

    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialize(Common.CallBack callBack)
    {
        this.callBack = callBack;
    }

    void OnNavigationStarted()
    {
        animator.SetBool("Walk", true);
    }

    void OnDestinationReached()
    {
        animator.SetBool("Walk", false);

        if (callBack != null)
            callBack();
    }

    public void HeroStep()
    {
        Common.audioController.SFXPlay("HeroStep");
    }
}
