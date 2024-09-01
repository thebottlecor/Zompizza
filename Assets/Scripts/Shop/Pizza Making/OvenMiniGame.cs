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

    public TextMeshProUGUI keyText;
    public GameObject keyPadObj;
    public TextMeshProUGUI[] gradeTexts;

    public Button keyTextBtn;

    private SerializableDictionary<KeyMap, KeyMapping> HotKey => SettingManager.Instance.keyMappings;
    private TextManager tm => TextManager.Instance;

    private void Start()
    {
        maxTimer = UnityEngine.Random.Range(1.4f, 1.6f);
        Init();

        //for (int i = 0; i <= 100; i++)
        //{
        //    float hpPercent = i * 0.01f;
        //    float hpRating;
        //    if (hpPercent == 1f)
        //    {
        //        hpRating = Constant.remainHpRating1;
        //    }
        //    else if (hpPercent >= Constant.remainHP_Percent)
        //    {
        //        hpRating = Constant.Point05(((Constant.remainHpRating2 - Constant.remainHpRating3) / (1f - Constant.remainHP_Percent)) * hpPercent +
        //            ((-Constant.remainHP_Percent * Constant.remainHpRating2) + Constant.remainHpRating3) / (1f - Constant.remainHP_Percent));
        //    }
        //    else
        //    {
        //        hpRating = Constant.Point05((-1f * Constant.remainHpRating4 / Constant.remainHP_Percent) * hpPercent + Constant.remainHpRating4);
        //    }
        //    Debug.Log(hpPercent + " :: " + hpRating);
        //}

        gradeTexts[0].text = tm.GetCommons("Bad");
        gradeTexts[1].text = tm.GetCommons("Good");
        gradeTexts[2].text = tm.GetCommons("Perfect");
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
        keyTextBtn.interactable = true;

        AudioManager.Instance.PlaySFX(Sfx.kitchenTimer, true);
    }
    private void OnPizzaComplete(object sender, OrderInfo e)
    {
        UIManager.Instance.shopUI.orderPanel.SetActive(true);
        UIManager.Instance.shopUI.makingPanel.SetActive(false);
        UINaviHelper.Instance.SetFirstSelect();
    }

    public void Init()
    {
        for (int i = 0; i < gradeTexts.Length; i++)
        {
            gradeTexts[i].gameObject.SetActive(false);
        }

        //KeyBtnUIUpdate();
        keyTextBtn.interactable = false;
        operating = false;

        oven_stay.SetActive(true);
        oven_operating.SetActive(false);

        timer = 0f;
        angle_progress = 90f;
        gaugeIndicator.material = null;
        gaugeIndicator.rectTransform.rotation = Quaternion.Euler(0, 0, angle_progress);
        oven_operatingMat.SetFloat("_ColorSwapBlend", 0);

        //buttonPressed = false;
    }

    private void KeyBtnUIUpdate()
    {
        if (tm == null) return;

        var pad = Gamepad.current;
        if (pad == null)
            keyText.text = $"{tm.GetCommons("Stop")} ({HotKey[KeyMap.carBreak].GetName()})";
        else
            keyText.text = $"{tm.GetCommons("Stop")}";
        keyPadObj.SetActive(pad != null);
        RectTransform keyRect = keyText.transform as RectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(keyRect);
        RectTransform buttonRect = (keyTextBtn.transform as RectTransform);
        buttonRect.sizeDelta = new Vector2(keyRect.sizeDelta.x + 144f, buttonRect.sizeDelta.y);
    }

    public void StartOven(OrderInfo orderInfo)
    {
        Init();
        this.orderInfo = orderInfo;
        UIManager.Instance.shopUI.orderPanel.SetActive(false);
        UIManager.Instance.shopUI.makingPanel.SetActive(true);
        KeyBtnUIUpdate();
        UINaviHelper.Instance.SetFirstSelect();
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

        if (percent >= 1f)
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

        if (e.performed)
        {
            AudioManager.Instance.PlaySFX(Sfx.buttons);
            StopOven();
        }
    }
    public void ButtonClick()
    {
        if (!operating) return;

        AudioManager.Instance.PlaySFX(Sfx.buttons);
        StopOven();
    }

    public void StopOven()
    {
        if (!operating) return;

        AudioManager.Instance.StopSFX(true);
        AudioManager.Instance.PlaySFX(Sfx.complete);
        operating = false;
        keyTextBtn.interactable = false;

        oven_stay.SetActive(true);
        oven_operating.SetActive(false);

        float sin = Mathf.Sin(angle_progress * Mathf.Deg2Rad);
        int grade = 0;

        if (sin > Mathf.Sin(53.5f * Mathf.Deg2Rad))
        {

        }
        else if (sin > Mathf.Sin(17f * Mathf.Deg2Rad))
        {
            //grade = 1;
        }
        else if (sin > Mathf.Sin(4.4f * Mathf.Deg2Rad))
        {
            grade = 1;
            GM.Instance.player.cam.GamePadRumble(2f);
        }
        else if (sin > Mathf.Sin(-4.9f * Mathf.Deg2Rad))
        {
            grade = 2;
            GM.Instance.player.cam.GamePadRumble(5f);
        }
        else if (sin > Mathf.Sin(-18f * Mathf.Deg2Rad))
        {
            //grade = 2;
            grade = 1;
            GM.Instance.player.cam.GamePadRumble(2f);
        }
        else if (sin > Mathf.Sin(-53.5f * Mathf.Deg2Rad))
        {
            //grade = 1;
        }

        float pizzaHp = 0;
        int textIdx = 0;
        switch (grade)
        {
            case 0:
                pizzaHp = 0.5f;
                textIdx = 0;
                break;
            case 1:
                pizzaHp = 0.99f;
                textIdx = 1;
                break;
            case 2:
                pizzaHp = 1f;
                textIdx = 2;
                break;
        }
        StartCoroutine(ShowText(gradeTexts[textIdx]));

        orderInfo.hp = pizzaHp;
        OrderManager.Instance.PizzaMakingComplete(orderInfo);
        orderInfo = null;
    }

    private IEnumerator ShowText(TextMeshProUGUI tmp)
    {
        tmp.gameObject.SetActive(true);

        yield return CoroutineHelper.WaitForSecondsRealtime(1.9f);

        tmp.gameObject.SetActive(false);
    }
}
