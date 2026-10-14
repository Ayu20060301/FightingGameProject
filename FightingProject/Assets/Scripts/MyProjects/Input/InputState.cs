using UnityEngine;

/// <summary>
/// 方向入力
/// 7 8 9
/// 4 5 6
/// 1 2 3
/// </summary>
public enum Direction
{
    Neutral = 5,

    Up = 8,
    Down = 2,
    Left = 4,
    Right = 6,

    UpLeft = 7,
    UpRight = 9,
    DownLeft = 1,
    DownRight = 3
}

/// <summary>
/// 攻撃ボタン
/// </summary>
public enum AttackButton
{
    LightKick,
    HeavyKick,
    LightPunch,
    HeavyPunch,
    MidPunch,
    MidKick,
}

/// <summary>
/// 現在の入力状態
/// </summary>
public class InputState
{
    /// <summary>
    /// 現在の方向
    /// </summary>
    public Direction Direction { get; private set; }

    /// <summary>
    /// 弱キック
    /// </summary>
    public bool IsLightKick { get; private set; }

    /// <summary>
    /// 強キック
    /// </summary>
    public bool IsHeavyKick { get; private set; }

    /// <summary>
    /// 弱パンチ
    /// </summary>
    public bool IsLightPunch { get; private set; }

    /// <summary>
    /// 強パンチ
    /// </summary>
    public bool IsHeavyPunch { get; private set; }

    /// <summary>
    /// 中パンチ
    /// </summary>
    public bool IsMidPunch { get; private set; }

    /// <summary>
    /// 中キック
    /// </summary>
    public bool IsMidKick { get; private set; }

    /// <summary>
    /// ジャンプ入力
    /// </summary>
    public bool IsJump { get; private set; }

    /// <summary>
    /// ガード入力
    /// </summary>
    public bool IsGuard { get; private set; }

    public bool ISCrouch { get; private set; }

    /// <summary>
    /// 入力状態を更新する
    /// </summary>
    public void SetInput(
        Direction direction,
        bool isLightKick,
        bool isHeavyKick,
        bool isLightPunch,
        bool isHeavyPunch,
        bool isMidPunch,
        bool isMidKick,
        bool isJump,
        bool isGuard)
    {
        Direction = direction;
        IsLightKick = isLightKick;
        IsHeavyKick = isHeavyKick;
        IsLightPunch = isLightPunch;
        IsHeavyPunch = isHeavyPunch;
        IsMidPunch = isMidPunch;
        IsMidKick = isMidKick;
        IsJump = isJump;
        IsGuard = isGuard;
    }

    /// <summary>
    /// 攻撃入力を消費する
    /// </summary>
    public void ConsumeAttackInput()
    {
        IsLightKick = false;
        IsHeavyKick = false;
        IsLightPunch = false;
        IsHeavyPunch = false;
        IsMidPunch = false;
        IsMidKick = false;
    }

    /// <summary>
    /// ジャンプ入力を消費する
    /// </summary>
    public void ConsumeJumpInput()
    {
        IsJump = false;
    }
}
