using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Text;
using UnityEngine.InputSystem;

public class OvenMiniGame : EventListener
{

    public GameObject oven_stay;
    public GameObject oven_operating;

    public Material oven_operatingMat;

    private float maxTimer;
    private float timer;
    public Image gaugeIndicator;
    public Material gaugeIndicatorHighlightMat;

    public bool operating;
    private float angle_progress;
    private OrderInfo orderInfo;

    public TextMeshProUGUI[] gradeTexts;

    public TextMeshProUGUI pizzaMakeText;
    public Button pizzaMakeBtn;

    public TextMeshProUGUI resetText;
    public Button resetBtn;

    [Header("콤보")]
    public GameObject[] comboInfos;
    public TextMeshProUGUI[] normalTexts;
    public float[] normalComboBonus;
    public float[] specialComboBonus;
    public TextMeshProUGUI[] comboTexts;

    public GameObject[] specialComboHighlight;

    public SerializableDictionary<Ingredient, int> inputs;

    public TextMeshProUGUI inputText;
    public TextMeshProUGUI mileValueText;
    public TextMeshProUGUI mileText;
    public TextMeshProUGUI totalValueText;
    public TextMeshProUGUI totalMultipleText;
    public TextMeshProUGUI resultText;

    public Color highlightColor = new Color(0.8392157f, 0.9529412f, 0.4980392f, 1f);
    public Color disableColor = new Color(0.3207547f, 0.3207547f, 0.3207547f, 1f);

    public GameObject inputPanel;
    public GameObject oveningPanel;

    public int MaxPizzaInput => Constant.maxPizzaInput;
    public bool IsMaxInput => IngredientCount >= MaxPizzaInput;
    public int IngredientCount { get; private set; }
    private int mileGold;
    private int goldValue;
    private float multipleValue;
    private int resultGold;

    private bool specialCombo;
    private int normalCombo; // -1 해당 x  0부터 1종,2종,3종

    public TextMeshProUGUI oneWarning;

    private TextManager tm => TextManager.Instance;

    private void UpdateComboTexts()
    {
        if (orderInfo == null)
        {
            comboTexts[6].text = string.Empty;
            comboTexts[7].text = string.Empty;
            return;
        }

        int combo = orderInfo.comboSpecial;
        if (combo >= 1)
        {
            comboTexts[6].text = tm.GetCommons($"ComboSpecial{combo}");
            comboTexts[7].text = string.Format("x{0:F1}", specialComboBonus[combo]);
        }
        else
        {
            comboTexts[6].text = string.Empty;
            comboTexts[7].text = string.Empty;
        }
    }

    public void CalcOnlyCount()
    {
        IngredientCount = 0;
        foreach (var temp in inputs)
        {
            IngredientCount += temp.Value;
        }
    }
    public void CalcValues()
    {
        IngredientCount = 0;

        if (orderInfo != null)
            mileGold = (int)(Constant.delivery_reward_1km * orderInfo.km);
        else
            mileGold = 0;

        goldValue = 0;
        foreach (var temp in inputs)
        {
            IngredientCount += temp.Value;

            int tier = OrderManager.Instance.FindTier(temp.Key);
            int gold = (tier + 1) * Constant.delivery_reward_ingredients;
            if (OrderManager.Instance.bonusIngredients == (int)temp.Key) gold += (tier + 1) * Constant.bonus_reward_ingredients;

            goldValue += gold * temp.Value;
        }

        float globalMul = (1f + ResearchManager.Instance.globalEffect.goldGet);
        if (GameEventManager.Instance.ninjaPriceUp) // 첫번째 닌자 치킨의 제안 수락시 10% 가격 상승
            globalMul += 0.1f;
        if (GM.Instance.hardMode)
            globalMul += 0.1f;

        mileGold = (int)(mileGold * globalMul);
        goldValue = (int)(goldValue * globalMul);
    }

    public void SetHighlight(OrderInfo info, bool on = true)
    {
        int combo = info.comboSpecial;
        if (combo > 0) specialComboHighlight[combo - 1].SetActive(on);
    }
    public void CalcComboCheck()
    {
        specialCombo = false;
        normalCombo = -1;

        var cache = DataManager.Instance.ingredientLib;

        bool result = true;

        bool containMeat = false;
        bool containVegetable = false;
        bool containHerb = false;

        int totalCount = 0;
        foreach (var temp in inputs)
        {
            if (temp.Value <= 0) continue;
            totalCount += temp.Value;
            if (cache.meats.ContainsKey(temp.Key))
                containMeat = true;
            else if (cache.vegetables.ContainsKey(temp.Key))
                containVegetable = true;
            else if (cache.herbs.ContainsKey(temp.Key))
                containHerb = true;
        }

        if (containMeat && containVegetable && containHerb) normalCombo = 2;
        else
        {
            if (containMeat && !containVegetable && !containHerb) normalCombo = 0;
            else if (!containMeat && containVegetable && !containHerb) normalCombo = 0;
            else if (!containMeat && !containVegetable && containHerb) normalCombo = 0;
            else if (totalCount > 1) normalCombo = 1;
        }

        if (orderInfo != null)
        {
            int combo = orderInfo.comboSpecial;
            SetHighlight(orderInfo);

            switch (combo)
            {
                case 1:
                    foreach (var temp in inputs)
                    {
                        if (temp.Value > 0 && !cache.ComboSpecial1.ContainsKey(temp.Key))
                        {
                            result = false;
                            break;
                        }
                    }
                    break;
                case 2:
                    foreach (var temp in inputs)
                    {
                        if (temp.Value > 0 && !cache.ComboSpecial2.ContainsKey(temp.Key))
                        {
                            result = false;
                            break;
                        }
                    }
                    break;
                case 6:
                    foreach (var temp in inputs)
                    {
                        if (temp.Value > 0 && !cache.ComboSpecial6.ContainsKey(temp.Key))
                        {
                            result = false;
                            break;
                        }
                    }
                    break;
                case 7:
                    foreach (var temp in inputs)
                    {
                        if (temp.Value > 0 && !cache.ComboSpecial7.ContainsKey(temp.Key))
                        {
                            result = false;
                            break;
                        }
                    }
                    break;
                case 3:
                case 4:
                case 5:
                case 8:
                    result = false;
                    break;
            }

            switch (combo)
            {
                case 1:
                case 2:
                case 6:
                case 7:
                    if (totalCount == 0) result = false;
                    break;
                case 3:
                    if (containMeat && containVegetable && !containHerb)
                        result = true;
                    break;
                case 4:
                    if ((containVegetable || containHerb) && !containMeat)
                        result = true;
                    break;
                case 5:
                    if (containMeat && containVegetable && containHerb)
                        result = true;
                    break;
                case 8:
                    if (containMeat && containHerb && !containVegetable)
                        result = true;
                    break;
            }
        }
        else
            result = false;

        specialCombo = result;

        multipleValue = 1f;

        if (orderInfo != null)
        {
            if (specialCombo)
            {
                multipleValue = specialComboBonus[orderInfo.comboSpecial];
                if (normalCombo >= 0)
                {
                    multipleValue += normalComboBonus[normalCombo];
                }
            }
            else
            {
                if (normalCombo >= 0)
                {
                    multipleValue = normalComboBonus[normalCombo];
                }
            }
        }

        if (normalCombo == 0)
        {
            comboTexts[0].color = highlightColor;
            comboTexts[3].color = highlightColor;
        }
        else
        {
            comboTexts[0].color = disableColor;
            comboTexts[3].color = disableColor;
        }
        if (normalCombo == 1)
        {
            comboTexts[1].color = highlightColor;
            comboTexts[4].color = highlightColor;
        }
        else
        {
            comboTexts[1].color = disableColor;
            comboTexts[4].color = disableColor;
        }
        if (normalCombo == 2)
        {
            comboTexts[2].color = highlightColor;
            comboTexts[5].color = highlightColor;
        }
        else
        {
            comboTexts[2].color = disableColor;
            comboTexts[5].color = disableColor;
        }

        if (specialCombo)
        {
            comboTexts[6].color = highlightColor;
            comboTexts[7].color = highlightColor;
        }
        else
        {
            comboTexts[6].color = disableColor;
            comboTexts[7].color = disableColor;
        }

        resultGold = (int)(goldValue * multipleValue) + mileGold;
    }
    public void UpdateValueTexts()
    {
        CalcValues();
        CalcComboCheck();

        int count = this.IngredientCount;
        if (IsMaxInput)
            inputText.text = $"<color=#69F827>{tm.GetCommons("Input")} ({count}/{MaxPizzaInput})</color>";
        else
            inputText.text = $"{tm.GetCommons("Input")} ({count}/{MaxPizzaInput})";

        totalValueText.text = $"{goldValue}G";

        if (multipleValue > 1f)
            totalMultipleText.color = highlightColor;
        else
            totalMultipleText.color = disableColor;
        totalMultipleText.text = $"x{multipleValue:F1}";

        mileValueText.text = $"+{mileGold}G";

        if (resultGold > 0)
            resultText.text = $"<sprite=2> +{resultGold}G";
        else
            resultText.text = $"<sprite=2> {resultGold}G";

        oneWarning.gameObject.SetActive(false);

    }

    private void Start()
    {
        maxTimer = UnityEngine.Random.Range(1.4f, 1.6f);
        Init();

        normalTexts[0].text = tm.GetCommons("Bonus");
        normalTexts[1].text = tm.GetCommons("Combo");

        comboTexts[0].text = tm.GetCommons("ComboNormal1");
        comboTexts[1].text = tm.GetCommons("ComboNormal2");
        comboTexts[2].text = tm.GetCommons("ComboNormal3");
        comboTexts[3].text = string.Format("x{0:F1}", normalComboBonus[0]);
        comboTexts[4].text = string.Format("x{0:F1}", normalComboBonus[1]);
        comboTexts[5].text = string.Format("x{0:F1}", normalComboBonus[2]);

        mileText.text = tm.GetCommons("DeliveryFee");

        gradeTexts[0].text = tm.GetCommons("Bad");
        gradeTexts[1].text = tm.GetCommons("Good");
        gradeTexts[2].text = tm.GetCommons("Perfect");

        oneWarning.text = tm.GetCommons("OneWarning");

        for (int i = 0; i < comboInfos.Length; i++)
            comboInfos[i].SetActive(false);
    }

    protected override void AddListeners()
    {
        PizzaDirection.IngredientEnterEvent += OnIngredientEnter;
        PizzaDirection.PizzaCompleteEvent += OnPizzaComplete;

        InputHelper.SideBreakEvent += OnButtonPressed;
        InputHelper.OkayEvent += OnButtonPressed;
    }

    protected override void RemoveListeners()
    {
        PizzaDirection.IngredientEnterEvent -= OnIngredientEnter;
        PizzaDirection.PizzaCompleteEvent -= OnPizzaComplete;

        InputHelper.SideBreakEvent -= OnButtonPressed;
        InputHelper.OkayEvent -= OnButtonPressed;
    }

    private void OnIngredientEnter(object sender, EventArgs e)
    {
        oven_stay.SetActive(false);
        oven_operating.SetActive(true);

        operating = true;
        pizzaMakeBtn.interactable = false;
        resetBtn.interactable = false;

        oveningPanel.SetActive(true);

        AudioManager.Instance.PlaySFX(Sfx.kitchenTimer, true);
    }
    private void OnPizzaComplete(object sender, OrderInfo e)
    {
        UIManager.Instance.shopUI.orderPanel.SetActive(true);
        UIManager.Instance.shopUI.makingPanel.SetActive(false);

        UIManager.Instance.shopUI.OrderLoadCountTextUpdate();
        UIManager.Instance.OrderUIBtnUpdate();

        if (TutorialManager.Instance.step >= 9)
            UIManager.Instance.shopUI.shopCloseBtn.enabled = true;

        UINaviHelper.Instance.SetFirstSelect();
    }

    public void Init()
    {
        for (int i = 0; i < gradeTexts.Length; i++)
        {
            gradeTexts[i].gameObject.SetActive(false);
        }
        HideHighlight();

        //KeyBtnUIUpdate();
        pizzaMakeBtn.interactable = true;
        resetBtn.interactable = true;
        operating = false;

        oven_stay.SetActive(true);
        oven_operating.SetActive(false);

        inputPanel.SetActive(true);
        oveningPanel.SetActive(false);

        oneWarning.gameObject.SetActive(false); 

        timer = 0f;
        angle_progress = 90f;
        gaugeIndicator.material = null;
        gaugeIndicator.rectTransform.rotation = Quaternion.Euler(0, 0, angle_progress);
        oven_operatingMat.SetFloat("_ColorSwapBlend", 0);

        //buttonPressed = false;

        for (int i = 0; i < comboInfos.Length; i++)
            comboInfos[i].SetActive(true);
    }

    private void KeyBtnUIUpdate()
    {
        if (tm == null) return;

        //var pad = Gamepad.current;
        //if (pad == null)
        //    keyText.text = $"{tm.GetCommons("MakePizza")}\n({HotKey[KeyMap.carBreak].GetName()})";
        //else
        //    keyText.text = $"{tm.GetCommons("MakePizza")}";
        //keyPadObj.SetActive(pad != null);

        pizzaMakeText.text = $"{tm.GetCommons("MakePizza")}";
        resetText.text = $"{tm.GetCommons("Reset")}";

        //RectTransform keyRect = keyText.transform as RectTransform;
        //LayoutRebuilder.ForceRebuildLayoutImmediate(keyRect);
        //RectTransform buttonRect = (keyTextBtn.transform as RectTransform);
        //buttonRect.sizeDelta = new Vector2(keyRect.sizeDelta.x + 144f, buttonRect.sizeDelta.y);
    }

    public void SetPizzaPrepare(OrderInfo orderInfo)
    {
        UIManager.Instance.isDirecting = true;
        UIManager.Instance.shopUI.shopCloseBtn.enabled = false;

        Init();
        this.orderInfo = orderInfo;

        inputs = new SerializableDictionary<Ingredient, int>();
        UpdateComboTexts();
        UpdateValueTexts();
        OrderManager.Instance.comboMode = true;
        UIManager.Instance.UpdateIngredients();

        UIManager.Instance.shopUI.orderPanel.SetActive(false);
        UIManager.Instance.shopUI.makingPanel.SetActive(true);
        KeyBtnUIUpdate();
        UINaviHelper.Instance.SetFirstSelect();

        TutorialManager.Instance.OrderAccpeted();
        StartCoroutine(DelayHighlight());
    }
    private IEnumerator DelayHighlight()
    {
        yield return null;
        if (orderInfo != null)
            SetHighlight(orderInfo);
    }

    public void ResetInput()
    {
        inputs = new SerializableDictionary<Ingredient, int>();
        UpdateValueTexts();
        UIManager.Instance.UpdateIngredients();
    }

    public void HideHighlight()
    {
        for (int i = 0; i < specialComboHighlight.Length; i++)
        {
            specialComboHighlight[i].SetActive(false);
        }
    }

    public void MakePizza()
    {
        var hasIngredient = GM.Instance.ingredients;
        int totalCount = 0;
        foreach (var temp in hasIngredient)
        {
            totalCount += temp.Value;
        }
        int inputCount = 0;
        foreach (var temp in inputs)
        {
            inputCount += temp.Value;
        }
        if (totalCount > 0 && inputCount == 0) // 재료가 있는데, 아무것도 넣지 않았을 때
        {
            AudioManager.Instance.PlaySFX(Sfx.deny);
            oneWarning.gameObject.SetActive(true);
            return;
        }
        oneWarning.gameObject.SetActive(false);

        HideHighlight();
        // 재료 소모
        foreach (var temp in inputs)
        {
            GM.Instance.ingredients[temp.Key] -= temp.Value;
        }
        orderInfo.rewards = resultGold;
        OrderManager.Instance.comboMode = false;
        UIManager.Instance.UpdateIngredients();

        inputPanel.SetActive(false);
        UIManager.Instance.OffAll_Ingredient_Highlight();
        UINaviHelper.Instance.SetFirstSelect();

        AudioManager.Instance.PlaySFX(Sfx.ovenstart);
        OrderManager.Instance.pizzaDirection.RestartSequence(orderInfo, inputs);

        TutorialManager.Instance.PizzaOvening();
    }

    void Update()
    {
        if (!operating) return;

        timer += Time.unscaledDeltaTime;

        float percent = timer / maxTimer;

        if (percent >= 1f)
        {
            percent = 1f;
        }

        angle_progress = 90f - 180f * percent;
        oven_operatingMat.SetFloat("_ColorSwapBlend", percent);

        gaugeIndicator.rectTransform.rotation = Quaternion.Euler(0, 0, angle_progress);

        float sin = Mathf.Sin(angle_progress * Mathf.Deg2Rad);
        //if (sin <= Mathf.Sin(17f * Mathf.Deg2Rad) && sin > Mathf.Sin(-18f * Mathf.Deg2Rad))
        if (sin <= Mathf.Sin(4.4f * Mathf.Deg2Rad) && sin > Mathf.Sin(-4.9f * Mathf.Deg2Rad))
        {
            gaugeIndicator.material = gaugeIndicatorHighlightMat;
        }
        else
        {
            gaugeIndicator.material = null;
        }

        //Debug.Log(angle_progress + " >> " + Mathf.Sin(angle_progress * Mathf.Deg2Rad));

        if (percent >= 0.5f)
        {
            StopOven();
            return;
        }
        //if (buttonPressed)
        //{
        //    AudioManager.Instance.PlaySFX(Sfx.buttons);
        //    StopOven();
        //    return;
        //}
    }

    //bool buttonPressed;

    public void OnButtonPressed(object sender, InputAction.CallbackContext e)
    {
        if (!operating) return;

        //buttonPressed = e.performed;

        //if (e.performed)
        //{
        //    AudioManager.Instance.PlaySFX(Sfx.buttons);
        //    StopOven();
        //}
    }

    private void StopOven()
    {
        if (!operating) return;

        AudioManager.Instance.StopSFX(true);
        AudioManager.Instance.PlaySFX(Sfx.complete);
        operating = false;
        pizzaMakeBtn.interactable = false;
        resetBtn.interactable = false;

        oven_stay.SetActive(true);
        oven_operating.SetActive(false);

        //float sin = Mathf.Sin(angle_progress * Mathf.Deg2Rad);
        //int grade = 0;

        //if (sin > Mathf.Sin(53.5f * Mathf.Deg2Rad))
        //{

        //}
        //else if (sin > Mathf.Sin(17f * Mathf.Deg2Rad))
        //{
        //    //grade = 1;
        //}
        //else if (sin > Mathf.Sin(4.4f * Mathf.Deg2Rad))
        //{
        //    grade = 1;
        //    GM.Instance.player.cam.GamePadRumble(2f);
        //}
        //else if (sin > Mathf.Sin(-4.9f * Mathf.Deg2Rad))
        //{
        //    grade = 2;
        //    GM.Instance.player.cam.GamePadRumble(5f);
        //}
        //else if (sin > Mathf.Sin(-18f * Mathf.Deg2Rad))
        //{
        //    //grade = 2;
        //    grade = 1;
        //    GM.Instance.player.cam.GamePadRumble(2f);
        //}
        //else if (sin > Mathf.Sin(-53.5f * Mathf.Deg2Rad))
        //{
        //    //grade = 1;
        //}

        GM.Instance.player.cam.GamePadRumble(5f);

        if (IngredientCount > 0)
            StartCoroutine(ShowText(gradeTexts[2]));
        else
            StartCoroutine(ShowText(gradeTexts[1]));


        OrderManager.Instance.PizzaMakingComplete(orderInfo);
        orderInfo = null;

        for (int i = 0; i < comboInfos.Length; i++)
            comboInfos[i].SetActive(false);
    }

    private IEnumerator ShowText(TextMeshProUGUI tmp)
    {
        tmp.gameObject.SetActive(true);

        yield return CoroutineHelper.WaitForSecondsRealtime(1.9f);

        tmp.gameObject.SetActive(false);
    }
}
