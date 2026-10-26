
using System.Xml;

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


    //左右を反転した方向を返す(テンキー配置の左列と右列を入れ替える)
    public static Direction Mirror(this Direction dir)
    {
        int n = (int)dir;
        int col = (n - 1) % 3; //0:左 1:中央 2:右
        int row = (n - 1) / 3;
        return (Direction)(row * 3 + (2 - col) + 1);
    }
}
