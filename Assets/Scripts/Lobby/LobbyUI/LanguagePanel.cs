using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguagePanel : MonoBehaviour
{

    public Transform objectParent;
    public GameObject languageObject_Source;

    private TextManager tm => TextManager.Instance;
    //[SerializeField] private TextMeshProUGUI languagePanelOpenButtonTmp;
    [SerializeField] private TextMeshProUGUI languageSettingsTMP;

    private List<LanguageObject> languageObjects;

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
            case SystemLanguage.Russian:
                firstLanguage = Language.ru;
                break;
        }

        languageObjects = new List<LanguageObject>();
        List<UINavi> tempList = new List<UINavi>();

        LanguageObject saveLanObj = null;
        LanguageObject languageObject = Instantiate(languageObject_Source, objectParent).GetComponent<LanguageObject>();
        languageObject.Init(firstLanguage, this);
        languageObjects.Add(languageObject);
        tempList.Add(languageObject.GetComponent<UINavi>());
        LanguageObject firstObj = languageObject;
        if (firstLanguage == TextManager.Instance.language)
            saveLanObj = languageObject;

        for (int i = 0; i < (int)Language.LAST; i++)
        {
            if ((Language)i != firstLanguage)
            {
                languageObject = Instantiate(languageObject_Source, objectParent).GetComponent<LanguageObject>();
                Language language = (Language)i;
                languageObject.Init(language, this);
                languageObjects.Add(languageObject);
                tempList.Add(languageObject.GetComponent<UINavi>());
                if (language == TextManager.Instance.language)
                    saveLanObj = languageObject;
            }
        }
        for (int i = 0; i < tempList.Count; i++)
        {
            if (i > 0)
                tempList[i].left = tempList[i - 1];
            else
                tempList[i].left = tempList[tempList.Count - 1];

            if (i < tempList.Count - 1)
                tempList[i].right = tempList[i + 1];
            else
                tempList[i].right = tempList[0];

            tempList[i].down = UINaviHelper.Instance.title_settings_close;
        }
        if (saveLanObj != null)
            firstObj = saveLanObj;
        SetHighlight(firstObj);

        //languagePanelOpenButtonTmp.text = tm.GetCommons("Language2", tm.language);
        languageSettingsTMP.text = tm.GetCommons("Language");
    }

    public void SetLanguage(Language language, LanguageObject self)
    {
        TextManager.Instance.SetLanguage(language);
        //languagePanelOpenButtonTmp.text = tm.GetCommons("Language2", language);
        languageSettingsTMP.text = tm.GetCommons("Language");
        SaveManager.Instance.SaveConfig();

        SetHighlight(self);
    }

    private void SetHighlight(LanguageObject self)
    {
        for (int i = 0; i < languageObjects.Count; i++)
        {
            languageObjects[i].image.color = DataManager.Instance.uiLib.button_MainColor;
        }

        self.image.color = DataManager.Instance.uiLib.button_HighlightColor;

        UINaviHelper.Instance.title_language_first = self.GetComponent<UINavi>();
    }

}
