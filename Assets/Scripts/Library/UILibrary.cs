using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;

[CreateAssetMenu(fileName = "UILibrary", menuName = "Library/UI")]
public class UILibrary : ScriptableObject
{

    public Color button_MainColor;
    public Color button_HighlightColor;
    public Color button_subColor;
    public Color button_inactiveColor;

    public Color button_Upgrade;

    public Sprite shopButton;
    public Sprite shopButtonHighlighted;
    public Sprite gold;
    public Sprite[] plus;

    public Color[] customerPinColor;
    public Gradient pizaaHpGradient;

    public Gradient timeLightGradient;
    public Gradient timeLightGradient_Bloodmoon;


    public Sprite[] customerProfile;

    public SerializableDictionary<Ingredient, Sprite> ingredients;

    public Color miniOrderUI_maskColor;
    public Color miniOrderUI_maskColor_angry;

    public Color order_unselect_Color = Color.white;
    public Color order_select_Color;


    //public SerializableDictionary<PadKeyCode, Sprite> padKeyUIs;
    public SerializableDictionary<PadKeyCode, Sprite> padKeyUIsXbox;
    public SerializableDictionary<PadKeyCode, Sprite> padKeyUIsPS;


    [Serializable]
    public struct ResearchLineInfo
    {
        public float thickness;
        public Sprite sprite;
    }
    public List<ResearchLineInfo> researchLineInfos;


    public List<Sprite> spaceshipParts;

    public Sprite[] villagerItems;
    public Sprite[] villagerProfile;
    public Sprite[] villagerSkills;
}
