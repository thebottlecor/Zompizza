using UnityEngine;
using Steamworks;
using System.Text;

public class SteamHelper : Singleton<SteamHelper>
{

	protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

	private readonly string[] APIName = new string[1] { "a00" };
	private bool[] isClear = new bool[1];

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
