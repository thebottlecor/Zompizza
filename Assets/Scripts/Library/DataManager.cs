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

        //uiLib.padKeyUIsXbox = new SerializableDictionary<PadKeyCode, Sprite>();
        //uiLib.padKeyUIsPS = new SerializableDictionary<PadKeyCode, Sprite>();
        //foreach (var tt in uiLib.padKeyUIs)
        //{
        //    SerializableDictionary<PadKeyCode, Sprite>.Pair tqq = new SerializableDictionary<PadKeyCode, Sprite>.Pair(tt.Key, tt.Value);

        //    uiLib.padKeyUIsXbox.Add(tqq);
        //    uiLib.padKeyUIsPS.Add(tqq);
        //}
    }

    private void Start()
    {
        researches = researchLib.GetHashMap();
        DOTween.Init();

        ingredientLib.SetArray();

        //ingredientLib.Debug2();
    }

    public override void CallAfterAwake()
    {

    }
    public override void CallAfterStart(ConfigData config)
    {

    }

}
