using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatManager : Singleton<StatManager>
{
    public int acceptedOrders;
    public int completedOrders;

    public int totalEarn;
    public float totalRating;

    public float carMileage;

    private int totalDeliveryOnTime;
    public float averageDeliveryTime;
    private float totalDeliveryHp;
    public float averageDeliveryHp;

    public int foundVisionRecipes;

    private int todayEarn;
    public int highestEarningDay;
    public int highestEarningValue;

    private float todayRating;
    public int highestRatingDay;
    public float highestRatingValue;

    public int carCrash;
    public int hitZombies;

    public TextMeshProUGUI[] statTexts;
    private TextManager tm => TextManager.Instance;

    public void UpdateText()
    {
        int idx = 0;
        statTexts[idx++].text = $"{tm.GetCommons("TotalAcceptedOrders")} : {acceptedOrders}";
        float completePercent = (acceptedOrders > 0 ? ((float)completedOrders / acceptedOrders) * 100f : 0f);
        statTexts[idx++].text = $"{tm.GetCommons("TotalCompletedOrders")} : {completedOrders} ({completePercent:F0}%)";

        statTexts[idx++].text = $"<sprite=2> {tm.GetCommons("TotalSales")} : {totalEarn}$";
        statTexts[idx++].text = $"<sprite=2> {tm.GetCommons("HighestSalesDay")} : {string.Format(tm.GetCommons("Day"), highestEarningDay + 1)}, {highestEarningValue}$";

        statTexts[idx++].text = $"<sprite=1> {tm.GetCommons("TotalRating")} : {totalRating:0.#}";
        statTexts[idx++].text = $"<sprite=1> {tm.GetCommons("HighestRatingDay")} : {string.Format(tm.GetCommons("Day"), highestRatingDay + 1)}, {highestRatingValue:0.#}";

        statTexts[idx++].text = $"<sprite=3> {tm.GetCommons("AverageDeliveryOnTime")} : {averageDeliveryTime * 100f:F0}%";
        statTexts[idx++].text = $"<sprite=0> {tm.GetCommons("AveragePizzaHealth")} : {averageDeliveryHp * 100f:F0}%";

        statTexts[idx++].text = $"<sprite=7> {tm.GetCommons("FoundVisionRecipes")} : {foundVisionRecipes} / {ResearchManager.Instance.HiddenRecipeCount}";
        statTexts[idx++].text = $"{tm.GetCommons("Mileage")} : {carMileage:0.#}km";

        statTexts[idx++].text = $"{tm.GetCommons("CarCrash")} : {carCrash}";
        statTexts[idx++].text = $"{tm.GetCommons("HitZombies")} : {hitZombies}";
    }


    public void NextDay()
    {
        todayEarn = 0;
        todayRating = 0;
    }

    public void CalcAverageDeliveryStat(float overTime, float remainHp, int earn, float rating)
    {
        completedOrders++;

        if (overTime <= 0f)
        {
            totalDeliveryOnTime++;
        }

        averageDeliveryTime = (float)totalDeliveryOnTime / completedOrders;

        totalDeliveryHp += remainHp;

        averageDeliveryHp = totalDeliveryHp / completedOrders;

        todayEarn += earn;
        todayRating += rating;
        UpdateHighestDay();
    }

    private void UpdateHighestDay()
    {
        if (highestEarningValue < todayEarn)
        {
            highestEarningDay = GM.Instance.day;
            highestEarningValue = todayEarn;
        }
        if (highestRatingValue < todayRating)
        {
            highestRatingDay = GM.Instance.day;
            highestRatingValue = todayRating;
        }
    }

}
