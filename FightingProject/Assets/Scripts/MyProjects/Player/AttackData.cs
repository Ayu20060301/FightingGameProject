using UnityEngine;

[System.Serializable]
public class AttackData
{
    [Header("メモ")]
    public string label;   //Inspectorで見分けるための名前(例:通常2段目)

    [Header("ダメージ")]
    public int damage = 5;
    public int hitStunTicks = 20;
    public int guardStunTicks = 12;

    [Header("発生・持続(Tick)")]
    public int startupTicks = 6;
    public int activeTicks = 4;

    [Header("ノックバック")]
    public float knockback = 3.0f;
    public float guardKnockback = 2.0f;

    [Header("ガードゲージ")]
    public float guardDamage = 10.0f;

    //攻撃判定が有効か
    public bool IsActive(int tick)
    {
        return tick >= startupTicks &&
               tick < startupTicks + activeTicks;
    }

    //攻撃が終了しているか
    public bool IsFinished(int tick)
    {
        return tick >= startupTicks + activeTicks;
    }
}
