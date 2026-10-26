using UnityEngine;
using System.Collections.Generic;

public class CommandDetector
{
    private static readonly Direction[] RushMoveCommand =
        { Direction.Down,Direction.DownRight,Direction.Right};

    private static readonly Direction[] GeigekiCommand =
        { Direction.Down,Direction.DownLeft,Direction.Left};

    private static readonly Direction[] AirMoveCommand =
        {Direction.Right,Direction.Down,Direction.DownRight};

    private static readonly Direction[] LowMoveCommand =
        {Direction.Down};


    private const int MaxHistory = 60;

    private readonly List<Direction> m_Buffer = new List<Direction>(MaxHistory);

    //コマンド受付猶予(Tick数)
    private readonly int m_CommandTicks;


    public CommandDetector(int commandTicks = 15)
    {
        m_CommandTicks = commandTicks;
    }


    public void Push(Direction direction)
    {
        if(m_Buffer.Count >= MaxHistory)
        {
            m_Buffer.RemoveAt(0);
        }
        m_Buffer.Add(direction);
    }


    public void Clear() => m_Buffer.Clear();


    public bool IsRUshMove(InputState s, bool isFacingRight)
        => Check(s.IsForwardAttack, RushMoveCommand, isFacingRight);

    public bool IsGeigeki(InputState s, bool isFacingRight)
        => Check(s.IsForwardAttack, GeigekiCommand, isFacingRight);


    public bool IsAirMove(InputState s, bool isFacingRight)
        => Check(s.IsForwardAttack, AirMoveCommand, isFacingRight);

    public bool IsDownMove(InputState s, bool isFacingRight)
        => Check(s.IsLowAttack, LowMoveCommand, isFacingRight);


    private bool Check(bool isButtonDown,IReadOnlyList<Direction> command,bool isFacingRight)
    {
        if (!isButtonDown) return false;
        if (m_Buffer.Count < command.Count) return false;

        int oldestIndex = Mathf.Max(0, m_Buffer.Count - m_CommandTicks);
        int commandIndex = command.Count - 1;

        for(int i = m_Buffer.Count - 1; i >= oldestIndex; i--)
        {
            Direction expected = isFacingRight
                ? command[commandIndex]
                : command[commandIndex].Mirror();

            if (m_Buffer[i] != expected) continue;

            if (--commandIndex < 0) return true;
        }

        return false;
    }
}
