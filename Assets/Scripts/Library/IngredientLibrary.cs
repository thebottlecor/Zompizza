using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;

[CreateAssetMenu(fileName = "IngredientLibrary", menuName = "Library/Ingredient")]
public class IngredientLibrary : ScriptableObject
{

    [Serializable]
    public struct Info
    {
        public bool valid;
        public int tier;
    }

    [Header("자원 분류")]
    public SerializableDictionary<Ingredient, Info> meats;
    public SerializableDictionary<Ingredient, Info> vegetables;
    public SerializableDictionary<Ingredient, Info> herbs;

    public Array ingredientTypes;

    public void SetArray()
    {
        ingredientTypes = Enum.GetValues(typeof(Ingredient));
    }

    //public void Debug2()
    //{
    //    meats = new SerializableDictionary<Ingredient, Info>();
    //    vegetables = new SerializableDictionary<Ingredient, Info>();
    //    herbs = new SerializableDictionary<Ingredient, Info>();
    //    for (int i = 0; i < 12; i++)
    //    {
    //        meats.Add(new SerializableDictionary<Ingredient, Info>.Pair { Key = (Ingredient)i, Value = new Info { tier = i / 2, valid = false } });
    //    }
    //    for (int i = 12; i < 24; i++)
    //    {
    //        vegetables.Add(new SerializableDictionary<Ingredient, Info>.Pair { Key = (Ingredient)i, Value = new Info { tier = (i-12) / 2, valid = false } });
    //    }
    //    for (int i = 24; i < 36; i++)
    //    {
    //        herbs.Add(new SerializableDictionary<Ingredient, Info>.Pair { Key = (Ingredient)i, Value = new Info { tier = (i-24) / 2, valid = false } });
    //    }
    //}

}
