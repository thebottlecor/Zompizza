using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparentObject : MonoBehaviour
{

	public float timer;

	private MeshRenderer[] meshes;

	public void Init(float timer)
    {
		meshes = GetComponentsInChildren<MeshRenderer>();
		this.timer = timer;
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
		for (int i = 0; i < meshes.Length; i++)
		{
			meshes[i].material = MaterialLibrary.Instance.transparentMaterial;
		}
	}
	public void ResetTransparent()
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			meshes[i].material = MaterialLibrary.Instance.baseMaterial;
		}
	}
}
