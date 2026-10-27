using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    private Transform targetTransform;
    private Vector3 targetOffset;

    public void SetTargetTransform(Transform targetTransform)
    {
        SetTargetTransform(targetTransform, Vector3.zero);
    }

    /// <summary>
    /// 设置跟随目标，并可带一个相对目标的偏移（例如盘子在柜台上叠放时的高度差）。
    /// </summary>
    public void SetTargetTransform(Transform targetTransform, Vector3 offset)
    {
        this.targetTransform = targetTransform;
        this.targetOffset = offset;
    }

    private void LateUpdate()
    {
        if (targetTransform == null)
        {
            return;
        }

        transform.position = targetTransform.TransformPoint(targetOffset);
        transform.rotation = targetTransform.rotation;
    }
}
