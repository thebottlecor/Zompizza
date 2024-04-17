using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingUIEffect : MonoBehaviour
{

    private RectTransform rectTransform;
    public Vector2 targetPos;
    public float animSpeed = 3f;
    private Vector2 initPos;
    private Vector2 animDir;


    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initPos = rectTransform.anchoredPosition;
        animDir = (targetPos - initPos).normalized;
    }

    void Update()
    {
        float dist = (targetPos - rectTransform.anchoredPosition).magnitude;
        float speed = animSpeed * Time.unscaledDeltaTime;

        if (dist < speed * 2f)
        {
            // ÃÊ±âÈ­
            rectTransform.anchoredPosition = initPos;
        }
        else
        {
            Vector2 newPos = rectTransform.anchoredPosition + animDir * speed;
            rectTransform.anchoredPosition = newPos;
        }
    }
}
