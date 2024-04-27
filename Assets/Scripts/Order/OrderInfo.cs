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

    // �ֹ��� ����
    public List<PizzaInfo> pizzas;

    // �ֹ� ��� ����� �ε��� ���� (OrderManager ����)
    public int goal;
    // �ֹ� ��� ��ҿ��� �Ÿ�
    public float distance;

    // ��� ������ �޴� �ݾ�
    public int rewards;

    // ��� ���� �ð�
    public float timeLimit;

}

[System.Serializable]
public struct PizzaInfo
{
    // �ʿ��� ���
    public SerializableDictionary<Ingredient, int> ingredients;
}

public enum Ingredient
{
    meat1,
    meat2,
    meat3,

    vegetable1,
    vegetable2,
    vegetable3,

    herb1,
    herb2,
    herb3,
}
