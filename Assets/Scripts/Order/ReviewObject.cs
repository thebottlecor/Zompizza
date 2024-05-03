using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class ReviewObject : Review
{

    public Image profile;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI context;

    private int day;
    private int customerIdx;
    private float timeRating; // -5 ~ 5 사이
    private float hpRating; // -5 ~ 5 사이

    public void Init(int day, int idx, float time, float hp)
    {
        this.day = day;
        customerIdx = idx;

        timeRating = time;
        hpRating = hp;

        profile.sprite = DataManager.Instance.uiLib.customerProfile[customerIdx];
        nameText.text = TextManager.Instance.GetNames(customerIdx + Constant.npcNameOffset);

        float rating = timeRating + hpRating;

        StringBuilder st = new StringBuilder();

        int emoji_Time;
        if (time >= 4f)
            emoji_Time = 4;
        else if (time >= 1f)
            emoji_Time = 3;
        else if (time > -1f)
            emoji_Time = 2;
        else if (time > -4f)
            emoji_Time = 1;
        else
            emoji_Time = 0;

        int emoji_Pizza;
        if (hp >= 4f)
            emoji_Pizza = 4;
        else if (hp >= 1f)
            emoji_Pizza = 3;
        else if (hp > -1f)
            emoji_Pizza = 2;
        else if (hp > -4f)
            emoji_Pizza = 1;
        else
            emoji_Pizza = 0;

        st.AppendFormat("<size=120%><sprite={0}></size>   <sprite=\"emoji\" index={1}>", 3, emoji_Time);
        st.Append("          ");
        st.AppendFormat("<size=120%><sprite={0}></size>   <sprite=\"emoji\" index={1}>", 0, emoji_Pizza);
        st.Append("\n<line-height=50%> \n");
        st.AppendFormat("   {0:0.#}  ", rating);

        bool minus = rating < 0;
        int count = (int)Mathf.Abs(rating);
        bool hasPoint = Mathf.Abs(rating) - count > 0; // 절반 별점 표시

        if (!minus)
        {
            for (int i = 0; i < count; i++)
            {
                st.Append("<sprite=1> ");
            }
            if (hasPoint) st.Append("<sprite=4> ");
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                st.Append("<sprite=1 color=#FF1C1C> ");
            }
            if (hasPoint) st.Append("<sprite=4 color=#FF1C1C> ");
        }

        context.text = st.ToString();
    }

}
