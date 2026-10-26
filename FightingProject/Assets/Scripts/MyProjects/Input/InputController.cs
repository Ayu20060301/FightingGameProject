using UnityEngine;

public class InputController : MonoBehaviour
{
    //方向入力とみなすスティックの値
    private const float DirectionThreshold = 0.5f;

    /// <summary>
    /// 現在の入力状態
    /// </summary>
    public InputState State { get; private set; } = new InputState();

    private InputSystem_Actions m_Input;

    //押下を保持するフラグ
    private bool m_IsJump;

    //New
    private bool m_IsNormalAttack;
    public bool m_IsLowAttack;
    public bool m_IsUpAttack;
    private bool m_IsForwardAttack;

    private void Awake()
    {
        m_Input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        m_Input.Player.Enable();
    }

    private void OnDisable()
    {
        m_Input.Player.Disable();
    }


    private void Update()
    {
        UpdateInput();
    }

    /// <summary>
    /// 入力を更新する
    /// </summary>
    private void UpdateInput()
    {

        var player = m_Input.Player;

       
        //押された入力は消費されるまで保持する
        m_IsJump |= player.Jump.WasPressedThisFrame();
        m_IsNormalAttack |= player.NormalAttack.WasPressedThisFrame();
        m_IsLowAttack |= player.LowAttack.WasPressedThisFrame();
        m_IsUpAttack |= player.UpAttack.WasPressedThisFrame();
        m_IsForwardAttack |= player.ForwardAttack.WasPressedThisFrame();

        bool isGuard = m_Input.Player.Guard.IsPressed();

        State.SetInput(
            GetDirection(),
            m_IsNormalAttack,
            m_IsLowAttack,
            m_IsUpAttack,
            m_IsForwardAttack,
            m_IsJump,
            isGuard);
    }


    /// <summary>
    /// ボタン入力を消費する
    /// </summary>
    public void ConsumeButtonInput()
    {
        //InputController側の保持フラグをリセット
        m_IsNormalAttack = false;
        m_IsLowAttack = false;
        m_IsUpAttack = false;
        m_IsForwardAttack = false;
        m_IsJump = false;

        //InputState側もリセット
        State.ConsumeAttackInput();
        State.ConsumeJumpInput();
    }

    /// <summary>
    /// Fusionへ渡したジャンプ入力を消費する
    /// </summary>
    public void ConsumeJumpInput()
    {
        m_IsJump = false;
        State.ConsumeJumpInput();
    }

    /// <summary>
    /// 方向入力を取得する
    /// </summary>
    /// <returns></returns>
    private Direction GetDirection()
    {

        Vector2 input = m_Input.Player.Move.ReadValue<Vector2>();


        int x = ToAxis(input.x);
        int y = ToAxis(input.y);

        switch(x,y)
        {
            case (-1, 1):  return Direction.UpLeft;
            case (0, 1):   return Direction.Up;
            case (1, 1):   return Direction.UpRight;
            case (-1, 0):  return Direction.Left;
            case (1, 0):   return Direction.Right;
            case (-1, -1): return Direction.DownLeft;
            case (0, -1):  return Direction.Down;
            case (1, -1):  return Direction.DownRight;
            default:       return Direction.Neutral;
        }
    }

    /// <summary>
    /// 軸の値を -1 / 0 / 1に変換する
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static int ToAxis(float value)
    {
        if (value > DirectionThreshold) return 1;
        if (value < -DirectionThreshold) return -1;
        return 0;
    }

}
