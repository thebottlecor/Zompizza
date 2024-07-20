using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public bool training;

    [Header("특수 : 튜토리얼")]
    public Zombie_Tutorial_Contact[] tutorialZombie;
    public GameObject trainingCenterSpawnBlock;
    public GameObject trainingCenter;
    public GameObject prologueObj;
    public Transform trainingCenterPos;
    public Transform prologuePos;

    public TextMeshProUGUI controlText;
    public GameObject controlPadObj;
    public TextMeshProUGUI driftText;
    public GameObject driftPadObj;

    public GameObject[] guideObjects;
    public TextMeshProUGUI[] guideTexts;

    public ShopGate shopGate;
    public GameObject shopGoal;
    public GameObject returnGoal;

    public GameObject[] indicators;

    private TextManager tm => TextManager.Instance;
    private SettingManager sm => SettingManager.Instance;

    public void Init()
    {
        trainingCenter.SetActive(false);
        prologueObj.SetActive(false);

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

        if (debug_TutorialDisable)
        {
            TutorialSkip();
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
            Step2();
            return;
        }
        if (debug_skipToReturn)
        {
            TutorialSkip();
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

        guideTexts[0].text = tm.GetCommons("Tutorial01");
        guideTexts[1].text = tm.GetCommons("Tutorial02");
        guideTexts[2].text = tm.GetCommons("Tutorial03");
        guideTexts[3].text = tm.GetCommons("Tutorial04");
        guideTexts[4].text = string.Format(tm.GetCommons("Tutorial05"), tm.GetKeyMaps(KeyMap.worldMap), sm.keyMappings[KeyMap.worldMap].GetName());
        guideTexts[5].text = tm.GetCommons("Tutorial06");
        guideTexts[6].text = tm.GetCommons("Tutorial07");
        guideTexts[7].text = tm.GetCommons("Tutorial08");
        guideTexts[8].text = tm.GetCommons("Tutorial09");
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
        player.StopPlayer(false);
        player.transform.position = trainingCenterPos.position;
        player.cam.ForceUpdate();
        training = true;

        guideObjects[0].SetActive(true);

        var pad = Gamepad.current;
        controlPadObj.SetActive(pad != null);
        controlText.gameObject.SetActive(pad == null);

        UIManager.Instance.ToggleDrivingInfo(false);
    }

    public void TutorialSkip()
    {
        trainingCenter.SetActive(false);
        prologueObj.SetActive(false);
        var player = GM.Instance.player;
        player.StopPlayer(instance:true);
        player.transform.position = GM.Instance.pizzeriaPos.position;
        player.cam.ForceUpdate_WhenMoving();

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
        trainingCenterSpawnBlock.SetActive(false);
        var scanGraph = AstarPath.active.data.recastGraph;
        ZombiePooler.Instance.astarPath.Scan(scanGraph);
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

    private void Step1_2()
    {
        TutorialSkip();
        prologueObj.SetActive(true);
        AudioManager.Instance.ToggleMute(true);

        var player = GM.Instance.player;
        player.transform.position = prologuePos.position;
        player.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        player.cam.ForceUpdate_WhenMoving();
        GM.Instance.timer = Constant.dayTime - 1f;
        GM.Instance.rainObj.SetActive(true);

        UIManager.Instance.ToggleDrivingInfo(false);

        step = 1;

        training = true;
        guideObjects[0].SetActive(false);
    }

    public void Step2()
    {
        TutorialSkip();
        training = true;
        //guideObjects[0].SetActive(false);
        guideObjects[1].SetActive(true);
        shopGoal.SetActive(true);
        GM.Instance.timer = Constant.dayTime * 0.75f;
        OrderManager.Instance.NewOrder_Tutorial();
        GM.Instance.rainObj.SetActive(false);
        step = 2;

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
            step = 4;
        }
    }
    private void OnPizzaComplete(object sender, OrderInfo e)
    {
        if (step == 4)
        {
            guideObjects[3].SetActive(true);
            indicators[1].SetActive(true);
            step = 5;
        }
    }
    public void ShopWindowHide()
    {
        if (step == 5)
        {
            guideObjects[3].SetActive(false);
            indicators[1].SetActive(false);
            step = 6;
        }
        else if (step == 11)
        {
            //guideObjects[7].SetActive(false);
            guideObjects[8].SetActive(false);
            step = 12;
        }
    }
    public void ShopWindowHideComplete()
    {
        if (step == 6)
        {
            guideObjects[4].SetActive(true);
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
            training = false;
            step = 10;
        }
    }
    public void NextDay()
    {
        if (step == 10)
        {
            //guideObjects[7].SetActive(true);
            guideObjects[8].SetActive(true);
            indicators[5].SetActive(false);
            step = 11;
        }
    }

    public void TutorialDisalbe()
    {
        step = 100;
        shopGate.alwaysClosed = false;
        UIManager.Instance.shopUI.shopCloseBtn.enabled = true;
        TutorialSkip();

        for (int i = 0; i < guideObjects.Length; i++)
        {
            guideObjects[i].SetActive(false);
        }
        for (int i =0; i < indicators.Length; i++)
        {
            indicators[i].SetActive(false);
        }
        shopGoal.SetActive(false);
        returnGoal.SetActive(false);
    }
}
