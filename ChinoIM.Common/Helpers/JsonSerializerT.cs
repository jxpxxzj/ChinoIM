﻿using ChinoIM.Common.Serialization;

namespace ChinoIM.Common.Helpers
{
    public class JsonSerializer<T> : ISerializer<T> where T: ISerializable
    {
        public T Deserialize(string data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }

        public string Serialize(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
