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
    meat4,
    meat5,
    meat6,
    meat7,
    meat8,
    meat9,
    meat10,
    meat11,
    meat12,
    meat13,
    meat14,

    vegetable1,
    vegetable2,
    vegetable3,
    vegetable4,
    vegetable5,
    vegetable6,
    vegetable7,
    vegetable8,
    vegetable9,
    vegetable10,
    vegetable11,
    vegetable12,
    vegetable13,
    vegetable14,

    herb1,
    herb2,
    herb3,
    herb4,
    herb5,
    herb6,
    herb7,
    herb8,
    herb9,
    herb10,
    herb11,
    herb12,
    herb13,
    herb14,
}
