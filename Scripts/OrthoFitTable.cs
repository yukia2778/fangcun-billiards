using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrthoFitTable : MonoBehaviour
{
    private Camera cam;
    // 16:9画面下，刚好完整显示球台的正交尺寸，后面要填数值
    public float baseOrthoSize = 5f;
    // 我们游戏标准比例16:9
    private float standardAspect = 16f / 9f;

    void Awake()
    {
        // 获取相机组件
        cam = GetComponent<Camera>();
        // 确保相机是正交模式
        cam.orthographic = true;
        // 启动时执行一次适配
        AdjustCameraSize();
    }

    void AdjustCameraSize()
    {
        // 当前游戏窗口的宽高比
        float nowAspect = (float)Screen.width / Screen.height;
        // 计算缩放系数：屏幕越窄，系数越大，相机可视范围放大
        float scaleFactor = standardAspect / nowAspect;
        // 修改相机正交大小
        cam.orthographicSize = baseOrthoSize * scaleFactor;
    }

    // 窗口拖拽缩放时实时适配
    void Update()
    {
        AdjustCameraSize();
    }
}