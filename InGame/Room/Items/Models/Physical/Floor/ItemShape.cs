namespace IHI.Server
{
    /// <summary>
    /// Describes the shape of a floor furni.
    /// </summary>
    public class ItemShape
    {
        private CollisionType fCollision;
        private byte fHeight;
        private byte fLength;
        private byte fWidth;

        public byte GetLength()
        {
            return fLength;
        }

        public byte GetWidth()
        {
            return fWidth;
        }

        public byte GetHeight()
        {
            return fHeight;
        }

        public CollisionType GetCollision()
        {
            return fCollision;
        }
    }
}