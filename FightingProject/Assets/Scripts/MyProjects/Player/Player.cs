using Fusion;
using NUnit.Framework.Constraints;
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
        Crouch //しゃがみ
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

    //CharacterController
    private CharacterController m_Controller;

    //移動量
    private Vector3 m_Move;


    //入力状態
    private InputState m_Input;


    //StateMachine
    public PlayerStateMachine m_StateMachine;

    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerGuardState GuardState { get; private set; }
    public PlayerCrouchState CrouchState { get; private set; }

    /// <summary>
    /// 現在の入力
    /// </summary>
    public InputState Input => m_Input;

    /// <summary>
    /// 現在のState
    /// </summary>
    public PlayerState CurrentState => m_StateMachine.CurrentState;

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

    }


    public override void Spawned()
    {
        //最初はIdle
        ChangeState(IdleState);
    }


    public override void FixedUpdateNetwork()
    {
        //入力取得
        if (!GetInput(out NetworkManager.PlayerInputData data)) return;


        //入力を更新
        UpdateInput(data);

        //Stateを更新
        m_StateMachine.Update();

        //移動
        UpdateMoveMent(data);
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
        //左右移動
        m_Move.x = data.horizontal * m_Speed;

        //重力
        m_Move.y += m_Gravity * Runner.DeltaTime;

        //移動
        m_Controller.Move(m_Move * Runner.DeltaTime);
    }

    /// <summary>
    /// Stateの変更
    /// </summary>
    /// <param name="state">過ぎに遷移するState</param>
    public void ChangeState(PlayerState state)
    {
        m_StateMachine.ChangeState(state);
    }

    /// <summary>
    /// アニメーションの変更
    /// </summary>
    /// <param name="state"></param>
    public void SetAnimationState(AnimationState state)
    {
        switch(state)
        {
            case AnimationState.Idle:
                m_Anim.SetFloat("Speed", 0.0f);
                m_Anim.SetBool("IsGuard", false);
                m_Anim.SetBool("IsCrouch", false);
                break;

            case AnimationState.Walk:
                m_Anim.SetFloat("Speed",1.0f);
                m_Anim.SetBool("IsGuard", false);
                m_Anim.SetBool("IsCrouch", false);
                break;

            case AnimationState.Guard:
                m_Anim.SetFloat("Speed",0.0f);
                m_Anim.SetBool("IsGuard", true);
                m_Anim.SetBool("IsCrouch", false);
                break;
            case AnimationState.Crouch:
                m_Anim.SetFloat("Speed", 0.0f);
                m_Anim.SetBool("IsCrouch", true);
                m_Anim.SetBool("IsGuard", false);
                break;
        }
    }

}
