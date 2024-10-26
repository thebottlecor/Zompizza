using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankingObj : MonoBehaviour
{


    public Image medal_Img;
    public Image logo_Img;
    public Image x_Img;

    public RectTransform graph_rect;

    public TextMeshProUGUI ratingText;
    public TextMeshProUGUI nameText;


    public void Init(int idx, int ranking, float rating, float percent)
    {
        ratingText.text = string.Format("{0:F0}", rating);

        switch (idx)
        {
            case 0:
                nameText.text = TextManager.Instance.GetCommons("RivalName0");
                break;
            case 1:
                nameText.text = TextManager.Instance.GetCommons("RivalName1");
                break;
            case 2:
                nameText.text = TextManager.Instance.GetCommons("You");
                break;
        }

        if (idx <= 1)
        {
            x_Img.gameObject.SetActive(rating <= 0f);
        }
        else
        {
            x_Img.gameObject.SetActive(false);
        }

        logo_Img.sprite = RivalManager.Instance.icons[idx];

        medal_Img.sprite = RivalManager.Instance.medals[ranking];

        graph_rect.sizeDelta = new Vector2(48f, 50f + 200f * percent);
    }

}
