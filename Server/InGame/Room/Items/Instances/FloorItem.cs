
namespace IHI.Server
{
    /// <summary>
    /// Represents any item of furni that goes on the Floor
    /// </summary>
    public abstract class FloorItem : Item
    {
        private FloorLocation fLocation;

        /// <summary>
        /// Returns the FloorLocation object for this FloorItem.
        /// </summary>
        public FloorLocation GetLocation()
        {
            return this.fLocation;
        }

        /// <summary>
        /// Use the roller effect to move the item. 
        /// WARNING: This does not change the actual location, just the visible location.
        /// NOTE: If any of the parameters are null then it will use the current position.
        /// </summary>
        public FloorItem RollerEffect(byte? FromX, byte? FromY, float? FromZ, byte? ToX, byte? ToY, float? ToZ)
        {
            if (FromX == null)
                FromX = this.fLocation.GetX();
            if (FromY == null)
                FromY = this.fLocation.GetY();
            if (FromZ == null)
                FromZ = this.fLocation.GetZ();

            if (ToX == null)
                ToX = this.fLocation.GetX();
            if (ToY == null)
                ToY = this.fLocation.GetY();
            if (ToZ == null)
                ToZ = this.fLocation.GetZ();

            // TODO: Roller Effect

            return this;
        }
    }
}
