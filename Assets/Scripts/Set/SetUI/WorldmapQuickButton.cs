using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldmapQuickButton : MonoBehaviour
{
    
    void Start()
    {
        // �̴ϸ� ��ũ��Ʈ���� ������ �̺�Ʈ ������Ʈ�� ��ư Ŭ���� ���⿡, ���߿� �̰��� �� ������ �ڽ����� ����
        StartCoroutine(SetLastChild());
    }

    IEnumerator SetLastChild()
    {
        yield return null;
        yield return null;

        transform.SetAsLastSibling();
    }
}
