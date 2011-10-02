namespace IHI.Server
{
    public interface IInstanceVariables
    {
        object GetInstanceVariable(string Name);

        IInstanceVariables SetInstanceVariable(string Name, object Value);
    }
}