using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PizzaDirection : MonoBehaviour
{

    public CanvasGroup parentPanel;

    public Transform initParent;

    public Animator pizzaBoxAnimator;
    


    public Transform pizza;
    public Transform pizzaEffect;

    public Transform stackTarget;
    public Transform stackTarget2;

    public float standard_Xpos_1920x1080 = 7.4f;

    private Sequence sequence;
    private OrderInfo info;

    public SerializableDictionary<Ingredient, Transform> ingredients;
    public GameObject ingredientsSource;
    public List<Transform> ingredientsRandomPos;

    public static EventHandler<OrderInfo> PizzaCompleteEvent;

    void Start()
    {
        // �̰� ������ ��ó�� �ʱ�ȭ
        ingredients = new SerializableDictionary<Ingredient, Transform>();
        var list = Enum.GetValues(typeof(Ingredient));
        int count = 0;
        foreach (var temp in list)
        {
            Ingredient key = (Ingredient)temp;
            var obj = Instantiate(ingredientsSource);
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = DataManager.Instance.uiLibrary.ingredients[key];
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

    }

    public void Init()
    {
        // �ػ󵵰� ����� �� �Լ��� �ҷ��ͼ� �������� ���� ������ ��

        // stackTarget ������ ������Ʈ
        float adjust_Xpos = standard_Xpos_1920x1080 * ((float)Screen.width / Screen.height) * (9f / 16f);

        Vector3 pos1 = stackTarget.localPosition;
        pos1.x = adjust_Xpos;
        stackTarget.localPosition = pos1;

        Vector3 pos2 = stackTarget2.localPosition;
        pos2.x = adjust_Xpos;
        stackTarget2.localPosition = pos2;

        gameObject.SetActive(false);
        gameObject.SetActive(true);


        sequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true);
        // ���� ���·� �ʱ�ȭ
        sequence.AppendCallback(() =>
        {
            UIManager.Instance.isDirecting = true;
            parentPanel.alpha = 1f;
            parentPanel.interactable = false;

            pizzaBoxAnimator.gameObject.SetActive(true);
            pizzaBoxAnimator.Play("Closed");
            pizza.gameObject.SetActive(false);
            pizzaEffect.gameObject.SetActive(false);
        });
        // ������
        sequence.Append(parentPanel.DOFade(0.1f, 0.5f));
        //sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            pizzaBoxAnimator.Play("Opening");
        });
        sequence.AppendInterval(0.25f);

        // ������ ����
        sequence.Append(ingredients[0].DOMoveX(pizzaBoxAnimator.transform.position.x, 0.75f).SetEase(Ease.OutQuad));
        sequence.Join(ingredients[0].DOMoveY(pizzaBoxAnimator.transform.position.y + 0.15f, 0.75f).SetEase(Ease.InQuad));
        sequence.Join(ingredients[0].DOScale(0.4f, 0.75f).SetEase(Ease.OutQuint));

        foreach (var temp in ingredients)
        {
            sequence.Join(temp.Value.DOMoveX(pizzaBoxAnimator.transform.position.x, 0.75f).SetEase(Ease.OutQuad));
            sequence.Join(temp.Value.DOMoveY(pizzaBoxAnimator.transform.position.y + 0.15f, 0.75f).SetEase(Ease.InQuad));
            sequence.Join(temp.Value.DOScale(0.4f, 0.75f).SetEase(Ease.OutQuint));
        }

        sequence.AppendCallback(() =>
        {
            foreach (var temp in ingredients)
            {
                temp.Value.SetParent(pizzaBoxAnimator.transform);
            }
            pizzaBoxAnimator.Play("Closing");
        });

        // 4�� ����
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
            pizzaBoxAnimator.Play("Opening");
            pizzaEffect.localScale = Vector3.one;
            pizzaEffect.gameObject.SetActive(true);
        });
        sequence.Append(pizzaBoxAnimator.transform.DOLocalRotate(new Vector3(0, 360, 0), 0.25f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear));
        sequence.Join(pizzaEffect.DOScale(5f, 0.25f).SetEase(Ease.InBounce));

        // ������ ȿ�� - �ϼ�
        sequence.AppendCallback(() =>
        {
            pizza.gameObject.SetActive(true);
        });

        // ���������� �ű��
        sequence.AppendInterval(0.75f);
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
        sequence.Append(pizzaBoxAnimator.transform.DOMove(stackTarget.position, 0.5f));
        sequence.Append(pizzaBoxAnimator.transform.DOMove(stackTarget2.position, 0.5f).SetEase(Ease.OutQuad));
        sequence.Join(parentPanel.DOFade(1f, 0.5f));
        sequence.AppendCallback(() =>
        {
            pizzaBoxAnimator.gameObject.SetActive(false);

            parentPanel.alpha = 1f;
            parentPanel.interactable = true;
            UIManager.Instance.isDirecting = false;

            if (info != null)
            {
                if (PizzaCompleteEvent != null)
                    PizzaCompleteEvent(this, info);
                info = null;
            }
        });

        sequence.Pause();
    }

    [ContextMenu("�����")]
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
            foreach (var temp in ingredients)
            {
                temp.Value.SetParent(initParent);
                if (ingredientList.Contains(temp.Key))
                    temp.Value.gameObject.SetActive(true);
                else
                    temp.Value.gameObject.SetActive(false);
            }
        }

        sequence.Restart();
    }

    public void RestartSequence(OrderInfo info)
    {
        if (info == null) return;

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
        foreach (var temp in ingredients)
        {
            temp.Value.SetParent(initParent);
            if (ingredientList.Contains(temp.Key))
                temp.Value.gameObject.SetActive(true);
            else
                temp.Value.gameObject.SetActive(false);
        }

        sequence.Restart();
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.G))
    //    {
    //        RestartSequence();
    //    }
    //}

}
