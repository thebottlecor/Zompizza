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
    private float time;
    private float hp;
    private int customerIdx;
    private int goal;

    private float timeRating; // -5 ~ 5 사이
    private float hpRating; // -5 ~ 5 사이

    public ReviewData GetReviewData()
    {
        return new ReviewData
        {
            day = day,
            time = time,
            hp = hp,
            customerIdx = customerIdx,
            goal = goal
        };
    }

    public float Init(int day, int idx, float time, float hp, int goal)
    {
        this.day = day;
        this.time = time;
        this.hp = hp;
        customerIdx = idx;
        this.goal = goal;

        timeRating = time;
        hpRating = hp;

        profile.sprite = DataManager.Instance.uiLib.customerProfile[customerIdx];
        nameText.text = TextManager.Instance.GetSurvivorName(customerIdx + Constant.npcNameOffset);

        float rating = timeRating + hpRating;

        int specialCase = 0;

        if (timeRating == -10000f)
        {
            rating = hpRating; // 주문 미배달의 경우
            specialCase = 1;
        }
        else if (timeRating == -1000f)
        {
            rating = hpRating; // 재료가 있어도 받지 않았던 경우
            specialCase = 2;
        }
        else if (timeRating == -100f)
        {
            rating = hpRating; // 재료가 부족했던 경우
            specialCase = 3;
        }

        StringBuilder st = new StringBuilder();

        int emoji_Time;
        if (time == 2.5f)
            emoji_Time = 4;
        else if (time >= 1.0f)
            emoji_Time = 3;
        else if (time > -0.5f)
            emoji_Time = 2;
        else if (time > -2f)
            emoji_Time = 1;
        else
            emoji_Time = 0;

        int emoji_Pizza;
        if (hp == 2.5f)
            emoji_Pizza = 4;
        else if (hp >= 1.0f)
            emoji_Pizza = 3;
        else if (hp > -0.5f)
            emoji_Pizza = 2;
        else if (hp > -2f)
            emoji_Pizza = 1;
        else
            emoji_Pizza = 0;

        switch (specialCase)
        {
            case 0:
                st.AppendFormat("<size=120%><sprite={0}></size>   <sprite=\"emoji\" index={1}>", 3, emoji_Time);
                st.Append("          ");
                st.AppendFormat("<size=120%><sprite={0}></size>   <sprite=\"emoji\" index={1}>", 0, emoji_Pizza);
                break;
            case 1:
                st.AppendFormat("<size=120%><sprite={0}></size>   <sprite=\"emoji\" index={1}>", 3, 5);
                st.Append("          ");
                st.AppendFormat("<size=120%><sprite={0}></size>   <sprite=\"emoji\" index={1}>", 0, 5);
                break;
            case 2:
                st.AppendFormat("<size=120%><sprite={0}></size>   <sprite=\"emoji\" index={1}>", 3, 5);
                break;
            case 3:
                st.AppendFormat("<size=120%><sprite={0}></size>   <sprite=\"emoji\" index={1}>", 0, 5);
                break;
        }

        st.Append("\n<line-height=50%> \n");
        st.AppendFormat("   {0:0.#}  ", rating);

        bool minus = rating < 0;
        int count = (int)Mathf.Abs(rating);
        bool hasPoint = Mathf.Abs(rating) - count > 0; // 절반 별점 표시

        if (!minus)
        {
            if (count == 0)
            {
                st.Append("<sprite=5> ");
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    st.Append("<sprite=1> ");
                }
                if (hasPoint) st.Append("<sprite=4> ");
            }
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

        return rating;
    }

}
