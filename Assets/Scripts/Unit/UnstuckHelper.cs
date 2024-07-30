using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnstuckHelper : MonoBehaviour
{

    private float cooldown;
    [SerializeField] private Button unstuckButton;

    private void Update()
    {
        if (unstuckButton != null)
            unstuckButton.interactable = cooldown <= 0f;

        if (cooldown > 0f)
            cooldown -= Time.deltaTime;
    }

    public void Unstuck()
    {
        if (cooldown > 0f) return;
        if (GM.Instance == null) return;
        if (GM.Instance.midNight) return; // 플레이어 인간 조작중 탈출 불가
        if (TutorialManager.Instance.training) return;

        var player = GM.Instance.player;

        if (player == null) return;

        cooldown = 10f;
        player.StopPlayer(false);

        //var node = AstarPath.active.GetNearest(player.transform.position, Pathfinding.NNConstraint.Walkable).position;

        Vector3 node = player.transform.position;
        node.y = 0f;

        CapsuleCollider coll = player.coll;

        //float radius = Mathf.Sqrt(Mathf.Pow(0.5f * coll.height, 2) + Mathf.Pow(coll.radius, 2));

        float radius = coll.height * 0.5f;
        int maxTry = 1000;

        while (true)
        {
            maxTry--;
            if (maxTry <= 0)
            {
                player.transform.position = GM.Instance.pizzeriaPos.position;
                player.cam.ForceUpdate();
                break;
            }

            var others = Physics.OverlapSphere(node, radius, 1 << LayerMask.NameToLayer("PathfindingBlock"));
            
            //for (int i = 0; i < 360; i++)
            //{
            //    Vector3 start = node;
            //    start.y = 1f;
            //    Vector3 target = Quaternion.AngleAxis(i, Vector3.up) * new Vector3(1f, 0f, 1f) * radius;
            //    Debug.DrawLine(start, start + target, Color.blue, 20f, false);
            //}

            //for (int i = 0; i < others.Length; i++)
            //{
            //    Debug.Log(others[i].gameObject.name);
            //}

            if (others.Length == 1)
            {
                player.transform.position = node;
                break;
            }

            float randX = UnityEngine.Random.Range(-1f, 1f);
            float randZ = (UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1) * Mathf.Sqrt(1 - Mathf.Pow(randX, 2));
            Vector3 randDir = new Vector3(randX, 0f, randZ) * radius;
            Vector3 otherPos = node + randDir;

            node = AstarPath.active.GetNearest(otherPos, Pathfinding.NNConstraint.Walkable).position;
            node.y = 0f;
        }
    }
}
