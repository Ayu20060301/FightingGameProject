using Fusion;
using System.Collections.Generic;
using UnityEngine;


public class Player : NetworkBehaviour
{
    public enum AnimationState
    {
        Idle,
        Walk,       //歩行
        BackWalk,   //バック移動
        Guard,      //ガード
        Crouch,     //しゃがみ
        Jump,       //ジャンプ
        Land,       //着地

        //攻撃
        NormalAttack,   //通常攻撃
        LowAttack,      //下攻撃
        UpAttack,       //上攻撃
        ForwardAttack,  //前攻撃

        //コマンド攻撃
        Tatsumaki,      //竜巻旋風脚

        //ダメージを受けた際の各アニメーション
        Hit,            //立ち喰らい
        GuardHit,       //立ちガード喰らい

        //掴み
        Grab,           //掴み
        Throw,          //投げ
        Grabbed,        //掴まれ中
    }


    #region 定数・静的データ

    //接地中に与える押し付け速度
    private const float GroundedStickVelocity = -2.0f;

    //落下時の重力倍率
    private const float FallGravityMultiplier = 1.5f;

    //アニメーションのクロスフェード時間
    private const float CrossFadeDuration = 0.05f;

    //頭上に乗っている判定で、ほぼ真上とみなすX差
    private const float HeadOverlapThreshold = 0.05f;

    //向きの判定で、同じ位置とみなすX差
    private const float FacingThreshold = 0.001f;

    //モデルの正面がローカル+Zの場合のY回転
    private const float FaceRightYaw = 90.0f;
    private const float FaceLeftYaw = -90.0f;

    //デバッグ表示時間
    private const float DebugGizmoDuration = 0.2f;

    //Animatorパラメーター
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsGuardParam = Animator.StringToHash("IsGuard");
    private static readonly int IsCrouchParam = Animator.StringToHash("IsCrouch");

    //AnimationState→Animatorのステート名
    private static readonly Dictionary<AnimationState, string> AnimationNames = new Dictionary<AnimationState, string>
    {
        { AnimationState.Idle,          "Idle"},
        { AnimationState.Walk,          "Walk"},
        { AnimationState.BackWalk,      "BackWalk"},
        { AnimationState.Guard,         "Guard"},
        { AnimationState.Crouch,        "Crouch"},
        { AnimationState.Jump,          "Jump"},
        { AnimationState.Land,          "Land"},
        { AnimationState.NormalAttack,  "NormalAttack"},
        { AnimationState.LowAttack,     "LowAttack"},
        { AnimationState.UpAttack,      "UpAttack"},
        { AnimationState.ForwardAttack, "ForwardAttack"},
        { AnimationState.Tatsumaki,     "Tatsumaki"},
        { AnimationState.Hit,           "Hit"},
        { AnimationState.GuardHit,      "GuardHit"},
        { AnimationState.Grab,          "Grab"},
        { AnimationState.Throw,         "Throw"},
        { AnimationState.Grabbed,       "Grabbed"},
    };

    //AttackSlot順に対応するGizmoの色
    private static readonly Color[] GizmoColors =
    {
        Color.yellow, new Color(1.0f, 0.6f, 0.0f), Color.red, Color.cyan,
        Color.blue, Color.magenta, Color.green, Color.white,
    };

    #endregion


    #region Inspector

    [Header("キャラクター")]
    [SerializeField]
    private CharacterData m_CharacterData;

    [Header("重力")]
    [SerializeField]
    private float m_Gravity = -9.8f;

    [Header("アニメーション")]
    [SerializeField]
    private Animator m_Anim;

    [Header("ノックバック")]
    [SerializeField]
    private float m_KnockbackDecay = 20.0f;

    [Header("頭上からのすり落ち")]
    [SerializeField]
    private float m_SlideOffSpeed = 3.0f;   //ずり落ちる速さ
    [SerializeField, Range(0.0f, 1.0f)]
    private float m_StandOnNormalY = 0.5f;  //接触面の法線Yがこれ以上なら「乗っている」とみなす

    [Header("当たり判定")]
    [SerializeField]
    private LayerMask m_HurtMask;

    #endregion


    #region フィールド・プロパティ

    //--- コンポーネント・参照 ---
    private CharacterController m_Controller;
    private RoundManager m_RoundManager;
    private Player m_Opponent;

    //--- 入力・コマンド ---
    private InputState m_Input;
    private readonly CommandDetector m_Command = new CommandDetector();

    //--- 移動 ---
    private Vector3 m_Move;
    private float m_KnockbackVelocityX;

    //相手の頭に乗っているか・ずり落ちる向き(右= +1、左= -1)
    private bool m_IsOnOpponentHead;
    private float m_SlideOffDir;

    //--- 判定 ---
    private readonly Collider[] m_HitBuffer = new Collider[4];

    //デバッグ表示用
    private Vector3 m_DebugCenter;
    private Vector3 m_DebugSize;
    private float m_DebugExpireTime;

    //--- StateMachine ---
    public PlayerStateMachine m_StateMachine;

    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerGuardState GuardState { get; private set; }
    public PlayerCrouchState CrouchState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerNormalAttackState NormalAttackState { get; private set; }
    public PlayerLowAttackState LowAttackState { get; private set; }
    public PlayerUpAttackState UpAttackState { get; private set; }
    public PlayerForwardAttackState ForwardAttackState { get; private set; }
    public PlayerHitState HitState { get; private set; }
    public PlayerGuardHitState GuardHitState { get; private set; }
    public PlayerGrabState GrabState { get; private set; }
    public PlayerGrabbedState GrabbedState { get; private set; }

    //--- キャラクターデータ ---
    public CharacterData Data => m_CharacterData;
    public int MaxHP => m_CharacterData.maxHP;
    public float MaxGuardGauge => m_CharacterData.maxGuardGauge;

    public AttackData NormalAttackData => m_CharacterData.GetAttack(AttackSlot.NormalAttack);
    public AttackData LowAttackData => m_CharacterData.GetAttack(AttackSlot.LowAttack);
    public AttackData UpAttackData => m_CharacterData.GetAttack(AttackSlot.UpAttack);
    public AttackData ForwardAttackData => m_CharacterData.GetAttack(AttackSlot.ForwardAttack);
    public AttackData RushMoveData => m_CharacterData.GetAttack(AttackSlot.RushMove);
    public AttackData GeigekiData => m_CharacterData.GetAttack(AttackSlot.Geigeki);
    public AttackData AirMoveData => m_CharacterData.GetAttack(AttackSlot.AirMove);
    public AttackData DownMoveData => m_CharacterData.GetAttack(AttackSlot.DownMove);

    //掴みデータ(既存のStateから参照されているため維持)
    public GrabData GrabData => m_CharacterData.grab;

    //--- 状態参照 ---
    public bool IsGrounded => m_Controller.isGrounded;
    public InputState Input => m_Input;
    public PlayerState CurrentState => m_StateMachine.CurrentState;

    //向き(右= +1、左= -1)
    public float FacingDir => this.transform.forward.x >= 0.0f ? 1.0f : -1.0f;
    public bool IsFacingRight => FacingDir > 0.0f;

    //KOしているか(HPはサーバーから同期されるので全クライアントで判定できる)
    public bool IsKO => HP <= 0;

    //ガード構え中、またはガード喰らい中か
    private bool IsGuarding => CurrentState == GuardState || CurrentState == GuardHitState;

    //左右移動出来ない状態か
    private bool IsHorizontalMoveLocked =>
        CurrentState == GuardState ||
        CurrentState == GuardHitState ||
        CurrentState == CrouchState ||
        CurrentState == NormalAttackState ||
        CurrentState == LowAttackState ||
        CurrentState == UpAttackState ||
        CurrentState == ForwardAttackState ||
        CurrentState == HitState ||
        CurrentState == GrabState ||
        CurrentState == GrabbedState;

    //--- ネットワーク同期 ---

    /// <summary>
    /// ネットワーク同期する向き
    /// </summary>
    [Networked]
    private float FacingYaw { get; set; }

    /// <summary>
    /// ネットワーク同期するHP
    /// </summary>
    [Networked]
    public int HP { get; set; }

    #endregion


    #region ガード(ネットワーク同期)

    /// <summary>
    /// ガードクラッシュ中か
    /// </summary>
    [Networked]
    public NetworkBool IsGuardBroken { get; set; }

    public bool IsCanGuard => !IsGuardBroken;

    /// <summary>
    /// ネットワーク同期するガードゲージ
    /// </summary>
    [Networked]
    public float GuardGauge { get; set; }

    /// <summary>
    /// 回復までの待ち時間
    /// </summary>
    [Networked]
    private TickTimer GuardRecoverTimer { get; set; }

    #endregion


    #region Unity / Fusion コールバック

    private void Awake()
    {
        if (m_CharacterData == null)
        {
            Debug.LogError($"[Player] CharacterDataが未設定です : {name}", this);
        }

        m_Controller = GetComponent<CharacterController>();
        m_Input = new InputState();
        m_StateMachine = new PlayerStateMachine();

        CreateStates();
    }


    public override void Spawned()
    {
        //全クライアントでカメラに登録
        RegisterToCamera(true);

        if (!Object.HasStateAuthority) return;

        HP = MaxHP;
        GuardGauge = MaxGuardGauge;
        FacingYaw = this.transform.eulerAngles.y;

        //最初はIdle
        ChangeState(IdleState);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        RegisterToCamera(false);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            UpdateFacing();
            UpdateGuardGauge();
            UpdateAuthority();
        }

        //同期された向きを各クライアントに適用
        this.transform.rotation = Quaternion.Euler(0.0f, FacingYaw, 0.0f);
    }

    #endregion


    #region 初期化

    /// <summary>
    /// Stateの生成
    /// </summary>
    private void CreateStates()
    {
        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        GuardState = new PlayerGuardState(this);
        CrouchState = new PlayerCrouchState(this);
        JumpState = new PlayerJumpState(this);
        NormalAttackState = new PlayerNormalAttackState(this);
        LowAttackState = new PlayerLowAttackState(this);
        UpAttackState = new PlayerUpAttackState(this);
        ForwardAttackState = new PlayerForwardAttackState(this);
        HitState = new PlayerHitState(this);
        GuardHitState = new PlayerGuardHitState(this);
        GrabState = new PlayerGrabState(this);
        GrabbedState = new PlayerGrabbedState(this);
    }

    /// <summary>
    /// FightCameraの追従対象へ登録・解除する
    /// </summary>
    private void RegisterToCamera(bool isAdd)
    {
        FightCamera cam = FindFirstObjectByType<FightCamera>();
        if (cam == null) return;

        if (isAdd)
        {
            cam.AddTarget(this.transform);
        }
        else
        {
            cam.RemoveTarget(this.transform);
        }
    }

    #endregion


    #region 入力・更新(StateAuthority)

    /// <summary>
    /// 入力取得 → State更新 → 移動(StateAuthority側のみ)
    /// </summary>
    private void UpdateAuthority()
    {
        //入力が取得できた場合のみゲーム処理を実行
        if (!GetInput(out NetworkManager.PlayerInputData data)) return;

        //試合中以外は入力を無効化
        if (!IsFightPhase())
        {
            data = default;
        }

        UpdateInput(data);
        m_StateMachine.Update();
        UpdateMovement(data);
    }

    /// <summary>
    /// 試合中(Fightフェーズ)か
    /// </summary>
    private bool IsFightPhase()
    {
        if (m_RoundManager == null)
        {
            m_RoundManager = FindFirstObjectByType<RoundManager>();
        }

        return m_RoundManager != null && m_RoundManager.Phase == RoundManager.RoundPhase.Fight;
    }

    /// <summary>
    /// 入力状態を更新する
    /// </summary>
    private void UpdateInput(NetworkManager.PlayerInputData data)
    {
        Direction direction = ConvertDirection(data.horizontal, data.vertical);

        //ガードクラッシュ中はガード入力を無効化
        bool isGuard = data.isGuard && IsCanGuard;

        m_Input.SetInput(direction,
                         data.isNormalAttack,
                         data.isLowAttack,
                         data.isUpAttack,
                         data.isForwardAttack,
                         data.isJump,
                         isGuard);
    }

    /// <summary>
    /// NetworkInputをDirectionへ変換する
    /// </summary>
    private static Direction ConvertDirection(float horizontal, float vertical)
    {
        int h = horizontal > 0.0f ? 1 : (horizontal < 0.0f ? -1 : 0);
        int v = vertical > 0.0f ? 1 : (vertical < 0.0f ? -1 : 0);

        switch (h, v)
        {
            case (-1, 1): return Direction.UpLeft;
            case (1, 1): return Direction.UpRight;
            case (-1, -1): return Direction.DownLeft;
            case (1, -1): return Direction.DownRight;
            case (-1, 0): return Direction.Left;
            case (1, 0): return Direction.Right;
            case (0, 1): return Direction.Up;
            case (0, -1): return Direction.Down;
            default: return Direction.Neutral;
        }
    }

    #endregion


    #region 移動・向き

    /// <summary>
    /// 移動処理
    /// </summary>
    private void UpdateMovement(NetworkManager.PlayerInputData data)
    {
        //接地中は落下速度をリセット
        if (m_Controller.isGrounded && m_Move.y < 0.0f)
        {
            m_Move.y = GroundedStickVelocity;
        }

        //左右移動(行動制限中は入力分を停止)
        float inputX = IsHorizontalMoveLocked ? 0.0f : data.horizontal * m_CharacterData.speed;

        //相手の頭に乗っている場合は、行動制限中でも左右にずり落とす
        float slideX = m_IsOnOpponentHead ? m_SlideOffDir * m_SlideOffSpeed : 0.0f;

        //Moveの最中にOnControllerColliderHitで再度セットされるので、先にリセットする
        m_IsOnOpponentHead = false;

        //入力 + ノックバック + ずり落ちを合成
        m_Move.x = inputX + m_KnockbackVelocityX + slideX;

        //ノックバックを0へ向けて直線的に減衰
        m_KnockbackVelocityX = Mathf.MoveTowards(m_KnockbackVelocityX, 0.0f, m_KnockbackDecay * Runner.DeltaTime);

        //重力(落下中は強める)
        float gravity = m_Move.y > 0.0f ? m_Gravity : m_Gravity * FallGravityMultiplier;
        m_Move.y += gravity * Runner.DeltaTime;

        m_Controller.Move(m_Move * Runner.DeltaTime);
    }

    /// <summary>
    /// ジャンプ
    /// </summary>
    public void Jump()
    {
        if (!m_Controller.isGrounded) return;

        //ジャンプ速度を設定
        m_Move.y = Mathf.Sqrt(m_CharacterData.jumpPower * -2.0f * m_Gravity);
    }

    /// <summary>
    /// ジャンプが終了したか
    /// </summary>
    public bool IsJumpFinished()
    {
        //地面に着地し、落下が終わったらジャンプ終了
        return m_Controller.isGrounded && m_Move.y <= 0.0f;
    }

    /// <summary>
    /// 相手の位置に応じて向きを更新する
    /// </summary>
    private void UpdateFacing()
    {
        if (m_Opponent == null) return;

        float deltaX = m_Opponent.transform.position.x - this.transform.position.x;
        if (Mathf.Abs(deltaX) < FacingThreshold) return;

        FacingYaw = deltaX > 0.0f ? FaceRightYaw : FaceLeftYaw;
    }

    /// <summary>
    /// 攻撃者から離れる方向にノックバックを与える
    /// </summary>
    private void ApplyKnockback(float attackerX, float power)
    {
        float dir = this.transform.position.x >= attackerX ? 1.0f : -1.0f;
        m_KnockbackVelocityX = dir * power;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //移動を行っているStateAuthority側だけで判定
        if (!Object.HasStateAuthority) return;

        //上から乗っている接触だけを対象にする
        if (hit.normal.y < m_StandOnNormalY) return;

        //相手プレイヤーか
        Player other = hit.collider.GetComponentInParent<Player>();
        if (other == null || other == this) return;

        float dx = this.transform.position.x - other.transform.position.x;

        //相手から離れる方向。ほぼ真上に乗った場合は来た側へ
        //(相手の向こう側へ抜けさせたい場合は符号を反転させる)
        float dir = Mathf.Abs(dx) > HeadOverlapThreshold
            ? (dx > 0.0f ? 1.0f : -1.0f)
            : -FacingDir;

        m_IsOnOpponentHead = true;
        m_SlideOffDir = dir;
    }

    #endregion


    #region State / 対戦相手

    /// <summary>
    /// Stateの変更
    /// </summary>
    public void ChangeState(PlayerState state)
    {
        m_StateMachine.ChangeState(state);
    }

    /// <summary>
    /// 対戦相手を登録
    /// </summary>
    public void SetOpponent(Player opponent)
    {
        m_Opponent = opponent;
    }

    #endregion


    #region ダメージ

    /// <summary>
    /// ダメージを受ける(StateAuthority側のみ実行)
    /// </summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="hitStunTicks">喰らい硬直(Tick数)</param>
    /// <param name="guardStunTicks">ガード硬直(Tick数)</param>
    public void TakeDamage(int damage, int hitStunTicks, int guardStunTicks, float attackerX, float knockback, float guardKnockback, float guardDamage)
    {
        if (!Object.HasStateAuthority) return;

        //KO済みなら以降の攻撃は無視
        if (IsKO) return;

        if (IsGuarding)
        {
            TakeGuardedHit(guardStunTicks, attackerX, knockback, guardKnockback, guardDamage);
        }
        else
        {
            TakeUnguardedHit(damage, hitStunTicks, attackerX, knockback);
        }
    }

    /// <summary>
    /// 投げのダメージ適用
    /// </summary>
    public void TakeThrowDamage(int damage, int hitStunTicks, float attackerX, float knockback)
    {
        if (!Object.HasStateAuthority) return;

        //KO済みなら以降の攻撃は無視
        if (IsKO) return;

        TakeUnguardedHit(damage, hitStunTicks, attackerX, knockback);
    }

    /// <summary>
    /// ガード成立時の処理
    /// </summary>
    private void TakeGuardedHit(int guardStunTicks, float attackerX, float knockback, float guardKnockback, float guardDamage)
    {
        GuardGauge -= guardDamage;
        ResetGuardRecoverTimer();

        //ゲージが尽きたらガードクラッシュ
        if (GuardGauge <= 0.0f)
        {
            ApplyKnockback(attackerX, knockback);
            BreakGuard();
            return;
        }

        //通常のガード成立
        ApplyKnockback(attackerX, guardKnockback);

        GuardHitState.Setup(guardStunTicks);

        if (CurrentState == GuardHitState)
        {
            GuardHitState.Restart();
        }
        else
        {
            ChangeState(GuardHitState);
        }
    }

    /// <summary>
    /// ガードされなかった場合(通常の喰らい・投げ)の処理
    /// </summary>
    private void TakeUnguardedHit(int damage, int hitStunTicks, float attackerX, float knockback)
    {
        HP = Mathf.Max(0, HP - damage);
        ApplyKnockback(attackerX, knockback);

        //HPが0になったらKO
        if (IsKO)
        {
            //ChangeState(KOState);
            return;
        }

        EnterHitState(hitStunTicks);
    }

    /// <summary>
    /// 喰らい状態へ遷移する(喰らい中なら硬直とアニメーションをやり直す)
    /// </summary>
    private void EnterHitState(int hitStunTicks)
    {
        HitState.Setup(AnimationState.Hit, hitStunTicks);

        if (CurrentState == HitState)
        {
            HitState.Restart();
        }
        else
        {
            ChangeState(HitState);
        }
    }

    #endregion


    #region 攻撃・掴み判定

    /// <summary>
    /// 攻撃判定を行い、当たったらtrueを返す
    /// </summary>
    public bool CheckAttackHit(AttackData data)
    {
        Vector3 center = data.GetWorldCenter(this.transform.position, FacingDir);

        //デバッグ表示用に記録
        m_DebugCenter = center;
        m_DebugSize = data.size;
        m_DebugExpireTime = Time.time + DebugGizmoDuration;

        int count = OverlapHurtBoxes(center, data.size);

        for (int i = 0; i < count; i++)
        {
            if (!TryGetTarget(i, out Player target)) continue;

            target.TakeDamage(
                data.damage,
                data.hitStunTicks,
                data.guardStunTicks,
                this.transform.position.x,
                data.knockback,
                data.guardKnockback,
                data.guardDamage);

            //1回の判定で当てるのは1回まで
            return true;
        }

        return false;
    }

    /// <summary>
    /// 掴み判定を行い、掴めたらtrueを返す
    /// </summary>
    public bool CheckGrabHit(GrabData data)
    {
        Vector3 center = data.GetWorldCenter(this.transform.position, FacingDir);

        int count = OverlapHurtBoxes(center, data.size);

        for (int i = 0; i < count; i++)
        {
            if (!TryGetTarget(i, out Player target)) continue;

            //掴めたかどうかは相手側が判断する(喰らい中・空中などは失敗)
            if (target.TakeGrab(this, data))
            {
                return true;
            }
        }

        return false;
    }

    public bool TakeGrab(Player attacker, GrabData data)
    {
        if (!Object.HasStateAuthority) return false;
        if (IsKO) return false;

        //掴めない状態(掴まれ中・喰らい中・空中)
        if (CurrentState == GrabbedState || CurrentState == HitState || !IsGrounded)
        {
            return false;
        }

        //ガード中・ガード喰らい中でも成立させる
        GrabbedState.Setup(attacker, data);
        ChangeState(GrabbedState);
        return true;
    }

    /// <summary>
    /// 指定範囲のHurtBoxを判定バッファへ取得し、ヒット数を返す
    /// </summary>
    private int OverlapHurtBoxes(Vector3 center, Vector3 size)
    {
        //CharacterControllerで動かした直後のColliderの位置をPhysicsへ反映
        Physics.SyncTransforms();

        //喰らい判定はTriggerなので明示する
        return Physics.OverlapBoxNonAlloc(
            center,
            size * 0.5f,
            m_HitBuffer,
            Quaternion.identity,
            m_HurtMask,
            QueryTriggerInteraction.Collide);
    }

    /// <summary>
    /// 判定バッファのi番目から、自分以外のPlayerを取得する
    /// </summary>
    private bool TryGetTarget(int index, out Player target)
    {
        target = null;

        HurtBox hurtBox = m_HitBuffer[index].GetComponent<HurtBox>();
        if (hurtBox == null || hurtBox.Owner == null || hurtBox.Owner == this)
        {
            return false;
        }

        target = hurtBox.Owner;
        return true;
    }

    #endregion


    #region ガード処理

    /// <summary>
    /// ガードゲージの消費・自然回復
    /// </summary>
    private void UpdateGuardGauge()
    {
        //ガード構え中は継続的に減少
        if (CurrentState == GuardState)
        {
            GuardGauge = Mathf.Max(0.0f, GuardGauge - m_CharacterData.guardDrainPerSecond * Runner.DeltaTime);

            //構えている間は、回復開始までの待ち時間を更新し続ける
            ResetGuardRecoverTimer();

            if (GuardGauge <= 0.0f)
            {
                BreakGuard();
            }

            return;
        }

        //ガード喰らい中は回復も減少もしない
        if (CurrentState == GuardHitState) return;

        //待ち時間中は回復しない
        if (!GuardRecoverTimer.ExpiredOrNotRunning(Runner)) return;

        GuardGauge = Mathf.Min(MaxGuardGauge, GuardGauge + m_CharacterData.guardRecoverPerSecond * Runner.DeltaTime);

        //一定量まで回復したらガード可能に戻す
        if (IsGuardBroken && GuardGauge >= MaxGuardGauge * m_CharacterData.guardUnlockRate)
        {
            IsGuardBroken = false;
        }
    }

    /// <summary>
    /// ガードクラッシュ
    /// </summary>
    private void BreakGuard()
    {
        GuardGauge = 0.0f;
        IsGuardBroken = true;

        //回復開始までの待ち時間
        ResetGuardRecoverTimer();

        RPC_GuardBreakCamera();

        EnterHitState(m_CharacterData.guardBreakStunTicks);
    }

    /// <summary>
    /// ガードゲージ回復までの待ち時間を開始し直す
    /// </summary>
    private void ResetGuardRecoverTimer()
    {
        GuardRecoverTimer = TickTimer.CreateFromSeconds(Runner, m_CharacterData.guardRecoverDelay);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_GuardBreakCamera()
    {
        if (GuardBreakCamera.Instance != null)
        {
            GuardBreakCamera.Instance.Play(this);
        }
    }

    #endregion


    #region アニメーション

    /// <summary>
    /// アニメーション開始イベントをRPCで通知
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayAnimation(AnimationState state)
    {
        PlayAnimationLocal(state);
    }

    /// <summary>
    /// 各クライアントでアニメーションを再生
    /// </summary>
    private void PlayAnimationLocal(AnimationState state)
    {
        ResetAnimationParameters();
        ApplyAnimationParameters(state);

        if (AnimationNames.TryGetValue(state, out string stateName))
        {
            m_Anim.CrossFade(stateName, CrossFadeDuration);
        }
    }

    /// <summary>
    /// アニメーションの基本パラメーターをリセット
    /// </summary>
    private void ResetAnimationParameters()
    {
        m_Anim.SetFloat(SpeedParam, 0.0f);
        m_Anim.SetBool(IsGuardParam, false);
        m_Anim.SetBool(IsCrouchParam, false);
    }

    /// <summary>
    /// ステートごとに必要なAnimatorパラメーターを設定
    /// </summary>
    private void ApplyAnimationParameters(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.Walk:
            case AnimationState.BackWalk:
                m_Anim.SetFloat(SpeedParam, 1.0f);
                break;
            case AnimationState.Guard:
                m_Anim.SetBool(IsGuardParam, true);
                break;
            case AnimationState.Crouch:
                m_Anim.SetBool(IsCrouchParam, true);
                break;
        }
    }

    /// <summary>
    /// アニメーションの再生速度を変更する
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        if (m_Anim != null)
        {
            m_Anim.speed = speed;
        }
    }

    /// <summary>
    /// アニメーション終了判定
    /// </summary>
    private bool IsAnimationFinished(AnimationState state)
    {
        string stateName = AnimationNames[state];

        //Animatorが遷移中なら終了判定しない
        if (m_Anim.IsInTransition(0))
        {
            LogAnimation($"{stateName} : 遷移中");
            return false;
        }

        AnimatorStateInfo stateInfo = m_Anim.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName(stateName))
        {
            LogAnimationWarning($"ステート名不一致 : 期待={stateName}, 現在のhash={stateInfo.fullPathHash}");
            return false;
        }

        if (stateInfo.loop)
        {
            LogAnimationWarning($"{stateName} : Loop設定がON");
            return false;
        }

        bool isFinished = stateInfo.normalizedTime >= 1.0f;

        if (isFinished)
        {
            LogAnimation($"{stateName} : 再生終了 normalizedTime = {stateInfo.normalizedTime:F2}");
        }

        return isFinished;
    }

    /// <summary>
    /// 指定アニメーションの正規化再生時間を取得する(0.0 = 開始、1.0 = 1周完了)
    /// </summary>
    public float GetAnimationNormalizedTime(AnimationState state)
    {
        if (m_Anim == null) return 0.0f;

        //遷移中は再生時間を取得しない
        if (m_Anim.IsInTransition(0)) return 0.0f;

        if (!AnimationNames.TryGetValue(state, out string stateName)) return 0.0f;

        AnimatorStateInfo stateInfo = m_Anim.GetCurrentAnimatorStateInfo(0);

        //指定したアニメーション以外なら取得しない
        if (!stateInfo.IsName(stateName)) return 0.0f;

        return stateInfo.normalizedTime;
    }

    //各攻撃アニメーションの終了判定
    public bool IsTatsumakiFinished() => IsAnimationFinished(AnimationState.Tatsumaki);
    public bool IsNormalAttackFinished() => IsAnimationFinished(AnimationState.NormalAttack);
    public bool IsLowAttackFinished() => IsAnimationFinished(AnimationState.LowAttack);
    public bool IsUpAttackFinished() => IsAnimationFinished(AnimationState.UpAttack);
    public bool IsForwardAttackFinished() => IsAnimationFinished(AnimationState.ForwardAttack);

    //エディタ上のみログ出力
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void LogAnimation(string message) => Debug.Log($"[Animation] {message}");

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void LogAnimationWarning(string message) => Debug.LogWarning($"[Animation] {message}");

    #endregion


    #region デバッグ表示

    //判定を出した直後だけ赤い箱を表示
    private void OnDrawGizmos()
    {
        if (Time.time > m_DebugExpireTime || m_DebugSize == Vector3.zero) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(m_DebugCenter, m_DebugSize);
    }

    //Playerを選択している間は、全技の判定範囲を色分けして表示
    private void OnDrawGizmosSelected()
    {
        if (m_CharacterData == null) return;

        for (int i = 0; i < GizmoColors.Length; i++)
        {
            DrawAttackGizmo(m_CharacterData.GetAttack((AttackSlot)i), GizmoColors[i]);
        }
    }

    private void DrawAttackGizmo(AttackData data, Color color)
    {
        if (data == null) return;

        Gizmos.color = color;
        Gizmos.DrawWireCube(data.GetWorldCenter(this.transform.position, FacingDir), data.size);
    }

    #endregion
}
