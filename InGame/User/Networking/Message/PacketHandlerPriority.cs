// 
// Copyright (C) 2012  Chris Chenery
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
namespace IHI.Server.Networking.Messages
{
    /// <summary>
    ///   The priority for packet handlers to execute.
    /// </summary>
    public enum PacketHandlerPriority
    {
        /// <summary>
        ///   Executed after High, Low and DefaultAction.
        ///   Only executed if the packet wasn't cancelled.
        /// </summary>
        Watcher = 0,
        /// <summary>
        ///   Executed after High and Low but before Water.
        ///   Only executed if High and Low didn't cancel the packet.
        ///   This is the default action for this packet.
        ///   DO NOT USE THIS FOR NON-STANDARD FEATURES!
        /// </summary>
        DefaultAction = 1,
        /// <summary>
        ///   Executed after High but before DefaultAction and Watcher.
        ///   Only executed if High didn't cancel the packet.
        /// </summary>
        Low = 2,
        /// <summary>
        ///   Executed before Low, DefaultAction and Watcher.
        ///   Always Executed.
        /// </summary>
        High = 3
    }
}