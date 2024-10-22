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
        int rand = UnityEngine.Random.Range(0, 10);

        var uiLib = DataManager.Instance.uiLib;
        if (rand <= 3)
        {
            // 40% 아이템
            Ingredient ingredient = GM.Instance.RandomIngredientGet();
            ingredientSprite.sprite = uiLib.ingredients[ingredient];
            UIManager.Instance.UpdateIngredients();
            UIManager.Instance.OrderUIBtnUpdate();
            plusSprite.sprite = uiLib.plus[0];
        }
        else if (rand <= 7)
        {
            // 40% 아이템
            int somethingNeeds = VillagerManager.Instance.GetNeededThings();
            int itemIdx;

            if (somethingNeeds > -1)
            {
                if (UnityEngine.Random.Range(0, 3) <= 1) // 66% 확률로 어떤 주민이 필요한 물품이 나옴
                {
                    itemIdx = VillagerManager.Instance.ItemGet(somethingNeeds);
                }
                else
                {
                    itemIdx = VillagerManager.Instance.RandomItemGet();
                }
            }
            else
            {
                itemIdx = VillagerManager.Instance.RandomItemGet();
            }

            ingredientSprite.sprite = uiLib.villagerItems[itemIdx];
            plusSprite.sprite = uiLib.plus[0];
        }
        else
        {
            int tier = ResearchManager.Instance.globalEffect.tier; // 33% 티어 * 200 돈 
            GM.Instance.AddGold(Constant.delivery_reward_ingredients * (tier + 1), GM.GetGoldSource.delivery);
            ingredientSprite.sprite = uiLib.gold;
            plusSprite.sprite = uiLib.plus[2 + tier];
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

        // 그냥 무조건 습득
        ShowCheck(); 

        //float dist = (GM.Instance.player.transform.position - transform.position).magnitude;
        //if (dist <= 10.5f)
        //    ShowCheck();
    }

    private void FindEffect()
    {
        AudioManager.Instance.PlaySFX(Sfx.itemGet);
        var source = DataManager.Instance.effectLib.appleBlastEffect;
        Vector3 pos = this.transform.position;
        pos.y += 2f;
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
