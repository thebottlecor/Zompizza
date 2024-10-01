using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AutosavingTMP : MonoBehaviour
{

    private bool inited;
    private float timer;
    private int count;

    private List<string> cache;

    [SerializeField] private TextMeshProUGUI tmp;

    [SerializeField] private float cooldown = 0.3f;

    public void Toggle(bool on)
    {
        if (on)
        {
            if (!inited)
            {
                inited = true;

                string baseStr = TextManager.Instance.GetCommons("Saving");

                cache = new List<string>();
                cache.Add(baseStr + ".");
                cache.Add(baseStr + "..");
                cache.Add(baseStr + "...");
            }

            timer = 0f;
            count = 2;
            tmp.text = cache[count];
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (timer > cooldown)
        {
            timer = 0f;
            count++;
            if (count >= cache.Count)
                count = 0;

            tmp.text = cache[count];
        }

        timer += Time.unscaledDeltaTime;
    }
}
