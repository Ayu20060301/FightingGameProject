using Fusion;
using UnityEngine;


public class Player : NetworkBehaviour
{
    #region 定数

    //--- 移動・重力 ---
    //接地中に与える押し付け速度
    private const float GroundedStickVelocity = -2.0f;

    //--- 向き ---
    //向きの判定で、同じ位置とみなすX差
    private const float FacingThreshold = 0.001f;

    //モデルの正面がローカル+Zの場合のY回転
    private const float FaceRightYaw = 90.0f;
    private const float FaceLeftYaw = -90.0f;

    //--- 押し出し(相手との重なり解消) ---
    //縦に重なっているとみなすY差
    private const float OverlapVerticalThreshold = 0.3f;

    //ほぼ真上・真下に重なっているとみなすX差
    private const float OverlapCenterThreshold = 0.05f;

    //押し出しの最大速度
    private const float PushOutSpeed = 10.0f;

    //--- ダブルタップ(回避) ---
    //スティックを離してから、再入力までの猶予時間(秒)
    private const float DoubleTapWindow = 0.25f;

    //タップとみなすスティックの倒し具合
    private const float TapThreshold = 0.5f;

    #endregion


    #region Inspector

    [Header("キャラクター")]
    [SerializeField]
    private CharacterData m_CharacterData;

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
    private HitBox m_SwordHitBox;

    [Header("ジャンプ")]
    [SerializeField]
    private float m_JumpHeight = 2.0f;
    [SerializeField]
    private float m_JumpMoveSpeed = 4.0f;    //斜めジャンプの横移動速度
    [SerializeField]
    private float m_JumpBufferTime = 0.1f;


    [Header("武器ドロップ")]
    [SerializeField]
    private Transform m_WeaponRoot;
    [SerializeField]
    private Collider m_WeaponPhysicsCollider;
    [SerializeField]
    private float m_DropForce = 2.0f;
    [SerializeField]
    private float m_DropTorque = 6.0f;

    #endregion


    #region フィールド

    //--- コンポーネント・参照 ---
    private CharacterController m_Controller;
    private RoundManager m_RoundManager;
    private Player m_Opponent;
    private PlayerAnimator m_PlayerAnim;

    //--- 入力 ---
    private InputState m_Input;
    private int m_HorizontalSign;            //今フレームの左右入力(-1,0,1)
    private int m_VerticalSign;              //今フレームの上下入力(-1,0,1)

    private bool m_IsPrevJumpInput;
    private bool m_IsJumpPressed;

    private bool m_IsPrevNormalAttackInput;
    private bool m_IsNormalAttackPressed;

    //--- 移動 ---
    private Vector3 m_Move;
    private float m_KnockbackVelocityX;

    //--- ジャンプ ---
    private float m_JumpBufferTimer;
    private int m_JumpMoveSign;              //ジャンプ中の横移動方向(-1,0,1)

    //--- 回避(ダブルタップ) ---
    private int m_PrevTapSign;               //前フレームの左右入力
    private int m_LastReleasedSign;          //直前に離した方向
    private float m_DoubleTapTimer;          //離してからの猶予時間
    private bool m_IsDodgePressed;
    private int m_DodgeSign;                 //回避した入力方向(-1,1)
    private float m_DodgeTimer;              //回避の移動が続く残り時間

    //--- 判定 ---
    private readonly Collider[] m_HitBuffer = new Collider[16];


    //---ルートモーション---
    private float m_RootMotionX;

    //--- 武器 ---
    private Transform m_WeaponParent;
    private Vector3 m_WeaponLocalPos;
    private Quaternion m_WeaponLocalRot;
    private Rigidbody m_WeaponBody;

    //--- ラウンド ---
    private Vector3 m_StartPosition;         //ラウンド開始時に戻す初期位置

    #endregion


    #region ネットワーク同期

    //ネットワーク同期する向き
    [Networked]
    private float FacingYaw { get; set; }

    //ネットワーク同期するHP
    [Networked]
    public int HP { get; set; }

    [Networked]
    public int PlayerIndex { get; set; } = -1;

    #endregion


    #region ステートマシン

    public PlayerStateMachine m_StateMachine;

    public PlayerStanceState StanceIdleState { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerCrouchIdleState CrouchIdleState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerNormalAttackState NormalAttackState { get; private set; }
    public PlayerLowAttackState LowAttackState { get; private set; }
    public PlayerForwardAttackState ForwardAttackState { get; private set; }
    public PlayerAirAttackState AirAttackState { get; private set; }
    public PlayerCrouchAttackState CrouchAttackState { get; private set; }
    public PlayerHitState HitState { get; private set; }
    public PlayerDodgeState DodgeState { get; private set; }
    public PlayerKOState KOState { get; private set; }
    public PlayerDeathState DeathState { get; private set; }
    public PlayerDownState DownState { get; private set; }
    public PlayerGuardState GuardState { get; private set; }
    public PlayerCrouchGuardState CrouchGuardState { get; private set; }


    public PlayerState CurrentState => m_StateMachine.CurrentState;

    /// <summary>
    /// Stateの変更
    /// </summary>
    public void ChangeState(PlayerState state)
    {
        m_StateMachine.ChangeState(state);
    }

    #endregion


    #region 公開プロパティ

    //--- キャラクターデータ ---
    public CharacterData Data => m_CharacterData;
    public int MaxHP => m_CharacterData.maxHP;

    public AttackData GetAttackData(AttackSlot slot) => m_CharacterData.GetAttack(slot);

    public AttackData NormalAttackData => m_CharacterData.GetAttack(AttackSlot.NormalAttack);
    public AttackData LowAttackData => m_CharacterData.GetAttack(AttackSlot.LowAttack);
    public AttackData ForwardAttackData => m_CharacterData.GetAttack(AttackSlot.ForwardAttack);
    public AttackData AirAttackData => m_CharacterData.GetAttack(AttackSlot.AirAttack);
    public AttackData CrouchAttackData => m_CharacterData.GetAttack(AttackSlot.CrouchAttack);

    //--- 状態参照 ---
    public InputState Input => m_Input;
    public bool IsGrounded => m_Controller.isGrounded;

    //KOしているか(HPはサーバーから同期されるので全クライアントで判定できる)
    public bool IsKO => HP <= 0;

    //--- 向き(右 = +1、左 = -1) ---
    public float FacingDir => this.transform.forward.x >= 0.0f ? 1.0f : -1.0f;
    public bool IsFacingRight => FacingDir > 0.0f;

    //--- 入力方向(右向きなら左入力、左向きなら右入力が後退) ---
    public bool IsMovingBackward => m_HorizontalSign * FacingDir < 0.0f && m_VerticalSign > 0;
    public bool IsMovingForward => m_HorizontalSign * FacingDir > 0.0f && m_VerticalSign > 0;
    public bool IsInputBackward => m_HorizontalSign * FacingDir < 0.0f;
    public bool IsInputForward => m_HorizontalSign * FacingDir > 0.0f;

    //--- 入力の立ち上がり ---
    public bool IsJumpPressed => m_IsJumpPressed;
    public bool IsNormalAttackPressed => m_IsNormalAttackPressed;

    //--- ジャンプ ---
    //1段ジャンプのみなので、接地していればジャンプ可能
    public bool IsCanJump => m_Controller.isGrounded;

    //ジャンプ入力が記憶されているか
    public bool IsJumpRequested => m_JumpBufferTimer > 0.0f;

    //--- 回避 ---
    public bool IsDodgePressed => m_IsDodgePressed;

    //ダブルタップした方向が向いている方向と同じか
    public bool IsDodgeForward => m_DodgeSign * FacingDir > 0.0f;

    public float StartX => m_StartPosition.x;

    #endregion


    #region State判定ヘルパー

    //踏み込み・移動ロックの対象になる攻撃State
    private bool IsAttackState =>
        CurrentState == NormalAttackState ||
        CurrentState == LowAttackState ||
        CurrentState == ForwardAttackState ||
        CurrentState == AirAttackState ||
        CurrentState == CrouchAttackState;

    //左右移動出来ない状態か(攻撃は地上のときだけロック。空中攻撃では空中制御を残す)
    private bool IsHorizontalMoveLocked =>
        CurrentState == CrouchIdleState ||
        CurrentState == HitState ||
        CurrentState == DodgeState ||
        CurrentState == KOState ||
        CurrentState == JumpState ||
        (IsAttackState && m_Controller.isGrounded);

    #endregion


    #region Unity / Fusion コールバック

    private void Awake()
    {
        if (m_CharacterData == null)
        {
            Debug.LogError($"[Player] CharacterDataが未設定です : {name}", this);
        }

        CacheWeapon();

        m_Controller = GetComponent<CharacterController>();
        m_Input = new InputState();
        m_StateMachine = new PlayerStateMachine();
        m_PlayerAnim = new PlayerAnimator(m_Anim);

        CreateStates();
    }

    public override void Spawned()
    {
        m_StartPosition = this.transform.position;

        //全クライアントでカメラに登録
        RegisterToCamera(true);

        if (!Object.HasStateAuthority) return;

        HP = MaxHP;
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
        StanceIdleState = new PlayerStanceState(this);
        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        CrouchIdleState = new PlayerCrouchIdleState(this);
        JumpState = new PlayerJumpState(this);
        NormalAttackState = new PlayerNormalAttackState(this);
        LowAttackState = new PlayerLowAttackState(this);
        ForwardAttackState = new PlayerForwardAttackState(this);
        AirAttackState = new PlayerAirAttackState(this);
        CrouchAttackState = new PlayerCrouchAttackState(this);
        HitState = new PlayerHitState(this);
        DodgeState = new PlayerDodgeState(this);
        KOState = new PlayerKOState(this);
        DeathState = new PlayerDeathState(this);
        DownState = new PlayerDownState(this);
    }

    /// <summary>
    /// 武器の初期配置を記憶する(ラウンドリセット用)
    /// </summary>
    private void CacheWeapon()
    {
        if (m_WeaponRoot == null) return;

        m_WeaponParent = m_WeaponRoot.parent;
        m_WeaponLocalPos = m_WeaponRoot.localPosition;
        m_WeaponLocalRot = m_WeaponRoot.localRotation;
        m_WeaponBody = m_WeaponRoot.GetComponent<Rigidbody>();
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


    #region ラウンド管理

    public void ResetForNextRound()
    {
        if (!Object.HasStateAuthority) return;

        //HP回復
        HP = MaxHP;

        //位置を初期位置へ
        m_Controller.enabled = false;
        this.transform.position = m_StartPosition;
        m_Controller.enabled = true;
        Physics.SyncTransforms();

        //速度・ノックバック・入力履歴をクリア
        m_Move = Vector3.zero;
        m_KnockbackVelocityX = 0.0f;
        m_JumpBufferTimer = 0.0f;
        m_JumpMoveSign = 0;
        m_IsPrevJumpInput = false;
        m_IsPrevNormalAttackInput = false;
        m_PrevTapSign = 0;
        m_LastReleasedSign = 0;
        m_DoubleTapTimer = 0.0f;
        m_DodgeTimer = 0.0f;
        m_IsDodgePressed = false;
        m_RootMotionX = 0.0f;

        //Idleへ
        ChangeState(IdleState);
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

    #endregion


    #region 入力・更新(StateAuthority)

    /// <summary>
    /// 入力取得 → State更新 → 移動(StateAuthority側のみ)
    /// </summary>
    private void UpdateAuthority()
    {
        //入力が取得できない場合、または試合中以外は入力を無効化
        if (!GetInput(out NetworkManager.PlayerInputData data) || !IsFightPhase())
        {
            data = default;
        }

        UpdateInput(data);
        m_StateMachine.Update();
        UpdateMovement(data);
    }

    /// <summary>
    /// 入力状態を更新する
    /// </summary>
    private void UpdateInput(NetworkManager.PlayerInputData data)
    {
        //ジャンプ入力の立ち上がりを検出
        m_IsJumpPressed = data.isJump && !m_IsPrevJumpInput;
        m_IsPrevJumpInput = data.isJump;

        //ジャンプ先行入力
        m_JumpBufferTimer = m_IsJumpPressed
            ? m_CharacterData.jumpBufferTime : Mathf.Max(0.0f, m_JumpBufferTimer - Runner.DeltaTime);

        //通常攻撃入力の立ち上がりを検出
        m_IsNormalAttackPressed = data.isNormalAttack && !m_IsPrevNormalAttackInput;
        m_IsPrevNormalAttackInput = data.isNormalAttack;

        UpdateDoubleTap(data.horizontal);

        m_HorizontalSign = ToSign(data.horizontal);
        m_VerticalSign = ToSign(data.vertical);

        m_Input.SetInput(
            ConvertDirection(m_HorizontalSign, m_VerticalSign),
            data.isNormalAttack,
            data.isLowAttack,
            data.isForwardAttack,
            data.isJump
        );
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

    /// <summary>
    /// ダブルタップ(回避入力)の検出
    /// </summary>
    private void UpdateDoubleTap(float horizontal)
    {
        m_IsDodgePressed = false;
        m_DoubleTapTimer = Mathf.Max(0.0f, m_DoubleTapTimer - Runner.DeltaTime);

        int sign = ToSign(horizontal, TapThreshold);

        if (sign == 0 && m_PrevTapSign != 0)
        {
            //スティックを中立に戻した瞬間に、猶予時間をスタート
            m_LastReleasedSign = m_PrevTapSign;
            m_DoubleTapTimer = DoubleTapWindow;
        }
        else if (sign != 0 && sign != m_PrevTapSign)
        {
            //中立から倒した瞬間
            if (m_DoubleTapTimer > 0.0f && sign == m_LastReleasedSign)
            {
                m_IsDodgePressed = true;
                m_DodgeSign = sign;
            }

            //成功・失敗どちらでも、猶予はリセット
            m_DoubleTapTimer = 0.0f;
            m_LastReleasedSign = 0;
        }

        m_PrevTapSign = sign;
    }

    #endregion


    #region 移動・向き

    /// <summary>
    /// 移動処理
    /// </summary>
    private void UpdateMovement(NetworkManager.PlayerInputData data)
    {
        float dt = Runner.DeltaTime;
        bool isGrounded = m_Controller.isGrounded;

        //接地中は落下速度をリセット
        if (isGrounded && m_Move.y < 0.0f)
        {
            m_Move.y = GroundedStickVelocity;
            m_JumpMoveSign = 0;
        }

        //左右移動
        m_Move.x = CalcHorizontalVelocity(data.horizontal);

        //ノックバックを0へ向けて直線的に減衰
        m_KnockbackVelocityX = Mathf.MoveTowards(m_KnockbackVelocityX, 0.0f, m_KnockbackDecay * dt);

        //垂直速度更新(落下中は重力を強くする)
        float gravity = -Mathf.Abs(m_CharacterData.gravity);
        if (m_Move.y < 0.0f)
        {
            gravity *= Mathf.Max(1.0f, m_CharacterData.fallMultiplier);
        }
        m_Move.y += gravity * dt;

        Vector3 delta = m_Move * dt;
        delta.x += m_RootMotionX;
        m_RootMotionX = 0.0f;

        m_Controller.Move(delta);

        ResolveOverlap();
    }

    /// <summary>
    /// 入力 + ノックバック + ジャンプ移動を合成した左右速度を返す
    /// </summary>
    private float CalcHorizontalVelocity(float inputHorizontal)
    {
        //空中制御倍率
        float airMultiplier = m_Controller.isGrounded ? 1.0f : m_CharacterData.airControl;

        //左右移動(行動制限中は入力分を停止)
        float inputX = IsHorizontalMoveLocked
            ? 0.0f
            : inputHorizontal * m_CharacterData.speed * airMultiplier;

        float jumpX = m_JumpMoveSign * m_CharacterData.jumpMoveSpeed;

        return inputX + jumpX + m_KnockbackVelocityX;
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
        if (Mathf.Abs(pos.y - opos.y) >= OverlapVerticalThreshold) return;

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


    #region ジャンプ・回避(Stateから呼ぶ)

    /// <summary>
    /// ジャンプ入力を消費する
    /// </summary>
    public void ConsumeJumpInput()
    {
        m_JumpBufferTimer = 0.0f;
    }

    public void StartJump()
    {
        float g = Mathf.Abs(m_CharacterData.gravity);
        m_Move.y = Mathf.Sqrt(2.0f * g * m_CharacterData.jumpHeight);
        m_JumpMoveSign = m_HorizontalSign;
        m_JumpBufferTimer = 0.0f;
    }

  

    public void AddRootMotion(float deltaX)
    {
        //移動はStateAuthorityだけが行う
        if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return;

        //攻撃中だけ反映する(歩き・待機のクリップ由来の微小な移動を無視)
        if (!IsAttackState && CurrentState != DodgeState) return;


        //相手に食い込む方向の移動は、重なりぎりぎりで止める
        if (m_Opponent != null)
        {
            float toOpponent = m_Opponent.transform.position.x - this.transform.position.x;
            float gap = Mathf.Abs(toOpponent) - (m_Controller.radius + m_Opponent.m_Controller.radius);

            if (Mathf.Sign(deltaX) == Mathf.Sign(toOpponent) && gap <= 0.0f) return;
        }

        m_RootMotionX += deltaX;
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
    /// <param name="attackerX">攻撃者のX座標(ノックバック方向の算出用)</param>
    /// <param name="knockback">ノックバックの強さ</param>
    public void TakeDamage(int damage, int hitStunTicks, float attackerX, float knockback)
    {
        if (!CanTakeHit) return;

        HP = Mathf.Max(0, HP - damage);
        ApplyKnockback(attackerX, knockback);

        //HPが0になったらKO
        if (IsKO)
        {
            ChangeState(KOState);
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
                this.transform.position.x,
                data.knockback
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

    public bool IsAnimationFinished(AnimationState state)
        => m_PlayerAnim.IsFinished(state);

    //各攻撃アニメーションの終了判定
    public bool IsNormalAttackFinished() => IsAnimationFinished(AnimationState.NormalAttack);
    public bool IsLowAttackFinished() => IsAnimationFinished(AnimationState.LowAttack);
    public bool IsForwardAttackFinished() => IsAnimationFinished(AnimationState.ForwardAttack);
    public bool IsAirAttackFinished() => IsAnimationFinished(AnimationState.AirAttack);
    public bool IsCrouchAttackFinished() => IsAnimationFinished(AnimationState.CrouchAttack);

    #endregion

}
