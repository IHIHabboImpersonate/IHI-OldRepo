
namespace IHI.Server
{
    public class PublicItem
    {
        string Sprite;
        byte X;
        byte Y;
        byte Z;
        byte Rotation;
        ItemState State;
    }

    enum ItemState
    {
        Open = 0,
        Block = 1,
        Seat = 2
    }
}
