
namespace IHI.Server
{
    /// <summary>
    /// Represents a item of wall furni.
    /// </summary>
    public abstract class WallItem : Item
    {
        private WallLocation fLocation;

        /// <summary>
        /// Returns the WallLocation object for this WallItem.
        /// </summary>
        public WallLocation GetLocation()
        {
            return this.fLocation;
        }
    }
}
