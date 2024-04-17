using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "EditorSettingLib", menuName = "Library/EditorSettingLib")]
public class EditorSettingLib : ScriptableObject
{

    public bool forceStart = true;

    public bool demoVersion;

}
