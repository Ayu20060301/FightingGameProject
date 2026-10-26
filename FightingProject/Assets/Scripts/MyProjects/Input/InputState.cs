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
    NormalAttack, //通常攻撃(Bボタン入力)
    LowAttack, //下攻撃(Aボタン)
    UpAttack, //上攻撃(Yボタン入力)

    ForwardAttack, //前攻撃(Xボタン入力)
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
    /// ジャンプ入力
    /// </summary>
    public bool IsJump { get; private set; }

    /// <summary>
    /// ガード入力
    /// </summary>
    public bool IsGuard { get; private set; }

    public bool ISCrouch { get; private set; }


    /// <summary>
    /// 通常攻撃
    /// </summary>
    public bool IsNormalAttack { get; private set; }

    /// <summary>
    /// 下攻撃
    /// </summary>
    public bool IsLowAttack { get; private set; }

    /// <summary>
    /// 上攻撃
    /// </summary>
    public bool IsUpAttack { get; private set; }

    /// <summary>
    /// 前攻撃
    /// </summary>
    public bool IsForwardAttack { get; private set; }

    /// <summary>
    /// 入力状態を更新する
    /// </summary>
    public void SetInput(
        Direction direction,
        bool isNormalAttack,
        bool isLowAttack,
        bool isUpAttack,
        bool isForwardAttack,
        bool isJump,
        bool isGuard)
    {
        Direction = direction;
        IsNormalAttack = isNormalAttack;
        IsLowAttack = isLowAttack;
        IsUpAttack = isUpAttack;
        IsForwardAttack = isForwardAttack;
        IsJump = isJump;
        IsGuard = isGuard;
    }

    /// <summary>
    /// 攻撃入力を消費する
    /// </summary>
    public void ConsumeAttackInput()
    {
        IsNormalAttack = false;
        IsLowAttack = false;
        IsUpAttack = false;
        IsForwardAttack = false;
    }

    /// <summary>
    /// ジャンプ入力を消費する
    /// </summary>
    public void ConsumeJumpInput()
    {
        IsJump = false;
    }
}
