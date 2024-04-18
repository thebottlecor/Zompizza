using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldmapQuickButton : MonoBehaviour
{
    
    void Start()
    {
        // 미니맵 스크립트에서 생성된 이벤트 오브젝트가 버튼 클릭을 막기에, 나중에 이것을 맨 마지막 자식으로 보냄
        StartCoroutine(SetLastChild());
    }

    IEnumerator SetLastChild()
    {
        yield return null;
        yield return null;

        transform.SetAsLastSibling();
    }
}
