using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    [Header("디버깅")]
    public bool debug_skipToShop;
    public bool debug_skipToReturn;
    public bool debug_TutorialDisable;

    [Space(20f)]

    public int step;

    public bool training;

    [Header("특수 : 튜토리얼")]
    public Zombie4[] tutorialZombie;
    public GameObject trainingCenterSpawnBlock;
    public GameObject trainingCenter;
    public Transform trainingCenterPos;

    public TextMeshProUGUI controlText;
    public TextMeshProUGUI driftText;

    public GameObject[] guideObjects;
    public TextMeshProUGUI[] guideTexts;

    public ShopGate shopGate;
    public GameObject shopGoal;
    public GameObject returnGoal;

    private TextManager tm => TextManager.Instance;
    private SettingManager sm => SettingManager.Instance;

    public void Init()
    {
        step = 0;

        UpdateText();
        for (int i = 0; i < guideObjects.Length; i++)
        {
            guideObjects[i].SetActive(true);
        }
        Canvas.ForceUpdateCanvases();

        for (int i = 0; i < tutorialZombie.Length; i++)
        {
            tutorialZombie[i].Init(ZombiePooler.Instance.target);
            tutorialZombie[i].gameObject.SetActive(false);
        }
        controlText.gameObject.SetActive(false);
        driftText.gameObject.SetActive(false);
        for (int i = 0; i < guideObjects.Length; i++)
        {
            guideObjects[i].SetActive(false);
        }
        shopGoal.SetActive(false);
        returnGoal.SetActive(false);

        if (debug_TutorialDisable)
        {
            TutorialSkip();
            step = 100;
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

        StringBuilder st2 = new StringBuilder();
        st2.AppendFormat("{0} - {1}", tm.GetKeyMaps(KeyMap.carBreak), sm.keyMappings[KeyMap.carBreak].GetName());
        st2.AppendLine();
        st2.AppendFormat(tm.GetCommons("Tutorial00"), tm.GetKeyMaps(KeyMap.carBreak));

        driftText.text = st2.ToString();

        guideTexts[0].text = tm.GetCommons("Tutorial01");
        guideTexts[1].text = tm.GetCommons("Tutorial02");
        guideTexts[2].text = tm.GetCommons("Tutorial03");
        guideTexts[3].text = tm.GetCommons("Tutorial04");
        guideTexts[4].text = string.Format(tm.GetCommons("Tutorial05"), tm.GetKeyMaps(KeyMap.worldMap), sm.keyMappings[KeyMap.worldMap].GetName());
        guideTexts[5].text = tm.GetCommons("Tutorial06");
        guideTexts[6].text = tm.GetCommons("Tutorial07");
        guideTexts[7].text = tm.GetCommons("Tutorial08");
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

    public void GoToTutorial()
    {
        trainingCenter.SetActive(true);
        GM.Instance.player.StopPlayer(false);
        GM.Instance.player.transform.position = trainingCenterPos.position;
        GM.Instance.player.cam.ForceUpdate();
        training = true;

        guideObjects[0].SetActive(true);
        controlText.gameObject.SetActive(true);
    }

    public void TutorialSkip()
    {
        trainingCenter.SetActive(false);
        GM.Instance.player.StopPlayer(instance:true);
        GM.Instance.player.transform.position = GM.Instance.pizzeriaPos.position;
        GM.Instance.player.cam.ForceUpdate_WhenMoving();
        for (int i = 0; i < tutorialZombie.Length; i++)
        {
            tutorialZombie[i].gameObject.SetActive(false);
            tutorialZombie[i].DriftOffContact(0f, 0f);
            tutorialZombie[i].transform.SetParent(trainingCenter.transform);
        }
        training = false;

        controlText.gameObject.SetActive(false);
        driftText.gameObject.SetActive(false);
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
                    Step2();
                break;
            case 2:
                if (step == 2)
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

    private void Step2()
    {
        TutorialSkip();
        training = true;
        guideObjects[0].SetActive(false);
        guideObjects[1].SetActive(true);
        shopGoal.SetActive(true);
        step = 2;

        shopGate.alwaysClosed = true;
    }

    private void Step3()
    {
        guideObjects[1].SetActive(false);
        guideObjects[2].SetActive(true);
        UIManager.Instance.shopUI.shopCloseBtn.enabled = false;
        step = 3;
    }

    public void OrderAccpeted()
    {
        if (step == 3)
        {
            guideObjects[2].SetActive(false);
            step = 4;
        }
    }
    private void OnPizzaComplete(object sender, OrderInfo e)
    {
        if (step == 4)
        {
            guideObjects[3].SetActive(true);
            step = 5;
        }
    }
    public void ShopWindowHide()
    {
        if (step == 5)
        {
            guideObjects[3].SetActive(false);
            step = 6;
        }
        else if (step == 11)
        {
            guideObjects[7].SetActive(false);
            step = 12;
        }
    }
    public void ShopWindowHideComplete()
    {
        if (step == 6)
        {
            guideObjects[4].SetActive(true);
            step = 7;
            shopGate.alwaysClosed = false;
        }
    }
    public void OrderCompleted()
    {
        if (step == 7)
        {
            guideObjects[4].SetActive(false);
            guideObjects[5].SetActive(true);
            returnGoal.SetActive(true);
            step = 8;
        }
    }
    private void Step9()
    {
        guideObjects[5].SetActive(false);
        guideObjects[6].SetActive(true);
        step = 9;
    }
    public void SendExploration()
    {
        if (step == 9)
        {
            guideObjects[6].SetActive(false);
            UIManager.Instance.shopUI.shopCloseBtn.enabled = true;
            training = false;
            step = 10;
        }
    }
    public void NextDay()
    {
        if (step == 10)
        {
            guideObjects[7].SetActive(true);
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
        shopGoal.SetActive(false);
        returnGoal.SetActive(false);
    }
}
