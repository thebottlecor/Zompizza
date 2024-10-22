using UnityEngine;
using Steamworks;
using System.Text;

public class SteamHelper : Singleton<SteamHelper>
{

	protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

	private readonly string[] APIName = new string[12] { "a00", "a01", "a02", "a03", "a04", "a05", "a06", "a07", "a08", "a09", "a10", "a11" };
	private bool[] isClear = new bool[12];

	private bool requestSuccess;
	public bool SteamOverlayIsOn { get; private set; }

	private void Start() 
	{
		if(SteamManager.Initialized) 
		{
			RequestUserStats();
		}
	}

	private void RequestUserStats()
	{
		requestSuccess = SteamUserStats.RequestCurrentStats();
		if (requestSuccess)
		{
			for (int i = 0; i < isClear.Length; i++)
			{
				SteamUserStats.GetAchievement(APIName[i], out isClear[i]);
			}
		}
	}

	public void AchieveWin()
    {
		if (SteamManager.Initialized)
        {
            CheckSteamRequest();
            if (requestSuccess)
            {
                CheckAchieve(0);
            }
        }
	}
    public void AchieveCat()
    {
        if (SteamManager.Initialized)
        {
            CheckSteamRequest();
            if (requestSuccess)
            {
                CheckAchieve(1);
            }
        }
    }
    public void AchieveTier(int tier)
    {
        if (SteamManager.Initialized)
        {
            CheckSteamRequest();
            if (requestSuccess)
            {
                if (tier >= 1)
                    CheckAchieve(2);
                if (tier >= 2)
                    CheckAchieve(3);
                if (tier >= 3)
                    CheckAchieve(4);
            }
        }
    }
    public void AchieveHiddenRecipes()
    {
        if (SteamManager.Initialized)
        {
            CheckSteamRequest();
            if (requestSuccess)
            {
                CheckAchieve(5);
            }
        }
    }
    public void AchieveMileage(float mileage)
    {
        if (SteamManager.Initialized)
        {
            CheckSteamRequest();
            if (requestSuccess)
            {
                if (mileage >= 10)
                    CheckAchieve(6);
                if (mileage >= 77)
                    CheckAchieve(7);
            }
        }
    }
    public void AchieveHit(int hit)
    {
        if (SteamManager.Initialized)
        {
            CheckSteamRequest();
            if (requestSuccess)
            {
                if (hit >= 100)
                    CheckAchieve(8);
                if (hit >= 500)
                    CheckAchieve(9);
            }
        }
    }
    public void AchieveVehicle()
    {
        if (SteamManager.Initialized)
        {
            CheckSteamRequest();
            if (requestSuccess)
            {
                CheckAchieve(10);
            }
        }
    }
    public void AchieveVillagers()
    {
        if (SteamManager.Initialized)
        {
            CheckSteamRequest();
            if (requestSuccess)
            {
                CheckAchieve(11);
            }
        }
    }

    private void CheckSteamRequest()
    {
        if (!requestSuccess)
            RequestUserStats();
    }
    private void CheckAchieve(int idx)
    {
        if (!isClear[idx])
        {
            if (SteamUserStats.SetAchievement(APIName[idx]))
            {
                isClear[idx] = true;
                SteamUserStats.StoreStats();
            }
        }
    }

    public override void CallAfterAwake()
    {

    }

    public override void CallAfterStart(ConfigData config)
    {

    }

}
