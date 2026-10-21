using UnityEngine;
using System;

[Serializable]
public class GrabData
{
    [Header("発生・持続(Tick)")]
    public int startupTicks = 5; //発生
    public int activeTicks = 2; //持続
    public int whiffRecoveryTicks = 30; //空振り時の硬直


    [Header("判定ボックス(キャラ・前方が+x)")]
    public Vector3 offset = new Vector3(0.4f, 1.0f, 0.0f);
    public Vector3 size = new Vector3(0.5f, 0.6f, 0.6f);

    [Header("成立後")]
    public int grabHoldTicks = 40; //掴みモーションの長さ
    public int damage = 12;
    public int hitStunTicks = 30; //投げ銭後の喰らい硬直
    public float throwKnockback = 5.0f;

    public bool IsActive(int tick)
    {
        return tick >= startupTicks && tick < startupTicks + activeTicks;
    }

    public Vector3 GetWorldCenter(Vector3 origin,float facingDir)
    {
        return origin + new Vector3(offset.x * facingDir, offset.y, offset.z);
    }
}
