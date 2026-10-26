using UnityEngine;

/// <summary>
/// 通常技のスロット
/// </summary>
public enum AttackSlot
{
   //通常技
   NormalAttack,  //通常攻撃(Xボタン)
   LowAttack,　　 //下攻撃(Aボタン)
   UpAttack,     //上攻撃(Yボタン)
   ForwardAttack, //前攻撃(Bボタン)

   //コマンド技
   RushMove,    //突進技 
   Geigeki,    //迎撃技
   AirMove,  //対空技
   DownMove,  //降下技
}

/// <summary>
/// キャラクターごとの数値・技データ
/// キャラを追加するときは、このアセットを複製して値を変えるだけにする
/// </summary>
[CreateAssetMenu(menuName = "Fighting/CharacterData", fileName = "CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("基本")]
    public string characterName;
    public int maxHP = 100;

    [Header("移動")]
    public float speed = 2.0f;
    public float jumpPower = 3.0f;

    [Header("ガード")]
    public float maxGuardGauge = 100.0f;

    //回復速度
    public float guardRecoverPerSecond = 15.0f;

    //削られてからの回復開始までの待ち時間
    public float guardRecoverDelay = 1.0f;

    //クラッシュの硬直(Tick数)
    public int guardBreakStunTicks = 90;

    //ガード構え中に減る速さ(1秒あたり)
    public float guardDrainPerSecond = 5.0f;

    [Range(0.0f, 1.0f)]
    public float guardUnlockRate = 0.3f;

    [Header("通常技")]
    [SerializeField]
    private AttackData m_NormalAttack;
    [SerializeField]
    private AttackData m_LowAttack;
    [SerializeField]
    private AttackData m_UpAttack;
    [SerializeField]
    private AttackData m_ForwardAttack;


    [Header("コマンド技")]
    [SerializeField]
    private AttackData m_RushMove;
    [SerializeField]
    private AttackData m_Geigeki;
    [SerializeField]
    private AttackData m_AirMove;
    [SerializeField]
    private AttackData m_DownMove;



    [Header("掴み")]
    public GrabData grab;

    /// <summary>
    /// スロットから攻撃データを取得
    /// </summary>
    public AttackData GetAttack(AttackSlot slot)
    {
        switch (slot)
        {
            case AttackSlot.NormalAttack: return m_NormalAttack;
            case AttackSlot.LowAttack: return m_LowAttack;
            case AttackSlot.UpAttack: return m_UpAttack;
            case AttackSlot.ForwardAttack: return m_ForwardAttack;
            case AttackSlot.RushMove: return m_RushMove;
            case AttackSlot.Geigeki: return m_Geigeki;
            case AttackSlot.AirMove: return m_AirMove;
            case AttackSlot.DownMove: return m_DownMove;
            default: return null;
        }
    }
}
