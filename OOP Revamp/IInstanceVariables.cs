namespace IHI.Server
{
    public interface IInstanceVariables
    {
        object GetInstanceVariable(string name);

        IInstanceVariables SetInstanceVariable(string name, object value);
    }
}