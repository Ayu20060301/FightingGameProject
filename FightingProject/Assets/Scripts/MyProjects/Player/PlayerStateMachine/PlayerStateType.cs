/// <summary>
/// プレイヤーのステート種類
/// </summary>
public enum PlayerStateType
{
    Idle,
    Walk,
    Crouch,
    Guard,
    Jump,
    Fall,
    Hit,
    KnockDown
}

/// <summary>
/// 攻撃の種類
/// </summary>
public enum AttackType
{
    None,
    LightPunch,
    HeavyPunch,
    LightKick,
    HeavyKick,
}
