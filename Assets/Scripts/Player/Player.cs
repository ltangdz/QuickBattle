using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Attack,
    Accelerate,
    Heal,
}

public class Player : MonoBehaviour, IDamagable
{
    public static Player LocalInstance { get; private set; }
    
    [Space(10)]
    [Header("SO文件配置")]
    [SerializeField] private PlayerLevelInfoListSO playerLevelInfoListSO;

    private List<PlayerLevelInfoSO> playerLevelInfoList;
    
    [Space(10)]
    [Header("玩家属性")]
    [Tooltip("当前血量")]
    [SerializeField] private float curHP = 100;
    [Tooltip("最大血量")]
    [SerializeField] private float maxHP = 100;
    [Tooltip("移动速度")]
    [SerializeField] private float moveSpeed = 10f;
    [Tooltip("攻击力")]
    [SerializeField] private float atk = 30;
    [Tooltip("能量值")]
    [SerializeField] private int energy = 0;
    [Tooltip("最大能量值 (能量值满可以升级)")]
    [SerializeField] private int maxEnergy = 0;
    [Tooltip("玩家等级")]
    [SerializeField] private int level = 1;   
    [Tooltip("玩家最高等级")]
    [SerializeField] private int maxLevel = 10;
    [Tooltip("玩家名称")]
    [SerializeField] private string playerName = "player";
    [Tooltip("玩家分数")]
    [SerializeField] private int score = 0;
    [Tooltip("玩家击杀数")]
    [SerializeField] private int killAmount = 0;
    
    [Space(10)]
    [Header("攻击技能")]
    [Tooltip("攻击cd")]
    [SerializeField] private float attackTimer = 0f;
    [Tooltip("最大攻击cd")]
    [SerializeField] private float attackTimerMax = 1f;
    
    [Space(10)]
    [Header("加速技能")]
    [Tooltip("加速cd")]
    [SerializeField] private float accelerateTimer = 0f;
    [Tooltip("最大加速cd")]
    [SerializeField] private float accelerateTimerMax = 10f;
    [Tooltip("加速持续时长")]
    [SerializeField] private float accelerateTime = 3f;
    [Tooltip("加速百分比")]
    [SerializeField] private float acceleratePercent = 100f;
    
    [Space(10)]
    [Header("回血技能")]
    [Tooltip("回血cd")]
    [SerializeField] private float healTimer = 0f;
    [Tooltip("最大回血cd")]
    [SerializeField] private float healTimerMax = 15f;

    [Space(10)]
    [Header("跳跃参数")]
    [Tooltip("是否使用重力")]
    [SerializeField] private bool useGravity = true;    // 是否使用重力
    [Tooltip("跳跃初速度")]
    [SerializeField] private float jumpForce = 20f;     // 跳跃初速度
    [Tooltip("重力加速度")]
    [SerializeField] private float gravity = 50f;       // 重力加速度
    
    [Space(10)] 
    [Header("其他")] 
    [Tooltip("碰撞层")] public LayerMask collisionLayerMask;
    [Tooltip("子弹预制体")] public GameObject bulletPrefab;
    [Tooltip("子弹生成位置")] public Transform shootPos;
    [Tooltip("玩家出生点")] public List<Vector3> playerSpawnPoints;
    
    
    [Space(10)]
    [Header("玩家状态")]
    public bool isGrounded; // 是否在地面上
    public bool isDead;     // 是否死亡

    private Rigidbody rb;
    private Vector3 moveDir = Vector3.forward;
    private Camera mainCamera;
    private Collider playerCollider;
    
    
    private void Start()
    {
        GameInput.Instance.OnJumpEvent += (sender, args) => HandleJumpInput();
        GameInput.Instance.OnUseSkillEvent += (sender, skillType) => HandleSkillInput(skillType);
        
        mainCamera = Camera.main;
        
        // Todo: 在 OnNetworkSpawn 中修改
        EventManager.Instance.AddListener(EventName.EnergyBlockPicked, OnEnergyBlockPicked);
        
        InitBroadcastPlayerInfo();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        
        // Todo: 在 OnNetworkSpawn 中修改
        LocalInstance = this;
        playerLevelInfoList = playerLevelInfoListSO.playerLevelInfoList;

        InitPlayerLevelInfo();
    }

    /// <summary>
    /// 初始时广播玩家状态
    /// </summary>
    private void InitBroadcastPlayerInfo()
    {
        this.TriggerEvent(EventName.LocalLevelChanged, new LocalLevelChangedEventArgs{newLevel = level});
        this.TriggerEvent(EventName.LocalHpChanged, new LocalHpChangedEventArgs{newHP = curHP, maxHP = maxHP});
        this.TriggerEvent(EventName.LocalEnergyChanged, new LocalEnergyChangedEventArgs{newEnergy = energy, energyToUpgrade = maxEnergy});
        this.TriggerEvent(EventName.LocalKillAmountChanged, new LocalKillAmountChangedEventArgs{newKillAmount = killAmount});
    }

    /// <summary>
    /// 初始化玩家信息
    /// </summary>
    private void InitPlayerLevelInfo()
    {
        level = 1;
        maxLevel = playerLevelInfoList.Count;
        float newMaxHP = playerLevelInfoList[level - 1].maxHP;
        UpdateHp(newMaxHP, newMaxHP);
        
        atk = playerLevelInfoList[level - 1].atk;
        maxEnergy = playerLevelInfoList[level - 1].energyToUpgrade;
        energy = 0;
    }

    /// <summary>
    /// 玩家升级时调用
    /// </summary>
    private void UpdatePlayerLevelInfo()
    {
        float newMaxHP = playerLevelInfoList[level - 1].maxHP;
        UpdateHp(0f, newMaxHP);
        
        atk = playerLevelInfoList[level - 1].atk;
        energy -= maxEnergy;
        maxEnergy = playerLevelInfoList[level - 1].energyToUpgrade;
    }

    private void Update()
    {
        if (isDead) return;
        
        UpdateSkillCoolDowns();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        HandleMovement();
    }

    /// <summary>
    /// 重置技能cd
    /// </summary>
    private void ResetSkillTimer()
    {
        attackTimer = 0f;
        accelerateTimer = 0f;
        healTimer = 0f;
    }

    /// <summary>
    /// 玩家移动逻辑
    /// </summary>
    private void HandleMovement()
    {
        // 水平移动
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        
        Quaternion cameraYRotation = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);
        
        // 有输入时才处理
        if (inputVector.magnitude > 0f)
        {
            moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
            moveDir = cameraYRotation * moveDir;
            
            rb.velocity = new Vector3(moveDir.x * moveSpeed, rb.velocity.y, moveDir.z * moveSpeed);

            // 插值 让玩家方向到目标方向平滑过渡
            float rotateSpeed = 10f;
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        }
        else
        {
            // 速度归零
            // rigidbody.velocity = Vector3.zero;
            
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
        
        if (!isGrounded && useGravity)
        {
            // 模拟重力 v = v_0 + gt
            rb.AddForce(Vector3.down * gravity * Time.deltaTime, ForceMode.VelocityChange);
        }
    }

    /// <summary>
    /// 处理计时器冷却
    /// </summary>
    private void UpdateSkillCoolDowns()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
        
        if (accelerateTimer > 0f)
        {
            accelerateTimer -= Time.deltaTime;
        }
        
        if (healTimer > 0f)
        {
            healTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 处理跳跃逻辑
    /// </summary>
    private void HandleJumpInput()
    {
        // 只有在地上能跳跃
        if (!isGrounded) return;

        Debug.Log("Jump!!!");
        
        // 处理跳跃输入
        rb.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision other)
    {
        // 碰到地面 且 速度向下
        if (other.gameObject.CompareTag(Settings.TAG_GROUND) && rb.velocity.y <= 0f)
        {
            Debug.Log("落地");
            isGrounded = true;
        }
    }
    
    private void OnCollisionExit(Collision other)
    {
        // 离地检测
        if (other.gameObject.CompareTag(Settings.TAG_GROUND))
        {
            Debug.Log("离地");
            isGrounded = false;
        }
    }

    /// <summary>
    /// 处理技能逻辑
    /// </summary>
    public void HandleSkillInput(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Attack:
                if (attackTimer > 0f)
                {
                    // 还在cd
                    Debug.Log("攻击技能在cd");
                }
                else
                {
                    Attack();
                }
                break;
                
            case SkillType.Accelerate:
                if (accelerateTimer > 0f)
                {
                    // 还在cd
                    Debug.Log("加速技能在cd");
                }
                else
                {
                    Accelerate();
                }
                break;
            
            case SkillType.Heal:
                if (healTimer > 0f)
                {
                    Debug.Log("回血技能在cd");
                }
                else if (Mathf.Approximately(curHP, maxHP))
                {
                    Debug.Log("当前满血 不可使用回血");
                }
                else
                {
                    Heal();
                }
                break;
        }
    }

    private void Attack()
    {
        Debug.Log(name + " 发射了一枚子弹");
        
        // Todo: 对象池优化
        // Bullet bullet = Instantiate(bulletPrefab, shootPos.position, Quaternion.identity).GetComponent<Bullet>();
        
        Bullet bullet = (Bullet)PoolManager.Instance.ReuseComponent(bulletPrefab, shootPos.position, Quaternion.identity);
        bullet.Init(this, moveDir);

        // 开启冷却
        attackTimer = attackTimerMax;
        this.TriggerEvent(EventName.AttackCdStarted, new SkillCdStartEventArgs{maxCd = attackTimerMax});
    }
    
    private void Accelerate()
    {
        // 开启加速协程
        StartCoroutine(AccelerateCoroutine());
        
        // 开启加速冷却
        accelerateTimer = accelerateTimerMax;
        this.TriggerEvent(EventName.AccelerateCdStarted, new SkillCdStartEventArgs{maxCd = accelerateTimerMax});
    }
    
    private IEnumerator AccelerateCoroutine()
    {
        float speed = moveSpeed;
        moveSpeed = speed * (1 + acceleratePercent / 100f);
        Debug.Log("开始加速");
        
        yield return new WaitForSeconds(accelerateTime);
        
        moveSpeed = speed;
        Debug.Log("加速结束");
    }
    
    /// <summary>
    /// 回血技能 为玩家自身回复 50% 血量, cd 15s
    /// </summary>
    private void Heal()
    {
        UpdateHp(maxHP / 2f, maxHP);

        // 开启回血冷却
        healTimer = healTimerMax;
        this.TriggerEvent(EventName.HealCdStarted, new SkillCdStartEventArgs{maxCd = healTimerMax});
    }

    /// <summary>
    /// 更新当前血量
    /// </summary>
    /// <param name="deltaHP">血量变化量</param>
    /// <param name="newMaxHP">最大血量</param>
    private void UpdateHp(float deltaHP, float newMaxHP)
    {
        maxHP = newMaxHP;
        curHP = Mathf.Clamp(curHP + deltaHP, 0f, maxHP);
        this.TriggerEvent(EventName.LocalHpChanged, new LocalHpChangedEventArgs { newHP = curHP , maxHP = maxHP});
    }
    
    /// <summary>
    /// 计算伤害 公式: atk * (0.9 + level * 0.1)
    /// </summary>
    /// <returns>伤害值</returns>
    /// <exception cref="NotImplementedException"></exception>
    public float CalculateDamage()
    {
        // 公式: atk * (0.9 + level * 0.1)
        // eg. 攻击力 30, 等级 3, 伤害 30 * (0.9 + 3 * 0.1) = 36
        return atk * (0.9f + level * 0.1f);
    }

    /// <summary>
    /// 玩家受伤逻辑
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="source"></param>
    public void TakeDamage(float damage, IDamagable source = null)
    {
        UpdateHp(-damage, maxHP);
        
        if (source != null)
        {
            Debug.Log($"玩家{playerName} 受到来自 {source.Name} 的 {damage} 点伤害, 血量 {curHP}/{maxHP}");
        }

        if (curHP <= 0f)
        {
            curHP = 0f;
            // 血量 < 0, 触发死亡事件
            PlayerDie(source);
        }
    }

    /// <summary>
    /// 玩家死亡逻辑: 
    /// 1. 增加伤害来源的击杀数
    /// 2. 自己分数的 50% 转移到伤害来源 (向上取整 25 / 2 = 13)
    /// 3. 死亡期间关闭输入和碰撞体, 3秒后随机到某位置复活
    /// </summary>
    /// <param name="source">伤害来源</param>
    private void PlayerDie(IDamagable source)
    {
        Debug.Log($"玩家{playerName}死亡！");
        isDead = true;
        rb.velocity = Vector3.zero;
        
        Player sourcePlayer = source as Player;
        if (sourcePlayer != null)
        {
            sourcePlayer.IncreaseKillAmount();
            
            // 分数转移
            int transferredScore = (score + 1) / 2;
            this.score -= transferredScore;
            sourcePlayer.SetScore(sourcePlayer.GetScore() + transferredScore);
        }
        
        // 开启复活协程
        StartCoroutine(RebornCoroutine());
    }

    private IEnumerator RebornCoroutine()
    {
        // 关闭输入和碰撞体
        GameInput.Instance.DisablePlayerInput();
        playerCollider.isTrigger = true;
        
        const float rebornTime = 3f;
        yield return new WaitForSeconds(rebornTime);
        
        // 找到合适的出生点复活
        RebornAtRandomPos();
        
        // 开启输入和碰撞体
        GameInput.Instance.EnablePlayerInput();
        isDead = false;
        playerCollider.isTrigger = false;
        
        Debug.Log($"玩家{playerName}复活");
    }

    private void RebornAtRandomPos()
    {
        // 重置所有cd和血量
        ResetSkillTimer();
        
        UpdateHp(maxHP, maxHP);
    }

    public string Name => playerName;

    public int GetScore() => score;
    public void SetScore(int score)
    {
        this.score = score;
        
        // 触发本地玩家分数改变事件
        this.TriggerEvent(EventName.LocalScoreChanged, new LocalScoreChangedEventArgs { newScore = score });
    }
    
    public int GetKillAmount() => killAmount;
    public void IncreaseKillAmount()
    {
        killAmount += 1;
        Debug.Log($"玩家{playerName}当前击杀数: {killAmount}");
        
        // 触发本地玩家击杀数改变事件
        this.TriggerEvent(EventName.LocalKillAmountChanged, new LocalKillAmountChangedEventArgs { newKillAmount = killAmount });
    }

    /// <summary>
    /// 玩家拾取能量块逻辑
    /// </summary>
    private void OnEnergyBlockPicked(object sender, EventArgs eventArgs)
    {
        // 增加 5 积分
        SetScore(score + 5);        
        
        // 增加 5 能量值
        AddEnergy(5);
    }

    private void AddEnergy(int increaseEnergy)
    {
        energy += increaseEnergy;
        
        // 能量条满 且 玩家没到最大等级
        if (energy >= maxEnergy && level < maxLevel)
        {
            level++;
            this.TriggerEvent(EventName.LocalLevelChanged, new LocalLevelChangedEventArgs{newLevel = level});
            UpdatePlayerLevelInfo();
        }
        
        this.TriggerEvent(EventName.LocalEnergyChanged,
            new LocalEnergyChangedEventArgs { newEnergy = energy, energyToUpgrade = maxEnergy });
    }
}
