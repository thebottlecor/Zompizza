using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MaskedUIHelper : MonoBehaviour
{
    public Image dummy;

    /// <summary>
    /// 참고 https://forum.unity.com/threads/masked-ui-elements-shader-not-updating.371542/
    /// 마스크 영역에 있는 UI는 별도의 참조가 되기 때문에, 같은 마스크 영역에서 {materialForRendering}를 사용해서 업데이트해야 함
    /// </summary>

    private void Update()
    {
        dummy.materialForRendering.SetFloat("_UnscaledTime", Time.unscaledTime);
    }
}
