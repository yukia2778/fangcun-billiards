using UnityEngine;

/// <summary>
/// 负责处理球杆控制。
/// 对外提供隐藏球杆和更新球杆位置的方法。
/// 与主游戏控制和菜单管理脚本协作。
/// </summary>
public class QueueController : MonoBehaviour
{
    public BallManager ballManager;     // 用于获取当前白球
    public GameObject rotationAnchor;   // 球杆旋转锚点，跟随白球位置
    public GameObject queue;            // 实际球杆对象，用于前后位移表现蓄力
    public MenuManager menuManager;     // 用于判断菜单打开时是否允许显示球杆

    public float tensionPower = 2.0f;   // 控制拖动鼠标时球杆后拉蓄力曲线，数值越大，短距离拖动越容易获得明显蓄力
    public float maxDisplayedImpulsePower = 3.0f; // 蓄力条满格对应的最大击球冲量，同时限制真实击球力度上限
    public float maxDragDistancePixels = 600.0f;  // 鼠标拖动达到该像素距离时蓄力满格，避免需要拖出屏幕才能满力

    private const float MinQueueDisplacement = 0.05f; // 球杆未蓄力时的基础后拉距离，避免球杆贴住白球
    private const float MaxQueueDisplacement = 0.6f;  // 蓄力满格时球杆的最大后拉距离，只影响视觉表现

    private Vector2 startMousePos;          // 鼠标按下起点，用于计算拖动距离
    private bool mouseDown;                 // 鼠标左键是否处于按下状态
    private bool suppressQueue = false;     // 球杆当前是否被隐藏
    private float impulsePower = 0.0f;      // 最近一次根据拖动距离计算出的击球力度
    private float suppressionDelay = 0.0f;  // 定时隐藏球杆的剩余秒数

    public float CurrentPowerNormalized { get; private set; } // 当前蓄力条比例，范围 0 到 1
    public bool IsCharging => mouseDown && !suppressQueue && suppressionDelay <= 0; // 是否正在蓄力，供 UI 判断显隐

    /// <summary>
    /// 启动时更新球杆位置。
    /// </summary>
    void Start()
    {
        UpdatePosition();
    }

    /// <summary>
    /// 获取当前白球，并将球杆旋转锚点移动到白球位置。
    /// </summary>
    public void UpdatePosition()
    {
        Transform queueBall = GetPrimaryQueueBall().transform;
        rotationAnchor.transform.position = queueBall.position;
        rotationAnchor.transform.rotation = queueBall.rotation;
    }

    /// <summary>
    /// 设置球杆隐藏或显示。
    /// 隐藏后的球杆不可见，也不会与球发生交互。
    /// 球滚动中或菜单打开时使用，避免玩家误触击球或干扰界面点击。
    /// </summary>
    /// <param name="suppressQueue">传入真表示隐藏，传入假表示显示。</param>
    public void SetSuppressQueue(bool suppressQueue)
    {
        if(menuManager.menuOpen && !suppressQueue)
        {
            return;
        }
        this.suppressQueue = suppressQueue;
    }

    /// <summary>
    /// 获取当前主白球。
    /// </summary>
    /// <returns>最新生成的白球；若存在多个白球，取最后一个。</returns>
    /// <exception cref="System.Exception"></exception>
    private BallMarker GetPrimaryQueueBall()
    {
        BallMarker[] queueMarkers = ballManager.GetAllQueueBalls();
        if (queueMarkers.Length == 0)
        {
            throw new System.Exception("请先生成白球。");
        }
        return queueMarkers[queueMarkers.Length -1];
    }

    /// <summary>
    /// 处理球杆瞄准、蓄力和击球输入。
    /// </summary>
    void Update()
    {
        // 球杆被隐藏或处于延迟隐藏期间时，跳过输入逻辑。
        if (suppressQueue || suppressionDelay > 0)
        {
            queue.SetActive(false);
            CurrentPowerNormalized = 0f;
            if(suppressionDelay > 0)
                suppressionDelay -= Time.deltaTime;
            return;
        }
        queue.SetActive(true);

        // 根据鼠标在世界坐标中的位置计算球杆朝向。
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(rotationAnchor.transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 direction = mouseWorldPos - rotationAnchor.transform.position;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        rotationAnchor.transform.rotation = Quaternion.Euler(2.75f, angle, 0);

        // 鼠标左键按下时记录起点，用于后续计算拖动距离。
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;
            startMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        // 鼠标按住期间按拖动距离后拉球杆，并换算成击球力度。
        if (mouseDown)
        {
            Vector2 currentMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            float distance = Vector2.Distance(startMousePos, currentMousePos);
            float safeMaxDragDistance = Mathf.Max(1.0f, maxDragDistancePixels);
            float safeTensionPower = Mathf.Max(0.01f, tensionPower);

            // 先把鼠标拖动距离归一化到 0 到 1，再用曲线参数调整蓄力手感。
            CurrentPowerNormalized = Mathf.Pow(Mathf.Clamp01(distance / safeMaxDragDistance), 1.0f / safeTensionPower);

            // 球杆后拉距离和真实击球冲量都使用同一个蓄力比例，保证 UI 与实际力度一致。
            float displacement = Mathf.Lerp(MinQueueDisplacement, MaxQueueDisplacement, CurrentPowerNormalized);
            Vector3 displacedQueuePos = new Vector3(0, 0, -displacement);
            queue.transform.SetLocalPositionAndRotation(displacedQueuePos, Quaternion.identity);

            impulsePower = Mathf.Lerp(0.0f, maxDisplayedImpulsePower, CurrentPowerNormalized);
        }

        // 鼠标松开时复位球杆，并对白球施加冲量。
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("释放球杆，击球力度：" + impulsePower);
            mouseDown = false;
            CurrentPowerNormalized = 0f;
            queue.transform.SetLocalPositionAndRotation(new Vector3(0, 0, -MinQueueDisplacement), Quaternion.identity);

            GameObject queueBall = GetPrimaryQueueBall().gameObject;
            queueBall.GetComponent<Rigidbody>().AddForce(direction * impulsePower, ForceMode.Impulse);
        }

    }

    /// <summary>
    /// 在指定秒数内隐藏球杆。
    /// </summary>
    /// <param name="seconds">隐藏持续时间，单位为秒。</param>
    public void SuppressFor(float seconds)
    {
        suppressionDelay = seconds;
    }
}
