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
            AudioManager.Instance.PlaySFX(Sfx.okay);
            LoadingSceneManager.Instance.PrologueStart();
        }
    }

    private string text;
    private float typingSpeed;
    public TMP_Text targetText;
    public TMP_Text pressAnyKeyText;

    void Start()
    {
        text = TextManager.Instance.GetCommons("Prologue");

        typingSpeed = 7.5f / text.Length;

        targetText.text = string.Empty;
        tmpCompleted = false;
        pressAnyKeyText.text = TextManager.Instance.GetCommons("PressAnyKey");
        pressAnyKeyText.gameObject.SetActive(false);
        StartCoroutine(TextPrint());
    }

    IEnumerator TextPrint()
    {
        int count = 0;

        while (count != text.Length)
        {
            if (count < text.Length)
            {
                targetText.text += text[count].ToString();
                count++;
            }

            if (count % 4 == 0)
                AudioManager.Instance.PlaySFX(Sfx.inputFieldStart);

            yield return CoroutineHelper.WaitForSeconds(typingSpeed);
        }

        tmpCompleted = true;
        pressAnyKeyText.gameObject.SetActive(true);
    }
}
