using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{

    public IngredientLibrary ingredientLib;
    public UILibrary uiLib;
    public MaterialLibrary materialLib;
    public CursorLibrary cursorLib;
    public EffectLibrary effectLib;

    public ResearchLibrary researchLib;
    public Dictionary<int, ResearchInfo> researches;

    protected override void Awake()
    {
        base.Awake();

    }

    private void Start()
    {
        researches = researchLib.GetHashMap();
        DOTween.Init();
    }

    public override void CallAfterAwake()
    {

    }
    public override void CallAfterStart(ConfigData config)
    {

    }

}
