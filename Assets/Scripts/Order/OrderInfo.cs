using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrderInfo
{

    // �ֹ��� �³��ߴ���?
    public bool accepted;

    // �ֹ��� �ε���
    public int customerIdx;

    // �ֹ� ��� ����� �ε��� ���� (OrderManager ����)
    public int goal;

    // ��� ������ �޴� �ݾ�
    public int rewards;

    // ��� ���� �ð�
    public float timeLimit;

}
