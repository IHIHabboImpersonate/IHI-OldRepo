using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHI.Server
{
    // TODO: Rename
    public interface IObjectVariables
    {
        object GetInstanceVariable(string Name);

        IObjectVariables SetInstanceVariable(string Name, object Value);


        string GetPersistantVariable(string Name);

        IObjectVariables SetPersistantVariable(string Name, string Value);
    }
}
