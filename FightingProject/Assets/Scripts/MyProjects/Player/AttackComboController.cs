using System.Data;
using UnityEngine;

/// <summary>
/// 通常攻撃コンボ管理
/// </summary>
public class AttackComboController : MonoBehaviour
{
    private const int MaxComboCount = 3;

    private int m_ComboCount = 1;
    private bool m_IsCanCombo;
    private bool m_IsComboInputBuffered;

    public int ComboCount => m_ComboCount;

    public bool IsCanCombo => m_IsCanCombo;

    /// <summary>
    /// コンボ受付開始
    /// </summary>
    public void StartComboWindow()
    {
        m_IsCanCombo = true;
    }

    /// <summary>
    /// コンボ受付終了
    /// </summary>
    public void EndComboWindow()
    {
        m_IsCanCombo = false;
    }

    /// <summary>
    /// 次の攻撃入力
    /// </summary>
    public void RegisterInput()
    {
        if(m_IsCanCombo && m_ComboCount < MaxComboCount)
        {
            m_IsComboInputBuffered = true;
        }
    }

    /// <summary>
    /// 次のコンボへ進める
    /// </summary>
    public bool IsTryNextCombo()
    {
        if(!m_IsComboInputBuffered || m_ComboCount >= MaxComboCount)
        {
            return false;
        }

        m_IsComboInputBuffered = false;
        m_IsCanCombo = false;
        m_ComboCount++;

        return true;
    }

    /// <summary>
    /// コンボ終了
    /// </summary>
    public void ResetCombo()
    {
        m_ComboCount = 1;
        m_IsCanCombo = false;
        m_IsComboInputBuffered = false;
    }
}
