using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
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
            int rand = UnityEngine.Random.Range(0, 10);

            var uiLib = DataManager.Instance.uiLib;
            if (rand <= 3)
            {
                // 40% 재료
                Ingredient ingredient = GM.Instance.RandomIngredientGet();
                ingredientSprite.sprite = uiLib.ingredients[ingredient];
                UIManager.Instance.UpdateIngredients();
                UIManager.Instance.OrderUIBtnUpdate();
                plusSprite.sprite = uiLib.plus[1];
            }
            else if (rand <= 7)
            {
                // 40% 돈
                GM.Instance.AddGold(200, GM.GetGoldSource.delivery);
                ingredientSprite.sprite = uiLib.gold;
                plusSprite.sprite = uiLib.plus[2];
            }
            else
            {
                // 20% 아이템
                int itemIdx = VillagerManager.Instance.RandomItemGet();
                ingredientSprite.sprite = uiLib.villagerItems[itemIdx];
                plusSprite.sprite = uiLib.plus[0];
            }

            FindEffect();
        }
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
