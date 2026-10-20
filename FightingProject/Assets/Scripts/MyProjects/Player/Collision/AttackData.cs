using UnityEngine;

[System.Serializable]
public class AttackData
{
    [Header("ダメージ")]
    public int damage = 5;
    public int hitStunTicks = 20;
    public int guardStunTicks = 12;

    [Header("発生・持続(Tick)")]
    public int startupTicks = 6; //発生
    public int activeTicks = 4; //持続

    [Header("判定ボックス(キャラ・前方が+X)")]
    public Vector3 offset = new Vector3(0.6f, 1.0f, 0.0f);
    public Vector3 size = new Vector3(0.6f, 0.4f, 0.6f);


    [Header("ノックバック")]
    public float knockback = 3.0f; //ヒット時に飛ばす速度
    public float guardKnockback = 2.0f; //ガード時に押し戻す速度


    [Header("ガードゲージ")]
    public float guardDamage = 10.0f; //ガード時に減らすゲージ量

    public bool IsActive(int tick)
    {
        return tick >= startupTicks && tick <= startupTicks + activeTicks;
    }

    /// <summary>
    /// 判定ボックスのワールド座標の中心を返す
    /// </summary>
    /// <param name="origin">キャラの位置</param>
    /// <param name="facingDir">向き(右= +1、左= -1)</param>
    public Vector3 GetWorldCenter(Vector3 origin, float facingDir)
    {
        return origin + new Vector3(offset.x * facingDir, offset.y, offset.z);
    }
}
