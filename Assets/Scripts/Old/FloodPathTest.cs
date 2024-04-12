using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloodPathTest : MonoBehaviour
{

    private ProceduralGraphMover proceduralGraphMover;

    public FloodPath fPath;

    private void Start()
    {
        proceduralGraphMover = GetComponent<ProceduralGraphMover>();
        UpdatePath();
    }

    private void UpdatePath()
    {
        fPath = FloodPath.Construct(proceduralGraphMover.target.position, null);
        AstarPath.StartPath(fPath);
        fPath.BlockUntilCalculated();
    }

    private void Update()
    {
        //if (proceduralGraphMover.updatingGraph)
        //{
        //    Debug.Log("Updating!");
        //    UpdatePath();
        //}

        UpdatePath();
    }
}
