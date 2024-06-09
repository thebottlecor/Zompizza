using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using MTAssets.EasyMinimapSystem;
using UnityEngine.UI;
using TMPro;

public class HiddenRecipeGoal : MonoBehaviour
{

    public ResearchInfo researchInfo;
    public GameObject goalEffectObj;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ResearchManager.Instance.ResearchUnlock_Force(researchInfo.idx);
            StatManager.Instance.foundVisionRecipes++;
            FindEffect();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        goalEffectObj.SetActive(false);
    }
    public void Show()
    {
        if (!ResearchManager.Instance.Researched(researchInfo.idx))
        {
            gameObject.SetActive(true);
            goalEffectObj.SetActive(true);
        }
    }

    private void FindEffect()
    {
        goalEffectObj.SetActive(false);
        AudioManager.Instance.PlaySFX(Sfx.visionRecipeFound);
        var source = DataManager.Instance.effectLib.goldCoinBlastEffect;
        Vector3 pos = this.transform.position;
        pos.y = 4f;
        var obj = Instantiate(source, pos, Quaternion.identity);
        Destroy(obj, 5f);
        Hide();
    }

}
