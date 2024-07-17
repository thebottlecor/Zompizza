using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RocketManager : Singleton<RocketManager>
{

    public TextMeshProUGUI countdownTMP;
    public TextMeshProUGUI projectTMP;
    public TextMeshProUGUI spaceTMP;
    public TextMeshProUGUI developTMP;
    public TextMeshProUGUI skipTMP;

    public Slider progressBar;
    public TextMeshProUGUI progressTMP;

    public Image partsImage;
    public TextMeshProUGUI currentStepsTMP;
    public TextMeshProUGUI currentCostTMP;


    public GameObject planetCam;
    public GameObject rocketCam;

    public GameObject panel;

    public int currentStep = 0;
    public const int MaxStep = 10;
    public const int Countdown = 30;

    public List<int> cost = new List<int>() { 5000, 10000, 20000, 40000, 80000, 120000, 160000, 200000, 240000, 280000 };

    [System.Serializable]
    public struct RocketModels
    {
        public List<GameObject> objs;
    }
    public List<RocketModels> rocketModels;

    private TextManager tm => TextManager.Instance;
    public bool Completed => currentStep >= MaxStep;
    private bool loading;

    public Button developBtn;
    public Button skipBtn;

    private void Start()
    {
        projectTMP.text = tm.GetCommons("SpaceshipProject");
        spaceTMP.text = tm.GetCommons("Spaceship");

        developTMP.text = tm.GetCommons("Develop");
        skipTMP.text = tm.GetCommons("Skip");

        UpdateModels();
    }

    public void UpdateModels()
    {
        for (int i = 0; i < currentStep + 1; i++)
        {
            var list = rocketModels[i].objs;
            for (int n = 0; n < list.Count; n++)
            {
                list[n].SetActive(true);
            }
        }
    }

    public void ShowPanel()
    {
        planetCam.SetActive(true);
        rocketCam.SetActive(true);

        UpdateProgressUI();
        UpdateStepUI();

        developBtn.interactable = true;
        skipBtn.interactable = true;

        panel.SetActive(true);
    }

    private void UpdateProgressUI()
    {
        countdownTMP.text = string.Format(tm.GetCommons("NuclearDay"), Countdown - GM.Instance.day);

        float currentProgress = (float)currentStep / MaxStep;

        progressBar.value = currentProgress;

        progressTMP.text = $"{tm.GetCommons("Progression")} {currentProgress * 100f:F0}%";
    }
    private void UpdateStepUI()
    {
        if (!Completed)
        {
            currentStepsTMP.text = $"({tm.GetCommons("Step")} {currentStep + 1} / {MaxStep}) PartsName";

            // ÆÄÃ÷ ÀÌ¹ÌÁö Á¶Á¤ & °¡°Ý Á¶Á¤
            partsImage.sprite = null;
            partsImage.color = Color.white;

            currentCostTMP.text = $"<sprite=2> {tm.GetCommons("Costs")} {cost[currentStep]}$";
        }
        else
        {
            currentStepsTMP.text = tm.GetCommons("Completed");

            partsImage.sprite = null;
            partsImage.color = new Color(0, 0, 0, 0);

            currentCostTMP.text = string.Empty;
        }
    }

    public void Develop()
    {
        if (loading) return;
        if (Completed) return;
        if (GM.Instance.gold < cost[currentStep]) return;

        GM.Instance.AddGold(-1 * cost[currentStep], GM.GetGoldSource.upgrade);
        currentStep++;

        // ¿¬Ãâ (¶Òµü¶Òµü ¹¶°Ô¹¶°Ô)
        loading = true;
        UpdateProgressUI();

        developBtn.interactable = false;
        skipBtn.interactable = false;


        // ¿¬Ãâ ¿Ï·á ÈÄ º¸¿©ÁÖ±â (Â¥¶ó¶õ)
        UpdateModels();

        // ¾à°£ ¶äµéÀÓ

        // ¿¬Ãâ ÈÄ ´Ý±â
        //loading = false;
        //Skip();
    }
    public void Skip()
    {
        if (loading) return;
        HidePanel();
        GM.Instance.NextDay_Late();
        loading = false;
    }

    public void HidePanel()
    {
        planetCam.SetActive(false);
        rocketCam.SetActive(false);

        panel.SetActive(false);
    }
}
