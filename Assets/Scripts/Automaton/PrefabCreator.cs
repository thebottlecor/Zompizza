using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabCreator : MonoBehaviour
{

    private Collider coll;
    public bool addColl;

    private void Start()
    {
        if (addColl)
            coll = this.gameObject.AddComponent<BoxCollider>();
        else
            coll = this.gameObject.GetComponent<Collider>();

        if (coll == null)
        {
            Debug.LogWarning(gameObject.name + " 콜라이더 없음");
            return;
        }

        this.gameObject.layer = LayerMask.NameToLayer("PathfindingBlock");

        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        yield return null;

        var parent = new GameObject(this.gameObject.name);
        parent.transform.position = this.transform.position;
        parent.layer = LayerMask.NameToLayer("EnvironmentObject");

        yield return null;

        this.gameObject.transform.SetParent(parent.transform);

        if (coll is BoxCollider)
        {
            var parentColl = parent.AddComponent<BoxCollider>();
            parentColl.center = (coll as BoxCollider).center;
            parentColl.size = (coll as BoxCollider).size;
        }
        else if (coll is CapsuleCollider)
        {
            var parentColl = parent.AddComponent<CapsuleCollider>();
            parentColl.center = (coll as CapsuleCollider).center;
            parentColl.radius = (coll as CapsuleCollider).radius;
            parentColl.height = (coll as CapsuleCollider).height;
        }

        Destroy(this);

    }
}
