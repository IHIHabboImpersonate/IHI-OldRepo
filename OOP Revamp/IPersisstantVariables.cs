namespace IHI.Server
{
    public interface IPersistantVariables
    {
        string GetPersistantVariable(string Name);

        IPersistantVariables SetPersistantVariable(string Name, string Value);
    }
}