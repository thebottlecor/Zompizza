using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class VillagerManagerObj : MonoBehaviour
{

    public Image profile;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI context;

    private int idx;

    public void Init(int idx)
    {
        this.idx = idx;

        profile.sprite = DataManager.Instance.uiLib.villagerProfile[idx];
        nameText.text = TextManager.Instance.GetVillagerName(idx);

        UpdateUI();

        gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        var villager = VillagerManager.Instance.villagers[idx];

        if (!villager.recruited || villager.expelled)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform as RectTransform);

        TextManager tm = TextManager.Instance;
        StringBuilder st = new StringBuilder();

        st.AppendFormat("{0} {1}", tm.GetCommons("Relations"), villager.relations + 1);
        st.AppendLine();
        st.AppendFormat("{0} <sprite=\"emoji\" index={1}>", tm.GetCommons("Condition"), villager.condition + 6);
        st.AppendLine();
        st.AppendFormat("{0} +{1}", tm.GetCommons("Income"), villager.Income());

        context.text = st.ToString();
    }

}
