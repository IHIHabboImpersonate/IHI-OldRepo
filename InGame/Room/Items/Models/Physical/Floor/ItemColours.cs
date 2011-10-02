namespace IHI.Server
{
    /// <summary>
    /// Describes the item colours for a floor furni.
    /// </summary>
    public class ItemColours
    {
        public string[] Colours { get; set; }

        public ItemColours(string[] colours)
        {
            Colours = colours;
        }
    }
}