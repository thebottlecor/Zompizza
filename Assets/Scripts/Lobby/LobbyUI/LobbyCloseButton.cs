using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCloseButton : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    public void Close()
    {
        canvas.gameObject.SetActive(false);
    }


}
