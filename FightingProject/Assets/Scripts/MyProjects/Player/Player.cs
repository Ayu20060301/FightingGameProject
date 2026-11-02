using Fusion;
using UnityEngine;


public class Player : NetworkBehaviour
{
    #region 定数・静的データ

    //接地中に与える押し付け速度
    private const float GroundedStickVelocity = -2.0f;

    //落下時の重力倍率
    private const float FallGravityMultiplier = 1.5f;

    //頭上に乗っている判定で、ほぼ真上とみなすX差
    private const float HeadOverlapThreshold = 0.05f;

    //向きの判定で、同じ位置とみなすX差
    private const float FacingThreshold = 0.001f;

    //モデルの正面がローカル+Zの場合のY回転
    private const float FaceRightYaw = 90.0f;
    private const float FaceLeftYaw = -90.0f;


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

    [Header("武器攻撃判定")]
    [SerializeField]
    private SwordHitBox m_SwordHitBox;

    #endregion


    #region フィールド・プロパティ

    //--- コンポーネント・参照 ---
    private CharacterController m_Controller;
    private RoundManager m_RoundManager;
    private Player m_Opponent;

    private PlayerAnimator m_PlayerAnim;

    //--- 入力・コマンド ---
    private InputState m_Input;
    private readonly CommandDetector m_Command = new CommandDetector();

    //--- 移動 ---
    private Vector3 m_Move;
    private float m_KnockbackVelocityX;

    //相手が離れる方向に入力しているか
    public bool IsMovingBackward
    {
        get
        {
            Direction dir = m_Input.Direction;
            bool isLeft = dir == Direction.Left || dir == Direction.UpLeft || dir == Direction.DownLeft;
            bool isRight = dir == Direction.Right || dir == Direction.UpRight || dir == Direction.DownRight;

            //右向きなら左入力が後退、左向きなら右入力が後退
            return IsFacingRight ? isLeft : isRight;
        }
    }

    //相手の頭に乗っているか・ずり落ちる向き(右= +1、左= -1)
    private bool m_IsOnOpponentHead;
    private float m_SlideOffDir;

    //--- 判定 ---
    private readonly Collider[] m_HitBuffer = new Collider[16];


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
        CurrentState == HitState;

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


    #region ジャンプ

    //定数
    private const int MaxJumpCount = 2;  //地上ジャンプ含む総ジャンプ回数

    //フィールド
    private int m_JumpCount;
    private bool m_IsPrevJumpInput;

    private bool m_IsJumpPressed;


    //プロパティ
    public bool IsJumpPressed => m_IsJumpPressed;

    //ジャンプ回数が残っているか
    public bool IsCanJump => m_Controller.isGrounded || m_JumpCount < MaxJumpCount;


    #endregion


    #region 通常攻撃

    private bool m_IsPrevNormalAttackInput;
    private bool m_IsNormalAttackPressed;


    public bool IsNormalAttackPressed => m_IsNormalAttackPressed;

    public int NormalComboLength => 1 + m_CharacterData.normalCombo.Length;

    public AttackData GetNormalComboData(int index)
        => index == 0 ? NormalAttackData : m_CharacterData.normalCombo[index - 1];
  
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

        m_PlayerAnim = new PlayerAnimator(m_Anim);

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

        //ジャンプ入力の立ち上がりを検出
        m_IsJumpPressed = data.isJump && !m_IsPrevJumpInput;
        m_IsPrevJumpInput = data.isJump;

        //通常攻撃入力の立ち上がりを検出(コンボ用)

        m_IsNormalAttackPressed = data.isNormalAttack && !m_IsPrevNormalAttackInput;
        m_IsPrevNormalAttackInput = data.isNormalAttack;

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
            m_JumpCount = 0;
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
    /// ジャンプ(回数を消費する)。できなければfalse
    /// </summary>
    public bool Jump()
    {
        if (!IsCanJump) return false;

        //地上から飛ぶ場合は1回目、空中から飛ぶ場合は回数を引き継いで加算
        m_JumpCount = Mathf.Max(m_JumpCount, m_Controller.isGrounded ? 0 : 1) + 1;

        //ジャンプ速度を設定
        m_Move.y = Mathf.Sqrt(m_CharacterData.jumpPower * -2.0f * m_Gravity);
        return true;
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
        if (!Object.HasStateAuthority)
            return false;

        if (data == null || m_SwordHitBox == null)
            return false;

        // 剣のBoxColliderからワールド座標を取得
        m_SwordHitBox.GetWorldBox(
            out Vector3 center,
            out Vector3 halfExtents,
            out Quaternion rotation
        );


        // 剣の形状でHurtBoxを検索
        Physics.SyncTransforms();

        int count = Physics.OverlapBoxNonAlloc(
            center,
            halfExtents,
            m_HitBuffer,
            rotation,
            m_HurtMask,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < count; i++)
        {
            if (!TryGetTarget(i, out Player target))
                continue;

            target.TakeDamage(
                data.damage,
                data.hitStunTicks,
                data.guardStunTicks,
                this.transform.position.x,
                data.knockback,
                data.guardKnockback,
                data.guardDamage
            );

            return true;
        }

        return false;
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

        EnterHitState(m_CharacterData.guardBreakStunTicks);
    }

    /// <summary>
    /// ガードゲージ回復までの待ち時間を開始し直す
    /// </summary>
    private void ResetGuardRecoverTimer()
    {
        GuardRecoverTimer = TickTimer.CreateFromSeconds(Runner, m_CharacterData.guardRecoverDelay);
    }

    #endregion


    #region アニメーション

    /// <summary>
    /// アニメーション開始イベントをRPCで通知
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayAnimation(AnimationState state)
    {
        m_PlayerAnim.Play(state);
    }

    public void SetAnimationSpeed(float speed) => m_PlayerAnim.SetSpeed(speed);

    public float GetAnimationNormalizedTime(AnimationState state)
        => m_PlayerAnim.GetNormalizedTime(state);


    //各攻撃アニメーションの終了判定
    public bool IsNormalAttackFinished() => m_PlayerAnim.IsFinished(AnimationState.NormalAttack);
    public bool IsNormalAttack2Finished() => m_PlayerAnim.IsFinished(AnimationState.NormalAttack2);
    public bool IsNormalAttack3Finished() => m_PlayerAnim.IsFinished(AnimationState.NormalAttack3);
    public bool IsLowAttackFinished() => m_PlayerAnim.IsFinished(AnimationState.LowAttack);
    public bool IsUpAttackFinished() => m_PlayerAnim.IsFinished(AnimationState.UpAttack);
    public bool IsForwardAttackFinished() => m_PlayerAnim.IsFinished(AnimationState.ForwardAttack);


    #endregion
}
