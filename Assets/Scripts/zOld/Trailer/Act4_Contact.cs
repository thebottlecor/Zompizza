using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Act4_Contact : MonoBehaviour
{

    public PlayerController player;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Direction();
        }
    }

    public void Direction()
    {
        player.ShakeOffAllZombies();
        player.dirftContactBlockTimer = 2f;
    }
}
