using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageObject : MonoBehaviour
{

    public TextMeshProUGUI tmp;
    [SerializeField] private LanguagePanel panel;
    [SerializeField] private Language language;

    public void Init(Language language, LanguagePanel panel)
    {
        this.panel = panel;
        this.language = language;
        tmp.text = TextManager.Instance.GetCommons("Language2", language);
    }

    public void SetLanguage()
    {
        panel.SetLanguage(language);
        AudioManager.Instance.PlaySFX(Sfx.buttons); // 언어 변경 버튼
        panel.Hide();
    }
}
