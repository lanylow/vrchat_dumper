using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mono.Cecil;

namespace vrchat_dumper
{
    internal static class Program
    {
        private static StreamWriter _streamWriter;
        
        private static readonly Dictionary<string, TypeDefinition> TypeCache = new Dictionary<string, TypeDefinition>();

        private static readonly Dictionary<string, Func<TypeDefinition, bool>> TypeRules = new Dictionary<string, Func<TypeDefinition, bool>>
        {
            { "UserSelectionManager", delegate(TypeDefinition definition)
                {
                    return 
                        definition.Fields.Count(field => field.FieldType.Name.Contains("APIUser")) == 2 && 
                        definition.Methods.Any(method => method.Name.Contains("OnDestroy")) &&
                        definition.Methods.Any(method => method.Name.Contains("Awake"));
                } 
            },
            {
                "Player", delegate(TypeDefinition definition)
                {
                    return
                        definition.Fields.Any(field => field.Name.Contains("_vrcplayer")) &&
                        definition.Methods.Any(method => method.Name.Contains("OnNetworkReady"));
                }
            },
            {
                "VRCInputManager", delegate(TypeDefinition definition)
                {
                    var matched = definition.NestedTypes.Any(type => type.IsEnum && type.Fields.Any(field => field.Name.Contains("SafetyLevel")));

                    if (!matched)
                    {
                        return false;
                    }
                    
                    foreach (var typeName in 
                             from field in definition.Fields 
                             where field.IsStatic && field.IsPrivate && field.FieldType.FullName.Contains("System.Collections.Generic.Dictionary`2<System.String,") 
                             select field.FieldType.FullName.Split(',')[1].Replace(">", "") into typeName 
                             where typeName != "System.Object" 
                             select typeName)
                    {
                        _streamWriter.WriteLine("Type " + typeName + " matched with VRCInput");
                    }

                    return true;
                }
            },
            {
                "VRCPlayer", delegate(TypeDefinition definition)
                {
                    return
                        definition.Methods.Any(method => method.Name.Contains("ReloadAvatarNetworkedRPC"));
                }
            },
            {
                "VRCMotionState", delegate(TypeDefinition definition)
                {
                    return
                        definition.NestedTypes.Any(type => type.IsEnum && type.Fields.Any(field => field.Name.Contains("Airborne")));
                }
            },
            {
                "PedalOption", delegate(TypeDefinition definition)
                {
                    return
                        definition.NestedTypes.Any(type => type.IsEnum && type.Fields.Any(field => field.Name.Contains("Default")) && type.Fields.Any(field => field.Name.Contains("Back")));
                }
            },
            {
                "ActionMenu", delegate(TypeDefinition definition)
                {
                    return
                        definition.NestedTypes.Any(type => type.IsEnum && type.Fields.Any(field => field.Name.Contains("Summer")));
                }
            },
            {
                "the type that contains player join events", delegate(TypeDefinition definition)
                {
                    return
                        definition.Methods.Any(method => method.Name.Contains("OnPlayerJoined") && method.IsPrivate && method.Parameters.Count == 1) &&
                        definition.Methods.Any(method => method.Name.Contains("OnPlayerLeft") && method.IsPrivate && method.Parameters.Count == 1) &&
                        definition.Fields.Any(field => field.FieldType.FullName.Contains("System.DateTime"));
                }
            }
        };

        private static readonly Dictionary<string, Func<MethodDefinition, bool>> MethodRules = new Dictionary<string, Func<MethodDefinition, bool>>
        {
            {
                "ActionMenu.NewPedalOption", definition => 
                    definition.DeclaringType == TypeCache["ActionMenu"] &&
                    definition.ReturnType == TypeCache["PedalOption"]
            },
            {
                "ActionMenu.OpenMainPage", definition => 
                    definition.DeclaringType == TypeCache["ActionMenu"] &&
                    definition.IsPublic &&
                    definition.ReturnType.FullName.Contains("System.Void") &&
                    definition.Name != ".ctor" &&
                    definition.Name != "Reset" &&
                    definition.Parameters.Count == 0
            },
            {
                "PedalOption.OnClicked", definition => 
                    definition.DeclaringType == TypeCache["PedalOption"] &&
                    definition.ReturnType.FullName.Contains("System.Void") &&
                    definition.IsPublic &&
                    definition.Parameters.Count == 0 &&
                    definition.Name != ".ctor"
            },
            {
                "PedalOption.SetName", delegate(MethodDefinition definition)
                {
                    return
                        definition.DeclaringType == TypeCache["PedalOption"] &&
                        definition.ReturnType.FullName.Contains("System.Void") &&
                        definition.IsSetter &&
                        definition.Parameters.All(parameter => parameter.ParameterType.FullName.Contains("System.String"));
                }
            }
        };

        public static void Main(string[] args)
        {
            var gameAssembly = ModuleDefinition.ReadModule(Environment.CurrentDirectory + @"\Assembly-CSharp.dll");

            if (File.Exists(Environment.CurrentDirectory + @"\output.txt"))
            {
                File.Delete(Environment.CurrentDirectory + @"\output.txt");
            }

            _streamWriter = new StreamWriter(File.Create(Environment.CurrentDirectory + @"\output.txt"));
            
            foreach (var type in gameAssembly.Types)
            {
                foreach (var rule in TypeRules.Where(rule => rule.Value(type)))
                {
                    TypeCache.Add(rule.Key, type);
                    _streamWriter.WriteLine("Type " + type.Name + " matched with " + rule.Key);
                }
            }
            
            _streamWriter.WriteLine(Environment.NewLine);
            
            foreach (var type in TypeCache)
            {
                foreach (var method in type.Value.Methods)
                {
                    foreach (var rule in MethodRules.Where(rule => rule.Value(method)))
                    {
                        _streamWriter.WriteLine("Method " + method.DeclaringType.Name + "." + method.Name + " matched with " + rule.Key);
                    }
                }
            }
            
            _streamWriter.Close();
        }
    }
}