using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{

    public UILibrary uiLibrary;
    public MaterialLibrary materialLibrary;
    public CursorLibrary cursorLibrary;
    public EffectLibrary effectLibrary;

    protected override void Awake()
    {
        base.Awake();

    }

    private void Start()
    {
        DOTween.Init();
    }

    public override void CallAfterAwake()
    {

    }
    public override void CallAfterStart(ConfigData config)
    {

    }

}
