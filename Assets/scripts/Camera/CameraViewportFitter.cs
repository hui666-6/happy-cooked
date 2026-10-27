using UnityEngine;

// 摄像机取景自适应：
// 保持设计取景（默认 16:9）在任意屏幕宽高比下都完整可见。
// 摄像机位置与角度保持不变，仅按屏幕比例调整垂直视野（FOV）。
public class CameraViewportFitter : MonoBehaviour
{
    [SerializeField] private float referenceAspect = 16f / 9f;

    private Camera targetCamera;
    private float baseFov;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        baseFov = targetCamera.fieldOfView;
    }

    private void LateUpdate()
    {
        Apply();
    }

    private void Apply()
    {
        if (targetCamera == null || targetCamera.orthographic)
        {
            return;
        }

        float aspect = targetCamera.aspect;
        if (Mathf.Approximately(aspect, 0f))
        {
            return;
        }

        float targetFov = baseFov;
        if (aspect < referenceAspect)
        {
            // 屏幕比设计取景更窄：增大垂直视野，保证设计宽度始终完整显示
            targetFov = 2f * Mathf.Atan(
                Mathf.Tan(baseFov * 0.5f * Mathf.Deg2Rad) * referenceAspect / aspect) * Mathf.Rad2Deg;
        }

        if (!Mathf.Approximately(targetCamera.fieldOfView, targetFov))
        {
            targetCamera.fieldOfView = targetFov;
        }
    }
}
