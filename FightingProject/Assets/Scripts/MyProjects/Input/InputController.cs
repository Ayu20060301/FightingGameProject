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
    private bool m_IsLightKick;
    private bool m_IsHeavyKick;
    private bool m_IsLightPunch;
    private bool m_IsHeavyPunch;
    private bool m_IsMidPunch;
    private bool m_IsMidKick;
    private bool m_IsJump;

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
        m_IsLightKick |= player.LightKick.WasPressedThisFrame();
        m_IsMidKick |= player.MidKick.WasPressedThisFrame();
        m_IsHeavyKick |= player.HeavyKick.WasPressedThisFrame();
        m_IsLightPunch |= player.LightPunch.WasPressedThisFrame();
        m_IsMidPunch |= player.MidPunch.WasPressedThisFrame();
        m_IsHeavyPunch |= player.HeavyPunch.WasPressedThisFrame();
        m_IsJump |= player.Jump.WasPressedThisFrame();

        bool isGuard = m_Input.Player.Guard.IsPressed();

        State.SetInput(
            GetDirection(),
            m_IsLightKick,
            m_IsHeavyKick,
            m_IsLightPunch,
            m_IsHeavyPunch,
            m_IsMidPunch,
            m_IsMidKick,
            m_IsJump,
            isGuard);
    }


    /// <summary>
    /// ボタン入力を消費する
    /// </summary>
    public void ConsumeButtonInput()
    {
        //InputController側の保持フラグをリセット
        m_IsLightKick = false;
        m_IsMidKick = false;
        m_IsHeavyKick = false;
        m_IsLightPunch = false;
        m_IsMidPunch = false;
        m_IsHeavyPunch = false;
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
