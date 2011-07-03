
namespace IHI.Server
{
    /// <summary>
    /// Describes the shape of a floor furni.
    /// </summary>
    public class ItemShape
    {
        private byte fWidth;
        private byte fHeight;
        private byte fLength;
        private CollisionType fCollision;

        public byte GetLength()
        {
            return this.fLength;
        }

        public byte GetWidth()
        {
            return this.fWidth;
        }

        public byte GetHeight()
        {
            return this.fHeight;
        }

        public CollisionType GetCollision()
        {
            return this.fCollision;
        }
    }
}
