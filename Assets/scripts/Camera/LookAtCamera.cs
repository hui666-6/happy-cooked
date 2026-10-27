using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public enum Mode
    {
        LookAt,
        LookAtInverted,
        CameraForward

    }
    [SerializeField] private Mode mode;

    // 缓存摄像机的 Transform，避免每帧调用 Camera.main（内部是 FindGameObjectWithTag，开销随实例数放大）
    private Transform camTransform;

    private void Awake()
    {
        CacheCamera();
    }

    private void CacheCamera()
    {
        Camera cam = Camera.main;
        if (cam != null) camTransform = cam.transform;
    }

    void Update()
    {
        // 兜底：若 Awake 时主摄像机还未就绪（或被销毁重建），这里再取一次
        if (camTransform == null)
        {
            CacheCamera();
            if (camTransform == null) return;
        }

        switch (mode)
        {
            case Mode.LookAt:
                transform.LookAt(camTransform);
                break;
            case Mode.LookAtInverted:
                transform.LookAt(transform.position - camTransform.position + transform.position);
                break;
            case Mode.CameraForward:
                transform.forward = camTransform.forward;
                break;
            default:
                break;
        }
    }
}
