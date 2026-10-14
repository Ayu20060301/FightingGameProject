
public static class DirectionExtensions
{
    //歩行入力
    public static bool IsHorizontal(this Direction direction)
    {
        return direction == Direction.Left || direction == Direction.Right;
    }

    //しゃがみ入力
    public static bool IsDown(this Direction direction)
    {
        return direction == Direction.Down || direction == Direction.DownLeft || direction == Direction.DownRight;
    }

}