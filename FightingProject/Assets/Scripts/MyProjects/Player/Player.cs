using Fusion;
using NUnit.Framework.Constraints;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class Player : NetworkBehaviour
{

    public enum AnimationState
    {
        Idle,
        Walk, //歩行
        Guard, //ガード
        Crouch, //しゃがみ
        Jump, //ジャンプ

        //攻撃

        lightPunch,  //弱パンチ
        heavyPunch,  //強パンチ
        lightKick,  //弱キック
        heavyKick,  //強キック

    }

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

    //CharacterController
    private CharacterController m_Controller;

    //移動量
    private Vector3 m_Move;


    //入力状態
    private InputState m_Input;

    /// <summary>
    /// 着地しているか
    /// </summary>
    public bool IsGrounded => m_Controller.isGrounded;


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

    /// <summary>
    /// 現在の入力
    /// </summary>
    public InputState Input => m_Input;

    /// <summary>
    /// 現在のState
    /// </summary>
    public PlayerState CurrentState => m_StateMachine.CurrentState;

    [Header("ネットワーク同期する向き")]
    [Networked]
    private float FacingYaw { get; set; }


    //対戦相手
    private Player m_Opponent;

    private void Awake()
    {
        m_Controller = GetComponent<CharacterController>();

        //入力状態の初期化
        m_Input = new InputState();

        //StateMachineの生成
        m_StateMachine = new PlayerStateMachine();

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
                UpdateMoveMent(data);
            }
        }

        //同期された向きを各クライアントに適用
        this.transform.rotation = Quaternion.Euler(0.0f, FacingYaw, 0.0f);

    }


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
                         data.isJump,
                         data.isGuard
                         );
    }


    /// <summary>
    /// NetworkInputをDirectionへ変換する
    /// </summary>
    private Direction ConvertDirection(float horizontal,float vertical)
    {
        if (horizontal == 0.0f &&
            vertical == 0.0f)
        {
            return Direction.Neutral;
        }

        if (horizontal < 0.0f &&
            vertical > 0.0f)
        {
            return Direction.UpLeft;
        }

        if (horizontal > 0.0f &&
            vertical > 0.0f)
        {
            return Direction.UpRight;
        }

        if (horizontal < 0.0f &&
            vertical < 0.0f)
        {
            return Direction.DownLeft;
        }

        if (horizontal > 0.0f &&
            vertical < 0.0f)
        {
            return Direction.DownRight;
        }

        if (horizontal < 0.0f)
        {
            return Direction.Left;
        }

        if (horizontal > 0.0f)
        {
            return Direction.Right;
        }

        if (vertical > 0.0f)
        {
            return Direction.Up;
        }

        return Direction.Down;
    }


    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="data">入力データ</param>
    private void UpdateMoveMent(NetworkManager.PlayerInputData data)
    {

        //接地中は落下速度をリセット
        if(m_Controller.isGrounded && m_Move.y < 0.0f)
        {
            m_Move.y = -2.0f;
        }


        if (CurrentState == GuardState ||
            CurrentState == CrouchState ||
            CurrentState == LightPunchState ||
            CurrentState == HeavyPunchState ||
            CurrentState == LightKickState ||
            CurrentState == HeavyKickState)
        {
            m_Move.x = 0.0f;
        }
        else
        {
            //左右移動
            m_Move.x = data.horizontal * m_Speed;
        }

        
        float gravity = m_Move.y > 0.0f ? m_Gravity : m_Gravity * 1.5f;

        //重力
        m_Move.y += gravity * Runner.DeltaTime;

        //移動
        m_Controller.Move(m_Move * Runner.DeltaTime);

        //画面端の移動制限
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
    /// <param name="opponent"></param>
    public void SetOpponent(Player opponent)
    {
        m_Opponent = opponent;
    }

    /// <summary>
    /// 相手の位置に応じて向きを更新する
    /// </summary>
    private void UpdateFacing()
    {
        //向きの決定はStateAuthority側だけで行う
        if (!Object.HasStateAuthority) return;

        if (m_Opponent == null) return;

        //相手とのX座標差
        float deltaX = m_Opponent.transform.position.x - this.transform.position.x;

        if (Mathf.Abs(deltaX) < 0.001f) return;

        //モデルの正面がローカル+Zの場合
        FacingYaw = deltaX > 0.0f ? 90.0f : -90.0f;
    }


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
        //アニメーションの基本状態をリセット
        ResetAnimationState();

        switch (state)
        {
            case AnimationState.Idle:
                m_Anim.CrossFade("Idle", 0.05f);
                break;

            case AnimationState.Walk:
                m_Anim.SetFloat("Speed", 1.0f);
                m_Anim.CrossFade("Walk", 0.05f);
                break;

            case AnimationState.Guard:
                m_Anim.SetBool("IsGuard", true);
                m_Anim.CrossFade("Guard", 0.05f);
                break;

            case AnimationState.Crouch:
                m_Anim.SetBool("IsCrouch", true);
                m_Anim.CrossFade("Crouch", 0.05f);
                break;

            case AnimationState.Jump:
                m_Anim.CrossFade("Jump", 0.05f);
                break;

            case AnimationState.lightPunch:
                m_Anim.CrossFade("LightPunch", 0.05f);
                break;

            case AnimationState.heavyPunch:
                m_Anim.CrossFade("HeavyPunch", 0.05f);
                break;

            case AnimationState.lightKick:
                m_Anim.CrossFade("LightKick", 0.05f);
                break;

            case AnimationState.heavyKick:
                m_Anim.CrossFade("HeavyKick", 0.05f);
                break;
        }
    }

    /// <summary>
    /// アニメーションの基本状態をリセット
    /// </summary>
    private void ResetAnimationState()
    {
        m_Anim.SetFloat("Speed", 0.0f);
        m_Anim.SetBool("IsGuard", false);
        m_Anim.SetBool("IsCrouch", false);
    }


    private bool IsAnimationFinished(string stateName)
    {

        //Animatorが遷移中なら終了判定しない
        if(m_Anim.IsInTransition(0))
        {
            Debug.Log($"[Animation] {stateName} : 遷移中");
            return false;
        }

        AnimatorStateInfo stateInfo = m_Anim.GetCurrentAnimatorStateInfo(0);

        if(!stateInfo.IsName(stateName))
        {
            Debug.LogWarning($"[Animation]ステート名不一致: " + $"期待={stateName},現在は{stateInfo.fullPathHash}");
            return false;
        }

        if (stateInfo.loop)
        {
            Debug.LogWarning($"[Animation] {stateName}: Loop設定がON");
            return false;
        }

        bool isFinished = stateInfo.normalizedTime >= 1.0f;

        if(isFinished)
        {
            Debug.Log($"[Animation] {stateName}: 再生終了" + $"normalizedTime = {stateInfo.normalizedTime:F2}");
        }

        return isFinished;
    }

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
    public bool IsLightPunchFinished()
    {
        return IsAnimationFinished("LightPunch");
    }


    /// <summary>
    /// 強パンチのアニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    public bool IsHeavyPunchFinished()
    {
        return IsAnimationFinished("HeavyPunch");
    }

    /// <summary>
    /// 弱キックのアニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    public bool IsLightKickFinished()
    {
        return IsAnimationFinished("LightKick");
    }

    /// <summary>
    /// 強キックのアニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    public bool IsHeavyKickFinished()
    {
        return IsAnimationFinished("HeavyKick");
    }
}
