using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UI;

public class TutorialManager : Singleton<TutorialManager>
{
    [Header("디버깅")]
    public bool debug_skipToPrologue;
    public bool debug_skipToShop;
    public bool debug_skipToReturn;
    public bool debug_TutorialDisable;
    public bool debug_fixTime;
    public bool debug_fixTime_Noon;

    [Space(20f)]

    public int step;
    public bool manModeEntered; // 주민 튜토 1번만 뜨도록

    public bool training;
    public GameObject blackScreen;
    public TextMeshProUGUI oneYearsLaterTMP;

    [Header("특수 : 튜토리얼")]
    public Zombie_Tutorial_Contact[] tutorialZombie;
    public GameObject trainingCenter;
    public GameObject prologueObj;
    public Transform trainingCenterPos;
    public Transform prologuePos;
    public Transform tutoReturnPos;

    public TextMeshProUGUI controlText;
    public GameObject controlPadObj;
    public TextMeshProUGUI driftText;
    public GameObject driftPadObj;
    public TextMeshProUGUI installText;
    public TextMeshProUGUI installPadObjTMP;
    public GameObject installPadObj;

    public GameObject[] guideObjects;
    public TextMeshProUGUI[] guideTexts;

    public ShopGate shopGate;
    public GameObject shopGoal;
    public GameObject returnGoal;

    public GameObject[] indicators;

    private TextManager tm => TextManager.Instance;
    private SettingManager sm => SettingManager.Instance;

    public void Init(bool tutorialOn)
    {
        //trainingCenter.SetActive(false);
        //prologueObj.SetActive(false);

        step = 0;

        UpdateText();
        for (int i = 0; i < guideObjects.Length; i++)
        {
            guideObjects[i].SetActive(true);
        }
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < tutorialZombie.Length; i++)
        {
            tutorialZombie[i].Init(ZombiePooler.Instance.currentTarget);
            tutorialZombie[i].gameObject.SetActive(false);
        }
        controlText.gameObject.SetActive(false);
        controlPadObj.SetActive(false);
        driftText.gameObject.SetActive(false);
        driftPadObj.SetActive(false);
        installText.gameObject.SetActive(false);
        installPadObj.SetActive(false);
        for (int i = 0; i < guideObjects.Length; i++)
        {
            guideObjects[i].SetActive(false);
        }
        for (int i = 0; i < indicators.Length; i++)
        {
            indicators[i].SetActive(false);
        }
        shopGoal.SetActive(false);
        returnGoal.SetActive(false);

        if (debug_TutorialDisable || !tutorialOn)
        {
            TutorialSkip(true);
            step = 100;
            return;
        }

        if (debug_skipToPrologue)
        {
            Step1_2();
            return;
        }

        if (debug_skipToShop)
        {
            SkipToShop_method();
            return;
        }
        if (debug_skipToReturn)
        {
            TutorialSkip(true);
            training = true;
            GM.Instance.timer = Constant.dayTime;
            Step9();
            return;
        }

        GoToTutorial();
    }

    public void UpdateText()
    {
        StringBuilder st = new StringBuilder();
        st.AppendFormat("<color=#51FF55>{0}</color> - {1}", tm.GetKeyMaps(KeyMap.changePOV), sm.keyMappings[KeyMap.changePOV].GetName());
        st.AppendLine();
        st.AppendFormat("{0} - {1}", tm.GetKeyMaps(KeyMap.carForward), sm.keyMappings[KeyMap.carForward].GetName());
        st.AppendLine();
        st.AppendFormat("{0} - {1}", tm.GetKeyMaps(KeyMap.carBackward), sm.keyMappings[KeyMap.carBackward].GetName());
        st.AppendLine();
        st.AppendFormat("{0} - {1}", tm.GetKeyMaps(KeyMap.carLeft), sm.keyMappings[KeyMap.carLeft].GetName());
        st.AppendLine();
        st.AppendFormat("{0} - {1}", tm.GetKeyMaps(KeyMap.carRight), sm.keyMappings[KeyMap.carRight].GetName());
        st.AppendLine();
        st.AppendFormat("{0} - {1}", tm.GetKeyMaps(KeyMap.carBreak), sm.keyMappings[KeyMap.carBreak].GetName());

        controlText.text = st.ToString();

        guideTexts[0].text = tm.GetCommons("Tutorial01"); // 운전 연습장
        guideTexts[1].text = tm.GetCommons("Tutorial02"); // 가게 진입
        guideTexts[2].text = tm.GetCommons("Tutorial03"); // 주문 받기
        guideTexts[3].text = tm.GetCommons("Tutorial04"); // 배달 시작
        //guideTexts[4].text = string.Format(tm.GetCommons("Tutorial05"), tm.GetKeyMaps(KeyMap.worldMap), sm.keyMappings[KeyMap.worldMap].GetName()); // 배달중
        guideTexts[5].text = tm.GetCommons("Tutorial06"); // 복귀
        guideTexts[6].text = tm.GetCommons("Tutorial07"); // 탐험
        guideTexts[7].text = tm.GetCommons("Tutorial08"); // 평점

        guideTexts[8].text = tm.GetCommons("Tutorial09"); // 우주선
        guideTexts[9].text = tm.GetCommons("Tutorial10"); // 연구
        guideTexts[10].text = tm.GetCommons("Tutorial11"); // 차량
        guideTexts[11].text = tm.GetCommons("Tutorial12"); // 주민
        guideTexts[12].text = tm.GetCommons("Tutorial13"); // 주민 - 다음날로

        guideTexts[13].text = tm.GetCommons("Tutorial03_2"); // 피자 만들기
        guideTexts[14].text = tm.GetCommons("Tutorial08_2"); // 점프대
    }

    private void DriftTextUpdate(bool pad)
    {
        StringBuilder st2 = new StringBuilder();
        if (pad)
        {
            st2.Append(" ");
            st2.AppendLine();
            st2.AppendFormat(tm.GetCommons("Tutorial00"), tm.GetKeyMaps(KeyMap.carBreak));
        }
        else
        {
            st2.AppendFormat("{0} - {1}", tm.GetKeyMaps(KeyMap.carBreak), sm.keyMappings[KeyMap.carBreak].GetName());
            st2.AppendLine();
            st2.AppendFormat(tm.GetCommons("Tutorial00"), tm.GetKeyMaps(KeyMap.carBreak));
        }
        driftText.text = st2.ToString();
    }
    private void InstallTextUpdate(bool pad)
    {
        StringBuilder st2 = new StringBuilder();
        if (pad)
        {
            //st2.Append(" ");
            //st2.AppendLine();
            //st2.AppendFormat(tm.GetCommons("Tutorial00"), tm.GetKeyMaps(KeyMap.enterStore));
            installPadObjTMP.text = tm.GetCommons("Install");
        }
        else
        {
            st2.AppendFormat("{0} - {1}", tm.GetCommons("Install"), sm.keyMappings[KeyMap.enterStore].GetName());
        }
        installText.text = st2.ToString();
    }

    public void ToggleDriftGuide(bool on)
    {
        if (on)
        {
            var pad = Gamepad.current;
            driftPadObj.SetActive(pad != null);
            DriftTextUpdate(pad != null);
            driftText.gameObject.SetActive(true);
        }
        else
        {
            driftPadObj.SetActive(false);
            driftText.gameObject.SetActive(false);
        }
    }
    public void ToggleInstallGuide(bool on)
    {
        if (on)
        {
            var pad = Gamepad.current;
            installPadObj.SetActive(pad != null);
            InstallTextUpdate(pad != null);
            installText.gameObject.SetActive(true);
        }
        else
        {
            installPadObj.SetActive(false);
            installText.gameObject.SetActive(false);
        }
    }

    protected override void AddListeners()
    {
        TutorialEnterTrigger.PlayerArriveEvent += OnPlayerEnter;
        PizzaDirection.PizzaCompleteEvent += OnPizzaComplete;
    }

    protected override void RemoveListeners()
    {
        TutorialEnterTrigger.PlayerArriveEvent -= OnPlayerEnter;
        PizzaDirection.PizzaCompleteEvent -= OnPizzaComplete;
    }

    public void PadCheck()
    {
        switch (step)
        {
            case 0:
                var pad = Gamepad.current;
                controlPadObj.SetActive(pad != null);
                controlText.gameObject.SetActive(pad == null);
                break;
        }
    }

    public void GoToTutorial()
    {
        trainingCenter.SetActive(true);
        var player = GM.Instance.player;
        player.StopPlayer(true);
        player.transform.position = trainingCenterPos.position;
        player.cam.ForceUpdate();
        training = true;

        guideObjects[0].SetActive(true);

        var pad = Gamepad.current;
        controlPadObj.SetActive(pad != null);
        controlText.gameObject.SetActive(pad == null);

        UIManager.Instance.ToggleDrivingInfo(false);
        Cursor.visible = false;
    }

    public void TutorialSkip(bool hide)
    {
        if (hide)
        {
            trainingCenter.SetActive(false);
            prologueObj.SetActive(false);
        }
        var player = GM.Instance.player;
        player.StopPlayer(instance:true);
        player.transform.position = GM.Instance.pizzeriaPos.position;
        player.cam.ForceUpdate();

        for (int i = 0; i < tutorialZombie.Length; i++)
        {
            tutorialZombie[i].gameObject.SetActive(false);
            tutorialZombie[i].DriftOffContact(0f, 0f);
            tutorialZombie[i].transform.SetParent(trainingCenter.transform);
        }
        player.ShakeOffAllZombies();

        training = false;

        controlText.gameObject.SetActive(false);
        controlPadObj.SetActive(false);
        driftText.gameObject.SetActive(false);
        driftPadObj.SetActive(false);

        UIManager.Instance.ToggleDrivingInfo(true);
        AudioManager.Instance.ToggleMute(false);
    }

    private void OnPlayerEnter(object sender, int e)
    {
        switch (e)
        {
            case 0: // 붙는 좀비 소환
                if (step == 0)
                    Step1();
                break;
            case 1:
                if (step == 1)
                {
                    AudioManager.Instance.PlaySFX(Sfx.complete);
                    Step1_2();
                }
                break;
            case 2:
                if (step == 1)
                {
                    DialogueManager.Instance.SetDialogue(0);
                    var pad = Gamepad.current;
                    Cursor.visible = pad == null;
                    //Step2();
                }
                else if (step == 2)
                    Step3();
                break;
            case 3:
                if (step == 8)
                    Step9();
                break;
        }
    }

    private void Step1()
    {
        //var scanGraph = AstarPath.active.data.recastGraph;
        //ZombiePooler.Instance.astarPath.Scan(scanGraph);
        StartCoroutine(SpawnDelay());

        var pad = Gamepad.current;
        driftPadObj.SetActive(pad != null);
        DriftTextUpdate(pad != null);
        driftText.gameObject.SetActive(true);

        step = 1;
    }
    private IEnumerator SpawnDelay()
    {
        yield return null;
        yield return null;
        for (int i = 0; i < tutorialZombie.Length; i++)
        {
            tutorialZombie[i].gameObject.SetActive(true);
        }
    }

    private IEnumerator BlackScreen(float time, Action action)
    {
        GM.Instance.stop_control = true;
        Time.timeScale = 0f;
        blackScreen.SetActive(true);

        yield return CoroutineHelper.WaitForSecondsRealtime(time);

        GM.Instance.stop_control = false;
        Time.timeScale = 1f;
        blackScreen.SetActive(false);

        action?.Invoke();
    }

    private void Step1_2()
    {
        AudioManager.Instance.PlaySFX(Sfx.raid);
        oneYearsLaterTMP.text = tm.GetCommons("OutbreakDay");
        oneYearsLaterTMP.rectTransform.localScale = 0.75f * Vector3.one;
        oneYearsLaterTMP.gameObject.SetActive(true);
        Sequence seq = DOTween.Sequence().SetUpdate(true).SetAutoKill(true);
        seq.AppendInterval(1.25f);
        seq.Append(oneYearsLaterTMP.rectTransform.DOScale(0.25f, 0.6f).SetEase(Ease.InExpo));

        StartCoroutine(BlackScreen(1.75f, null));

        TutorialSkip(false);
        prologueObj.SetActive(true);
        AudioManager.Instance.ToggleMute(true);

        var player = GM.Instance.player;
        player.transform.position = prologuePos.position;
        player.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        player.cam.ForceUpdate();
        GM.Instance.timer = Constant.dayTime - 1f;
        GM.Instance.rainObj.SetActive(true);

        UIManager.Instance.ToggleDrivingInfo(false);
        Cursor.visible = false;

        step = 1;

        training = true;
        guideObjects[0].SetActive(false);
    }

    public void Step2_Before()
    {
        GM.Instance.rainObj.SetActive(false);

        oneYearsLaterTMP.text = tm.GetCommons("OneYearLater");
        oneYearsLaterTMP.rectTransform.localScale = Vector3.zero;
        oneYearsLaterTMP.gameObject.SetActive(true);
        Sequence seq = DOTween.Sequence().SetUpdate(true).SetAutoKill(true);
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() =>
        {
            oneYearsLaterTMP.rectTransform.localScale = 0.5f * Vector3.one;
        });
        seq.Append(oneYearsLaterTMP.rectTransform.DOScale(1f, 1.5f));
        seq.AppendInterval(1f);
        seq.Append(oneYearsLaterTMP.rectTransform.DOScale(0.5f, 0.6f).SetEase(Ease.InExpo));

        GM.Instance.timer = Constant.dayTime * 0.75f;

        StartCoroutine(BlackScreen(3.5f, () => 
        { 
            DialogueManager.Instance.SetDialogue(1);
            GM.Instance.TimeUpdate();
        }));

        ReturnToShop();

        UIManager.Instance.ToggleDrivingInfo(false);
    }
    private void ReturnToShop()
    {
        TutorialSkip(true);
        training = true;
        step = 2;
        var player = GM.Instance.player;
        player.transform.position = tutoReturnPos.position;
        player.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        player.cam.ForceUpdate();
    }

    private void SkipToShop_method()
    {
        GM.Instance.timer = Constant.dayTime * 0.75f;
        GM.Instance.TimeUpdate();
        ReturnToShop();
        Step2();
    }

    public void Step2()
    {
        UIManager.Instance.ToggleDrivingInfo(true);
        guideObjects[1].SetActive(true);
        shopGoal.SetActive(true);
        OrderManager.Instance.NewOrder_Tutorial();

        shopGate.alwaysClosed = true;
    }

    private void Step3()
    {
        guideObjects[1].SetActive(false);
        guideObjects[2].SetActive(true);
        indicators[0].SetActive(true);
        UIManager.Instance.shopUI.shopCloseBtn.enabled = false;
        step = 3;
    }

    public void OrderAccpeted()
    {
        if (step == 3)
        {
            guideObjects[2].SetActive(false);
            indicators[0].SetActive(false);
            guideObjects[13].SetActive(true); // 피자 만들기
            indicators[6].SetActive(true); // 오븐
            step = 4;
        }
    }
    public void PizzaOvening()
    {
        guideObjects[13].SetActive(false); // 피자 만들기
        indicators[6].SetActive(false); // 오븐
    }
    private void OnPizzaComplete(object sender, OrderInfo e)
    {
        if (step == 4)
        {
            guideObjects[3].SetActive(true);
            indicators[1].SetActive(true);
            step = 5;

            training = false; // 피자 만든 후 트레이닝 해제
        }
    }
    public void ShopWindowHide()
    {
        if (step == 100) return;

        switch (step)
        {
            case 5:
                guideObjects[3].SetActive(false);
                indicators[1].SetActive(false);
                step = 6;
                break;
            case 11:
                NextDay_After();
                if (GM.Instance.installJumpEnable)
                    guideObjects[14].SetActive(true);
                break;
            case 13:
                ThreeDay_After();
                break;
            case 15:
                FourDay_After();
                break;
        }
    }
    public void DayChanged()
    {
        if (step == 100) return;

        int day = GM.Instance.day;
        switch (day)
        {
            case 2:
                NextDay_After();
                guideObjects[14].SetActive(false);
                break;
            case 3:
                ThreeDay_After();
                break;
            case 4:
                FourDay_After();
                break;
        }
    }
    public void NoMoreEvented()
    {
        if (step == 100) return;

        int day = GM.Instance.day;
        switch (day)
        {
            case 1:
                NextDay();
                break;
            case 2:
                ThreeDay();
                break;
            case 3:
                FourDay();
                break;
        }
    }

    public void RocketWindowHide()
    {
        if (step == 10)
        {
            guideObjects[8].SetActive(false);
        }
    }
    public void ShopWindowHideComplete()
    {
        if (step == 6)
        {
            var pad = Gamepad.current;
            if (pad != null)
            {
                int padType = UINaviHelper.Instance.PadType;
                switch (padType)
                {
                    case 0:
                        guideTexts[4].text = string.Format(tm.GetCommons("Tutorial05"), tm.GetKeyMaps(KeyMap.worldMap), "<sprite=10>"); // 배달중 플스
                        break;
                    default:
                        guideTexts[4].text = string.Format(tm.GetCommons("Tutorial05"), tm.GetKeyMaps(KeyMap.worldMap), "<sprite=11>"); // 배달중 엑박
                        break;
                }
            }
            else
            {
                guideTexts[4].text = string.Format(tm.GetCommons("Tutorial05"), tm.GetKeyMaps(KeyMap.worldMap), sm.keyMappings[KeyMap.worldMap].GetName()); // 배달중
            }
            guideObjects[4].SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(guideTexts[4].transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(guideObjects[4].transform as RectTransform);
            indicators[2].SetActive(true);
            step = 7;
            shopGate.alwaysClosed = false;
        }
    }
    public void OrderCompleted()
    {
        if (step == 7)
        {
            guideObjects[4].SetActive(false);
            indicators[2].SetActive(false);
            guideObjects[5].SetActive(true);
            indicators[3].SetActive(true);
            returnGoal.SetActive(true);
            step = 8;
        }
        else if (step == 12)
            guideObjects[14].SetActive(false);
    }
    public void JumpRampInstalled()
    {
        if (step == 12)
            guideObjects[14].SetActive(false);
    }
    private void Step9()
    {
        guideObjects[5].SetActive(false);
        indicators[3].SetActive(false);
        guideObjects[6].SetActive(true);
        indicators[4].SetActive(true);
        step = 9;
    }
    public void SendExploration()
    {
        if (step == 9)
        {
            guideObjects[6].SetActive(false);
            indicators[4].SetActive(false);
            indicators[5].SetActive(true);
            UIManager.Instance.shopUI.shopCloseBtn.enabled = true;
            step = 10;
        }
    }
    public void Spaceship()
    {
        if (step == 10)
        {
            guideObjects[8].SetActive(true);
        }
    }
    public void NextDay()
    {
        if (step == 10)
        {
            guideObjects[7].SetActive(true); // 평점
            indicators[5].SetActive(false);
            step = 11;
        }
    }
    private void NextDay_After()
    {
        guideObjects[7].SetActive(false); // 평점
        step = 12;
    }
    public void ThreeDay() // 탐험, 이벤트 완료후
    {
        if (step == 12)
        {
            UIManager.Instance.shopUI.SelectSubPanel(2);
            guideObjects[9].SetActive(true);
            step = 13;
        }
    }
    private void ThreeDay_After()
    {
        guideObjects[9].SetActive(false);
        step = 14;
    }
    public void FourDay()
    {
        if (step == 14)
        {
            UIManager.Instance.shopUI.SelectSubPanel(3);
            UIManager.Instance.shopUI.ChangeViewVehicle(true);
            guideObjects[10].SetActive(true);
            step = 15;
        }
    }
    private void FourDay_After()
    {
        guideObjects[10].SetActive(false);
        step = 16;
    }

    public void ManMove_Enter()
    {
        if (step == 100) return;

        if (!manModeEntered)
        {
            guideObjects[11].SetActive(true);
        }
    }
    public void ManMove_TalkEnd()
    {
        if (step == 100) return;

        if (!manModeEntered)
        {
            guideObjects[11].SetActive(false);
            guideObjects[12].SetActive(true);
            manModeEntered = true;
        }
    }
    public void Midnight_Leave()
    {
        guideObjects[11].SetActive(false);
        guideObjects[12].SetActive(false);
    }

    //public void TutorialDisalbe()
    //{
    //    step = 100;
    //    shopGate.alwaysClosed = false;
    //    UIManager.Instance.shopUI.shopCloseBtn.enabled = true;
    //    TutorialSkip();

    //    for (int i = 0; i < guideObjects.Length; i++)
    //    {
    //        guideObjects[i].SetActive(false);
    //    }
    //    for (int i =0; i < indicators.Length; i++)
    //    {
    //        indicators[i].SetActive(false);
    //    }
    //    shopGoal.SetActive(false);
    //    returnGoal.SetActive(false);
    //}
}
