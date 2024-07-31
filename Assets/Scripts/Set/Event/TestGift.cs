using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using MTAssets.EasyMinimapSystem;
using UnityEngine.UI;
using TMPro;

public class TestGift : MonoBehaviour
{

    public float ��ǥ���� = 1f;
    public float �ְ���� = 5f;

    public Transform ó����ġ;
    public Transform Ÿ��;

    public Ease Y���� = Ease.OutQuad;
    public Ease Y����2 = Ease.OutQuad;
    public Ease XZ���� = Ease.InQuad;

    public float XZ�̵��ð� = 0.75f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            GiftBoxEffect();
        }

    }


    [ContextMenu("�׽�Ʈ")]
    public void GiftBoxEffect()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(true);
        sequence.AppendCallback(() =>
        {
            Vector3 pos = ó����ġ.position;
            pos.y = ��ǥ����;
            transform.position = pos;

            transform.DOMoveX(Ÿ��.position.x, XZ�̵��ð�).SetEase(XZ����);
            transform.DOMoveZ(Ÿ��.position.z, XZ�̵��ð�).SetEase(XZ����);

            transform.DOMoveY(�ְ����, XZ�̵��ð� * 0.5f).SetEase(Y����);
        });
        sequence.AppendInterval(XZ�̵��ð� * 0.5f);
        sequence.AppendCallback(() =>
        {
            transform.DOMoveY(��ǥ����, XZ�̵��ð� * 0.5f).SetEase(Y����2);
        });
    }

}
