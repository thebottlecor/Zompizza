using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PrologueObject : MonoBehaviour
{

    private bool tmpCompleted;
    private bool keydowned;

    private void Update()
    {
        if (!tmpCompleted) return;
        if (keydowned) return;

        if (Input.anyKeyDown)
        {
            keydowned = true;
            LoadingSceneManager.Instance.PrologueStart();
        }
    }

    private string text;
    public TMP_Text targetText;
    public TMP_Text pressAnyKeyText;

    void Start()
    {
        text = TextManager.Instance.GetCommons("Prologue");
        targetText.text = string.Empty;
        tmpCompleted = false;
        pressAnyKeyText.text = TextManager.Instance.GetCommons("PressAnyKey");
        pressAnyKeyText.gameObject.SetActive(false);
        StartCoroutine(textPrint());
    }

    IEnumerator textPrint()
    {
        int count = 0;

        while (count != text.Length)
        {
            if (count < text.Length)
            {
                targetText.text += text[count].ToString();
                count++;
            }

            yield return new WaitForSeconds(0.075f);
        }

        tmpCompleted = true;
        pressAnyKeyText.gameObject.SetActive(true);
    }
}
