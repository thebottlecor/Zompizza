using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyDirection : MonoBehaviour
{
    public Transform initParent;
    public Transform stackTarget;

    public float standard_Xpos_1920x1080 = 7.4f;

    private Sequence sequence;

    public GameObject ingredientsSource;
    public GameObject ingredientsSource2;
    public int max;
    public int max2;
    public List<Transform> ingredientsRandomPos;
    public List<Transform> ingredientsRandomPos2;

    private List<Transform> ii;
    private List<Transform> ii2;


    void Start()
    {
        ii = new List<Transform>();
        ii2 = new List<Transform>();

        for (int i = 0; i < max; i++)
        {
            var obj = Instantiate(ingredientsSource);
            obj.transform.position = ingredientsRandomPos[i].position;
            obj.transform.SetParent(initParent);
            ii.Add(obj.transform);
            obj.SetActive(false);
        }
        for (int i = 0; i < max2; i++)
        {
            var obj = Instantiate(ingredientsSource2);
            obj.transform.position = ingredientsRandomPos2[i].position;
            obj.transform.SetParent(initParent);
            ii2.Add(obj.transform);
            obj.SetActive(false);
        }

        Init();
    }

    public void Init()
    {
        // 해상도가 변경될 때 함수를 불러와서 시퀸스를 새로 만들어야 함

        // stackTarget 포지션 업데이트
        float adjust_Xpos = standard_Xpos_1920x1080 * ((float)Screen.width / Screen.height) * (9f / 16f);

        Vector3 pos1 = stackTarget.localPosition;
        pos1.x = adjust_Xpos;
        stackTarget.localPosition = pos1;

        //gameObject.SetActive(false);
        //gameObject.SetActive(true);


        sequence = DOTween.Sequence().SetAutoKill(false).SetUpdate(true);
        // 닫힌 상태로 초기화
        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < ii.Count; i++)
            {
                ii[i].gameObject.SetActive(true);
                ii[i].position = ingredientsRandomPos[i].position;
                ii[i].localScale = Vector3.one * 0.5f;
            }
            for (int i = 0; i < ii2.Count; i++)
            {
                ii2[i].gameObject.SetActive(true);
                ii2[i].position = ingredientsRandomPos2[i].position;
                ii2[i].localScale = Vector3.one * 0.5f;
            }
            transform.localScale = Vector3.one;
        });

        //sequence.Append(this.transform.DOPunchScale(new Vector3(0.1f, 1f, 0.1f), 0.5f));
        float time = 0.25f;

        for (int i = 0; i < ii.Count; i++)
        {
            sequence.Join(ii[i].DOLocalMoveX(stackTarget.transform.localPosition.x, time).SetEase(Ease.OutQuad));
            sequence.Join(ii[i].DOLocalMoveY(stackTarget.transform.localPosition.y, time).SetEase(Ease.InQuad));
            sequence.Join(ii[i].DOScale(0.2f, time).SetEase(Ease.OutQuint));
        }
        for (int i = 0; i < ii2.Count; i++)
        {
            sequence.Join(ii2[i].DOLocalMoveX(stackTarget.transform.localPosition.x, time).SetEase(Ease.OutQuad));
            sequence.Join(ii2[i].DOLocalMoveY(stackTarget.transform.localPosition.y, time).SetEase(Ease.InQuad));
            sequence.Join(ii2[i].DOScale(0.2f, time).SetEase(Ease.OutQuint));
        }

        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < ii.Count; i++)
            {
                ii[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < ii2.Count; i++)
            {
                ii2[i].gameObject.SetActive(false);
            }
        });

        sequence.Pause();
    }

    [ContextMenu("재시작")]
    public void RestartSequence_Debug()
    {
        sequence.Restart();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RestartSequence_Debug();
        }
    }
}
