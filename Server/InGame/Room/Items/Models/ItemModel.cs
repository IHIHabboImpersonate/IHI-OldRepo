
namespace IHI.Server
{
    /// <summary>
    /// Represents a model of an item of furni.
    /// </summary>
    public abstract class ItemModel
    {
        private string fSprite;

        public string GetSprite()
        {
            return this.fSprite;
        }
    }
}
