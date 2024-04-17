using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewSaveObject : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private Button button;

    public void Init(Action action)
    {
        button.onClick.AddListener(() => action());
        UIUpdate();
    }

    public void UIUpdate()
    {
        tmp.text = TextManager.Instance.GetCommons("NewSave");
    }
}
