using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 读取球杆控制器的蓄力进度，并显示为屏幕上的蓄力条。
/// </summary>
public class PowerBarDisplay : MonoBehaviour
{
    public QueueController queueController; // 提供当前蓄力值的球杆控制器
    public GameObject container;            // 蓄力条整体对象，用于未蓄力时隐藏
    public Image fillImage;                 // 蓄力条填充图像，fillAmount 表示当前蓄力比例

    private RectTransform fillRectTransform; // 直接控制填充区域宽度，避免 Image 类型设置不正确导致看起来不变化

    /// <summary>
    /// 初始化时隐藏蓄力条，避免开始界面出现无效 UI。
    /// </summary>
    private void Start()
    {
        CacheFillTransform();
        UpdateFill(0f);
        SetVisible(false);
    }

    /// <summary>
    /// 每帧根据球杆蓄力状态刷新显示。
    /// </summary>
    private void Update()
    {
        if (queueController == null || fillImage == null)
        {
            SetVisible(false);
            return;
        }

        bool visible = queueController.IsCharging;
        float normalizedPower = visible ? queueController.CurrentPowerNormalized : 0f;
        SetVisible(visible);
        UpdateFill(normalizedPower);
    }

    /// <summary>
    /// 缓存填充图像的矩形组件，后续按宽度显示真实蓄力比例。
    /// </summary>
    private void CacheFillTransform()
    {
        if (fillImage != null)
        {
            fillRectTransform = fillImage.rectTransform;
        }
    }

    /// <summary>
    /// 同时更新 Image 填充值和 RectTransform 宽度，确保蓄力条按玩家蓄力大小变化。
    /// </summary>
    /// <param name="normalizedPower">蓄力比例，范围 0 到 1。</param>
    private void UpdateFill(float normalizedPower)
    {
        if (fillImage == null)
        {
            return;
        }

        normalizedPower = Mathf.Clamp01(normalizedPower);
        fillImage.fillAmount = normalizedPower;

        if (fillRectTransform == null)
        {
            CacheFillTransform();
        }

        if (fillRectTransform != null)
        {
            fillRectTransform.anchorMin = Vector2.zero;
            fillRectTransform.anchorMax = new Vector2(normalizedPower, 1f);
            fillRectTransform.offsetMin = Vector2.zero;
            fillRectTransform.offsetMax = Vector2.zero;
            fillRectTransform.pivot = new Vector2(0f, 0.5f);
        }
    }

    /// <summary>
    /// 统一控制蓄力条显隐，优先隐藏配置的容器对象。
    /// </summary>
    /// <param name="visible">是否显示蓄力条。</param>
    private void SetVisible(bool visible)
    {
        if (container != null)
        {
            container.SetActive(visible);
        }
        else
        {
            gameObject.SetActive(visible);
        }
    }
}
