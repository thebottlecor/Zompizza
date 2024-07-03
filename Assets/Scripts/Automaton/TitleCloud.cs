using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCloud : MonoBehaviour
{
    public float targetZ = 200f;
    public float speed = 1f;

    public Vector3 initPos = new Vector3(-31.6f, 23.02f, 25f);

    void Update()
    {
        transform.position += speed * Time.unscaledDeltaTime * new Vector3(0f, 0f, 1f);

        if (transform.position.z >= targetZ)
        {
            transform.position = initPos;
        }
    }
}
