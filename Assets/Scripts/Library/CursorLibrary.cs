using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CreateAssetMenu(fileName = "CursorLibrary", menuName = "Library/Cursor")]
public class CursorLibrary : ScriptableObject
{

    public Texture2D normal;
    public Texture2D demolish;
    public Texture2D removeNaturalObject;
    public Texture2D priorityOverlay;
    public Texture2D upgrade;

}
