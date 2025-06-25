using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BirdController : MonoBehaviour
{
    [Header("Boid Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 2f;
    public float avoidanceForce = 5f; // 规避力度
    public float maxSteerForce = 3f;

    [Header("Behavior Weights")]
    public float cohesionWeight = 1f;
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1f;
    public float boundsWeight = 2f;
    public float boundsEmergencyForce = 5f; // 新增紧急拉力


    private Rigidbody rb;
    private BirdsManager manager;
    private Vector3 boundsCenter;
    private float boundsRadius;

    [Header("Hunger Settings")]
    public float hungerRate = 0.01f;
    public float hunger = 0f;
    public float hungerThreshold = 0.7f;
    public float eatingDistance = 0.5f;
    public float feedingRate = 0.2f;

    [Header("Food Seeking")]
    public float foodPerceptionRadius = 5f;
    public float foodSeekWeight = 2f;
    private GameObject currentFoodTarget;

    [Header("Flight Settings")]
    public float minFlightHeight = 5f;
    public float maxFlightHeight = 8f;
    public float heightAdjustForce = 8f;

    [Header("Individuality Settings")]
    [Range(0, 1), Tooltip("0=完全群聚, 1=完全独立")]
    public float independenceFactor = 0.2f;
    public float randomForceStrength = 3f; // 独立飞行时的随机力强度

    [Header("Ground Avoidance")]
    public float groundDetectionRange = 5f; // 地面检测距离
    public float groundAvoidForce = 10f;    // 避障力度

    [Header("Smooth Flight Settings")]
    public float verticalSmoothTime = 1.5f;  // 增加平滑时间
    public float maxVerticalSpeed = 1.5f;    // 降低最大垂直速度
    public float heightSmoothTime = 0.8f;  // 高度调整平滑时间
    public float maxHeightChangeSpeed = 1.5f; // 最大垂直速度
    public float rotationSmoothness = 5f;
    private float verticalVelocity;          // 垂直速度缓存

    [Header("Advanced Control")]
    public AnimationCurve heightAdjustCurve = new AnimationCurve(
    new Keyframe(0, 0),
    new Keyframe(0.5f, 1),
    new Keyframe(1, 0)
        );

    [Header("Entrance Settings")]
    public float entranceDuration = 2f;   // 完全控制权过渡时间
    private float entranceTimer = 0f;

    [Header("Smooth Rotation")]
    public float pitchSmoothness = 3f;  // 俯仰平滑度

    [Header("Vertical Movement Curve")]
    public AnimationCurve verticalMovementCurve = new AnimationCurve(
    new Keyframe(0, 0),
    new Keyframe(0.5f, 1),
    new Keyframe(1, 0)
);

    // 决策树根节点
    private DecisionTreeNode rootDecision;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        manager = FindAnyObjectByType<BirdsManager>();

        rb.constraints = RigidbodyConstraints.FreezePositionY
                   | RigidbodyConstraints.FreezeRotationX
                   | RigidbodyConstraints.FreezeRotationZ;

        // Random initial velocity
        rb.linearVelocity = Random.insideUnitSphere * maxSpeed;

        // Cache bounds info
        boundsCenter = manager.transform.position;
        boundsRadius = manager.boundsRadius;

        // 初始化饥饿
        hunger = Random.Range(0f, 0.5f);

        // 初始化决策树
        InitializeDecisionTree();

        // 为每只鸟添加随机行为差异
        minSpeed += Random.Range(-1f, 1f);
        maxSpeed += Random.Range(-1f, 1f);
        perceptionRadius *= Random.Range(0.8f, 1.2f);
        foodPerceptionRadius *= Random.Range(0.7f, 1.3f);
    }

    void Update()
    {
        // 严格限制垂直速度
        Vector3 vel = rb.linearVelocity;
        if (vel.y > 3f)  // 最大上升速度
        {
            vel.y = 3f;
            rb.linearVelocity = vel;
        }
        else if (vel.y < -2f)  // 最大下降速度
        {
            vel.y = -2f;
            rb.linearVelocity = vel;
        }

        if (rb.linearVelocity != Vector3.zero)
        {
            // 计算目标朝向（限制俯仰角度）
            Vector3 flatForward = rb.linearVelocity.normalized;
            flatForward.y *= 0.3f; // 减小垂直方向的影响

            Quaternion targetRot = Quaternion.LookRotation(flatForward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                pitchSmoothness * Time.deltaTime
            );
        }

        // 边界强制修正（放在Update开头）
        float distToCenter = Vector3.Distance(transform.position, boundsCenter);
        if (distToCenter > boundsRadius * 0.95f)
        {
            Vector3 dirToCenter = (boundsCenter - transform.position).normalized;
            rb.AddForce(dirToCenter * boundsEmergencyForce * 2, ForceMode.Acceleration);
        }

        // 入场阶段行为控制
        if (entranceTimer < entranceDuration)
        {
            entranceTimer += Time.deltaTime;
            float progress = entranceTimer / entranceDuration;

            // 渐入式控制（初始完全按路径飞行，逐渐加入群聚行为）
            if (progress < 0.5f)
            {
                rb.linearDamping = 2f; // 初始高阻力保证稳定
            }
            else
            {
                rb.linearDamping = Mathf.Lerp(2f, 0.5f, (progress - 0.5f) * 2f);
            }
            return; // 入场阶段跳过其他行为
        }

        if (rb.linearVelocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothness * Time.deltaTime
            );
        }

        // 限制垂直速度
        Vector3 velocity = rb.linearVelocity;
        if (Mathf.Abs(velocity.y) > maxHeightChangeSpeed)
        {
            velocity.y = Mathf.Sign(velocity.y) * maxHeightChangeSpeed;
            rb.linearVelocity = velocity;
        }

        AvoidOtherBirds();

        // 强制位置修正（防止意外穿透）
        if (transform.position.y < 0.5f)
        {
            Vector3 pos = transform.position;
            pos.y = minFlightHeight;
            transform.position = pos;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }

        // 保持飞行高度
        MaintainFlightHeight();

        // 更新饥饿值
        hunger = Mathf.Clamp01(hunger + hungerRate * Time.deltaTime);

        // 执行决策树
        ActionNode action = (ActionNode)rootDecision.MakeDecision();
        action.PerformAction();


        // Limit speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
        else if (rb.linearVelocity.magnitude < minSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * minSpeed;
        }

        // Face direction of movement
        if (rb.linearVelocity != Vector3.zero)
        {
            transform.forward = rb.linearVelocity.normalized;
        }
    }

    void AvoidOtherBirds()
    {
        Collider[] nearbyBirds = Physics.OverlapSphere(transform.position, avoidanceRadius, LayerMask.GetMask("Birds"));
        Vector3 avoidForce = Vector3.zero;

        foreach (var bird in nearbyBirds)
        {
            if (bird.gameObject != this.gameObject)
            {
                float distance = Vector3.Distance(transform.position, bird.transform.position);
                avoidForce += (transform.position - bird.transform.position).normalized
                             * (avoidanceRadius / distance) * avoidanceForce;
            }
        }

        if (nearbyBirds.Length > 0)
        {
            rb.AddForce(avoidForce, ForceMode.Acceleration);
        }

        if (nearbyBirds.Length > 10) // 限制最大检测数量
        {
            System.Array.Sort(nearbyBirds, (a, b) =>
                Vector3.Distance(transform.position, a.transform.position)
                .CompareTo(Vector3.Distance(transform.position, b.transform.position)));
        }
    }
    public void UpdateBoid(Vector3 cohesion, Vector3 separation, Vector3 alignment, Vector3 boundsAvoidance)
    {
        // 按概率决定是否独立行动
        if (Random.value < independenceFactor)
        {
            // 完全独立模式 - 随机飞行
            Vector3 randomForce = Random.insideUnitSphere * randomForceStrength;
            rb.AddForce(randomForce, ForceMode.Acceleration);
        }
        else
        {
            // 正常群聚模式
            Vector3 acceleration = Vector3.zero;
            acceleration += cohesion * cohesionWeight;
            acceleration += separation * separationWeight;
            acceleration += alignment * alignmentWeight;
            acceleration += boundsAvoidance * boundsWeight;

            if (acceleration != Vector3.zero)
            {
                acceleration = Vector3.ClampMagnitude(acceleration, maxSteerForce);
                rb.AddForce(acceleration, ForceMode.Acceleration);
            }
        }

    }

    void InitializeDecisionTree()
    {
        // 创建决策节点
        IsHungryDecision isHungry = gameObject.AddComponent<IsHungryDecision>();
        FoodNearbyDecision foodNearby = gameObject.AddComponent<FoodNearbyDecision>();

        // 创建行为节点
        ForageAction forageAction = gameObject.AddComponent<ForageAction>();
        FlockAction flockAction = gameObject.AddComponent<FlockAction>();

        // 构建决策树
        isHungry.trueNode = foodNearby;
        isHungry.falseNode = flockAction;

        foodNearby.trueNode = forageAction;
        foodNearby.falseNode = flockAction;

        rootDecision = isHungry;
    }

    public bool IsHungry()
    {
        return hunger > hungerThreshold;
    }

    void MaintainFlightHeight()
    {

        float currentHeight = transform.position.y;
        Vector3 adjustForce = Vector3.zero;

        // 强制高度限制（比min/max更严格的边界）
        float hardMinHeight = minFlightHeight + 1f;  // 安全缓冲
        float hardMaxHeight = maxFlightHeight - 2f;  // 安全缓冲

        // 高度超出硬边界时强制修正
        if (currentHeight < hardMinHeight)
        {
            adjustForce = Vector3.up * heightAdjustForce * 1.5f;
        }
        else if (currentHeight > hardMaxHeight)
        {
            adjustForce = Vector3.down * heightAdjustForce * 1.5f;

            // 额外水平转向力（避免在高空盘旋）
            Vector3 horizontalVel = rb.linearVelocity;
            horizontalVel.y = 0;
            rb.AddForce(horizontalVel.normalized * 2f, ForceMode.Acceleration);
        }

        // 应用常规高度微调（平滑控制）
        float targetHeight = Mathf.Lerp(minFlightHeight, maxFlightHeight, 0.7f);
        float heightError = targetHeight - currentHeight;
        adjustForce += Vector3.up * heightError * heightAdjustForce * 0.3f;

        rb.AddForce(Vector3.ClampMagnitude(adjustForce, heightAdjustForce * 2f),
                   ForceMode.Acceleration);


        /*// 计算目标高度（在min/max之间插值）
        float targetHeight = Mathf.Lerp(minFlightHeight, maxFlightHeight, 0.5f);*/

        // 使用平滑阻尼控制高度
        float newHeight = Mathf.SmoothDamp(
            transform.position.y,
            targetHeight,
            ref verticalVelocity,
            verticalSmoothTime,
            maxVerticalSpeed
        );

        // 计算需要施加的力
        float heightDifference = newHeight - transform.position.y;
        Vector3 liftForce1 = Vector3.up * heightDifference * heightAdjustForce;

        // 应用力（限制最大力度）
        rb.AddForce(Vector3.ClampMagnitude(liftForce1, heightAdjustForce * 0.3f),
                   ForceMode.Acceleration);

        float normalizedHeight = Mathf.InverseLerp(minFlightHeight, maxFlightHeight, transform.position.y);
        float curveFactor = verticalMovementCurve.Evaluate(normalizedHeight);

        // 应用曲线控制的力
        Vector3 liftForce2 = Vector3.up * curveFactor * heightAdjustForce * 0.2f;
        rb.AddForce(liftForce2, ForceMode.Acceleration);


    }

    public bool HasFoodNearby()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, foodPerceptionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Food"))
            {
                currentFoodTarget = hitCollider.gameObject;
                return true;
            }
        }
        currentFoodTarget = null;
        return false;
    }

    public void SeekFood()
    {
        if (currentFoodTarget != null)
        {
            // 计算朝向食物的力
            Vector3 foodDirection = (currentFoodTarget.transform.position - transform.position).normalized;
            rb.AddForce(foodDirection * foodSeekWeight, ForceMode.Acceleration);

            // 检查是否到达食物
            if (Vector3.Distance(transform.position, currentFoodTarget.transform.position) < eatingDistance)
            {
                EatFood();
            }
        }
    }

    private void EatFood()
    {
        hunger = Mathf.Clamp01(hunger - feedingRate);
        Destroy(currentFoodTarget);
        currentFoodTarget = null;
    }

    public void NormalFlocking()
    {
        // 正常群聚行为由BoidsManager控制
    }
}