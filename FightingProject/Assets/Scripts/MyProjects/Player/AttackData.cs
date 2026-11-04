using UnityEngine;

/// <summary>
/// 1つの攻撃に関するパラメータをまとめたデータクラス。
/// Inspectorで編集でき、ダメージ・硬直・発生/持続フレーム・ノックバックなどを保持する。
/// 時間の単位はすべてTick(ネットワーク同期用の固定ステップ)。
/// </summary>
[System.Serializable]
public class AttackData
{
    // ------------------------------------------------------------
    // メモ
    // ------------------------------------------------------------
    [Header("メモ")]
    [Tooltip("Inspectorで見分けるための名前(例:通常2段目)")]
    public string label;

    // ------------------------------------------------------------
    // ダメージ・硬直
    // ------------------------------------------------------------
    [Header("ダメージ")]
    [Tooltip("ヒット時に与えるダメージ量")]
    [Min(0)] public int damage = 5;

    [Tooltip("ヒット時に相手が動けなくなるTick数")]
    [Min(0)] public int hitStunTicks = 20;

    [Tooltip("ガードされた時に相手が動けなくなるTick数")]
    [Min(0)] public int guardStunTicks = 12;

    // ------------------------------------------------------------
    // 攻撃判定のタイミング
    // ------------------------------------------------------------
    [Header("発生・持続(Tick)")]
    [Tooltip("攻撃開始から判定が出るまでのTick数(発生フレーム)")]
    [Min(0)] public int startupTicks = 6;

    [Tooltip("判定が出続けるTick数(持続フレーム)")]
    [Min(0)] public int activeTicks = 4;

    // ------------------------------------------------------------
    // ノックバック
    // ------------------------------------------------------------
    [Header("ノックバック")]
    [Tooltip("ヒット時の吹き飛ばし量")]
    [Min(0f)] public float knockback = 3.0f;

    [Tooltip("ガードされた時の押し戻し量")]
    [Min(0f)] public float guardKnockback = 2.0f;

    // ------------------------------------------------------------
    // ガードゲージ
    // ------------------------------------------------------------
    [Header("ガードゲージ")]
    [Tooltip("ガードされた時に相手のガードゲージを削る量")]
    [Min(0f)] public float guardDamage = 10.0f;

    // ------------------------------------------------------------
    // 派生値(Inspectorには表示されない)
    // ------------------------------------------------------------

    /// <summary>
    /// 攻撃判定が終わる境界のTick(この値以降は判定なし)。
    /// startupTicks + activeTicks。
    /// </summary>
    public int ActiveEndTick => startupTicks + activeTicks;

    // ------------------------------------------------------------
    // 判定メソッド
    // ------------------------------------------------------------

    /// <summary>
    /// 指定Tickで攻撃判定が有効かどうか。
    /// 発生Tick以上、かつ判定終了Tick未満なら有効。
    /// </summary>
    /// <param name="tick">攻撃開始からの経過Tick</param>
    public bool IsActive(int tick)
    {
        return tick >= startupTicks && tick < ActiveEndTick;
    }

    /// <summary>
    /// 指定Tickで攻撃判定がすでに終了しているかどうか。
    /// </summary>
    /// <param name="tick">攻撃開始からの経過Tick</param>
    public bool IsFinished(int tick)
    {
        return tick >= ActiveEndTick;
    }
}