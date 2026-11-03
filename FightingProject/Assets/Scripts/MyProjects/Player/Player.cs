using Fusion;
using UnityEngine;


public class Player : NetworkBehaviour
{
    #region 定数

    //--- 移動・重力 ---
    //接地中に与える押し付け速度
    private const float GroundedStickVelocity = -2.0f;

    //落下時の重力倍率
    private const float FallGravityMultiplier = 1.5f;

    //--- 向き ---
    //向きの判定で、同じ位置とみなすX差
    private const float FacingThreshold = 0.001f;

    //モデルの正面がローカル+Zの場合のY回転
    private const float FaceRightYaw = 90.0f;
    private const float FaceLeftYaw = -90.0f;

    //--- 押し出し(相手との重なり解消) ---
    //ほぼ真上・真下に重なっているとみなすX差
    private const float OverlapCenterThreshold = 0.05f;

    //押し出しの最大速度
    private const float PushOutSpeed = 10.0f;

    //--- 攻撃・ステップ ---
    //攻撃中に踏み込みを許可する区間(アニメーションの正規化時間)
    private const float AttackStepEndTime = 0.3f;

    //ステップ入力として扱うスティックの倒し量
    private const float StepInputThreshold = 0.5f;

    //2回目の入力を受け付ける時間
    private const float StepInputWindow = 0.25f;

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

    [Header("当たり判定")]
    [SerializeField]
    private LayerMask m_HurtMask;

    [Header("武器攻撃判定")]
    [SerializeField]
    private SwordHitBox m_SwordHitBox;

    [Header("攻撃中の踏み込み")]
    [SerializeField]
    private float m_AttackStepSpeed = 2.0f;  //攻撃中に前入力した際の移動速度

    [Header("ステップ")]
    [SerializeField]
    private float m_StepSpeed = 8.0f;        //ステップ中の移動速度

    [SerializeField]
    private float m_StepDuration = 0.15f;    //ステップが続く時間(秒)

    #endregion


    #region フィールド

    //--- コンポーネント・参照 ---
    private CharacterController m_Controller;
    private RoundManager m_RoundManager;
    private Player m_Opponent;
    private PlayerAnimator m_PlayerAnim;

    //--- 入力 ---
    private InputState m_Input;
    private int m_HorizontalSign;        //今フレームの左右入力(-1,0,1)

    private bool m_IsPrevJumpInput;
    private bool m_IsJumpPressed;

    private bool m_IsPrevNormalAttackInput;
    private bool m_IsNormalAttackPressed;

    //--- 移動 ---
    private Vector3 m_Move;
    private float m_KnockbackVelocityX;

    //--- ステップ入力 ---
    private int m_PrevHorizontalDir;     //前フレームのスティック方向(-1,0,1)
    private int m_StepTapDir;            //1回目にタップした方向
    private float m_StepTapTimer;        //2回目を受け付ける残り時間

    //--- 判定 ---
    private readonly Collider[] m_HitBuffer = new Collider[16];

    #endregion


    #region ステートマシン

    public PlayerStateMachine m_StateMachine;

    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerGuardState GuardState { get; private set; }
    public PlayerCrouchState CrouchState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerNormalAttackState NormalAttackState { get; private set; }
    public PlayerLowAttackState LowAttackState { get; private set; }
    public PlayerForwardAttackState ForwardAttackState { get; private set; }
    public PlayerHitState HitState { get; private set; }
    public PlayerGuardHitState GuardHitState { get; private set; }
    public PlayerStepState StepState { get; private set; }

    public PlayerState CurrentState => m_StateMachine.CurrentState;

    /// <summary>
    /// Stateの変更
    /// </summary>
    public void ChangeState(PlayerState state)
    {
        m_StateMachine.ChangeState(state);
    }

    #endregion


    #region ネットワーク同期

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

    /// <summary>
    /// ガードクラッシュ中か
    /// </summary>
    [Networked]
    public NetworkBool IsGuardBroken { get; set; }

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


    #region 公開プロパティ

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
    public InputState Input => m_Input;
    public bool IsGrounded => m_Controller.isGrounded;

    //向き(右= +1、左= -1)
    public float FacingDir => this.transform.forward.x >= 0.0f ? 1.0f : -1.0f;
    public bool IsFacingRight => FacingDir > 0.0f;

    //相手から離れる方向に入力しているか(右向きなら左入力、左向きなら右入力が後退)
    public bool IsMovingBackward => m_HorizontalSign * FacingDir < 0.0f;

    //KOしているか(HPはサーバーから同期されるので全クライアントで判定できる)
    public bool IsKO => HP <= 0;

    public bool IsCanGuard => !IsGuardBroken;

    //--- 入力の立ち上がり ---
    public bool IsJumpPressed => m_IsJumpPressed;
    public bool IsNormalAttackPressed => m_IsNormalAttackPressed;

    //--- ジャンプ ---
    //1段ジャンプのみなので、接地していればジャンプ可能
    public bool IsCanJump => m_Controller.isGrounded;

    //ステップ中か
    public bool IsStepping => CurrentState == StepState;

    #endregion


    #region State判定ヘルパー

    //ガード構え中、またはガード喰らい中か
    private bool IsGuarding => CurrentState == GuardState || CurrentState == GuardHitState;

    //踏み込み・移動ロックの対象になる攻撃State
    private bool IsAttackState =>
        CurrentState == NormalAttackState ||
        CurrentState == LowAttackState ||
        CurrentState == ForwardAttackState;

    //左右移動出来ない状態か(攻撃は地上のときだけロック。空中攻撃では空中制御を残す)
    private bool IsHorizontalMoveLocked =>
        CurrentState == GuardState ||
        CurrentState == GuardHitState ||
        CurrentState == CrouchState ||
        CurrentState == HitState ||
        CurrentState == StepState ||
        (IsAttackState && m_Controller.isGrounded);

    //攻撃中に前入力で踏み込めるか(地上攻撃のみ)
    private bool IsAttackStepState => IsAttackState && m_Controller.isGrounded;

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
        ForwardAttackState = new PlayerForwardAttackState(this);
        HitState = new PlayerHitState(this);
        GuardHitState = new PlayerGuardHitState(this);
        StepState = new PlayerStepState(this);
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

    /// <summary>
    /// 対戦相手を登録
    /// </summary>
    public void SetOpponent(Player opponent)
    {
        m_Opponent = opponent;

        //CharacterController同士は衝突させない(頭に乗れなくする。重なりはResolveOverlapで解消)
        if (m_Opponent != null)
        {
            Physics.IgnoreCollision(m_Controller, m_Opponent.m_Controller);
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
        UpdateStepInput(data.horizontal);
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

        //通常攻撃入力の立ち上がりを検出
        m_IsNormalAttackPressed = data.isNormalAttack && !m_IsPrevNormalAttackInput;
        m_IsPrevNormalAttackInput = data.isNormalAttack;

        m_HorizontalSign = ToSign(data.horizontal);
        Direction direction = ConvertDirection(m_HorizontalSign, ToSign(data.vertical));

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
    /// 同じ方向への2回入力を検出してステップを開始する
    /// </summary>
    private void UpdateStepInput(float horizontal)
    {
        int dir = ToSign(horizontal, StepInputThreshold);

        //受付時間が過ぎたら1回目の入力を忘れる
        m_StepTapTimer -= Runner.DeltaTime;
        if (m_StepTapTimer <= 0.0f)
        {
            m_StepTapDir = 0;
        }

        //ニュートラルから倒した瞬間だけをタップとして扱う
        bool isTap = dir != 0 && dir != m_PrevHorizontalDir;
        m_PrevHorizontalDir = dir;

        if (!isTap) return;

        //受付時間内に同じ方向をもう一度倒した
        if (m_StepTapDir == dir && IsCanStartStep())
        {
            StepState.Setup(dir, m_StepDuration);
            ChangeState(StepState);
            return;
        }

        //1回目のタップとして記録
        m_StepTapDir = dir;
        m_StepTapTimer = StepInputWindow;
    }

    private bool IsCanStartStep()
    {
        return m_Controller.isGrounded && (CurrentState == IdleState || CurrentState == WalkState);
    }

    /// <summary>
    /// 値を -1 / 0 / 1 に変換する(threshold以下は0)
    /// </summary>
    private static int ToSign(float value, float threshold = 0.0f)
    {
        if (value > threshold) return 1;
        if (value < -threshold) return -1;
        return 0;
    }

    /// <summary>
    /// 左右・上下の符号(-1,0,1)をDirectionへ変換する
    /// </summary>
    private static Direction ConvertDirection(int h, int v)
    {
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

        m_Move.x = CalcHorizontalVelocity(data.horizontal);

        //ノックバックを0へ向けて直線的に減衰
        m_KnockbackVelocityX = Mathf.MoveTowards(m_KnockbackVelocityX, 0.0f, m_KnockbackDecay * Runner.DeltaTime);

        //重力(落下中は強める)
        float gravity = m_Move.y > 0.0f ? m_Gravity : m_Gravity * FallGravityMultiplier;
        m_Move.y += gravity * Runner.DeltaTime;

        m_Controller.Move(m_Move * Runner.DeltaTime);
        ResolveOverlap();
    }

    /// <summary>
    /// 入力 + ノックバック + 踏み込み + ステップを合成した左右速度を返す
    /// </summary>
    private float CalcHorizontalVelocity(float inputHorizontal)
    {
        //左右移動(行動制限中は入力分を停止)
        float inputX = IsHorizontalMoveLocked ? 0.0f : inputHorizontal * m_CharacterData.speed;

        //攻撃中は、向いている方向へ入力した時だけ少し踏み込む
        float stepX = GetAttackStepVelocity(inputHorizontal);

        float dashX = CurrentState == StepState ? StepState.Dir * m_StepSpeed : 0.0f;

        return inputX + m_KnockbackVelocityX + stepX + dashX;
    }

    /// <summary>
    /// 攻撃中の踏み込み速度を返す
    /// </summary>
    private float GetAttackStepVelocity(float inputHorizontal)
    {
        if (!IsAttackStepState) return 0.0f;

        //向いている方向へ入力していない場合は踏み込まない
        if (inputHorizontal * FacingDir <= 0.0f) return 0.0f;

        //出始めの区間を過ぎたら止める
        float t = GetAnimationNormalizedTime(GetCurrentAttackAnimation());
        if (t >= AttackStepEndTime) return 0.0f;

        return FacingDir * m_AttackStepSpeed;
    }

    /// <summary>
    /// 今の攻撃Stateに対応するAnimationStateを返す
    /// </summary>
    private AnimationState GetCurrentAttackAnimation()
    {
        if (CurrentState == LowAttackState) return AnimationState.LowAttack;
        if (CurrentState == ForwardAttackState) return AnimationState.ForwardAttack;
        return AnimationState.NormalAttack;
    }

    /// <summary>
    /// 相手と重なっていたら、横方向に押し出す
    /// </summary>
    private void ResolveOverlap()
    {
        if (m_Opponent == null) return;

        Vector3 pos = this.transform.position;
        Vector3 opos = m_Opponent.transform.position;

        //縦に重なっていなければ何もしない
        if (Mathf.Abs(pos.y - opos.y) >= m_Controller.height) return;

        float dx = pos.x - opos.x;
        float minDist = m_Controller.radius + m_Opponent.m_Controller.radius;
        float overlap = minDist - Mathf.Abs(dx);
        if (overlap <= 0.0f) return;

        //ほぼ真上・真下で重なった時は、来た側へ逃がす
        float dir = Mathf.Abs(dx) > OverlapCenterThreshold ? Mathf.Sign(dx) : -FacingDir;

        //一気にワープさせないよう、速度を制限して押し出す
        float push = Mathf.Min(overlap, PushOutSpeed * Runner.DeltaTime);
        m_Controller.Move(new Vector3(dir * push, 0.0f, 0.0f));
    }

    /// <summary>
    /// ジャンプ。できなければfalse
    /// </summary>
    public bool Jump()
    {
        if (!IsCanJump) return false;

        //ジャンプ速度を設定(jumpPowerは実質ジャンプの高さ)
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

    #endregion


    #region ダメージ

    /// <summary>
    /// 攻撃を受けられる状態か(StateAuthority側かつKOしていない)
    /// </summary>
    private bool CanTakeHit => Object.HasStateAuthority && !IsKO;

    /// <summary>
    /// ダメージを受ける(StateAuthority側のみ実行)
    /// </summary>
    /// <param name="damage">ダメージ量</param>
    /// <param name="hitStunTicks">喰らい硬直(Tick数)</param>
    /// <param name="guardStunTicks">ガード硬直(Tick数)</param>
    public void TakeDamage(int damage, int hitStunTicks, int guardStunTicks, float attackerX, float knockback, float guardKnockback, float guardDamage)
    {
        if (!CanTakeHit) return;

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
    /// 投げのダメージ適用(ガード不可)
    /// </summary>
    public void TakeThrowDamage(int damage, int hitStunTicks, float attackerX, float knockback)
    {
        if (!CanTakeHit) return;

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


    #region 攻撃判定

    /// <summary>
    /// 攻撃判定を行い、当たったらtrueを返す
    /// </summary>
    public bool CheckAttackHit(AttackData data)
    {
        if (!Object.HasStateAuthority) return false;
        if (data == null || m_SwordHitBox == null) return false;

        //剣のBoxColliderからワールド座標を取得
        m_SwordHitBox.GetWorldBox(
            out Vector3 center,
            out Vector3 halfExtents,
            out Quaternion rotation
        );

        //剣の形状でHurtBoxを検索
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
            if (!TryGetTarget(i, out Player target)) continue;

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
    public bool IsLowAttackFinished() => m_PlayerAnim.IsFinished(AnimationState.LowAttack);
    public bool IsForwardAttackFinished() => m_PlayerAnim.IsFinished(AnimationState.ForwardAttack);

    #endregion
}