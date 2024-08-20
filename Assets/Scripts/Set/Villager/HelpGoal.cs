using MTAssets.EasyMinimapSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpGoal : MonoBehaviour
{

    public int villagerIdx;

    public float helpDist = 30f;
    private float helpAudioCooldown;

    public GameObject goalEffectObj;
    public GameObject helpObj;

    public MinimapItem minimapItem;
    //private Target target;

    private void Start()
    {
        Hide();

        //minimapItem.spriteColor = DataManager.Instance.uiLib.customerPinColor[villagerIdx];
        //target = minimapItem.GetComponent<Target>();
        //target.targetColor = minimapItem.spriteColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && helpObj.activeSelf)
        {
            // ¡÷πŒ ±∏√‚
            VillagerManager.Instance.Rescue(villagerIdx);
            FindEffect();
        }
    }

    private void Update()
    {
        if (helpObj.activeSelf)
        {
            float dist = (transform.position - GM.Instance.player.transform.position).magnitude;
            if (dist <= helpDist)
            {
                if (helpAudioCooldown <= 0f)
                {
                    var villager = VillagerManager.Instance.villagers[villagerIdx];
                    AudioManager.Instance.PlaySFX_Villager(1, villager.gender);
                    helpAudioCooldown = 5f;
                }
                else
                {
                    helpAudioCooldown -= Time.deltaTime;
                }
            }
        }
    }

    public void Hide()
    {
        helpObj.SetActive(false);
        goalEffectObj.SetActive(false);
    }
    public void Show()
    {
        helpObj.SetActive(true);
        goalEffectObj.SetActive(true);
    }

    private void FindEffect()
    {
        AudioManager.Instance.PlaySFX(Sfx.itemGet);
        var source = DataManager.Instance.effectLib.appleBlastEffect;
        Vector3 pos = this.transform.position;
        pos.y += 2f;
        var obj = Instantiate(source, pos, Quaternion.identity);
        Destroy(obj, 2f);
        Hide();
    }

}
