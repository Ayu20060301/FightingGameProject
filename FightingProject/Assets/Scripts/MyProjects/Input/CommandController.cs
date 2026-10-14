using Fusion;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コマンド入力を管理するクラス
/// </summary>
public class CommandController : MonoBehaviour
{
    //コマンド定義
    private static readonly Direction[] HadoukenCommand =
        { Direction.Down,Direction.DownRight,Direction.Right};

    private static readonly Direction[] ShoryukenCommand =
        { Direction.Right,Direction.Down,Direction.DownRight};

    private static readonly Direction[] TatsumakiCommand =
        { Direction.Down,Direction.DownLeft,Direction.Left};


    [SerializeField]
    private InputBuffer m_InputBuffer;

    [SerializeField]
    private InputController m_InputController;


   //コマンド入力を受け付ける最大フレーム数
    [SerializeField]
    private int m_CommandFrame = 15;


    public bool IsHadouken() => IsCommandInput(AttackButton.LightPunch, HadoukenCommand);
    public bool IsShoryuken() => IsCommandInput(AttackButton.MidPunch, ShoryukenCommand);
    public bool IsTatsumaki() => IsCommandInput(AttackButton.LightKick, TatsumakiCommand);


    private bool IsCommandInput(AttackButton button,IReadOnlyList<Direction> command)
    {
        //指定された攻撃ボタンが押されていない
        if(!IsAttackButtonDown(button))
        {
            return false;
        }


        IReadOnlyList<Direction> buffer = m_InputBuffer.GetBuffer();


        //入力履歴が足りない
        if(buffer.Count < command.Count)
        {
            return false;
        }

        //直近 m_CommandFrameフレーム分の履歴だけを新しい順に確認する
        int oldestIndex = Mathf.Max(0, buffer.Count - m_CommandFrame);
        int commandIndex = command.Count - 1;

        for(int i = buffer.Count - 1; i >= oldestIndex; i--)
        {
            if (buffer[i] != command[commandIndex]) continue;

            commandIndex--;

            if(commandIndex < 0)
            {
                return true;
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

        InputState state = m_InputController.State;

        switch(button)
        {
            case AttackButton.LightKick:   return state.IsLightKick;
            case AttackButton.MidKick:     return state.IsMidKick;
            case AttackButton.LightPunch:  return state.IsLightPunch;
            case AttackButton.MidPunch:    return state.IsMidPunch;
            default:                       return false;
        }
    }
}
