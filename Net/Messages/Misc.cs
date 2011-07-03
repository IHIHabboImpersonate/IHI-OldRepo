
namespace IHI.Server.Net.Messages
{
    public delegate void PacketHandler(IncomingMessage Message);

    /// <summary>
    /// The priority for packet handlers to execute.
    /// </summary>
    public static class PacketHandlerPriority
    {
        /// <summary>
        /// Executed after High, Low and DefaultAction.
        /// Only executed if the packet wasn't cancelled.
        /// </summary>
        public static byte Watcher = 0;
        /// <summary>
        /// Executed after High and Low but before Water.
        /// Only executed if High and Low didn't cancel the packet.
        /// This is the default action for this packet.
        /// DO NOT USE THIS FOR NON-STANDARD FEATURES!
        /// </summary>
        public static byte DefaultAction = 1;
        /// <summary>
        /// Executed after High but before DefaultAction and Watcher.
        /// Only executed if High didn't cancel the packet.
        /// </summary>
        public static byte Low = 2;
        /// <summary>
        /// Executed before Low, DefaultAction and Watcher.
        /// Always Executed.
        /// </summary>
        public static byte High = 3;
    }
}
