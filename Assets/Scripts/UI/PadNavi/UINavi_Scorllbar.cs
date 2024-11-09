using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UINavi_Scorllbar : UINavi
{
    //public float scrollbarSpeed = 2f;

    public float AdjustSpeed
    {
        get
        {
            float height = contents.sizeDelta.y;

            if (height < 1f) height = 1f;

            float value = Mathf.Min(4f, 900f / height);

            return value;
        }
    }

    public RectTransform contents;
}
