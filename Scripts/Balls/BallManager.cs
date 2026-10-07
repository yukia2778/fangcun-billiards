using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于生成、重生和获取台球对象。
/// 与主游戏控制和球杆控制脚本协作。
/// </summary>
public class BallManager : MonoBehaviour
{
    public GameObject[] ballPrefabs;        // 15 个目标球预制体，用于按三角阵列生成
    public GameObject queueBallPrefab;      // 白球预制体，用于犯规或重开时生成
    public GameObject queueBallsParent;     // 白球父对象，便于统一查找和清理
    public GameObject objectiveBallParent;  // 目标球父对象，便于统一查找和清理
    public float ballDiameter = 0.0572f;    // 球直径，用于计算三角阵列摆放间距
    public float queueXPos = -0.7f;         // 白球生成位置的 X 轴偏移
    public float xObjOffset = 0.78f;        // 目标球阵列起点的 X 轴偏移
    public bool respawnOnStart = false;     // 是否在游戏启动时强制重生所有球

    // 检查器按钮：在编辑器中重生目标球。
    [InspectorButton("RespawnObjectives")]
    public bool respawnObjectives;

    // 检查器按钮：在编辑器中重生白球。
    [InspectorButton("RespawnQueue")]
    public bool respawnQueue;

    /// <summary>
    /// 如果启用了启动重生，则在开始时重新摆放白球和目标球。
    /// </summary>
    void Start()
    {
        if (respawnOnStart)
        {
            ballDiameter = queueBallPrefab.transform.localScale.x;
            RespawnQueue();
            RespawnObjectives();
        }
    }

    /// <summary>
    /// 销毁当前所有白球，并生成一个新的白球。
    /// </summary>
    public void RespawnQueue()
    {
        foreach(BallMarker cue in GetAllQueueBalls())
        {
            // 重开和犯规重生都要求场景里立刻只保留一个白球，避免球杆定位到旧白球。
            DestroyImmediate(cue.transform.gameObject);
        }

        GameObject newCue = Instantiate(queueBallPrefab, new Vector3(queueXPos, 0.7925f, 0), new Quaternion(0, 0, 0, 0), queueBallsParent.transform);
        newCue.name = "CueBall";
    }

    /// <summary>
    /// 销毁当前所有目标球，并按三角阵列重新生成。
    /// </summary>
    public void RespawnObjectives()
    {
        Vector3 startPosition = new Vector3(0, 0, 0);

        foreach (BallMarker obj in GetAllObjectiveBalls())
        {
            DestroyImmediate(obj.transform.gameObject);
        }

        int ballIndex = 0;
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col <= row; col++)
            {
                if (ballIndex >= ballPrefabs.Length)
                    return;

                // 计算当前目标球在三角阵列中的位置。
                Vector3 position = startPosition +
                    new Vector3(xObjOffset + row * ballDiameter * Mathf.Sqrt(3) / 2, 0.7925f, col * ballDiameter - row * ballDiameter / 2);

                // 生成目标球，并保持归属到目标球父对象下。
                Instantiate(ballPrefabs[ballIndex], position, new Quaternion(180, 180, 0, 0), objectiveBallParent.transform);

                ballIndex++;
            }
        }
    }

    /// <summary>
    /// 返回所有球，包括目标球和白球。
    /// </summary>
    /// <returns>所有球的球标记数组。</returns>
    public BallMarker[] GetAllBalls()
    {
        List<BallMarker> list = new List<BallMarker>(GetAllObjectiveBalls());
        foreach(BallMarker marker in GetAllQueueBalls())
        {
            list.Add(marker);
        }
        return list.ToArray();
    }

    /// <summary>
    /// 返回当前所有目标球。
    /// </summary>
    /// <returns>目标球的球标记数组。</returns>
    public BallMarker[] GetAllObjectiveBalls()
    {
        return objectiveBallParent.GetComponentsInChildren<BallMarker>();
    }

    /// <summary>
    /// 返回当前所有白球。
    /// </summary>
    /// <returns>白球的球标记数组。</returns>
    public BallMarker[] GetAllQueueBalls()
    {
        return queueBallsParent.GetComponentsInChildren<BallMarker>();
    }
}
