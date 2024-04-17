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

    // 자원 생산 소비 통계 기준년
    public const int resourceChartYears = 10;

    // 하루 기본 노동시간
    public const float workTime = yearTime - 0.01f;

    // 음식을 골고루 먹기 위한 최대 먹은 것 기록 년 수 => 총 음식 종류와 같거나 커야 효과가 좋음
    public const int eatenFoodsChartYears = 5;


    // 애니메이션 프레임 레이트
    public const float animationFrameRate = 6f;
    // 프레임 1장당 걸리는 시간
    public const float animationFrameTime = 1 / animationFrameRate;
    public const int animationFrameTimeMilSec = (int)(animationFrameTime * 1000);

    // Gold 운반시 운반량에 곱해지는 배율
    public const int haulingGoldMultiple = 10;
    // 기본 유닛 운반량
    public const int haulingMax = 5;
    // 시장 인부 운반량
    public const int haulingMax_Market = 20;
    // 세대주 운반량
    public const int haulingMax_House = 10;
    // 건설자 운반량
    public const int haulingMax_Construction = 10;
    // 건설 중 바닥 점유 아이템 기본 운반량
    public const int haulingMax_Cleaning = 20;

    // 기본 건물이 이속에 지장을 주는 정도
    //public const float buildingInterruptingMoveSpeed = -0.25f;

    // 마력 공급시 증가하는 효율
    public const float poweredAddEfficiency = 1f;

    // 건물 작업 중 위치하게 되는 Local 좌표
    public readonly static Vector3 targetLocalPos = new Vector3(0f, -0.4f, 0f);
    // 건물 스프라이트에 가려지는 Local Y 좌표 (높을수록 기준이 밑으로 감)
    public const float buildingSortingY = 0.25f;

    // 일반적인 화재 지속 시간
    //public const float NormalFireLifetime = 20f;
    // 불은 건물을 다 태우기 전까지 절대 꺼지지 않음
    // 초당 화재 데미지 (건물 체력 10 기준) 40초 버팀
    public const float fireDamage = 0.25f;

    // 불의 체력 (우물에서 물을 길러 소방 작업을 하면, 초당 감소)
    public const float fireHp = 10f;
    // 불이 붙은 후, 몇초 지나면 다시 붙을 수 있는가?
    public const float fireBlockTime = 90f;
    // 소방관 진화 데미지
    public const float fireFighterDamage = 1f;

    // 화재 발생 확률 (info.fireOccurChance가 1이면, 10년에 1번쯤 발생한다는 의미)
    public const float fireOccurChance = yearTime * 12f;
    // 바람 부는 날 화재 발생률
    public const float fireOccurChance_windy = yearTime * 4f;
    // 가뭄 화재 발생률
    public const float fireOccurChance_drought = yearTime * 9f;

    // 초기 화재가 절대 일어나지 않는 보호 기간
    public const int fireBlockYear = 10;

    // 건물 자동 수복
    public const float buildingSelfHeal = 10f / (yearTime * 0.5f);

    // 일반적인 가뭄 지속 기간
    public const float NormalDroughtDuration = yearTime * 1.5f;
    // 가뭄 데미지 (작물 hp1, 과수 hp10)
    public const float droughtDamage = 9.8f / NormalDroughtDuration;

    // 일반적인 비 지속 기간
    public const float NormalRainyDuration = yearTime * 0.6f;
    // 일반적인 바람 지속 기간
    public const float NormalWindyDuration = yearTime * 0.7f;

    // 날씨 및 질병 정해지는 길이 (년)
    public const int weatherLength = 10;
    // 상인 방문 정해지는 길이 (년)
    public const int traderLength = 12;
    // 정기 무역선 (기본 물품) 상인 방문 간격
    public const int commonTraderArriveInterval = 4;

    // 길찾기 구역 분할하는 거리 기준
    public const int pathfindingSectionDist = 9;


    // 시민 예상 사망 나이
    public const int baseDeathAge = 59;
    // 시민 평균 수명 (예상 사망 나이 59 - 생성 평균 나이 29 = 30)
    public const int baseLifespan = 30;
    // 예상 수명 (59) 이전 사망률
    public const float baseDeathrate = 0.003f;
    // 예상 수명 이후 기본 사망률
    public const float overDeathrate = 0.91f;
    // 예상 수명 이후 나이당 증가 사망률
    public const float overoverDeathrate = 0.015f;

    // 기본 건강
    public const float baseHealth = 50f;

    // 기본 행복
    public const float baseHappiness = 50f;
    // 낮은 행복치
    public const float lowerHappiness = 10f;
    // 높은 행복치
    public const float higherHappiness = 100f;
    // 빈 주거 공간에 따른 최대 이민
    public const int maxImmigrantsByHousingSpace = 3;

    // 약한 질병 (5년 방치시 사망)
    public const float weakSickIntensity = 1f / (5f * yearTime);
    // 일반적인 질병 SickIntensity (3년 방치시 사망)
    public const float normalSickIntensity = 1f / (3f * yearTime);
    // 급성 질병 (1년 방치시 사망)
    public const float hardSickIntensity = 1f / yearTime;
    // 회복 속도
    public const float hpRegen = 1f / yearTime;

    // 질병에 절대 걸리지 않는 기간 (게임 시작해서)
    public const int diseaseBlockAge = 10;

    // 치료 속도 (일반적인 질병 1/10년 치료시 완치)
    // 급성 질병은 3배의 시간이 필요 // 약한 질병은 필요 시간이 (1/5) 수준
    public const float normalHeal = normalSickIntensity * (10f / yearTime);

    // 길드 최대 교육 기간
    public const int guildAgeLimit = 2;


    // 소비세율에 따른 사치품 가치 평가
    public const float exciseDuty_Normal = 1.2f;
    public const float exciseDuty_Low = 0.75f;
    public const float exciseDuty_High = 1.5f;
}
