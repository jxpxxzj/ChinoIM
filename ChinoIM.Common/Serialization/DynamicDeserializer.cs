﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace ChinoIM.Common.Serialization
{
    public class DynamicDeserializer
    {
        private static VersionConfigToNamespaceAssemblyObjectBinder versionBinder;
        private static BinaryFormatter formatter;


        private static void Initialize()
        {
            versionBinder = new VersionConfigToNamespaceAssemblyObjectBinder();
            formatter = new BinaryFormatter()
            {
                AssemblyFormat = FormatterAssemblyStyle.Simple,
                Binder = versionBinder
            };
        }

        public static Object Deserialize(Stream stream)
        {
            if (formatter == null)
                Initialize();
            return formatter.Deserialize(stream);
        }

        #region Nested type: VersionConfigToNamespaceAssemblyObjectBinder

        internal sealed class VersionConfigToNamespaceAssemblyObjectBinder : SerializationBinder
        {
            private readonly Dictionary<string, Type> cache = new Dictionary<string, Type>();

            public override Type BindToType(string assemblyName, string typeName)
            {

                if (cache.TryGetValue(assemblyName + typeName, out var typeToDeserialize))
                    return typeToDeserialize;

                List<Type> tmpTypes = new List<Type>();
                Type genType = null;

                try
                {
                    if (typeName.Contains("System.Collections.Generic") && typeName.Contains("[["))
                    {
                        string[] splitTyps = typeName.Split(new char[] { '[' });

                        foreach (string typ in splitTyps)
                        {
                            if (typ.Contains("Version"))
                            {
                                string asmTmp = typ.Substring(typ.IndexOf(',') + 1);
                                string asmName = asmTmp.Remove(asmTmp.IndexOf(']')).Trim();
                                string typName = typ.Remove(typ.IndexOf(','));
                                tmpTypes.Add(BindToType(asmName, typName));
                            }
                            else if (typ.Contains("Generic"))
                            {
                                genType = BindToType(assemblyName, typ);
                            }
                        }
                        if (genType != null && tmpTypes.Count > 0)
                        {
                            return genType.MakeGenericType(tmpTypes.ToArray());
                        }
                    }

                    string ToAssemblyName = assemblyName.Split(',')[0];
                    Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var a in Assemblies)
                    {
                        if (a.FullName.Split(',')[0] == ToAssemblyName)
                        {
                            typeToDeserialize = a.GetType(typeName);
                            break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }

                cache.Add(assemblyName + typeName, typeToDeserialize);

                return typeToDeserialize;
            }
        }

        #endregion
    }
}
