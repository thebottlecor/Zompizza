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

    public void Debug2()
    {
        meats = new SerializableDictionary<Ingredient, bool>();
        vegetables = new SerializableDictionary<Ingredient, bool>();
        herbs = new SerializableDictionary<Ingredient, bool>();
        for (int i = 0; i < 14; i++)
        {
            meats.Add(new SerializableDictionary<Ingredient, bool>.Pair { Key = (Ingredient)i, Value = false });
        }
        for (int i = 14; i < 28; i++)
        {
            vegetables.Add(new SerializableDictionary<Ingredient, bool>.Pair { Key = (Ingredient)i, Value = false });
        }
        for (int i = 28; i < 42; i++)
        {
            herbs.Add(new SerializableDictionary<Ingredient, bool>.Pair { Key = (Ingredient)i, Value = false });
        }
    }

}
