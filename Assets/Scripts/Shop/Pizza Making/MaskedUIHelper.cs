using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MaskedUIHelper : MonoBehaviour
{
    public Image dummy;

    /// <summary>
    /// ���� https://forum.unity.com/threads/masked-ui-elements-shader-not-updating.371542/
    /// ����ũ ������ �ִ� UI�� ������ ������ �Ǳ� ������, ���� ����ũ �������� {materialForRendering}�� ����ؼ� ������Ʈ�ؾ� ��
    /// </summary>

    private void Update()
    {
        dummy.materialForRendering.SetFloat("_UnscaledTime", Time.unscaledTime);
    }
}
