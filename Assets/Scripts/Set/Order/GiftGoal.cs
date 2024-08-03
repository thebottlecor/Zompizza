using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MTAssets.EasyMinimapSystem;
using UnityEngine.UI;
using TMPro;

public class GiftGoal : MonoBehaviour
{

    public GameObject goalEffectObj;
    public GameObject boxObj;
    public SpriteRenderer ingredientSprite;
    public SpriteRenderer plusSprite;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && boxObj.activeSelf)
        {
            ShowCheck();
        }
    }

    private void ShowCheck()
    {
        int rand = UnityEngine.Random.Range(0, 9);

        var uiLib = DataManager.Instance.uiLib;
        if (rand <= 2)
        {
            // 33% 재료
            Ingredient ingredient = GM.Instance.RandomIngredientGet();
            ingredientSprite.sprite = uiLib.ingredients[ingredient];
            UIManager.Instance.UpdateIngredients();
            UIManager.Instance.OrderUIBtnUpdate();
            plusSprite.sprite = uiLib.plus[1];
        }
        else if (rand <= 5)
        {
            int tier = ResearchManager.Instance.globalEffect.tier; // 33% 티어 * 200 돈 
            GM.Instance.AddGold(100 * (tier + 1), GM.GetGoldSource.delivery);
            ingredientSprite.sprite = uiLib.gold;
            plusSprite.sprite = uiLib.plus[2 + tier];
        }
        else
        {
            // 33% 아이템
            int itemIdx = VillagerManager.Instance.RandomItemGet();
            ingredientSprite.sprite = uiLib.villagerItems[itemIdx];
            plusSprite.sprite = uiLib.plus[0];
        }

        FindEffect();
    }

    public void Hide()
    {
        boxObj.SetActive(false);
        goalEffectObj.SetActive(false);
    }
    public void Show()
    {
        boxObj.SetActive(true);
        goalEffectObj.SetActive(true);

        float dist = (GM.Instance.player.transform.position - transform.position).magnitude;
        if (dist <= 5f)
            ShowCheck();
    }

    private void FindEffect()
    {
        AudioManager.Instance.PlaySFX(Sfx.itemGet);
        var source = DataManager.Instance.effectLib.appleBlastEffect;
        Vector3 pos = this.transform.position;
        pos.y = 2f;
        var obj = Instantiate(source, pos, Quaternion.identity);
        Destroy(obj, 2f);
        Hide();
        StartCoroutine(HideIngredientObj());
    }

    private IEnumerator HideIngredientObj()
    {
        ingredientSprite.gameObject.SetActive(true);

        yield return CoroutineHelper.WaitForSeconds(2f);

        ingredientSprite.gameObject.SetActive(false);
    }

}
