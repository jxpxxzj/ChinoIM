namespace ChinoIM.Common.Serialization
{
    public interface ISerializer<T>
    {
        string Serialize(T obj);
        T Deserialize(string data);
    }
}
