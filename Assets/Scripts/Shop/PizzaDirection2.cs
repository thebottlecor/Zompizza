using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// 백업본
public class PizzaDirection2 : EventListener
{
    public CanvasGroup parentPanel;

    public Transform initParent;

    public Animator pizzaBoxAnimator;

    [System.Serializable]
    public struct pizzaModel
    {
        public Mesh mesh;
        public Material material;
    }
    public List<pizzaModel> pizzaModels;
    public Transform pizza;
    public Transform pizzaEffect;

    public Transform stackTarget11;
    public Transform stackTarget22;

    private Sequence sequence;
    private Sequence sequence2; // 재료가 빨려들어가는 연출
    private OrderInfo info;

    public SerializableDictionary<Ingredient, Transform> ingredients;
    public GameObject ingredientsSource;
    public List<Transform> ingredientsRandomPos;

    [SerializeField] private RectTransform shopPanel;
    [SerializeField] private Vector2 initShopPanelPos;

    public static EventHandler<OrderInfo> PizzaCompleteEvent;

    protected override void AddListeners()
    {
        SettingManager.ResolutionChangedEvent += OnResolutionChanged;
    }

    protected override void RemoveListeners()
    {
        SettingManager.ResolutionChangedEvent -= OnResolutionChanged;
    }

    private void Awake()
    {
        initShopPanelPos = new Vector2(shopPanel.offsetMin.y, shopPanel.offsetMax.y);
        shopPanel.offsetMin = new Vector2(0, 0);
        shopPanel.offsetMax = new Vector2(0, 0);
        Canvas.ForceUpdateCanvases();
    }

    void Start()
    {
        // 이건 별도의 맨처음 초기화
        ingredients = new SerializableDictionary<Ingredient, Transform>();
        var list = Enum.GetValues(typeof(Ingredient));
        int count = 0;
        foreach (var temp in list)
        {
            Ingredient key = (Ingredient)temp;
            var obj = Instantiate(ingredientsSource);
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = DataManager.Instance.uiLib.ingredients[key];
            obj.transform.position = ingredientsRandomPos[count].position;
            obj.transform.SetParent(initParent);
            count++;
            if (count >= ingredientsRandomPos.Count)
                count = 0;
            ingredients.Add(new SerializableDictionary<Ingredient, Transform>.Pair { Key = key, Value = obj.transform });
        }

        Init();

        foreach (var temp in ingredients)
        {
            temp.Value.gameObject.SetActive(false);
        }
        pizzaBoxAnimator.gameObject.SetActive(false);
        pizza.gameObject.SetActive(false);
        pizzaEffect.gameObject.SetActive(false);

        shopPanel.offsetMin = new Vector2(0, initShopPanelPos.x);
        shopPanel.offsetMax = new Vector2(0, initShopPanelPos.y);
    }

    private void OnResolutionChanged(object sender, EventArgs e)
    {
        StartCoroutine(SequenceUpdate());
    }
    private IEnumerator SequenceUpdate()
    {
        UIManager.Instance.changingResolution = true;

        yield return null;

        shopPanel.gameObject.SetActive(false);
        initShopPanelPos = new Vector2(shopPanel.offsetMin.y, shopPanel.offsetMax.y);
        shopPanel.offsetMin = new Vector2(0, 0);
        shopPanel.offsetMax = new Vector2(0, 0);
        Canvas.ForceUpdateCanvases();

        yield return null;

        Init();

        shopPanel.offsetMin = new Vector2(0, initShopPanelPos.x);
        shopPanel.offsetMax = new Vector2(0, initShopPanelPos.y);
        shopPanel.gameObject.SetActive(true);

        UIManager.Instance.changingResolution = false;
    }

    private void OnDestroy()
    {
        SequenceKill();
    }
    private void SequenceKill()
    {
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }
        if (sequence2 != null)
        {
            sequence2.Kill();
            sequence2 = null;
        }
    }

    public void Init()
    {
        // 연출 변경에 따른 추가
        ResetPos();

        SequenceKill();

        sequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true);
        // 닫힌 상태로 초기화
        sequence.AppendCallback(() =>
        {
            transform.localScale = Vector3.one;

            UIManager.Instance.isDirecting = true;

            parentPanel.alpha = 1f;
            parentPanel.interactable = false;

            pizzaBoxAnimator.gameObject.SetActive(true);
            pizzaBoxAnimator.transform.localEulerAngles = new Vector3(45f, 180f, 0f);
            pizzaBoxAnimator.Play("Closed");
            pizza.gameObject.SetActive(false);
            pizzaEffect.gameObject.SetActive(false);
            pizzaEffect.localScale = Vector3.one;
        });
        // 열린다
        sequence.Append(parentPanel.DOFade(0.1f, 0.25f));
        //sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            pizzaBoxAnimator.Play("Opening");
        });
        sequence.AppendInterval(0.25f);
        sequence.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySFX(Sfx.chop, true);
        });

        // 재료들이 들어간다
        sequence.AppendInterval(0.75f); // 더미

        foreach (var temp in ingredients)
        {
            sequence.Join(temp.Value.DOLocalMoveX(pizzaBoxAnimator.transform.localPosition.x, 0.75f).SetEase(Ease.OutQuad));
            sequence.Join(temp.Value.DOLocalMoveY(pizzaBoxAnimator.transform.localPosition.y + 0.15f, 0.75f).SetEase(Ease.InQuad));
            sequence.Join(temp.Value.DOScale(0.4f, 0.75f).SetEase(Ease.OutQuint));
        }

        sequence.AppendCallback(() =>
        {
            AudioManager.Instance.StopSFX(true);
            foreach (var temp in ingredients)
            {
                temp.Value.SetParent(pizzaBoxAnimator.transform);
            }
            pizzaBoxAnimator.Play("Closing");
            AudioManager.Instance.PlaySFX(Sfx.kitchenTimer);
        });

        // 4번 돈다
        sequence.Append(pizzaBoxAnimator.transform.DOLocalRotate(new Vector3(0, 360, 0), 0.25f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear));
        sequence.AppendCallback(() =>
        {
            foreach (var temp in ingredients)
            {
                temp.Value.gameObject.SetActive(false);
            }
        });
        sequence.Append(pizzaBoxAnimator.transform.DOLocalRotate(new Vector3(0, 360, 0), 0.25f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear));
        sequence.Append(pizzaBoxAnimator.transform.DOLocalRotate(new Vector3(0, 360, 0), 0.25f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear));
        sequence.AppendCallback(() =>
        {
            AudioManager.Instance.PlaySFX(Sfx.complete);
            pizzaBoxAnimator.Play("Opening");
            pizzaEffect.localScale = Vector3.one;
            pizzaEffect.gameObject.SetActive(true);
        });
        sequence.Append(pizzaBoxAnimator.transform.DOLocalRotate(new Vector3(0, 360, 0), 0.25f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear));
        sequence.Join(pizzaEffect.DOScale(5f, 0.25f).SetEase(Ease.InBack));

        // 빵빠레 효과 - 완성
        sequence.AppendCallback(() =>
        {
            pizza.gameObject.SetActive(true);
        });

        sequence.Append(pizzaBoxAnimator.transform.DOPunchScale(new Vector3(1f, 0.05f, 0.05f), 0.25f));
        sequence.Join(pizzaEffect.DOPunchScale(new Vector3(1f, 0.05f, 0.05f), 0.25f));

        // 오른쪽으로 옮기기
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            pizzaBoxAnimator.Play("Closing");
        });
        sequence.AppendInterval(0.25f);
        sequence.AppendCallback(() =>
        {
            pizza.gameObject.SetActive(false);
            pizzaEffect.gameObject.SetActive(false);
        });
        sequence.Append(pizzaBoxAnimator.transform.DOMove(stackTarget22.position, 0.5f).SetEase(Ease.OutQuad));
        sequence.Join(parentPanel.DOFade(1f, 0.5f));
        sequence.AppendCallback(() =>
        {
            pizzaBoxAnimator.gameObject.SetActive(false);

            parentPanel.alpha = 1f;
            parentPanel.interactable = true;
            UIManager.Instance.isDirecting = false;

            if (PizzaCompleteEvent != null)
                PizzaCompleteEvent(this, null);
            info = null;
        });

        sequence.Pause();
    }

    private void ResetPos()
    {
        Vector3 tempPos2 = stackTarget11.position;
        tempPos2.z = 0.86f;
        pizzaEffect.transform.position = tempPos2;
        tempPos2.z = 0f;
        pizzaBoxAnimator.transform.position = tempPos2;
    }

    [ContextMenu("재시작")]
    public void RestartSequence_Debug()
    {
        if (info != null)
        {
            HashSet<Ingredient> ingredientList = new HashSet<Ingredient>();
            for (int i = 0; i < info.pizzas.Count; i++)
            {
                foreach (var temp in info.pizzas[i].ingredients)
                {
                    if (!ingredientList.Contains(temp.Key))
                        ingredientList.Add(temp.Key);
                }
            }
            int count = 0;
            foreach (var temp in ingredients)
            {
                temp.Value.position = ingredientsRandomPos[count].position;
                count++;
                if (count >= ingredientsRandomPos.Count)
                    count = 0;
                temp.Value.localScale = Vector3.one;

                temp.Value.SetParent(initParent);
                if (ingredientList.Contains(temp.Key))
                    temp.Value.gameObject.SetActive(true);
                else
                    temp.Value.gameObject.SetActive(false);
            }
        }
        else
        {
            int count = 0;
            foreach (var temp in ingredients)
            {
                temp.Value.position = ingredientsRandomPos[count].position;
                count++;
                if (count >= ingredientsRandomPos.Count)
                    count = 0;
                temp.Value.localScale = Vector3.one;

                temp.Value.SetParent(initParent);
                temp.Value.gameObject.SetActive(true);
            }
        }

        int random = UnityEngine.Random.Range(0, pizzaModels.Count);
        pizza.GetComponent<MeshFilter>().mesh = pizzaModels[random].mesh;
        pizza.GetComponent<MeshRenderer>().material = pizzaModels[random].material;

        sequence.Restart();
    }

    public void StopSequence()
    {
        if (sequence.IsPlaying())
        {
            transform.localScale = Vector3.zero;
        }
    }

    public void RestartSequence(OrderInfo info)
    {
        if (info == null) return;

        if (sequence.IsPlaying())
        {
            if (PizzaCompleteEvent != null)
                PizzaCompleteEvent(this, info);
        }

        this.info = info;

        HashSet<Ingredient> ingredientList = new HashSet<Ingredient>();
        for (int i = 0; i < info.pizzas.Count; i++)
        {
            foreach (var temp in info.pizzas[i].ingredients)
            {
                if (!ingredientList.Contains(temp.Key))
                    ingredientList.Add(temp.Key);
            }
        }

        int count = 0;
        foreach (var temp in ingredients)
        {
            temp.Value.position = ingredientsRandomPos[count].position;
            count++;
            if (count >= ingredientsRandomPos.Count)
                count = 0;
            temp.Value.localScale = Vector3.one;

            temp.Value.SetParent(initParent);
            if (ingredientList.Contains(temp.Key))
                temp.Value.gameObject.SetActive(true);
            else
                temp.Value.gameObject.SetActive(false);
        }

        int random = UnityEngine.Random.Range(0, pizzaModels.Count);
        pizza.GetComponent<MeshFilter>().mesh = pizzaModels[random].mesh;
        pizza.GetComponent<MeshRenderer>().material = pizzaModels[random].material;

        sequence.Restart();
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.G))
    //    {
    //        RestartSequence();
    //    }
    //}

    //[Header("동영상 연출용")]
    //[SerializeField] private GameObject[] upgradeObjects;
    //[SerializeField] private int curretUpgrade;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.H))
    //    {
    //        if (curretUpgrade >= upgradeObjects.Length)
    //            curretUpgrade = 0;

    //        for (int i = 0; i < upgradeObjects.Length;i ++)
    //        {
    //            upgradeObjects[i].SetActive(false);
    //        }
    //        upgradeObjects[curretUpgrade++].SetActive(true);
    //    }
    //}

}
