using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class RocketManager : Singleton<RocketManager>
{
    [Serializable]
    public struct SaveData
    {
        public int currentStep;
    }

    public SaveData Save()
    {
        SaveData data = new SaveData
        {
            currentStep = this.currentStep,
        };
        return data;
    }

    public void Load(SaveData data)
    {
        currentStep = data.currentStep;
        UpdateModels();
    }

    public TextMeshProUGUI countdownTMP;
    public TextMeshProUGUI projectTMP;
    public TextMeshProUGUI spaceTMP;
    public TextMeshProUGUI developTMP;
    public TextMeshProUGUI skipTMP;
    public TextMeshProUGUI lanuchTMP;

    public Slider progressBar;
    public TextMeshProUGUI progressTMP;

    public Image partsImage;
    public TextMeshProUGUI currentStepsTMP;
    public TextMeshProUGUI currentCostTMP;


    public GameObject planetCam;
    public GameObject rocketCam;

    public GameObject panel;

    public Animator darkPanelAnim;
    public Image darkPanelImage;

    public int currentStep = 0;
    public const int MaxStep = 10;
    public const int Countdown = 30;

    //private List<int> cost = new List<int>() { 3000, 6000, 12000, 24000, int.MaxValue - 99, int.MaxValue - 98, int.MaxValue - 97, int.MaxValue - 96, int.MaxValue - 95, int.MaxValue - 94 };
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
    public GameObject launchBtn;

    public GameObject[] effects;
    public GameObject[] effects2;
    public GameObject[] effects3;

    [Header("ShopUIs")]
    public TextMeshProUGUI projectTMP2;
    public Slider progressBar2;
    public TextMeshProUGUI progressTMP2;
    public TextMeshProUGUI currentStepsTMP2;
    public TextMeshProUGUI currentCostTMP2;

    private void Start()
    {
        projectTMP.text = tm.GetCommons("SpaceshipProject");
        projectTMP2.text = tm.GetCommons("SpaceshipProject");
        spaceTMP.text = tm.GetCommons("Spaceship");

        developTMP.text = tm.GetCommons("Develop");
        lanuchTMP.text = tm.GetCommons("Launch");

        HidePanel();
        UpdateModels();
    }

    public int GetCost()
    {
        return (int)(cost[currentStep] * GameEventManager.Instance.MysteryAidEffect());
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
        TutorialManager.Instance.Spaceship();
        planetCam.SetActive(true);
        rocketCam.SetActive(true);

        UpdateProgressUI();
        UpdateStepUI();

        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].SetActive(false);
        }
        for (int i = 0; i < effects2.Length; i++)
        {
            effects2[i].SetActive(false);
        }
        for (int i = 0; i < effects3.Length; i++)
        {
            effects3[i].SetActive(false);
        }

        developBtn.gameObject.SetActive(!Completed);

        skipTMP.text = tm.GetCommons("Skip");
        skipBtn.gameObject.SetActive(GM.Instance.day != Countdown);
        launchBtn.SetActive(GM.Instance.day == Countdown);

        GM.Instance.SetLight(0f, 0); // ·ÎÄÏ ¹à°Ô º¸ÀÌ°Ô ³·À¸·Î Á¶Á¤

        panel.SetActive(true);
    }

    private void UpdateProgressUI()
    {
        countdownTMP.text = string.Format(tm.GetCommons("NuclearDay"), Countdown - GM.Instance.day);

        float currentProgress = (float)currentStep / MaxStep;
        if (currentProgress <= 0f) currentProgress = 0.01f;

        progressBar.value = currentProgress;

        progressTMP.text = $"{tm.GetCommons("Progression")} {currentProgress * 100f:F0}%";
    }

    public void UpdateShopUI()
    {
        float currentProgress = (float)currentStep / MaxStep;
        if (currentProgress <= 0f) currentProgress = 0.01f;
        progressBar2.value = currentProgress;
        progressTMP2.text = $"{tm.GetCommons("Progression")} {currentProgress * 100f:F0}%";

        if (!Completed)
        {
            currentStepsTMP2.text = $"({tm.GetCommons("Step")} {currentStep + 1} / {MaxStep}) {tm.GetSpaceships(currentStep)}";
            currentCostTMP2.text = $"<sprite=2> {tm.GetCommons("Costs")} {GetCost()}G";
        }
        else
        {
            currentStepsTMP2.text = tm.GetCommons("Completed");
            currentCostTMP2.text = tm.GetCommons("CompletedWait");
        }
    }

    private void UpdateStepUI()
    {
        if (!Completed)
        {
            currentStepsTMP.text = $"<size=90%>({tm.GetCommons("Step")} {currentStep + 1} / {MaxStep}) </size>{tm.GetSpaceships(currentStep)}";

            // ÆÄÃ÷ ÀÌ¹ÌÁö Á¶Á¤ & °¡°Ý Á¶Á¤
            partsImage.sprite = DataManager.Instance.uiLib.spaceshipParts[currentStep];
            //partsImage.color = Color.white;

            //if (cost[currentStep] >= int.MaxValue - 100) // µ¥¸ð °³¹ß ÁßÁö
            //{
            //    currentCostTMP.text = tm.GetCommons("DemoInvalid2");
            //}
            //else
            //{
            //    if (cost[currentStep] > GM.Instance.gold)
            //        currentCostTMP.text = $"<color=#AB5239><sprite=2> {tm.GetCommons("Costs")} {cost[currentStep]}G</color>";
            //    else
            //        currentCostTMP.text = $"<sprite=2> {tm.GetCommons("Costs")} {cost[currentStep]}G";
            //}

            if (GetCost() > GM.Instance.gold)
                currentCostTMP.text = $"<color=#AB5239><sprite=2> {tm.GetCommons("Costs")} {GetCost()}G</color>";
            else
                currentCostTMP.text = $"<sprite=2> {tm.GetCommons("Costs")} {GetCost()}G";
        }
        else
        {
            currentStepsTMP.text = tm.GetCommons("Completed");

            partsImage.sprite = DataManager.Instance.uiLib.spaceshipParts[MaxStep];
            //partsImage.color = new Color(0, 0, 0, 0);

            currentCostTMP.text = string.Empty;
        }
    }

    public void Develop()
    {
        if (loading) return;
        if (Completed) return;
        //if (cost[currentStep] >= int.MaxValue - 100) return; // µ¥¸ð °³¹ß ÁßÁö
        if (GM.Instance.gold < GetCost())
        {
            AudioManager.Instance.PlaySFX(Sfx.deny);
            return;
        }

        GM.Instance.AddGold(-1 * GetCost(), GM.GetGoldSource.upgrade);

        float displayProgress = (float)currentStep / MaxStep;
        if (displayProgress <= 0f) displayProgress = 0.01f;

        currentStep++;

        float target = (float)currentStep / MaxStep;

        DOVirtual.Float(displayProgress, target, 1.6f, (x) =>
        {
            progressBar.value = x;
            progressTMP.text = $"{tm.GetCommons("Progression")} {x * 100f:F0}%";

        }).SetEase(Ease.OutCirc).SetUpdate(true);

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(true);
        sequence.AppendCallback(() =>
        {
            // ¿¬Ãâ (¶Òµü¶Òµü ¹¶°Ô¹¶°Ô)
            loading = true;

            developBtn.gameObject.SetActive(false);
            skipBtn.gameObject.SetActive(false);
            launchBtn.SetActive(false);
            UINaviHelper.Instance.SetFirstSelect();
        });
        sequence.AppendInterval(0.15f);
        sequence.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySFX(Sfx.Repair);
            effects[0].SetActive(true);
        });
        sequence.AppendInterval(0.15f);
        sequence.AppendCallback(() =>
        {
            effects2[0].SetActive(true);
        });
        sequence.AppendInterval(0.15f);
        sequence.AppendCallback(() =>
        {
            effects[1].SetActive(true);
        });
        sequence.AppendInterval(0.15f);
        sequence.AppendCallback(() =>
        {
            effects2[1].SetActive(true);
        });
        sequence.AppendInterval(0.15f);
        sequence.AppendCallback(() =>
        {
            effects[2].SetActive(true);
        });
        sequence.AppendInterval(0.15f);
        sequence.AppendCallback(() =>
        {
            effects2[2].SetActive(true);
        });
        sequence.AppendInterval(0.15f);
        sequence.AppendCallback(() =>
        {
            effects[3].SetActive(true);
        });
        sequence.AppendInterval(0.15f);
        sequence.AppendCallback(() =>
        {
            effects2[3].SetActive(true);
        });
        sequence.AppendInterval(0.3f);
        sequence.AppendCallback(() =>
        {
            // ¿¬Ãâ ¿Ï·á ÈÄ º¸¿©ÁÖ±â (Â¥¶ó¶õ)
            UpdateModels();
            AudioManager.Instance.PlaySFX(Sfx.complete);
            for (int i = 0; i < effects3.Length; i++)
            {
                effects3[i].SetActive(true);
            }
        });
        // ¾à°£ ¶äµéÀÓ
        sequence.AppendInterval(1.5f);
        sequence.AppendCallback(() =>
        {
            // ¿¬Ãâ ÈÄ ´Ý±â
            loading = false;
            skipTMP.text = tm.GetCommons("Close");

            if (Completed)
            {
                skipBtn.gameObject.SetActive(false);
                launchBtn.SetActive(true);
            }
            else
            {
                skipBtn.gameObject.SetActive(GM.Instance.day != Countdown);
                launchBtn.SetActive(GM.Instance.day == Countdown);
            }
            UINaviHelper.Instance.SetFirstSelect();
        });
    }
    public void Skip()
    {
        if (loading) return;
        TutorialManager.Instance.RocketWindowHide();

        if (Completed || GM.Instance.day == Countdown)
        {
            // ¹ß»ç ¿¬Ãâ
            Sequence sequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(true);
            sequence.AppendCallback(() =>
            {
                // ¿¬Ãâ (¶Òµü¶Òµü ¹¶°Ô¹¶°Ô)
                loading = true;

                darkPanelAnim.enabled = true;
                developBtn.gameObject.SetActive(false);
                skipBtn.gameObject.SetActive(false);
                launchBtn.SetActive(false);
                UINaviHelper.Instance.SetFirstSelect();
            });
            sequence.AppendInterval(0.15f);
            sequence.AppendCallback(() =>
            {
                AudioManager.Instance.ToggleMute(true);
                AudioManager.Instance.PlaySFX(Sfx.rocketAlarm);
                AudioManager.Instance.PlaySFX(Sfx.rocketCountdown);
            });
            sequence.AppendInterval(6f);
            sequence.AppendCallback(() =>
            {
                SkipMethod();
            });
        }
        else
        {
            SkipMethod();
        }
    }

    private void SkipMethod()
    {
        darkPanelAnim.enabled = false;
        darkPanelImage.color = Color.black;
        HidePanel();

        if (Completed)
        {
            GM.Instance.Ending();
        }
        else
        {
            //GM.Instance.NextDay_Late();
            int currentVillager = VillagerManager.Instance.GetRecruitedVillagerCount();
            if (GM.Instance.day < Countdown && currentVillager > 0)
                GM.Instance.NextDay_Midnight();
            else
                GM.Instance.NextDay_Late(false);
        }
        loading = false;
    }

    public void HidePanel()
    {
        planetCam.SetActive(false);
        rocketCam.SetActive(false);

        panel.SetActive(false);
    }
}
