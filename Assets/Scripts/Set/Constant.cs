using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constant
{
    public const int startingMember = 3;
    public const int startingMember0 = 1;
    public const int startingMember2 = 10;
    public const int startingGolems = 0;

    public const float yearTime = 180f;

    // �ڿ� ���� �Һ� ��� ���س�
    public const int resourceChartYears = 10;

    // �Ϸ� �⺻ �뵿�ð�
    public const float workTime = yearTime - 0.01f;

    // ������ ���� �Ա� ���� �ִ� ���� �� ��� �� �� => �� ���� ������ ���ų� Ŀ�� ȿ���� ����
    public const int eatenFoodsChartYears = 5;


    // �ִϸ��̼� ������ ����Ʈ
    public const float animationFrameRate = 6f;
    // ������ 1��� �ɸ��� �ð�
    public const float animationFrameTime = 1 / animationFrameRate;
    public const int animationFrameTimeMilSec = (int)(animationFrameTime * 1000);

    // Gold ��ݽ� ��ݷ��� �������� ����
    public const int haulingGoldMultiple = 10;
    // �⺻ ���� ��ݷ�
    public const int haulingMax = 5;
    // ���� �κ� ��ݷ�
    public const int haulingMax_Market = 20;
    // ������ ��ݷ�
    public const int haulingMax_House = 10;
    // �Ǽ��� ��ݷ�
    public const int haulingMax_Construction = 10;
    // �Ǽ� �� �ٴ� ���� ������ �⺻ ��ݷ�
    public const int haulingMax_Cleaning = 20;

    // �⺻ �ǹ��� �̼ӿ� ������ �ִ� ����
    //public const float buildingInterruptingMoveSpeed = -0.25f;

    // ���� ���޽� �����ϴ� ȿ��
    public const float poweredAddEfficiency = 1f;

    // �ǹ� �۾� �� ��ġ�ϰ� �Ǵ� Local ��ǥ
    public readonly static Vector3 targetLocalPos = new Vector3(0f, -0.4f, 0f);
    // �ǹ� ��������Ʈ�� �������� Local Y ��ǥ (�������� ������ ������ ��)
    public const float buildingSortingY = 0.25f;

    // �Ϲ����� ȭ�� ���� �ð�
    //public const float NormalFireLifetime = 20f;
    // ���� �ǹ��� �� �¿�� ������ ���� ������ ����
    // �ʴ� ȭ�� ������ (�ǹ� ü�� 10 ����) 40�� ����
    public const float fireDamage = 0.25f;

    // ���� ü�� (�칰���� ���� �淯 �ҹ� �۾��� �ϸ�, �ʴ� ����)
    public const float fireHp = 10f;
    // ���� ���� ��, ���� ������ �ٽ� ���� �� �ִ°�?
    public const float fireBlockTime = 90f;
    // �ҹ�� ��ȭ ������
    public const float fireFighterDamage = 1f;

    // ȭ�� �߻� Ȯ�� (info.fireOccurChance�� 1�̸�, 10�⿡ 1���� �߻��Ѵٴ� �ǹ�)
    public const float fireOccurChance = yearTime * 12f;
    // �ٶ� �δ� �� ȭ�� �߻���
    public const float fireOccurChance_windy = yearTime * 4f;
    // ���� ȭ�� �߻���
    public const float fireOccurChance_drought = yearTime * 9f;

    // �ʱ� ȭ�簡 ���� �Ͼ�� �ʴ� ��ȣ �Ⱓ
    public const int fireBlockYear = 10;

    // �ǹ� �ڵ� ����
    public const float buildingSelfHeal = 10f / (yearTime * 0.5f);

    // �Ϲ����� ���� ���� �Ⱓ
    public const float NormalDroughtDuration = yearTime * 1.5f;
    // ���� ������ (�۹� hp1, ���� hp10)
    public const float droughtDamage = 9.8f / NormalDroughtDuration;

    // �Ϲ����� �� ���� �Ⱓ
    public const float NormalRainyDuration = yearTime * 0.6f;
    // �Ϲ����� �ٶ� ���� �Ⱓ
    public const float NormalWindyDuration = yearTime * 0.7f;

    // ���� �� ���� �������� ���� (��)
    public const int weatherLength = 10;
    // ���� �湮 �������� ���� (��)
    public const int traderLength = 12;
    // ���� ������ (�⺻ ��ǰ) ���� �湮 ����
    public const int commonTraderArriveInterval = 4;

    // ��ã�� ���� �����ϴ� �Ÿ� ����
    public const int pathfindingSectionDist = 9;


    // �ù� ���� ��� ����
    public const int baseDeathAge = 59;
    // �ù� ��� ���� (���� ��� ���� 59 - ���� ��� ���� 29 = 30)
    public const int baseLifespan = 30;
    // ���� ���� (59) ���� �����
    public const float baseDeathrate = 0.003f;
    // ���� ���� ���� �⺻ �����
    public const float overDeathrate = 0.91f;
    // ���� ���� ���� ���̴� ���� �����
    public const float overoverDeathrate = 0.015f;

    // �⺻ �ǰ�
    public const float baseHealth = 50f;

    // �⺻ �ູ
    public const float baseHappiness = 50f;
    // ���� �ູġ
    public const float lowerHappiness = 10f;
    // ���� �ູġ
    public const float higherHappiness = 100f;
    // �� �ְ� ������ ���� �ִ� �̹�
    public const int maxImmigrantsByHousingSpace = 3;

    // ���� ���� (5�� ��ġ�� ���)
    public const float weakSickIntensity = 1f / (5f * yearTime);
    // �Ϲ����� ���� SickIntensity (3�� ��ġ�� ���)
    public const float normalSickIntensity = 1f / (3f * yearTime);
    // �޼� ���� (1�� ��ġ�� ���)
    public const float hardSickIntensity = 1f / yearTime;
    // ȸ�� �ӵ�
    public const float hpRegen = 1f / yearTime;

    // ������ ���� �ɸ��� �ʴ� �Ⱓ (���� �����ؼ�)
    public const int diseaseBlockAge = 10;

    // ġ�� �ӵ� (�Ϲ����� ���� 1/10�� ġ��� ��ġ)
    // �޼� ������ 3���� �ð��� �ʿ� // ���� ������ �ʿ� �ð��� (1/5) ����
    public const float normalHeal = normalSickIntensity * (10f / yearTime);

    // ��� �ִ� ���� �Ⱓ
    public const int guildAgeLimit = 2;


    // �Һ����� ���� ��ġǰ ��ġ ��
    public const float exciseDuty_Normal = 1.2f;
    public const float exciseDuty_Low = 0.75f;
    public const float exciseDuty_High = 1.5f;
}
