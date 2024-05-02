using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class ReviewDayObject : Review
{

    public TextMeshProUGUI dayText;

    private int day;

    public void Init(int day)
    {
        this.day = day;

        dayText.text = string.Format("> " + TextManager.Instance.GetCommons("Day"), this.day + 1);
    }
}
