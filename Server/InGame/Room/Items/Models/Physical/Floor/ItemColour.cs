
namespace IHI.Server
{
    /// <summary>
    /// Describes the item colours for a floor furni.
    /// </summary>
    public class ItemColours
    {
        private string[] fColours;

        public string GetColour(byte Index)
        {
            return this.fColours[Index];
        }
    }
}
