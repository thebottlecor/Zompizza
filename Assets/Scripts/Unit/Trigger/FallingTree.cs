using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FallingTree : BaseSpawner
{

    private Vector3 initAngle;

    public Transform tree;

    public float fallingTime = 1f;
    public Vector3 targetFallingAngle;

    private void Start()
    {
        initAngle = tree.localEulerAngles;
    }

    public override void ResetStat()
    {
        base.ResetStat();

        tree.localEulerAngles = initAngle;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (GM.Instance.loading) return;

        if (other.gameObject.CompareTag("Player"))
        {
            triggered = true;

            tree.DORotate(targetFallingAngle, fallingTime).SetEase(Ease.InQuart);
            AudioManager.Instance.PlaySFX(Sfx.fallingTree);
        }
    }
}
