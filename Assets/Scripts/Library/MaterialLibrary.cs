using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;

[CreateAssetMenu(fileName = "MaterialLibrary", menuName = "Library/Material")]
public class MaterialLibrary : ScriptableObject
{

    public Material baseMaterial;
    public Material hitMaterial;
    public Material transparentMaterial;

    public Material baseMaterial2;
    public Material transparentMaterial2;

}
