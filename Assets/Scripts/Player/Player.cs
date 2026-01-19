using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum SkillType
{
    Attack,
    Accelerate,
    Heal,
}

public class Player : NetworkBehaviour, IDamagable
{
    public static Player LocalInstance { get; private set; }
    
    // [Space(10)]
    // [Header("SO文件配置")]
    // [SerializeField] private PlayerLevelInfoListSO playerLevelInfoListSO;
    private PlayerLevelInfoListSO playerLevelInfoListSO;
    
    [Space(10)]
    [Header("玩家属性")]
    [Tooltip("当前血量")]
    [SerializeField] private NetworkVariable<float> curHP = new NetworkVariable<float>(100f);
    [Tooltip("最大血量")]
    [SerializeField] private NetworkVariable<float> maxHP = new NetworkVariable<float>(100f);
    [Tooltip("移动速度")]
    [SerializeField] private NetworkVariable<float> moveSpeed = new NetworkVariable<float>(10f);
    [Tooltip("攻击力")]
    [SerializeField] private NetworkVariable<float> atk = new NetworkVariable<float>(30f);
    [Tooltip("能量值")]
    [SerializeField] private NetworkVariable<int> energy = new NetworkVariable<int>(0);
    [Tooltip("最大能量值 (能量值满可以升级)")]
    [SerializeField] private NetworkVariable<int> maxEnergy = new NetworkVariable<int>(10);
    [Tooltip("玩家等级")]
    [SerializeField] private NetworkVariable<int> level = new NetworkVariable<int>(1);   
    [Tooltip("玩家最高等级")]
    [SerializeField] private int maxLevel = 10;
    [Tooltip("玩家名称")]
    [SerializeField] private string playerName = "player";
    [Tooltip("玩家分数")]
    [SerializeField] private NetworkVariable<int> score = new NetworkVariable<int>(0);
    [Tooltip("玩家击杀数")]
    [SerializeField] private NetworkVariable<int> killAmount = new NetworkVariable<int>(0);
    
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
    [Tooltip("忽视碰撞层")] public LayerMask excludedLayerMask;
    [Tooltip("子弹预制体")] public Transform bulletPrefab;
    [Tooltip("子弹生成位置")] public Transform shootPos;
    [Tooltip("玩家出生点")] public List<Vector3> playerSpawnPoints;
    [Tooltip("方向箭头")] public GameObject arrow;
    
    
    [Space(10)]
    [Header("玩家状态")]
    // public bool isGrounded; // 是否在地面上
    public GroundDetectPoint groundDetectPoint;
    public bool isDead;     // 是否死亡

    private Rigidbody rb;
    private Camera mainCamera;
    private Collider playerCollider;
    
    public override void OnNetworkSpawn()
    {
        //Debug.Log($"NetworkObject 已生成: {NetworkObject.IsSpawned}, LocalPlayer: {IsLocalPlayer}");
        if (IsLocalPlayer)
        {
            Debug.Log($"玩家 {NetworkManager.Singleton.LocalClientId} 已连接");
            GameInput.Instance.OnJumpEvent += (sender, args) => HandlePlayerInputServerAuth(true);
            GameInput.Instance.OnUseSkillEvent += (sender, skillType) => HandleSkillInputServerAuth(skillType);
            mainCamera = Camera.main;
            
            curHP.OnValueChanged += OnLocalCurHPChanged;
            maxHP.OnValueChanged += OnLocalMaxHPChanged;
            energy.OnValueChanged += OnLocalEnergyChanged;
            level.OnValueChanged += OnLocalLevelChanged;
            score.OnValueChanged += OnLocalScoreChanged;
            killAmount.OnValueChanged += OnLocalKillAmountChanged;
            
            InitBroadcastPlayerInfo();
        }
        else
        {
            // 非本地玩家 隐藏方向箭头
            arrow.gameObject.SetActive(false);
        }

        if (NetworkManager.Singleton.IsServer)
        {
            InitPlayerLevelInfo();
         
            EventManager.Instance.AddListener(EventName.EnergyBlockPicked, OnEnergyBlockPicked);
        }
    }
    
    private void OnLocalCurHPChanged(float previousValue, float newValue)
    {
        this.TriggerEvent(EventName.LocalHpChanged, new LocalHpChangedEventArgs { newHP = newValue , maxHP = maxHP.Value});
    }
    
    private void OnLocalMaxHPChanged(float previousValue, float newValue)
    {
        this.TriggerEvent(EventName.LocalHpChanged, new LocalHpChangedEventArgs { newHP = curHP.Value , maxHP = newValue});
    }
    
    private void OnLocalEnergyChanged(int previousValue, int newValue)
    {
        this.TriggerEvent(EventName.LocalEnergyChanged,
            new LocalEnergyChangedEventArgs { newEnergy = newValue, energyToUpgrade = maxEnergy.Value });
    }
    
    private void OnLocalLevelChanged(int previousValue, int newValue)
    {
        this.TriggerEvent(EventName.LocalLevelChanged, new LocalLevelChangedEventArgs{newLevel = newValue});
    }
    
    private void OnLocalScoreChanged(int previousValue, int newValue)
    {
        // 触发本地玩家分数改变事件
        this.TriggerEvent(EventName.LocalScoreChanged, new LocalScoreChangedEventArgs { newScore = newValue });
    }
    
    private void OnLocalKillAmountChanged(int previousValue, int newValue)
    {
        // 触发本地玩家击杀数改变事件
        this.TriggerEvent(EventName.LocalKillAmountChanged, new LocalKillAmountChangedEventArgs { newKillAmount = newValue });
    }

    private void Awake()
    {
        // Debug.Log("Awake!!!");
        
        LocalInstance = this;
        
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();
        playerCollider.excludeLayers = excludedLayerMask;       // 设置玩家忽视碰撞层
        playerLevelInfoListSO = GameResources.Instance.playerLevelInfoListSO;
    }

    /// <summary>
    /// 初始时广播玩家状态
    /// </summary>
    private void InitBroadcastPlayerInfo()
    {
        this.TriggerEvent(EventName.LocalLevelChanged, new LocalLevelChangedEventArgs{newLevel = level.Value});
        this.TriggerEvent(EventName.LocalHpChanged, new LocalHpChangedEventArgs{newHP = curHP.Value, maxHP = maxHP.Value});
        this.TriggerEvent(EventName.LocalEnergyChanged, new LocalEnergyChangedEventArgs{newEnergy = energy.Value, energyToUpgrade = maxEnergy.Value});
        this.TriggerEvent(EventName.LocalKillAmountChanged, new LocalKillAmountChangedEventArgs{newKillAmount = killAmount.Value});
    }

    /// <summary>
    /// 初始化玩家信息 仅服务端
    /// </summary>
    private void InitPlayerLevelInfo()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        List<PlayerLevelInfoSO> playerLevelInfoList = playerLevelInfoListSO.playerLevelInfoList;
        
        level.Value = 1;
        maxLevel = playerLevelInfoList.Count;
        float newMaxHP = playerLevelInfoList[level.Value - 1].maxHP;
        UpdateHp(newMaxHP, newMaxHP);
        
        atk.Value = playerLevelInfoList[level.Value - 1].atk;
        maxEnergy.Value = playerLevelInfoList[level.Value - 1].energyToUpgrade;
        energy.Value = 0;
    }

    /// <summary>
    /// 玩家升级时调用 仅服务端
    /// </summary>
    private void UpdatePlayerLevelInfo()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        
        List<PlayerLevelInfoSO> playerLevelInfoList = playerLevelInfoListSO.playerLevelInfoList;

        float newMaxHP = playerLevelInfoList[level.Value - 1].maxHP;
        UpdateHp(0f, newMaxHP);
        
        atk.Value = playerLevelInfoList[level.Value - 1].atk;
        energy.Value -= maxEnergy.Value;
        maxEnergy.Value = playerLevelInfoList[level.Value - 1].energyToUpgrade;
    }

    private void Update()
    {
        if (isDead || !IsOwner)
        {
            return;
        }
    
        // 在自己的客户端计算技能cd
        UpdateSkillCoolDowns();
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

    private void FixedUpdate()
    {
        if (isDead || !IsOwner)
        {
            return;
        }
        
        // 发送指令序列到服务器, 请求位置更新
        HandlePlayerInputServerAuth();
    }

    private struct PlayerInput : INetworkSerializable
    {
        public Vector3 moveDir;     // 移动方向
        public bool jumpTrigger;    // 跳跃触发
        
        // 序列化（NGO要求，确保输入能网络传输）
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref moveDir);
            serializer.SerializeValue(ref jumpTrigger);
        }
    }
    
    // 本地输入缓存
    private Vector3 lastMoveDir = Vector3.forward;
    private PlayerInput playerInput;

    private void SetPlayerInput(Vector3 moveDir, bool jumpTrigger = false)
    {
        playerInput.moveDir = moveDir;
        playerInput.jumpTrigger = jumpTrigger;

        if (moveDir.magnitude > 0f)
        {
            // Debug.Log("上次输入缓存更新 " + moveDir);
            lastMoveDir = moveDir;
        }
    }
    
    /// <summary>
    /// 在服务器进行输入校验
    /// </summary>
    /// <param name="jumpTrigger">玩家是否跳跃</param>
    private void HandlePlayerInputServerAuth(bool jumpTrigger = false)
    {
        // 水平移动输入
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Quaternion cameraYRotation = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);
        
        // 计算移动方向
        Vector3 moveDir = cameraYRotation * new Vector3(inputVector.x, 0f, inputVector.y);
        
        SetPlayerInput(moveDir, jumpTrigger);
        
        HandlePlayerMovementServerRpc(playerInput);
        // Debug.Log($"客户端发送输入: {playerInput.moveDir}");
    }
    
    [ServerRpc]
    private void HandlePlayerMovementServerRpc(PlayerInput newPlayerInput)
    {
        // 更新输入缓存
        playerInput = newPlayerInput;
        // Debug.Log($"服务器收到输入 moveDir:{playerInput.moveDir}");
        
        HandlePlayerMovement();
        HandlePlayerJump();
    }
    
    /// <summary>
    /// 玩家移动逻辑 (服务器权威)
    /// </summary>
    private void HandlePlayerMovement()
    {
        Vector3 moveDir = playerInput.moveDir;
        
        // 有输入时才处理
        if (moveDir.magnitude > 0f)
        {
            rb.velocity = new Vector3(moveDir.x * moveSpeed.Value, rb.velocity.y, moveDir.z * moveSpeed.Value);

            // 插值 让玩家方向到目标方向平滑过渡
            float rotateSpeed = 10f;
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
        
        if (!groundDetectPoint.isGrounded && useGravity)
        {
            Debug.Log("玩家被重力影响");
            // 模拟重力逻辑 v = v_0 + gt
            rb.AddForce(Vector3.down * gravity * Time.deltaTime, ForceMode.VelocityChange);
        }
    }

    /// <summary>
    /// 处理跳跃逻辑 (服务器权威)
    /// </summary>
    private void HandlePlayerJump()
    {
        bool jumpTrigger = playerInput.jumpTrigger;
        if (!jumpTrigger)
        {
            return;
        }
        
        // 只有在地上 且触发跳跃 能跳跃
        if (!groundDetectPoint.isGrounded)
        {
            Debug.Log("在空中无法跳跃");
            return;
        }
        
        // 处理跳跃输入
        rb.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.VelocityChange);
        
        // 重置玩家输入
        SetPlayerInput(playerInput.moveDir);
    }

    /// <summary>
    /// 服务器验证技能输入
    /// </summary>
    public void HandleSkillInputServerAuth(SkillType skillType)
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
                else if (Mathf.Approximately(curHP.Value, maxHP.Value))
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

    /// <summary>
    /// 处理玩家攻击 (服务器权威)
    /// </summary>
    private void Attack()
    {
        AttackServerRpc(lastMoveDir);

        StartAttackCoolDown();
    }

    [ServerRpc]
    private void AttackServerRpc(Vector3 shootDir)
    {
        if (!IsServer) return;
        
        Debug.Log(playerName + " 发射了一枚子弹");
        
        // Bullet bullet = (Bullet)PoolManager.Instance.ReuseComponent(bulletPrefab, shootPos.position, Quaternion.identity);
        // var bulletNetworkObject = NetworkPoolManager.Instance.ReuseAndShowNetworkObject(bulletPrefab, shootPos.position, Quaternion.identity);

        Transform bulletTransform = Instantiate(bulletPrefab);
        NetworkObject bulletNetworkObject = bulletTransform.GetComponent<NetworkObject>();
        bulletNetworkObject.Spawn();
        
        Bullet bullet = bulletNetworkObject.GetComponent<Bullet>();
        bullet.InitBullet(this, shootDir, shootPos.position);
    }
    
    private void StartAttackCoolDown()
    {
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
        float speed = moveSpeed.Value;
        ChangeMoveSpeedServerRpc(speed * (1 + acceleratePercent / 100f));
        Debug.Log("开始加速");
        
        yield return new WaitForSeconds(accelerateTime);
        
        ChangeMoveSpeedServerRpc(speed);
        Debug.Log("加速结束");
    }

    [ServerRpc]
    private void ChangeMoveSpeedServerRpc(float speed)
    {
        moveSpeed.Value = speed;
    }

    /// <summary>
    /// 回血技能 为玩家自身回复 50% 血量, cd 15s
    /// </summary>
    private void Heal()
    {
        UpdateHp(maxHP.Value / 2f, maxHP.Value);

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
        if (NetworkManager.Singleton.IsServer)
        {
            maxHP.Value = newMaxHP;
            curHP.Value = Mathf.Clamp(curHP.Value + deltaHP, 0f, maxHP.Value);
        }
        else if (IsOwner)
        {
            UpdateHpServerRpc(deltaHP, newMaxHP);    
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateHpServerRpc(float deltaHp, float newMaxHp)
    {
        maxHP.Value = newMaxHp;
        curHP.Value = Mathf.Clamp(curHP.Value + deltaHp, 0f, maxHP.Value);
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
        return atk.Value * (0.9f + level.Value * 0.1f);
    }

    /// <summary>
    /// 玩家受伤逻辑
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="source"></param>
    public void TakeDamage(float damage, IDamagable source = null)
    {
        UpdateHp(-damage, maxHP.Value);
        
        if (source != null)
        {
            Debug.Log($"玩家{playerName} 受到来自 {source.Name} 的 {damage} 点伤害, 血量 {curHP}/{maxHP}");
        }

        if (curHP.Value <= 0f)
        {
            curHP.Value = 0f;
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
            int transferredScore = (score.Value + 1) / 2;
            score.Value -= transferredScore;
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
        
        UpdateHp(maxHP.Value, maxHP.Value);
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

    public string Name => playerName;

    public int GetScore() => score.Value;

    private void SetScore(int newScore)
    {
        score.Value = newScore;
    }
    
    public int GetKillAmount() => killAmount.Value;
    public void IncreaseKillAmount()
    {
        killAmount.Value += 1;
        Debug.Log($"玩家{playerName}当前击杀数: {killAmount}");
    }

    /// <summary>
    /// 玩家拾取能量块逻辑 服务端处理
    /// </summary>
    private void OnEnergyBlockPicked(object sender, EventArgs eventArgs)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // 增加 5 积分
        SetScore(score.Value + 5);        
        
        // 增加 5 能量值
        AddEnergy(5);
    }

    private void AddEnergy(int increaseEnergy)
    {
        energy.Value += increaseEnergy;
        
        // 能量条满 且 玩家没到最大等级
        if (energy.Value >= maxEnergy.Value && level.Value < maxLevel)
        {
            level.Value++;
            UpdatePlayerLevelInfo();
        }
    }
}
