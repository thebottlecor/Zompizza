using MTAssets.EasyMinimapSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllRenderedMinimaps : MonoBehaviour
{

    public List<MinimapScanner> scanners;
    public MinimapScanner.ScanResolution scanResolution = MinimapScanner.ScanResolution.Pixels512x512;

    [ContextMenu("¸ðµÎ ½ºÄµ")]
    public void Scan()
    {

        for (int i =0; i < scanners.Count; i++)
        {
            scanners[i].scanResolution = scanResolution;
            scanners[i].DoScanInThisAreaOfComponentAndShowOnMinimap();
        }
    }
}
