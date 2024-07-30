using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ExplorationSilder : MonoBehaviour
{
    public TextMeshProUGUI categoryText;
    public Slider slider;
    public TextMeshProUGUI percentText;

    [SerializeField] private bool intValue;

    private Action<float> changedAction;

    public void Init(float initValue, float initMax, Action<float> changedAction)
    {
        slider.maxValue = initMax;
        slider.value = initValue;
        this.changedAction = changedAction;
        ValueChanged(slider.value);
        slider.onValueChanged.AddListener(ValueChanged);
    }

    public float Percent => slider.value / slider.maxValue;

    private void ValueChanged(float a)
    {
        if (intValue)
            percentText.text = $"{a+1:F0}";
        else
            percentText.text = $"{a*10f:F0}%";

        changedAction(a);
    }

    public void UpdateUI()
    {
        if (intValue)
            percentText.text = $"{slider.value + 1:F0}";
        else
            percentText.text = $"{slider.value * 10f:F0}%";
    }

}
