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

    private List<int> cost = new List<int>() { 3000, 6000, 12000, 24000, int.MaxValue - 99, int.MaxValue - 98, int.MaxValue - 97, int.MaxValue - 96, int.MaxValue - 95, int.MaxValue - 94 };
    //public List<int> cost = new List<int>() { 5000, 10000, 20000, 40000, 80000, 120000, 160000, 200000, 240000, 280000 };

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

    public GameObject[] effects;
    public GameObject[] effects2;
    public GameObject[] effects3;

    private void Start()
    {
        projectTMP.text = tm.GetCommons("SpaceshipProject");
        spaceTMP.text = tm.GetCommons("Spaceship");

        developTMP.text = tm.GetCommons("Develop");

        HidePanel();
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

        if (cost[currentStep] >= int.MaxValue - 100) // 데모 개발 중지
        {
            developBtn.gameObject.SetActive(false);
        }
        else
            developBtn.gameObject.SetActive(true);
        skipTMP.text = tm.GetCommons("Skip");
        skipBtn.gameObject.SetActive(true);

        GM.Instance.globalLight.color = DataManager.Instance.uiLib.timeLightGradient.Evaluate(0); // 로켓 밝게 보이게 낮으로 조정

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
    private void UpdateStepUI()
    {
        if (!Completed)
        {
            currentStepsTMP.text = $"<size=90%>({tm.GetCommons("Step")} {currentStep + 1} / {MaxStep}) </size>{tm.GetSpaceships(currentStep)}";

            // 파츠 이미지 조정 & 가격 조정
            partsImage.sprite = DataManager.Instance.uiLib.spaceshipParts[currentStep];
            //partsImage.color = Color.white;

            if (cost[currentStep] >= int.MaxValue - 100) // 데모 개발 중지
            {
                currentCostTMP.text = tm.GetCommons("DemoInvalid2");
            }
            else
            {
                if (cost[currentStep] > GM.Instance.gold)
                    currentCostTMP.text = $"<color=#AB5239><sprite=2> {tm.GetCommons("Costs")} {cost[currentStep]}$</color>";
                else
                    currentCostTMP.text = $"<sprite=2> {tm.GetCommons("Costs")} {cost[currentStep]}$";
            }
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
        if (cost[currentStep] >= int.MaxValue - 100) return; // 데모 개발 중지
        if (GM.Instance.gold < cost[currentStep])
        {
            AudioManager.Instance.PlaySFX(Sfx.deny);
            return;
        }

        GM.Instance.AddGold(-1 * cost[currentStep], GM.GetGoldSource.upgrade);

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
            // 연출 (뚝딱뚝딱 뭉게뭉게)
            loading = true;

            developBtn.gameObject.SetActive(false);
            skipBtn.gameObject.SetActive(false);
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
            // 연출 완료 후 보여주기 (짜라란)
            UpdateModels();
            AudioManager.Instance.PlaySFX(Sfx.complete);
            for (int i = 0; i < effects3.Length; i++)
            {
                effects3[i].SetActive(true);
            }
        });
        // 약간 뜸들임
        sequence.AppendInterval(1.5f);
        sequence.AppendCallback(() =>
        {
            // 연출 후 닫기
            loading = false;
            skipTMP.text = tm.GetCommons("Close");
            skipBtn.gameObject.SetActive(true);
            UINaviHelper.Instance.SetFirstSelect();
        });
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
