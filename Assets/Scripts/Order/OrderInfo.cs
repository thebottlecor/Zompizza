using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public float km;

    // ��� ������ �޴� �ݾ�
    public int rewards;

    // ���� ���� Hp �ۼ�Ʈ (�ּ� 0 �ִ� 1)
    public float hp;

    // ��� ���� �ð�
    public float timeLimit;
    // ��� ������ ������ �ð�
    public float timer;

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
