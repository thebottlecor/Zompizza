using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamWishlist : MonoBehaviour
{

	// 정식 steam_appid.txt 앱ID 수정하기!!

	protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

	public static bool SteamOverlayActivated;

    private void Start()
    {
		if (SteamManager.Initialized)
		{
			m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
		}
	}

	private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
	{
		if (pCallback.m_bActive != 0)
		{
			SteamOverlayActivated = true;

			var um = UIManager.Instance;
			if (um != null)
			{
				if (um.Panels_Inactive)
				{
					um.utilUI.OpenSettings();
				}
			}
		}
		else
		{
			SteamOverlayActivated = false;
		}
	}

	public void WishlistNow()
	{
		if (SteamManager.Initialized)
		{
			SteamFriends.ActivateGameOverlayToWebPage("https://store.steampowered.com/app/2937920/");
		}
		else
        {
			Application.OpenURL("https://store.steampowered.com/app/2937920/");
		}
	}
}
