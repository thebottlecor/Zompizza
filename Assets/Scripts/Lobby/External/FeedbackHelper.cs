using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackHelper : MonoBehaviour
{

    public void OpenFeedback()
    {
        string tempUrl;

        switch (TextManager.Instance.language)
        {
            case Language.sc:
                tempUrl = "https://forms.gle/Rx44oYpTHdDvbeMX7";
                break;
            case Language.tc:
                tempUrl = "https://forms.gle/9Z6e35DLqheWumms8";
                break;
            case Language.kr:
                tempUrl = "https://forms.gle/6XDRvDsNr5LQiC9u5";
                break;
            case Language.jp:
                tempUrl = "https://forms.gle/uBDa4SXxC4kEAuRG6";
                break;
            default:
                tempUrl = "https://forms.gle/kZWAtbzk5pkAjoA99";
                break;
        }

        Application.OpenURL(tempUrl);
    }
}
