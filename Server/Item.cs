

namespace IHI.Server
{
    public class Item
    {
        uint ID;
        ItemTemplate Template;

        Position Position;

        Room Room;

        /// <summary>
        /// Use the roller effect to move the item.
        /// If any of the parameters are null then it will use the current position.
        /// </summary>
        public Item RollerEffect(byte? FromX, byte? FromY, float? FromZ, byte? ToX, byte? ToY, float? ToZ)
        {
            if (FromX == null)
                FromX = Position.X;
            if (FromY == null)
                FromY = Position.Y;
            if (FromZ == null)
                FromZ = Position.Z;

            if (ToX == null)
                ToX = Position.X;
            if (ToY == null)
                ToY = Position.Y;
            if (ToZ == null)
                ToZ = Position.Z;

            return this;
        }
    }
}
