using Fusion;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コマンド入力を管理するクラス
/// </summary>
public class CommandController : MonoBehaviour
{
    [SerializeField]
    private InputBuffer m_InputBuffer;

    [SerializeField]
    private InputController m_InputController;


    /// <summary>
    /// コマンド入力を受け付ける最大フレーム数
    /// </summary>
    [SerializeField]
    private int m_CommandFrame = 15;

    /// <summary>
    /// 波動拳のコマンド 
    /// ↓ ↘ → + 弱パンチ
    /// </summary>
    /// <returns></returns>
    public bool IsHadouken()
    {
        return IsCheckCommand(AttackButton.LightPunch,Direction.Down, Direction.DownRight, Direction.Right);
    }

    /// <summary>
    /// 昇竜拳のコマンド
    /// → ↓ ↘ + 強パンチ
    /// </summary>
    /// <returns></returns>
    public bool IsShoryuken()
    {
        return IsCheckCommand(AttackButton.HeavyPunch,Direction.Right, Direction.Down, Direction.DownRight);
    }


    /// <summary>
    /// 竜巻旋風脚のコマンド
    /// ↓ ↙ ← + 弱キック
    /// </summary>
    /// <returns></returns>
    public bool IsTatsumaki()
    {
        return IsCheckCommand(AttackButton.LightKick,Direction.Down, Direction.DownLeft, Direction.Left);
    }

    /// <summary>
    /// 上段足刀蹴りのコマンド
    /// ↓ ↘ → + 強キック
    /// </summary>
    /// <returns></returns>
    public bool IsSokutogeri()
    {
        return IsCheckCommand(AttackButton.HeavyKick,Direction.Down, Direction.DownRight, Direction.Right);
    }


    /// <summary>
    /// コマンドを判定する
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private bool IsCheckCommand(AttackButton button,params Direction[] command)
    {

        //指定された攻撃ボタンが押されていない
        if(!IsAttackButtonDown(button))
        {
            return false;
        }

        IReadOnlyList<Direction> buffer = m_InputBuffer.GetBuffer();

        //入力履歴が足りない
        if(buffer.Count < command.Length)
        {
            return false;
        }

        int commandIndex = command.Length - 1;


        //直近の入力だけを確認する
        int startIndex = Mathf.Max(0, buffer.Count - m_CommandFrame);

        for(int i = buffer.Count - 1; i >= startIndex; i--)
        {
            if (buffer[i] == command[commandIndex])
            {
                commandIndex--;

                if (commandIndex < 0) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 指定された攻撃ボタンが押されたか
    /// </summary>
    /// <param name="button">指定の攻撃ボタン</param>
    /// <returns></returns>
    private bool IsAttackButtonDown(AttackButton button)
    {
        switch(button)
        {
            case AttackButton.LightKick:
                return m_InputController.State.IsLightKick;
            case AttackButton.HeavyKick:
                return m_InputController.State.IsHeavyKick;
            case AttackButton.LightPunch:
                return m_InputController.State.IsLightPunch;
            case AttackButton.HeavyPunch:
                return m_InputController.State.IsHeavyPunch;
        }

        return false;
    }

}
