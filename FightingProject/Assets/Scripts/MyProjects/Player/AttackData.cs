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
}
