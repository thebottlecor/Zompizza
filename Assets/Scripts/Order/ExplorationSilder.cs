using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExplorationSilder : MonoBehaviour
{
    public TextMeshProUGUI categoryText;
    public Slider slider;
    public TextMeshProUGUI percentText;
    public TextMeshProUGUI levelText;

    private void Start()
    {
        slider.onValueChanged.AddListener(ValueChanged);
    }

    public float Percent => slider.value / slider.maxValue;

    private void ValueChanged(float a)
    {
        percentText.text = $"{a*10f:F0}%";
    }

}
