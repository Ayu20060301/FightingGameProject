using UnityEngine;

public class InputController : MonoBehaviour
{
    /// <summary>
    /// 現在の入力状態
    /// </summary>
    public InputState State { get; private set; } = new InputState();

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

        bool isLightKick = Input.GetButtonDown("LightKick");

        bool isHeavyKick = Input.GetButtonDown("HeavyKick");

        bool isLightPunch = Input.GetButtonDown("LightPunch");

        bool isHeavyPunch = Input.GetButtonDown("HeavyPunch");

        bool isJump = Input.GetButton("Jump");

        bool isGuard = Input.GetButton("Guard");

        State.SetInput(
            direction,
            isLightKick,
            isHeavyKick,
            isLightPunch,
            isHeavyPunch,
            isJump,
            isGuard);
    }


    /// <summary>
    /// 方向入力を取得する
    /// </summary>
    /// <returns></returns>
    private Direction GetDirection()
    {
        bool isUp = Input.GetKey(KeyCode.W) ||
                    Input.GetAxisRaw("Vertical") > 0.5f;

        bool isDown = Input.GetKey(KeyCode.S) ||
                      Input.GetAxisRaw("Vertical") < -0.5f;

        bool isLeft = Input.GetKey(KeyCode.A) ||
                      Input.GetAxisRaw("Horizontal") < -0.5f;

        bool isRight = Input.GetKey(KeyCode.D) ||
                       Input.GetAxisRaw("Horizontal") > 0.5f;

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
