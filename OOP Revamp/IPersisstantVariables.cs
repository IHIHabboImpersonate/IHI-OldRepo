namespace IHI.Server
{
    public interface IPersistantVariables
    {
        string GetPersistantVariable(string name);

        IPersistantVariables SetPersistantVariable(string name, string value);
    }
}