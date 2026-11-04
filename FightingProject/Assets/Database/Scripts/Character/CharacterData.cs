using UnityEngine;

/// <summary>
/// 通常技のスロット
/// </summary>
public enum AttackSlot
{
   //通常技
   NormalAttack,  //通常攻撃(Xボタン)
   LowAttack,　　 //下攻撃(Aボタン)
   ForwardAttack, //前攻撃(Bボタン)

   AirAttack,    //空中攻撃

   CrouchAttack, //しゃがみ攻撃
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


    [Header("通常攻撃")]
    [SerializeField]
    private AttackData m_NormalAttack;
    [SerializeField]
    private AttackData m_LowAttack;
    [SerializeField]
    private AttackData m_ForwardAttack;
    [SerializeField]
    private AttackData m_AirAttack;
    [SerializeField]
    private AttackData m_CrouchAttack;

    /// <summary>
    /// スロットから攻撃データを取得
    /// </summary>
    public AttackData GetAttack(AttackSlot slot)
    {
        switch (slot)
        {
            case AttackSlot.NormalAttack: return m_NormalAttack;
            case AttackSlot.LowAttack: return m_LowAttack;
            case AttackSlot.ForwardAttack: return m_ForwardAttack;
            case AttackSlot.AirAttack:  return m_AirAttack;
            default: return null;
        }
    }
}
