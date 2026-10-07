using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于检测进入收集器触发器的球，并通知主游戏控制脚本处理进袋逻辑。
/// </summary>
public class BallCollector : MonoBehaviour
{
    public GameController gameController;   // 用于通知主控制脚本结算进袋结果

    /// <summary>
    /// 当碰撞体进入触发器时，若该对象是球，则销毁它并交给主控制脚本结算。
    /// </summary>
    /// <param name="other">进入触发器的碰撞体。</param>
    private void OnTriggerEnter(Collider other)
    {
        BallMarker marker = GetMarker(other);
        if (marker == null)
            return;

        Destroy(other.transform.gameObject);
        gameController.OnBallCollected(marker);
    }

    /// <summary>
    /// 获取碰撞体所属对象上的球标记组件。
    /// </summary>
    /// <param name="collider">待检查的碰撞体。</param>
    /// <returns>如果是球则返回球标记组件，否则返回空。</returns>
    private BallMarker GetMarker(Collider collider)
    {
       return collider.transform.GetComponent<BallMarker>();
    }
}
