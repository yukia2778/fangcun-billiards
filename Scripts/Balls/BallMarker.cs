using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于标记对象是否为台球，并让其他脚本识别球的类型。
/// </summary>
public class BallMarker : MonoBehaviour
{
    public bool isFull = false;     // 为真表示全色球，为假表示半色球
    public bool isQueue = false;    // 是否为白球
    public bool isEight = false;    // 是否为 8 号球
}
