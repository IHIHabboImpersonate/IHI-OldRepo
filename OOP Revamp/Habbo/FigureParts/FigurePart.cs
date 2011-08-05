using System;
using System.Collections.Generic;
using System.Data;
using IHI.Server.Networking.Messages;
using IHI.Server.Users.Permissions;
using IHI.Server.Networking;
using System.Linq;
using IHI.Database;
using NHibernate;

using IHI.Server.Rooms;

namespace IHI.Server.Habbos
{
    public abstract class FigurePart
    {
        protected ushort fPrimaryColour;
        protected ushort fSecondaryColour;

        public ushort GetPrimaryColour()
        {
            return this.fPrimaryColour;
        }
        public ushort GetSecondaryColour()
        {
            return this.fSecondaryColour;
        }

        public FigurePart SetPrimaryColour(ushort Colour)
        {
            this.fPrimaryColour = Colour;
            return this;
        }
        public FigurePart SetSecondaryColour(ushort Colour)
        {
            this.fSecondaryColour = Colour;
            return this;
        }


        public abstract ushort GetModelID();

        public byte GetAmountOfColours()
        {
            if (this.fPrimaryColour == 0)
                return 0;
            if (this.fSecondaryColour == 0)
                return 1;
            return 2;
        }
    }
}