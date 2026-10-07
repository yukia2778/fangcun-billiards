using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责管理菜单显示状态。
/// 对外提供打开和关闭菜单的方法。
/// 与主游戏控制、球杆控制和计分板脚本协作。
/// </summary>
public class MenuManager : MonoBehaviour
{
    public QueueController queueController;
    public Canvas menu;
    public bool menuOpen = false;
    public bool showMenuOnStart = true; // 为真时 Play Mode 启动先显示开始界面，玩家点击开始后才允许击球

    private bool gameStarted = false;   // 游戏是否已经从开始界面进入正式击球流程

    /// <summary>
    /// 每次进入 Play Mode 后重置开始界面状态。
    /// 这可以避免编辑器启用快速进入 Play Mode 时，上一轮运行留下的 gameStarted 状态导致菜单不显示。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ResetMenuStateAfterSceneLoaded()
    {
        MenuManager[] managers = FindObjectsOfType<MenuManager>(true);
        foreach (MenuManager manager in managers)
        {
            manager.ResetForPlayModeStart();
        }
    }

    /// <summary>
    /// 根据配置决定启动时显示开始界面或直接进入游戏。
    /// </summary>
    void Start()
    {
        ResetForPlayModeStart();
    }

    /// <summary>
    /// 按下退出键时切换菜单显示。
    /// 菜单打开时隐藏球杆，避免界面点击被击球输入干扰。
    /// </summary>
    void Update()
    {
        // 开始界面不能被退出键关闭，必须点击“开始游戏”进入正式流程。
        if (!gameStarted)
        {
            if (!menuOpen)
            {
                OpenStartMenu();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !Input.GetKey(KeyCode.Tab))
        {
            if (menuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
    }

    /// <summary>
    /// 打开菜单。
    /// </summary>
    public void OpenMenu()
    {
        menu.gameObject.SetActive(true);
        menuOpen = true;
        queueController.SetSuppressQueue(true);
    }

    /// <summary>
    /// 关闭菜单。
    /// </summary>
    public void CloseMenu()
    {
        menu.gameObject.SetActive(false);
        menuOpen = false;
        if (gameStarted)
        {
            queueController.SetSuppressQueue(false);
        }
    }

    /// <summary>
    /// 打开开始界面，并锁定球杆输入。
    /// </summary>
    public void OpenStartMenu()
    {
        gameStarted = false;
        menu.gameObject.SetActive(true);
        menuOpen = true;
        queueController.SetSuppressQueue(true);
    }

    /// <summary>
    /// 标记正式游戏开始，并关闭开始界面。
    /// </summary>
    public void MarkGameStarted()
    {
        gameStarted = true;
        CloseMenu();
    }

    /// <summary>
    /// 根据启动配置恢复菜单状态。
    /// 该方法可被 Start 和运行时初始化钩子重复调用，确保 Play Mode 每次都从正确界面开始。
    /// </summary>
    private void ResetForPlayModeStart()
    {
        if (showMenuOnStart)
        {
            OpenStartMenu();
        }
        else
        {
            gameStarted = true;
            CloseMenu();
        }
    }
}
