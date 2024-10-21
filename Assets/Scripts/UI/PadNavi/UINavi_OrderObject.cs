using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UINavi_OrderObject : UINavi
{

    public OrderUIObject orderUIObject;

    public override void Highlight(int padType, DataManager data)
    {
        base.Highlight(padType, data);

        orderUIObject.OnPointerEnter(null);
    }

    public override void DeHighlight()
    {
        base.DeHighlight();

        orderUIObject.OnPointerExit(null);
    }
}
