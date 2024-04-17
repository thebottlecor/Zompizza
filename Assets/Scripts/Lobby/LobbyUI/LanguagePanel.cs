using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguagePanel : MonoBehaviour
{

    public Transform objectParent;
    public GameObject languageObject_Source;

    private TextManager tm => TextManager.Instance;
    [SerializeField] private TextMeshProUGUI languagePanelOpenButtonTmp;
    [SerializeField] private TextMeshProUGUI languageSettingsTMP;

    public void Init()
    {
        Language firstLanguage = Language.en;
        var systemLanguage = Application.systemLanguage;
        switch (systemLanguage)
        {
            case SystemLanguage.ChineseTraditional:
                firstLanguage = Language.tc;
                break;
            case SystemLanguage.ChineseSimplified:
                firstLanguage = Language.sc;
                break;
            case SystemLanguage.Chinese:
                firstLanguage = Language.sc;
                break;
            case SystemLanguage.Japanese:
                firstLanguage = Language.jp;
                break;
            case SystemLanguage.Korean:
                firstLanguage = Language.kr;
                break;
        }

        LanguageObject languageObject = Instantiate(languageObject_Source, objectParent).GetComponent<LanguageObject>();
        languageObject.Init(firstLanguage, this);

        for (int i = 0; i < (int)Language.LAST; i++)
        {
            if ((Language)i != firstLanguage)
            {
                languageObject = Instantiate(languageObject_Source, objectParent).GetComponent<LanguageObject>();
                languageObject.Init((Language)i, this);
            }
        }
        languagePanelOpenButtonTmp.text = tm.GetCommons("Language2", tm.language);
        languageSettingsTMP.text = tm.GetCommons("Language");
    }

    public void SetLanguage(Language language)
    {
        TextManager.Instance.SetLanguage(language);
        languagePanelOpenButtonTmp.text = tm.GetCommons("Language2", language);
        languageSettingsTMP.text = tm.GetCommons("Language");
        SaveManager.Instance.SaveConfig();
    }

    public void Show()
    {
        Lobby.Instance.CloseAllPanel();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

}
