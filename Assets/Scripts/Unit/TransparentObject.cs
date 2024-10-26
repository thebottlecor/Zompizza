using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentObject : MonoBehaviour
{

	public float timer;

	private bool specialObj;
	private MeshRenderer[] meshes;

	private SpecialTransparent specialTransparent;

	private Material prevMaterial;
	private Material transMaterial;

	public void Init(float timer, bool specialObj)
    {
		this.timer = timer;
		this.specialObj = specialObj;

		if (specialObj)
			specialTransparent = GetComponent<SpecialTransparent>();
		else
		{
			meshes = GetComponentsInChildren<MeshRenderer>();
			prevMaterial = meshes[0].sharedMaterial;
			if (prevMaterial == DataManager.Instance.materialLib.baseMaterial2)
            {
				transMaterial = DataManager.Instance.materialLib.transparentMaterial2;
            }
			//else if (prevMaterial == DataManager.Instance.materialLib.baseMaterial)
			else
			{
				transMaterial = DataManager.Instance.materialLib.transparentMaterial;

			}
		}

		BecomeTransparent();
    }

    private void Update()
    {
		timer -= Time.deltaTime;

		if (timer <= 0)
        {
			End();

		}
    }

	public void End()
    {
		ResetTransparent();
		Destroy(this);
	}


    public void BecomeTransparent()
	{
		if (!specialObj)
		{
			for (int i = 0; i < meshes.Length; i++)
			{
				meshes[i].sharedMaterial = transMaterial;
				meshes[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
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
				meshes[i].sharedMaterial = prevMaterial;
				meshes[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
			}
		}
		else
		{
			specialTransparent.ResetTransparent();
		}
	}
}
