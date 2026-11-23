using UnityEngine;

/// <summary>
/// 通常技のスロット
/// </summary>
public enum AttackSlot
{
   //通常技
   NormalAttack,  
   NormalAttack2,  
   NormalAttack3,  
   NormalAttack4,  
   NormalAttack5
        ,  
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

    [Header("ジャンプ")]
    [Tooltip("ジャンプの高さ")]
    public float jumpHeight = 1.5f;

    [Tooltip("斜めジャンプの横移動速度")]
    public float jumpMoveSpeed = 3.0f;

    [Tooltip("重力")]
    public float gravity = -20.0f;

    [Tooltip("落下時の重力倍率")]
    public float fallMultiplier = 1.4f;

    [Tooltip("空中での移動制御")]
    [Range(0.0f, 1.0f)]
    public float airControl = 0.6f;

    [Tooltip("ジャンプ入力を記憶する時間")]
    public float jumpBufferTime = 0.1f;

    [Header("通常攻撃")]
    [SerializeField]
    private AttackData m_NormalAttack;
    [SerializeField]
    private AttackData m_NormalAttack2;
    [SerializeField]
    private AttackData m_NormalAttack3;
    [SerializeField]
    private AttackData m_NormalAttack4;
    [SerializeField]
    private AttackData m_NormalAttack5;

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
            case AttackSlot.NormalAttack:  return m_NormalAttack;
            case AttackSlot.NormalAttack2: return m_NormalAttack2;
            case AttackSlot.NormalAttack3: return m_NormalAttack3;
            case AttackSlot.NormalAttack4: return m_NormalAttack4;
            case AttackSlot.NormalAttack5: return m_NormalAttack5;
            case AttackSlot.LowAttack: return m_LowAttack;
            case AttackSlot.ForwardAttack: return m_ForwardAttack;
            case AttackSlot.AirAttack:  return m_AirAttack;
            case AttackSlot.CrouchAttack: return m_CrouchAttack; 
            default: return null;
        }
    }
}

[System.Serializable]
public class ComboStep
{
    public AnimationState animation;
    public AttackData attack;
    public int cancelStartTick;  //次段入力の受付開始
    public int cancelEndTick;    //受付終了
}