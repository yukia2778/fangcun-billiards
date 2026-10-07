using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

/// <summary>
/// 负责在屏幕中央短暂显示提示消息。
/// 与主游戏控制脚本协作。
/// </summary>
public class OverlayMessageManager : MonoBehaviour
{
    public TMP_Text text;           // 用于显示提示内容的文本组件

    private float timer = 0;
    private string message = null;

    /// <summary>
    /// 当计时器大于 0 时显示文本，否则隐藏文本。
    /// </summary>
    void Update()
    {
        if (message == null)
            return;

        // 提示时间结束后隐藏消息并清空状态。
        if (timer <= 0)
        {
            this.message = null;
            timer = 0;
            text.gameObject.SetActive(false);
            return;
        }

        // 提示时间未结束时刷新文本内容。
        text.gameObject.SetActive(true);
        text.text = message;
        timer -= Time.deltaTime;    // 每帧扣减剩余显示时间。
    }

    /// <summary>
    /// 在屏幕中央显示指定时长的文本消息。
    /// </summary>
    /// <param name="message">消息内容。</param>
    /// <param name="seconds">显示持续时间，单位为秒。</param>
    public void DisplayMessage(string message, float seconds)
    {
        Debug.Log("显示消息：" + message + "，持续 " + seconds + " 秒。");
        this.timer = seconds;
        this.message = message;
    }
}
