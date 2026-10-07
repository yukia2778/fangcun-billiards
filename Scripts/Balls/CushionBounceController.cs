using UnityEngine;

/// <summary>
/// 让台球撞到库边时按碰撞法线稳定反弹。
/// 该脚本只处理带有库边物理材质的碰撞，不影响球与球、球与桌布之间的普通物理碰撞。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CushionBounceController : MonoBehaviour
{
    public string cushionMaterialName = "CushionPhysicsMaterial"; // 库边物理材质名称，用于识别哪些碰撞需要手动反弹
    public float bounceFactor = 0.7f;                             // 反弹保留的水平速度比例，1 表示完全镜面反弹
    public float minimumIncomingSpeed = 0.05f;                    // 低于该速度时不再反弹，避免球静止后被反复弹开
    public float bounceCooldown = 0.05f;                          // 反弹冷却时间，避免一次接触过程中重复修正速度

    private Rigidbody cachedRigidbody;    // 缓存刚体，避免每次碰撞时重复 GetComponent
    private Vector3 lastPlanarVelocity;    // 碰撞前一帧的水平速度，用于避免 Unity 碰撞求解后速度已经被清零
    private float lastBounceTime = -1.0f;  // 最近一次执行反弹的时间

    /// <summary>
    /// 初始化时缓存刚体组件。
    /// </summary>
    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 在物理更新前记录球的水平速度，后续碰撞反弹使用这个入射速度。
    /// </summary>
    private void FixedUpdate()
    {
        Vector3 planarVelocity = Vector3.ProjectOnPlane(cachedRigidbody.velocity, Vector3.up);
        if (planarVelocity.sqrMagnitude > minimumIncomingSpeed * minimumIncomingSpeed)
        {
            lastPlanarVelocity = planarVelocity;
        }
    }

    /// <summary>
    /// 球首次碰到库边时尝试执行镜面反弹。
    /// </summary>
    /// <param name="collision">Unity 传入的碰撞信息。</param>
    private void OnCollisionEnter(Collision collision)
    {
        TryReflectFromCushion(collision);
    }

    /// <summary>
    /// 球贴在库边且速度被物理求解压低时，再给一次保守的反弹修正。
    /// </summary>
    /// <param name="collision">Unity 传入的碰撞信息。</param>
    private void OnCollisionStay(Collision collision)
    {
        Vector3 planarVelocity = Vector3.ProjectOnPlane(cachedRigidbody.velocity, Vector3.up);
        if (planarVelocity.sqrMagnitude < minimumIncomingSpeed * minimumIncomingSpeed)
        {
            TryReflectFromCushion(collision);
        }
    }

    /// <summary>
    /// 如果碰撞对象是库边，则按水平法线反射入射速度。
    /// </summary>
    /// <param name="collision">Unity 传入的碰撞信息。</param>
    private void TryReflectFromCushion(Collision collision)
    {
        if (!IsCushionCollision(collision) || Time.time - lastBounceTime < bounceCooldown)
        {
            return;
        }

        Vector3 incomingVelocity = lastPlanarVelocity;
        if (incomingVelocity.sqrMagnitude < minimumIncomingSpeed * minimumIncomingSpeed)
        {
            return;
        }

        ContactPoint contact = collision.GetContact(0);
        Vector3 planarNormal = Vector3.ProjectOnPlane(contact.normal, Vector3.up);
        if (planarNormal.sqrMagnitude < 0.0001f)
        {
            return;
        }

        planarNormal.Normalize();
        if (Vector3.Dot(incomingVelocity, planarNormal) > 0.0f)
        {
            planarNormal = -planarNormal;
        }

        // 只修正水平速度，竖直方向仍交给 Unity 物理处理，避免球被库边弹飞。
        Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, planarNormal) * bounceFactor;
        cachedRigidbody.velocity = new Vector3(reflectedVelocity.x, cachedRigidbody.velocity.y, reflectedVelocity.z);
        lastPlanarVelocity = reflectedVelocity;
        lastBounceTime = Time.time;
    }

    /// <summary>
    /// 判断本次碰撞是否来自库边。
    /// 优先通过物理材质识别，名称兜底用于处理材质实例化后的情况。
    /// </summary>
    /// <param name="collision">Unity 传入的碰撞信息。</param>
    /// <returns>是否为库边碰撞。</returns>
    private bool IsCushionCollision(Collision collision)
    {
        PhysicMaterial material = collision.collider.sharedMaterial;
        if (material != null && material.name.StartsWith(cushionMaterialName))
        {
            return true;
        }

        return collision.collider.gameObject.name.StartsWith("库边");
    }
}
