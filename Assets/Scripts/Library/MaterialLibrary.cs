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
    public Material transparentMaterial;

}
