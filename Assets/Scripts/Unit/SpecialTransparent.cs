using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTransparent : MonoBehaviour
{

	public List<Material> materials2;
	public List<Material> materials3;


	public MeshRenderer[] meshes;

    private List<List<Material>> baseMaterials;
    private List<List<Material>> transparentMaterials;

    private void Start()
    {
        baseMaterials = new List<List<Material>>();
        transparentMaterials = new List<List<Material>>();
        for (int i = 0; i < meshes.Length; i++)
        {
            List<Material> list = new List<Material>();
            meshes[i].GetSharedMaterials(list);

            baseMaterials.Add(list);

            List<Material> list2 = new List<Material>();
            for (int k = 0; k < meshes[i].sharedMaterials.Length; k++)
            {
                for (int n = 0; n < materials2.Count; n++)
                {
                    if (meshes[i].sharedMaterials[k] == materials2[n])
                    {
                        list2.Add(materials3[n]);
                        break;
                    }
                }
            }
            transparentMaterials.Add(list2);
        }
    }

    public void ForceReset()
    {
        var trans = GetComponent<TransparentObject>();
        if (trans != null)
            trans.End();
    }

	public void BecomeTransparent()
	{
        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].SetMaterials(transparentMaterials[i]);
        }
    }
	public void ResetTransparent()
	{
        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i].SetMaterials(baseMaterials[i]);
        }
    }
}
