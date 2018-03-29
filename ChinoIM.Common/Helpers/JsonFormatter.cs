﻿using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace ChinoIM.Common.Helpers
{
    public class JsonFormatter<T> : IFormatter, IFormatProvider
    {
        public SerializationBinder Binder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public StreamingContext Context { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ISurrogateSelector SurrogateSelector { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public object Deserialize(Stream serializationStream)
        {
            serializationStream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(serializationStream))
            {
                var str = reader.ReadToEnd();
                return JsonSerializer.Deserialize<T>(str);
            }
        }

        public object GetFormat(Type formatType)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            var writer = new StreamWriter(serializationStream);
            var json = JsonSerializer.Serialize(graph);
            writer.Write(json);
            writer.Flush();
        }
    }
}
