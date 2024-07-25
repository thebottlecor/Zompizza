using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public struct ResearchEffect
{
    public float goldGet;
    public float ratingGet;

    public int explore_max_pay;
    public float explore_cost;
    public float explore_get_bonus;

    public int tier;

    // �̿� **
    public int meat_tier;
    public int vegetable_tier;
    public int herb_tier;
    public int production_tier;
    public float production_bonus;
    // �̿� **

    public int pizzeriaExpand;

    public int raidDefense;

    public float customer_timelimit;
    public int customer_max_tier;
    public int customer_max_amount;
    public int order_max;
    // �̿�
    public int customer_max_type;

    public int maxSpeed;
    public float damageReduce;
    public float acceleration;

    public void ShowgoldGet(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = goldGet;
        if (value != 0f)
        {
            string sub;
            if (value > 0f)
                sub = string.Format(tm.defaultCultureInfo, "+{0:0.#}%", value * 100f);
            else
                sub = string.Format(tm.defaultCultureInfo, "{0:0.#}%", value * 100f);
            st.AppendFormat(tm.GetCommons("UpgradeEffect0"), "<sprite=2>", sub);
            st.AppendLine();
        }
    }
    public void ShowratingGet(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = ratingGet;
        if (value != 0f)
        {
            string sub;
            if (value > 0f)
                sub = string.Format(tm.defaultCultureInfo, "+{0:0.#}%", value * 100f);
            else
                sub = string.Format(tm.defaultCultureInfo, "{0:0.#}%", value * 100f);
            st.AppendFormat(tm.GetCommons("UpgradeEffect1"), "<sprite=1>", sub);
            st.AppendLine();
        }
    }

    public void Showexplore_max_pay(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = explore_max_pay * 100;
        if (value != 0)
        {
            string sub;
            if (value > 0)
                sub = string.Format("+{0}", value);
            else
                sub = string.Format("{0}", value);
            st.AppendFormat(tm.GetCommons("UpgradeEffect2"), sub);
            st.AppendLine();
        }
    }
    public void Showexplore_cost(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = explore_cost;
        if (value != 0f)
        {
            string sub;
            if (value > 0f)
                sub = string.Format(tm.defaultCultureInfo, "+{0:0.#}%", value * 100f);
            else
                sub = string.Format(tm.defaultCultureInfo, "{0:0.#}%", value * 100f);
            st.AppendFormat(tm.GetCommons("UpgradeEffect3"), sub);
            st.AppendLine();
        }
    }
    public void Showexplore_get_bonus(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = explore_get_bonus;
        if (value != 0f)
        {
            string sub;
            if (value > 0f)
                sub = string.Format(tm.defaultCultureInfo, "+{0:0.#}%", value * 100f);
            else
                sub = string.Format(tm.defaultCultureInfo, "{0:0.#}%", value * 100f);
            st.AppendFormat(tm.GetCommons("UpgradeEffect4"), sub);
            st.AppendLine();
        }
    }

    public void Showtier(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = tier;
        if (value > 0)
        {
            st.AppendFormat(tm.GetCommons("UpgradeEffect5"), value + 1);
            st.AppendLine();
        }
    }

    public void Showmeat_tier(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = meat_tier;
        if (value > 0)
        {
            st.Append(tm.GetCommons("UpgradeEffect6"));
            st.AppendLine();
        }
    }
    public void Showvegetable_tier(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = vegetable_tier;
        if (value > 0)
        {
            st.Append(tm.GetCommons("UpgradeEffect7"));
            st.AppendLine();
        }
    }
    public void Showherb_tier(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = herb_tier;
        if (value > 0)
        {
            st.Append(tm.GetCommons("UpgradeEffect8"));
            st.AppendLine();
        }
    }
    public void Showproduction_tier(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = production_tier;
        if (value > 0)
        {
            st.AppendFormat(tm.GetCommons("UpgradeEffect9"), value);
            st.AppendLine();
        }
    }
    public void Showproduction_bonus(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = production_bonus;
        if (value != 0f)
        {
            string sub;
            if (value > 0f)
                sub = string.Format(tm.defaultCultureInfo, "+{0:0.#}%", value * 100f);
            else
                sub = string.Format(tm.defaultCultureInfo, "{0:0.#}%", value * 100f);
            st.AppendFormat(tm.GetCommons("UpgradeEffect10"), sub);
            st.AppendLine();
        }
    }

    public void ShowpizzeriaExpand(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = pizzeriaExpand;
        if (value == 1) //*** ����
        {
            st.Append(tm.GetCommons("UpgradeEffect11"));
            st.AppendLine();
        }
    }

    public void ShowraidDefense(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = raidDefense;
        if (value > 0)
        {
            st.Append(tm.GetCommons("UpgradeEffect12"));
            st.AppendLine();
        }
    }

    public void Showcustomer_timelimit(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = customer_timelimit;
        if (value != 0f)
        {
            string sub;
            if (value > 0f)
                sub = string.Format(tm.defaultCultureInfo, "+{0:0.#}%", value * 100f);
            else
                sub = string.Format(tm.defaultCultureInfo, "{0:0.#}%", value * 100f);
            st.AppendFormat(tm.GetCommons("UpgradeEffect13"), sub);
            st.AppendLine();
        }
    }
    public void Showcustomer_max_tier(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = customer_max_tier;
        if (value > 0)
        {
            st.AppendFormat(tm.GetCommons("UpgradeEffect14"), value + 1);
            st.AppendLine();
        }
    }
    public void Showcustomer_max_amount(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = customer_max_amount;
        if (value != 0)
        {
            string sub;
            if (value > 0)
                sub = string.Format("+{0}", value);
            else
                sub = string.Format("{0}", value);
            st.AppendFormat(tm.GetCommons("UpgradeEffect15"), sub);
            st.AppendLine();
        }
    }
    public void Showorder_max(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = order_max;
        if (value != 0)
        {
            string sub;
            if (value > 0)
                sub = string.Format("+{0}", value);
            else
                sub = string.Format("{0}", value);
            st.AppendFormat(tm.GetCommons("UpgradeEffect20"), sub);
            st.AppendLine();
        }
    }
    public void Showcustomer_max_type(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = customer_max_type;
        if (value != 0)
        {
            string sub;
            if (value > 0)
                sub = string.Format("+{0}", value);
            else
                sub = string.Format("{0}", value);
            st.AppendFormat(tm.GetCommons("UpgradeEffect16"), sub);
            st.AppendLine();
        }
    }


    public void ShowmaxSpeed(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = maxSpeed;
        if (value != 0)
        {
            string sub;
            if (value > 0f)
                sub = string.Format("+{0}km/h", value);
            else
                sub = string.Format("{0}km/h", value);
            st.AppendFormat(tm.GetCommons("UpgradeEffect17"), sub);
            st.AppendLine();
        }
    }
    public void ShowdamageReduce(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = damageReduce;
        if (value != 0f)
        {
            string sub;
            if (value > 0f)
                sub = string.Format(tm.defaultCultureInfo, "+{0:0.#}%", value * 100f);
            else
                sub = string.Format(tm.defaultCultureInfo, "{0:0.#}%", value * 100f);
            st.AppendFormat(tm.GetCommons("UpgradeEffect18"), sub);
            st.AppendLine();
        }
    }
    public void Showacceleration(StringBuilder st)
    {
        var tm = TextManager.Instance;
        var value = acceleration;
        if (value != 0f)
        {
            string sub;
            if (value > 0f)
                sub = string.Format(tm.defaultCultureInfo, "+{0:0.#}%", value * 100f);
            else
                sub = string.Format(tm.defaultCultureInfo, "{0:0.#}%", value * 100f);
            st.AppendFormat(tm.GetCommons("UpgradeEffect19"), sub);
            st.AppendLine();
        }
    }
}
//[System.Serializable]
//public struct ResearchUnlockRecipe
//{
//    public BuildingType building;
//    public int recipeIdx;
//}

[CreateAssetMenu(fileName = "ResearchInfo", menuName = "Research/New ResearchInfo")]
public class ResearchInfo : ScriptableObject
{

    public int idx;

    public bool main;
    public int tier; // ������ ������ �󸶳� �ؾ� �ϴ���
    public int romanNum; // �ߺ��Ǵ� ������ ��ȭ�� ������ ���, �θ� ���ڷ� ǥ�� (1���� II�� ǥ�õ�)

    //public int height;
    public enum ResearchGroup
    {
        upgrade = 0,
        vehicle = 1,
    }

    public ResearchGroup group;

    // �ִ� ���� ���� Ƚ��
    public int max = 1;
    [Space(5f)]
    public int cost; // �⺻ ���
    public float increaseCost; // ������ Ƚ������ �����ϴ� ���
    [Space(5f)]
    public float researchPoint; // �Һ�Ǵ� ����
    public float increaseRP; // ���� Ƚ������ �����ϴ� ����
    [Space(5f)]
    public float rating_require; // �ʿ��� ���� (�Һ���� ���� - ��� �����Ǵ� ���� ���)
    public float increaseRating_require; // ������ Ƚ������ �����ϴ� �ʿ��� ����
    [Space(5f)]
    public List<ResearchInfo> needResearch;
    [Space(5f)]
    public bool hidden; // ��� ������ - �����Ǳ� ������ (�߰�������) �������� ����
    public bool invalid;
    [Space(5f)]
    public ResearchEffect effect;

    //public List<BuildingType> unlockBuildings;
    //public List<ResearchUnlockRecipe> unlockRecipes;

    public Sprite icon;

    public int Cost(int level = 0)
    {
        int value = cost;

        if (level >= 1)
            value += (int)(level * increaseCost);

        return value;
    }
    public float ResearchPoint(int level = 0)
    {
        float value = researchPoint;

        if (level >= 1)
            value += level * increaseRP;

        return value;
    }
    public float Rating_Require(int level = 0)
    {
        float value = rating_require;

        if (level >= 1)
            value += level * increaseRating_require;

        return value;
    }

    public ResearchEffect AddEffect(ResearchEffect global)
    {

        ResearchEffect temp = new ResearchEffect
        {
            goldGet = effect.goldGet + global.goldGet,
            ratingGet = effect.ratingGet + global.ratingGet,

            explore_max_pay = effect.explore_max_pay + global.explore_max_pay,
            explore_cost = effect.explore_cost + global.explore_cost,
            explore_get_bonus = effect.explore_get_bonus + global.explore_get_bonus,

            //tier = effect.tier + global.tier,

            meat_tier = effect.meat_tier + global.meat_tier,
            vegetable_tier = effect.vegetable_tier + global.vegetable_tier,
            herb_tier = effect.herb_tier + global.herb_tier,
            //production_tier = effect.production_tier + global.production_tier,
            production_bonus = effect.production_bonus + global.production_bonus,

            //pizzeriaExpand = effect.pizzeriaExpand + global.pizzeriaExpand,

            //raidDefense = effect.raidDefense + global.raidDefense,

            customer_timelimit = effect.customer_timelimit + global.customer_timelimit,
            //customer_max_tier = effect.customer_max_tier + global.customer_max_tier,
            customer_max_amount = effect.customer_max_amount + global.customer_max_amount,
            order_max = effect.order_max + global.order_max,
            customer_max_type = effect.customer_max_type + global.customer_max_type,

            maxSpeed = effect.maxSpeed + global.maxSpeed,
            damageReduce = effect.damageReduce + global.damageReduce,
            acceleration = effect.acceleration + global.acceleration,
        };

        if (effect.tier > global.tier)
            temp.tier = effect.tier;
        else
            temp.tier = global.tier;

        if (effect.production_tier > global.production_tier)
            temp.production_tier = effect.production_tier;
        else
            temp.production_tier = global.production_tier;

        if (effect.pizzeriaExpand > global.pizzeriaExpand)
            temp.pizzeriaExpand = effect.pizzeriaExpand;
        else
            temp.pizzeriaExpand = global.pizzeriaExpand;

        if (effect.raidDefense > global.raidDefense)
            temp.raidDefense = effect.raidDefense;
        else
            temp.raidDefense = global.raidDefense;

        if (effect.customer_max_tier > global.customer_max_tier)
            temp.customer_max_tier = effect.customer_max_tier;
        else
            temp.customer_max_tier = global.customer_max_tier;

        return temp;
    }
}
