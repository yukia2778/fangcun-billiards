using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 用于显示当前回合玩家。
/// </summary>
public class TurnDisplay : MonoBehaviour
{
    public TMP_Text textComponent;          // 显示当前回合的文本组件
    public GameController gameController;   // 用于读取主控制脚本中的当前回合状态

    /// <summary>
    /// 初始化时显示玩家一的回合和目标球状态。
    /// </summary>
    void Start()
    {
        UpdateTurnText();
    }

    /// <summary>
    /// 从主控制脚本读取当前回合和目标球，并刷新显示文本。
    /// </summary>
    void Update()
    {
        UpdateTurnText();
    }

    /// <summary>
    /// 组合“玩家回合 + 目标球类型”的左上角提示。
    /// </summary>
    private void UpdateTurnText()
    {
        if (gameController.playerOneTurn)
        {
            textComponent.text = "玩家一回合  击打目标球：" + gameController.GetObjectiveLabel(true);
        }
        else
        {
            textComponent.text = "玩家二回合  击打目标球：" + gameController.GetObjectiveLabel(false);
        }
    }
}
