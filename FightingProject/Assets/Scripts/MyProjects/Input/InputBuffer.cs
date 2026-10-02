using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 入力履歴を管理するクラス
/// </summary>
public class InputBuffer : MonoBehaviour
{
    [SerializeField]
    private InputController m_InputController;

    /// <summary>
    /// 入力を保存する最大フレーム数
    /// </summary>
    [SerializeField]
    private int m_MaxBufferSize = 30;

    /// <summary>
    /// 入力履歴
    /// </summary>
    private readonly List<Direction> m_Buffer = new();


    private void LateUpdate()
    {
        AddInput(m_InputController.State.Direction);
    }

    /// <summary>
    /// 入力を追加する
    /// </summary>
    /// <param name="direction"></param>
    private void AddInput(Direction direction)
    {
        m_Buffer.Add(direction);

        if(m_Buffer.Count > m_MaxBufferSize)
        {
            m_Buffer.RemoveAt(0);
        }
    }

    /// <summary>
    /// 入力履歴を取得する
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<Direction> GetBuffer()
    {
        return m_Buffer;
    }

    /// <summary>
    /// バッファをクリアする
    /// </summary>
    public void Clear()
    {
        m_Buffer.Clear();
    }

}
