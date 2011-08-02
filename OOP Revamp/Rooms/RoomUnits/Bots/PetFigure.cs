using System;
using System.Collections.Generic;
using System.Data;
using IHI.Server.Networking.Messages;
using IHI.Server.Users.Permissions;
using IHI.Server.Networking;
using System.Linq;
using IHI.Database;
using NHibernate;

namespace IHI.Server.Rooms
{
    public struct PetFigure : IFigure
    {

        public string GetStandardFigure()
        {
            throw new NotImplementedException();
        }
    }
}