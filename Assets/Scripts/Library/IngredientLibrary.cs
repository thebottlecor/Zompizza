using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;

[CreateAssetMenu(fileName = "IngredientLibrary", menuName = "Library/Ingredient")]
public class IngredientLibrary : ScriptableObject
{

    [Header("자원 분류")]
    public SerializableDictionary<Ingredient, bool> meats;
    public SerializableDictionary<Ingredient, bool> vegetables;
    public SerializableDictionary<Ingredient, bool> herbs;

}
