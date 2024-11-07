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

    public bool villagerItem;
    public bool notVillagerItem;

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Player") && boxObj.activeSelf)
    //    {
    //        ShowCheck();
    //    }
    //}

    private void ShowCheck()
    {
        int rand = UnityEngine.Random.Range(0, 10);

        if (notVillagerItem)
        {
            if (rand <= 3)
            {
                // 40% 재료
                GetIngredients();
            }
            else if (rand <= 6)
            {
                // 30% 아이템
                GetVillagerItems();
            }
            else
            {
                // 30% 골드
                GetGold();
            }
        }
        else if (villagerItem)
        {
            GetVillagerItems();
        }
        else
        {
            if (rand <= 2)
            {
                // 30% 재료
                GetIngredients();
            }
            else if (rand <= 7)
            {
                // 50% 아이템
                GetVillagerItems();
            }
            else
            {
                // 20% 골드
                GetGold();
            }
        }

        notVillagerItem = false;
        villagerItem = false;

        GetEffect();
    }

    private void GetGold()
    {
        int tier = ResearchManager.Instance.globalEffect.tier;
        GM.Instance.AddGold(Constant.delivery_reward_ingredients * (tier + 1), GM.GetGoldSource.delivery);

        var uiLib = DataManager.Instance.uiLib;
        ingredientSprite.sprite = uiLib.gold;
        plusSprite.sprite = uiLib.plus[2 + tier];
    }

    private void GetIngredients()
    {
        Ingredient ingredient = GM.Instance.RandomIngredientGet();

        var uiLib = DataManager.Instance.uiLib;
        ingredientSprite.sprite = uiLib.ingredients[ingredient];
        UIManager.Instance.UpdateIngredients();
        UIManager.Instance.OrderUIBtnUpdate();
        plusSprite.sprite = uiLib.plus[0];
    }

    private void GetVillagerItems()
    {
        int somethingNeeds = VillagerManager.Instance.GetNeededThings();
        int itemIdx;

        if (somethingNeeds > -1)
        {
            if (UnityEngine.Random.Range(0, 5) <= 3) // 80% 확률로 어떤 주민이 필요한 물품이 나옴
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

        var uiLib = DataManager.Instance.uiLib;
        ingredientSprite.sprite = uiLib.villagerItems[itemIdx];
        plusSprite.sprite = uiLib.plus[0];
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

    private void GetEffect()
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
