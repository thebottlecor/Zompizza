using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Act6_Capsule : MonoBehaviour
{

    public Transform target;

    public Animator[] animators;


    void Start()
    {
        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        target.transform.position = this.transform.position;
        target.transform.rotation = this.transform.rotation;
        target.transform.localScale = this.transform.localScale;

        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].SetBool("Walk", true);
        }
    }

}
