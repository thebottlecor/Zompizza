using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


public enum Language
{
    sc,
    tc,
    en,
    jp,
    kr,
    ru,
    LAST,
}

public class TextManager : Singleton<TextManager>
{

    public Language language;

    public static string Green = "<color=#5AFF5A>";
    public static string SoftGreen = "<color=#81FF81>";
    public static string Orange = "<color=#FFD378>";
    public static string Red = "<color=#FF5A5A>";
    public static string SoftRed = "<color=#FF8B8B>";
    public static string Yellow = "<color=#EBFFA5>";

    Dictionary<Ingredient, Dictionary<string, object>> ingredients;
    public string GetIngredient(Ingredient idx) => ingredients[idx][language.ToString()].ToString();

    Dictionary<int, Dictionary<string, object>> recipes;
    public string GetRecipes(int idx) => recipes[idx][language.ToString()].ToString();

    Dictionary<int, Dictionary<string, object>> researches;
    public string GetResearch(int idx) => researches[idx][language.ToString()].ToString();

    Dictionary<string, Dictionary<string, object>> commons;
    public string GetCommons(string str) => commons[str][language.ToString()].ToString();
    public string GetCommons(string str, Language lan) => commons[str][lan.ToString()].ToString();

    Dictionary<KeyMap, Dictionary<string, object>> keymaps;
    public string GetKeyMaps(KeyMap idx) => keymaps[idx][language.ToString()].ToString();

    Dictionary<KeyCode, Dictionary<string, object>> keycodes;
    public string GetKeyCodes(KeyCode idx) => keycodes[idx]["all"].ToString();
    public bool HasKeyCode(KeyCode idx) => keycodes.ContainsKey(idx);

    Dictionary<KeyCode, Dictionary<string, object>> inputSystems;
    public string GetInputSystems(KeyCode idx) => inputSystems[idx]["all"].ToString();
    public bool HasInputSystems(KeyCode idx) => inputSystems.ContainsKey(idx);

    Dictionary<int, Dictionary<string, object>> characters;
    public string GetNames(int idx) => characters[idx][language.ToString()].ToString();

    Dictionary<int, Dictionary<string, object>> vehicles;
    public string GetVehicles(int idx) => vehicles[idx][language.ToString()].ToString();

    public CultureInfo defaultCultureInfo = new CultureInfo("en-US");

    protected override void Awake()
    {
        base.Awake();
        CallAfterAwake();
    }

    public override void CallAfterAwake()
    {

    }
    public override void CallAfterStart(ConfigData config)
    {
        if (config == null)
        {
            SetFirstLanguage();
        }
        else
        {
            if (System.Enum.TryParse(typeof(Language), config.language, out object result))
            {
                SetLanguage((Language)result);
            }
            else
                SetFirstLanguage();
        }
    }

    private void SetFirstLanguage()
    {
        Language firstLanguage = Language.en;
        var systemLanguage = Application.systemLanguage;
        switch (systemLanguage)
        {
            case SystemLanguage.ChineseTraditional:
                firstLanguage = Language.tc;
                break;
            case SystemLanguage.ChineseSimplified:
                firstLanguage = Language.sc;
                break;
            case SystemLanguage.Chinese:
                firstLanguage = Language.sc;
                break;
            case SystemLanguage.Japanese:
                firstLanguage = Language.jp;
                break;
            case SystemLanguage.Korean:
                firstLanguage = Language.kr;
                break;
            case SystemLanguage.Russian:
                firstLanguage = Language.ru;
                break;
        }
        SetLanguage(firstLanguage);
    }

    public void SetLanguage(Language language)
    {
        this.language = language;

        ingredients = CSVReader.ReadCSV<Ingredient>("TextManager - resource.csv");
        recipes = CSVReader.ReadCSV<int>("TextManager - recipe.csv");
        researches = CSVReader.ReadCSV<int>("TextManager - research.csv");
        commons = CSVReader.ReadCSV<string>("TextManager - common.csv");
        keymaps = CSVReader.ReadCSV<KeyMap>("TextManager - keymap.csv");
        keycodes = CSVReader.ReadCSV<KeyCode>("TextManager - keycode.csv");
        inputSystems = CSVReader.ReadCSV<KeyCode>("TextManager - inputSystem.csv");
        characters = CSVReader.ReadCSV<int>("TextManager - character.csv");
        vehicles = CSVReader.ReadCSV<int>("TextManager - vehicle.csv");

        Lobby.Instance.UpdateTexts();
        SettingManager.Instance.UpdateTexts();
        //SaveManager.Instance.UpdateTexts();
    }
}
