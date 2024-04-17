using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI descriptionTMP;

    [SerializeField] private TextMeshProUGUI acceptTMP;
    [SerializeField] private TextMeshProUGUI cancelTMP;

    [SerializeField] private Button acceptButton;
    [SerializeField] private Button cancelButton;

    public void Init(string description, string acceptText, Action action)
    {
        descriptionTMP.text = description;

        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(() => 
        {
            action();
            //Lobby.Instance.MenuButtonSound();
            Hide();
        });

        var tm = TextManager.Instance;
        acceptTMP.text = acceptText;
        cancelTMP.text = tm.GetCommons("Cancel");

        gameObject.SetActive(true);
    }

    public void Hide() // Cancel ¹öÆ°
    {
        gameObject.SetActive(false);
    }
}
