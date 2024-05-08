using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RepaySilder : MonoBehaviour
{
    public TextMeshProUGUI repaymentText;
    public TextMeshProUGUI repaymentValueText;
    public Slider slider;

    public Button repayButton;
    public TextMeshProUGUI repayBtnText;

    private TextManager tm => TextManager.Instance;

    private void Start()
    {
        slider.maxValue = 50;

        slider.onValueChanged.AddListener(ValueChanged);

        slider.value = 10;
        ValueChanged(slider.value);

        UpdateText();
    }

    public float Percent => slider.value / slider.maxValue;

    private void ValueChanged(float a)
    {
        repaymentValueText.text = $"<color=#DBA04C>=></color> {a*100:F0}$";
    }

    public void UpdateText()
    {
        repaymentText.text = tm.GetCommons("Repayment");
        repayBtnText.text = tm.GetCommons("Repay");
    }

    public void Repay()
    {
        int value = (int)(slider.value * 100);

        LoanManager.Instance.Repay(value);
    }

}
