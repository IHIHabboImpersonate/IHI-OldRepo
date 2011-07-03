
namespace IHI.Server
{
    /// <summary>
    /// Represents any item of furni.
    /// </summary>
    public abstract class Item
    {
        private uint fID;
        private ItemModel fModel;
        private Room fRoom;

        /// <summary>
        /// Returns the item ID from the database
        /// </summary>
        public uint GetID()
        {
            return this.fID;
        }

        /// <summary>
        /// Returns the ItemModel object of this item.
        /// </summary>
        public ItemModel GetModel()
        {
            return this.fModel;
        }

        /// <summary>
        /// Returns the Room object this item is in.
        /// </summary>
        /// <returns></returns>
        public Room GetRoom()
        {
            return this.fRoom;
        }
    }
}
