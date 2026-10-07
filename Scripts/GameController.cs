using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责处理回合、胜负、犯规等主游戏逻辑。
/// 与球杆控制、进袋收集、回合显示协作。
/// </summary>
public class GameController : MonoBehaviour
{
    // 关联的场景对象和脚本
    public MenuManager menuManager; // 用于打开或关闭菜单
    public BallManager ballManager; // 用于重生白球和目标球
    public QueueController queueController; // 用于隐藏或显示球杆
    public OverlayMessageManager onScreenMessage; // 用于在屏幕中央显示提示

    // 玩家得分（改为私有，仅内部胜负判断使用，外部不再访问）
    private int scorePlayerOne;
    private int scorePlayerTwo;

    // 公开状态标记
    public bool playerOneTurn; // 为真表示玩家一回合，为假表示玩家二回合
    public bool playerOneHasHalf; // 为真表示玩家一打半色球、玩家二打全色球；为假表示相反
    public bool firstBallInGame;

    // 私有状态标记
    private bool turnover; // 是否处于回合结束状态；为假表示球仍在滚动
    private bool gameOver; // 游戏是否已经结束
    private bool foul; // 当前回合是否发生犯规
    private bool scored; // 当前回合是否打进了自己的目标球

    // 计时器
    private float preGameTimer; // 开始结算前等待球落稳的时间
    private float afterGameTimer; // 游戏结束后打开菜单前的等待时间

    void Start()
    {
        InitializeGlobals();
    }

    /// <summary>
    /// 处理回合切换、犯规和游戏结束状态。
    /// </summary>
    void Update()
    {
        // 球生成时略高于球桌，先等待其落下并静止，避免开局立即触发结算。
        if (preGameTimer > 0)
        {
            preGameTimer -= Time.deltaTime;
            return;
        }

        // 游戏结束后不再处理回合，只等待一段时间后打开菜单。
        if (gameOver)
        {
            queueController.SetSuppressQueue(true);
            // 计时结束后显示菜单，给胜负提示留出展示时间。
            if (afterGameTimer > 0)
            {
                afterGameTimer -= Time.deltaTime;
                return;
            }

            menuManager.OpenMenu();
        }

        // 所有球停止后开始结算当前回合。
        if (IsStatic() && !turnover)
        {
            // 白球进袋等犯规发生后，回合结束时重生白球。
            if (foul)
            {
                ballManager.RespawnQueue();
                ResetPreGameTimer();
            }


            turnover = true;
            queueController.UpdatePosition();
            queueController.SetSuppressQueue(false);

            // 没有犯规且打进自己的目标球时，当前玩家继续出杆。
            if (scored && !foul)
            {
                scored = false;
                return;
            }

            foul = false;
            scored = false;

            // 未获得续杆机会时切换玩家。
            if (playerOneTurn)
            {
                Debug.Log("回合结束。玩家二出杆。");
                onScreenMessage.DisplayMessage("玩家二回合", 2);
                playerOneTurn = false;
                return;
            }
            else
            {
                Debug.Log("回合结束。玩家一出杆。");
                onScreenMessage.DisplayMessage("玩家一回合", 2);
                playerOneTurn = true;
                return;
            }
        }

        // 球开始运动后标记回合进行中，并隐藏球杆，避免移动中继续击球。
        if (!IsStatic() && turnover)
        {
            turnover = false;
            queueController.SetSuppressQueue(true);
        }
    }

    /// <summary>
    /// 将游戏恢复到初始状态。
    /// </summary>
    public void ResetGame()
    {
        ballManager.RespawnObjectives();
        ballManager.RespawnQueue();

        InitializeGlobals();

        menuManager.MarkGameStarted();
        queueController.UpdatePosition();
        queueController.SetSuppressQueue(false);
        queueController.SuppressFor(0.2f); // 短暂隐藏球杆，避免点击开始游戏按钮后立刻误触击球。
    }

    /// <summary>
    /// 将全局回合和计分状态重置为初始值。
    /// </summary>
    private void InitializeGlobals()
    {
        playerOneTurn = true;
        playerOneHasHalf = true;
        ResetPreGameTimer();
        afterGameTimer = 4.0f;
        gameOver = false;
        playerOneTurn = true;
        scorePlayerOne = 0;
        scorePlayerTwo = 0;
        turnover = true;
        firstBallInGame = true;
        foul = false;
        scored = false;
    }

    /// <summary>
    /// 获取指定玩家当前应击打的目标球类型。
    /// </summary>
    /// <param name="forPlayerOne">是否查询玩家一的目标球。</param>
    /// <returns>目标球类型文本，首颗有效进球前返回未确定。</returns>
    public string GetObjectiveLabel(bool forPlayerOne)
    {
        if (firstBallInGame)
        {
            return "未确定";
        }

        bool playerHasHalf = forPlayerOne ? playerOneHasHalf : !playerOneHasHalf;
        return playerHasHalf ? "半色球" : "全色球";
    }

    /// <summary>
    /// 重置开局等待计时器。
    /// </summary>
    private void ResetPreGameTimer()
    {
        preGameTimer = 0.5f;
    }

    /// <summary>
    /// 关闭游戏程序。
    /// </summary>
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("退出游戏。");
    }

    /// <summary>
    /// 当进袋收集器收集到球后调用。
    /// 根据球类型处理胜负、犯规或得分。
    /// </summary>
    /// <param name="marker">被收集球上的球标记组件。</param>
    public void OnBallCollected(BallMarker marker)
    {
        // 8 号球进袋时，根据当前玩家是否已经打完目标球判定胜负。
        if (marker.isEight)
        {
            if ((playerOneTurn && scorePlayerOne >= 7) || (!playerOneTurn && scorePlayerTwo < 7))
            {
                onScreenMessage.DisplayMessage("玩家一获胜", 4);
                queueController.SetSuppressQueue(true);
                Debug.Log("游戏结束。玩家一获胜。");
            }
            else
            {
                onScreenMessage.DisplayMessage("玩家二获胜", 4);
                queueController.SetSuppressQueue(true);
                Debug.Log("游戏结束。玩家二获胜。");
            }

            gameOver = true;
            return;
        }

        // 白球进袋视为犯规，回合结算时会重生白球。
        if (marker.isQueue)
        {
            Debug.Log("犯规。");
            foul = true;
            return;
        }

        // 首颗有效进球决定双方目标球类型。
        if (firstBallInGame)
        {
            firstBallInGame = false;
            if ((playerOneTurn && !marker.isFull) || (!playerOneTurn && marker.isFull))
            {
                playerOneHasHalf = true;
                onScreenMessage.DisplayMessage("玩家一目标：半色球\n玩家二目标：全色球", 2);
                Debug.Log("玩家一目标为半色球，玩家二目标为全色球。");
            }
            else
            {
                playerOneHasHalf = false;
                onScreenMessage.DisplayMessage("玩家一目标：全色球\n玩家二目标：半色球", 2);
                Debug.Log("玩家一目标为全色球，玩家二目标为半色球。");
            }
        }

        // 根据进袋球类型给对应玩家加分，记录当前回合是否获得续杆机会（删除得分打印日志）
        if ((marker.isFull && playerOneHasHalf) || (!playerOneHasHalf && !marker.isFull))
        {
            scorePlayerTwo++;
            if (!playerOneTurn)
            {
                scored = true;
            }
        }
        else
        {
            scorePlayerOne++;
            if (playerOneTurn)
            {
                scored = true;
            }
        }
    }

    /// <summary>
    /// 判断所有球是否已经静止。
    /// 通过累计所有球的速度大小判断，速度总和为 0 时才认为回合可结算。
    /// </summary>
    /// <returns>所有球是否静止。</returns>
    private bool IsStatic()
    {
        float totalVelo = 0.0f;
        foreach (BallMarker marker in ballManager.GetAllBalls())
        {
            Rigidbody rigid = marker.gameObject.GetComponent<Rigidbody>();
            if (rigid == null)
                continue;

            totalVelo += rigid.velocity.magnitude;
        }

        return totalVelo == 0;
    }
}