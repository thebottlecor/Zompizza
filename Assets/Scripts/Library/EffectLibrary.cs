using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;

[CreateAssetMenu(fileName = "EffectLibrary", menuName = "Library/Effect")]
public class EffectLibrary : ScriptableObject
{

    public GameObject dollarBoomEffect;

    public GameObject goldCoinBlastEffect;

    public GameObject appleBlastEffect;

    public GameObject explodeEffect;

    public GameObject[] hitEffects;

}
