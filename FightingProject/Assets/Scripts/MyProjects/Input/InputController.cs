using UnityEngine;

public class InputController : MonoBehaviour
{
    /// <summary>
    /// 現在の入力状態
    /// </summary>
    public InputState State { get; private set; } = new InputState();

    private InputSystem_Actions m_Input;

    private bool m_IsLightKick;
    private bool m_IsHeavyKick;
    private bool m_IsLightPunch;
    private bool m_IsHeavyPunch;

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
        Direction direction = GetDirection();

   
        //攻撃入力を保持
        if(m_Input.Player.LightKick.WasPressedThisFrame())
        {
            m_IsLightKick = true;
        }

        if(m_Input.Player.HeavyKick.WasPressedThisFrame())
        {
            m_IsHeavyKick = true;
        }

        if(m_Input.Player.LightPunch.WasPressedThisFrame())
        {
            m_IsLightPunch = true;
        }

        if(m_Input.Player.HeavyPunch.WasPressedThisFrame())
        {
            m_IsHeavyPunch = true;
        }

        bool isJump = m_Input.Player.Jump.IsPressed();
        bool isGuard = m_Input.Player.Guard.IsPressed();

        State.SetInput(
            direction,
            m_IsLightKick,
            m_IsHeavyKick,
            m_IsLightPunch,
            m_IsHeavyPunch,
            isJump,
            isGuard);
    }


    /// <summary>
    /// 方向入力を取得する
    /// </summary>
    /// <returns></returns>
    private Direction GetDirection()
    {

        Vector2 input = m_Input.Player.Move.ReadValue<Vector2>();

        bool isUp = input.y > 0.5f;
        bool isDown = input.y < -0.5f;
        bool isLeft = input.x < -0.5f;
        bool isRight = input.x > 0.5f;

        if (isDown && isRight)
        {
            return Direction.DownRight;
        }
        if (isDown && isLeft)
        {
            return Direction.DownLeft;
        }
        if (isUp && isRight)
        {
            return Direction.UpRight;
        }
        if (isUp && isLeft)
        {
            return Direction.UpLeft;
        }
        if (isUp)
        {
            return Direction.Up;
        }
        if (isDown)
        {
            return Direction.Down;
        }
        if (isRight)
        {
            return Direction.Right;
        }
        if (isLeft)
        {
            return Direction.Left;
        }

        return Direction.Neutral;
    }
}
