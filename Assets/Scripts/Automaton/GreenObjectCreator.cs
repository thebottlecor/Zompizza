using System.Collections.Generic;
using UnityEngine;

public class GreenObjectCreator : MonoBehaviour
{

    public GreenObjectLib lib;

    public BoxCollider coll;

    public int creatingCount = 20;
    public int creatingCount2 = 20;

    void Start()
    {
        for (int i = 0; i < creatingCount; i++)
        {
            Create(lib.treePrefabs, 6f);
        }



        for (int i = 0; i < creatingCount2; i++)
        {
            Create(lib.stonePrefabs, 12f);
        }

        Destroy(coll);
        Destroy(this);
    }

    private void Create(GameObject[] list, float radius)
    {
        int random = UnityEngine.Random.Range(0, list.Length);
        var obj = Instantiate(list[random], this.transform);

        obj.transform.localEulerAngles = new Vector3(0f, UnityEngine.Random.Range(-75f, 75f), 0f);

        while (true)
        {
            float randomX = UnityEngine.Random.Range(Mathf.FloorToInt(transform.position.x - coll.size.x * 0.5f), Mathf.FloorToInt(transform.position.x + coll.size.x * 0.5f));
            float randomZ = UnityEngine.Random.Range(Mathf.FloorToInt(transform.position.z - coll.size.z * 0.5f), Mathf.FloorToInt(transform.position.z + coll.size.z * 0.5f));
            Vector3 newPos = new Vector3(randomX, 0f, randomZ);

            if (Physics.OverlapSphere(newPos, radius, 1 << LayerMask.NameToLayer("PathfindingBlock")).Length == 1)
            {
                obj.transform.position = newPos;
                break;
            }
        }
    }
}
