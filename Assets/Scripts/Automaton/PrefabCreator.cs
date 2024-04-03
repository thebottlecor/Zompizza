using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabCreator : MonoBehaviour
{

    private BoxCollider coll;

    private void Start()
    {
        this.gameObject.layer = LayerMask.NameToLayer("PathfindingBlock");
        coll = this.gameObject.AddComponent<BoxCollider>();

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
        var parentColl = parent.AddComponent<BoxCollider>();
        parentColl.center = coll.center;
        parentColl.size = coll.size;

        Destroy(this);

    }
}
