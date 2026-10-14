using Fusion;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using Debug = UnityEngine.Debug;


public class Player : NetworkBehaviour
{

    public enum AnimationState
    {
        Idle,
        Walk, //歩行
        Guard, //ガード
        Crouch, //しゃがみ
        Jump, //ジャンプ

        Land, //着地

        //攻撃

        LightPunch,  //弱パンチ
        HeavyPunch,  //強パンチ
        LightKick,  //弱キック
        HeavyKick,  //強キック

        MidKick,    //中キック
        MidPunch,  //中パンチ

        //コマンド攻撃
        Tatsumaki, //竜巻旋風脚
    }


    #region 定数・静的データ

    //接地中に与える押し付け速度
    private const float GroundedStickVelocity = -2.0f;

    //落下時の重力倍率
    private const float FallGravityMultiplier = 1.5f;

    //アニメーションのクロスフェード時間
    private const float CrossFadeDuration = 0.05f;

    //Animatorパラメーター
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsGuardParam = Animator.StringToHash("IsGuard");
    private static readonly int IsCrouchParam = Animator.StringToHash("IsCrouch");


    //AnimationState→Animatorのステート名
    private static readonly Dictionary<AnimationState, string> AnimationNames = new Dictionary<AnimationState, string>
    {
        { AnimationState.Idle,          "Idle"},
        { AnimationState.Walk,          "Walk"},
        { AnimationState.Guard,         "Guard"},
        { AnimationState.Crouch,        "Crouch"},
        { AnimationState.Jump,          "Jump"},
        { AnimationState.Land,          "Land"},
        { AnimationState.LightPunch,    "LightPunch"},
        { AnimationState.HeavyPunch,    "HeavyPunch"},
        { AnimationState.LightKick,     "LightKick"},
        { AnimationState.HeavyKick,     "HeavyKick"},
        { AnimationState.MidKick,       "MidKick"},
        { AnimationState.MidPunch,      "MidPunch"},
        { AnimationState.Tatsumaki,     "Tatsumaki"},
    };

    #endregion


    #region Inspector

    [Header("移動")]
    [SerializeField]
    private float m_Speed = 2.0f;

    [Header("ジャンプ")]
    [SerializeField]
    private float m_JumpPower = 3.0f;

    [Header("重力")]
    [SerializeField]
    private float m_Gravity = -9.8f;

    [Header("アニメーション")]
    [SerializeField]
    private Animator m_Anim;

    [Header("画面端の移動制限")]
    [SerializeField]
    private float m_MinX = -3.5f;
    [SerializeField]
    private float m_MaxX = 3.5f;

    #endregion


    #region フィールド・プロパティ

    //CharacterController
    private CharacterController m_Controller;

    //移動量
    private Vector3 m_Move;

    //入力状態
    private InputState m_Input;

    //対戦相手
    private Player m_Opponent;



    //StateMachine
    public PlayerStateMachine m_StateMachine;

    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerGuardState GuardState { get; private set; }
    public PlayerCrouchState CrouchState { get; private set; }

    public PlayerJumpState JumpState { get; private set; }

    public PlayerLightPunchState LightPunchState { get; private set; }
    public PlayerHeavyPunchState HeavyPunchState { get; private set; }
    public PlayerLightKickState LightKickState { get; private set; }
    public PlayerHeavyKickState HeavyKickState { get; private set; }
    public PlayerMidKickState MidKickState { get; private set; }
    public PlayerMidPunchState MidPunchState { get; private set; }


    /// <summary>
    /// 着地しているか
    /// </summary>
    public bool IsGrounded => m_Controller.isGrounded;

    /// <summary>
    /// 現在の入力
    /// </summary>
    public InputState Input => m_Input;

    /// <summary>
    /// 現在のState
    /// </summary>
    public PlayerState CurrentState => m_StateMachine.CurrentState;

  
    /// <summary>
    /// ネットワーク同期する向き
    /// </summary>
    [Networked]
    private float FacingYaw { get; set; }

    /// <summary>
    /// 左右移動出来ない状態か(ガード・しゃがみ・攻撃中)
    /// </summary>
    private bool IsHorizontalMoveLocked =>
        CurrentState == GuardState ||
        CurrentState == CrouchState ||
        CurrentState == LightPunchState ||
        CurrentState == HeavyPunchState ||
        CurrentState == LightKickState ||
        CurrentState == HeavyKickState ||
        CurrentState == MidKickState ||
        CurrentState == MidPunchState;

    #endregion

    #region Unity / Fusion コールバック

    private void Awake()
    {
        m_Controller = GetComponent<CharacterController>();

        //入力状態の初期化
        m_Input = new InputState();

        //StateMachineの生成
        m_StateMachine = new PlayerStateMachine();

        //State生成
        CreateStates();
    }


    public override void Spawned()
    {
        //最初はIdle
        ChangeState(IdleState);
    }

    public override void FixedUpdateNetwork()
    {
        if(Object.HasStateAuthority)
        {
            UpdateFacing();


            //入力が取得できた場合のみゲーム処理を実行
            if(GetInput(out NetworkManager.PlayerInputData data))
            {
                //入力取得
                UpdateInput(data);

                //State更新
                m_StateMachine.Update();

                //移動
                UpdateMovement(data);
            }
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
        //State生成
        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        GuardState = new PlayerGuardState(this);
        CrouchState = new PlayerCrouchState(this);
        JumpState = new PlayerJumpState(this);
        LightPunchState = new PlayerLightPunchState(this);
        HeavyPunchState = new PlayerHeavyPunchState(this);
        LightKickState = new PlayerLightKickState(this);
        HeavyKickState = new PlayerHeavyKickState(this);
        MidKickState = new PlayerMidKickState(this);
        MidPunchState = new PlayerMidPunchState(this);
    }

    #endregion

    #region 入力

    /// <summary>
    /// 入力状態を更新する
    /// </summary>
    /// <param name="data">入力データ</param>
    private void UpdateInput(NetworkManager.PlayerInputData data)
    {
        Direction direction = ConvertDirection(data.horizontal, data.vertical);


        m_Input.SetInput(direction,
                         data.isLightKick,
                         data.isHeavyKick,
                         data.isLightPunch,
                         data.isHeavyPunch,
                         data.isMidPunch,
                         data.isMidKick,
                         data.isJump,
                         data.isGuard
                         );
    }


    /// <summary>
    /// NetworkInputをDirectionへ変換する
    /// </summary>
    private Direction ConvertDirection(float horizontal,float vertical)
    {
        int h = horizontal > 0.0f ? 1 : (horizontal < 0.0f ? -1 : 0);
        int v = vertical > 0.0f ? 1 : (vertical < 0.0f ? -1 : 0);

        switch(h,v)
        {
            case (-1, 1):  return Direction.UpLeft;
            case (1, 1):   return Direction.UpRight;
            case (-1, -1): return Direction.DownLeft;
            case (1, -1):  return Direction.DownRight;
            case (-1, 0):  return Direction.Left;
            case (1, 0):   return Direction.Right;
            case (0, 1):   return Direction.Up;
            case (0, -1):  return Direction.Down;
            default:       return Direction.Neutral;
        }
    }

    #endregion


    #region 移動・向き

    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="data">入力データ</param>
    private void UpdateMovement(NetworkManager.PlayerInputData data)
    {

        //接地中は落下速度をリセット
        if(m_Controller.isGrounded && m_Move.y < 0.0f)
        {
            m_Move.y = GroundedStickVelocity;
        }


        //左右移動(行動制限中は停止)
        m_Move.x = IsHorizontalMoveLocked ? 0.0f : data.horizontal * m_Speed;

      

        //重力(落下中は強める)
        
        float gravity = m_Move.y > 0.0f ? m_Gravity : m_Gravity * FallGravityMultiplier;
        m_Move.y += gravity * Runner.DeltaTime;

        //移動
        m_Controller.Move(m_Move * Runner.DeltaTime);

        //画面端の移動制限
        ClampPositionX();
    }

    /// <summary>
    /// 画面端の移動制限
    /// </summary>
    private void ClampPositionX()
    {
        Vector3 position = this.transform.position;
        position.x = Mathf.Clamp(position.x, m_MinX, m_MaxX);
        this.transform.position = position;
    }

    /// <summary>
    /// ジャンプ
    /// </summary>
    public void Jump()
    {
        if (!m_Controller.isGrounded) return;

        //ジャンプ速度を設定
        m_Move.y = Mathf.Sqrt(m_JumpPower * -2.0f * m_Gravity);
    }


    /// <summary>
    /// 相手の位置に応じて向きを更新する
    /// </summary>
    private void UpdateFacing()
    {
        if (m_Opponent == null) return;

        //相手とのX座標差
        float deltaX = m_Opponent.transform.position.x - this.transform.position.x;

        if (Mathf.Abs(deltaX) < 0.001f) return;

        //モデルの正面がローカル+Zの場合
        FacingYaw = deltaX > 0.0f ? 90.0f : -90.0f;
    }

    #endregion


    #region State / 対戦相手

    /// <summary>
    /// Stateの変更
    /// </summary>
    /// <param name="state">次に遷移するState</param>
    public void ChangeState(PlayerState state)
    {
        m_StateMachine.ChangeState(state);
    }

    /// <summary>
    /// 対戦相手を登録
    /// </summary>
    /// <param name="opponent">対戦相手</param>
    public void SetOpponent(Player opponent)
    {
        m_Opponent = opponent;
    }

    #endregion

    /// <summary>
    /// アニメーション開始イベントをRPCで通知
    /// </summary>
    /// <param name="state"></param>
    [Rpc(RpcSources.StateAuthority,RpcTargets.All)]
    public void RPC__PlayAnimation(AnimationState state)
    {
        PlayAnimationLocal(state);
    }


    /// <summary>
    /// 各クライアントでアニメーションを再生
    /// </summary>
    /// <param name="state"></param>
    private void PlayAnimationLocal(AnimationState state)
    {
        ResetAnimationParameters();
        ApplyAnimationParameters(state);

        if(AnimationNames.TryGetValue(state,out string stateName))
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
    /// <param name="state"></param>
    private void ApplyAnimationParameters(AnimationState state)
    {
        switch(state)
        {
            case AnimationState.Walk:
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
    /// アニメーション終了判定
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private bool IsAnimationFinished(AnimationState state)
    {
        string stateName = AnimationNames[state];

        //Animatorが遷移中なら終了判定しない
        if(m_Anim.IsInTransition(0))
        {
            LogAnimation($"{stateName} : 遷移中");
            return false;
        }


        AnimatorStateInfo stateInfo = m_Anim.GetCurrentAnimatorStateInfo(0);

        if(!stateInfo.IsName(stateName))
        {
            LogAnimationWarning($"ステート名不一致 : 期待={stateName},現在のhssh = {stateInfo.fullPathHash}");

            return false;
        }

        if(stateInfo.loop)
        {
            LogAnimationWarning($"{stateName} : Loop設定がON");

            return false;
        }

        bool isFinished = stateInfo.normalizedTime >= 1.0f;

        if(isFinished)
        {
            LogAnimation($"{stateName} : 再生終了 normalizedTime = {stateInfo.normalizedTime:F2}");
        }

        return isFinished;
    }


    //エディタ上のみログ出力
    [Conditional("UNITY_EDITOR")]
    private static void LogAnimation(string message) => Debug.Log($"[Animation] {message}");

    [Conditional("UNITY_EDITOR")]
    private static void LogAnimationWarning(string message) => Debug.LogWarning($"[Animation] {message}");


    /// <summary>
    /// ジャンプが終了したか
    /// </summary>
    /// <returns></returns>
    public bool IsJumpFinished()
    {
        //地面に着地し、落下が終わったらジャンプ終了
        return m_Controller.isGrounded && m_Move.y <= 0.0f;
    }

    /// <summary>
    /// 弱パンチのアニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    public bool IsLightPunchFinished() => IsAnimationFinished(AnimationState.LightPunch);

    /// <summary>
    /// 強パンチのアニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    public bool IsHeavyPunchFinished() => IsAnimationFinished(AnimationState.HeavyPunch);

    /// <summary>
    /// 弱キックのアニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    public bool IsLightKickFinished() => IsAnimationFinished(AnimationState.LightKick);
    
    /// <summary>
    /// 強キックのアニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    public bool IsHeavyKickFinished() => IsAnimationFinished(AnimationState.HeavyKick);

    /// <summary>
    /// 中キックのアニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    public bool IsMidKickFinished() => IsAnimationFinished(AnimationState.MidKick);

    /// <summary>
    /// 中パンチのアニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    public bool IsMidPunchFinished() => IsAnimationFinished(AnimationState.MidPunch);

    /// <summary>
    /// 竜巻旋風脚のアニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    public bool IsTatsumakiFinished() => IsAnimationFinished(AnimationState.Tatsumaki);
}
