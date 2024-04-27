using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;

[CreateAssetMenu(fileName = "UILibrary", menuName = "Library/UI")]
public class UILibrary : ScriptableObject
{

    public Sprite shopButton;
    public Sprite shopButtonHighlighted;


    public Sprite[] customerProfile;

    public SerializableDictionary<Ingredient, Sprite> ingredients;

}
