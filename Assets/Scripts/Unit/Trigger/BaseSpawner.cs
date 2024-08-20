using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSpawner : MonoBehaviour
{
    public bool triggered;
    public BoxCollider spawnPos;

    public virtual void ResetStat()
    {
        triggered = false;
    }
}
