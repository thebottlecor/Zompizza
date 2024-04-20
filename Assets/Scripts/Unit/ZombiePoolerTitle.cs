using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombiePoolerTitle : MonoBehaviour
{

    [Header("���� ��")]
    public ZombieModel[] models_Info;
    [System.Serializable]
    public struct ZombieModel
    {
        public Material material;
        public Mesh mesh;
    }

}
