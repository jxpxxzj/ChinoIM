namespace ChinoIM.Common.Serialization
{
    public interface ISerializer<T> where T: ISerializable
    {
        string Serialize(T obj);
        T Deserialize(string data);
    }
}
