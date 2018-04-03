namespace ChinoIM.Common.Serialization
{
    public interface ISerializable
    {
        void ReadFromStream(SerializationReader reader);
        void WriteToStream(SerializationWriter writer);
    }
}
