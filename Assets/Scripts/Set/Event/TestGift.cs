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

    public float 목표높이 = 1f;
    public float 최고높이 = 5f;

    public Transform 처음위치;
    public Transform 타겟;

    public Ease Y이지 = Ease.OutQuad;
    public Ease Y이지2 = Ease.OutQuad;
    public Ease XZ이지 = Ease.InQuad;

    public float XZ이동시간 = 0.75f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            GiftBoxEffect();
        }

    }


    [ContextMenu("테스트")]
    public void GiftBoxEffect()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetAutoKill(true);
        sequence.AppendCallback(() =>
        {
            Vector3 pos = 처음위치.position;
            pos.y = 목표높이;
            transform.position = pos;

            transform.DOMoveX(타겟.position.x, XZ이동시간).SetEase(XZ이지);
            transform.DOMoveZ(타겟.position.z, XZ이동시간).SetEase(XZ이지);

            transform.DOMoveY(최고높이, XZ이동시간 * 0.5f).SetEase(Y이지);
        });
        sequence.AppendInterval(XZ이동시간 * 0.5f);
        sequence.AppendCallback(() =>
        {
            transform.DOMoveY(목표높이, XZ이동시간 * 0.5f).SetEase(Y이지2);
        });
    }

}
