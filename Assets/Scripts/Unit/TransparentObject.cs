using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentObject : MonoBehaviour
{

	public float timer;

	private bool specialObj;
	private MeshRenderer[] meshes;

	private SpecialTransparent specialTransparent;

	public void Init(float timer, bool specialObj)
    {
		this.timer = timer;
		this.specialObj = specialObj;

		if (specialObj) 
			specialTransparent = GetComponent<SpecialTransparent>();
		else 
			meshes = GetComponentsInChildren<MeshRenderer>();

		BecomeTransparent();
    }

    private void Update()
    {
		timer -= Time.deltaTime;

		if (timer <= 0)
        {
			ResetTransparent();
			Destroy(this);
        }
    }


    public void BecomeTransparent()
	{
		if (!specialObj)
		{
			for (int i = 0; i < meshes.Length; i++)
			{
				meshes[i].material = DataManager.Instance.materialLib.transparentMaterial;
			}
		}
		else
        {
			specialTransparent.BecomeTransparent();
		}
	}
	public void ResetTransparent()
	{
		if (!specialObj)
		{
			for (int i = 0; i < meshes.Length; i++)
			{
				meshes[i].material = DataManager.Instance.materialLib.baseMaterial;
			}
		}
		else
		{
			specialTransparent.ResetTransparent();
		}
	}
}
