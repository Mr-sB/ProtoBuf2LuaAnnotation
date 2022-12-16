using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProtoBuf2LuaAnnotation.Editor
{
    public class LuaAnnotationWindow : EditorWindow
    {
        public const string ClassAnnotationTemplate = "---@class {0}\n";
        public const string ClassTemplate = "local {0} = {1}\n";
        public const string FieldContentTemplate = "\n    {0} = {1},{2}";
        public const string FieldAnnotationTemplate = "---@field public {0} {1}{2}\n";
        public const string ReturnTemplate = "return {{{0}}}\n";
        public const string ReturnContentTemplate = "\n    {0} = {0},";

        private string protoDirectory;
        private string luaDirectory;
        private string prefix;

        [MenuItem("Tools/ProtoBuf2LuaAnnotation")]
        private static void ShowWindow()
        {
            var window = GetWindow<LuaAnnotationWindow>(false, "LuaAnnotationWindow", true);
            window.minSize = new Vector2(400f, 200f);
        }
        
        private void OnGUI()
        {
            protoDirectory = DrawPath("ProtoBuf", protoDirectory);
            luaDirectory = DrawPath("LuaAnnotation", luaDirectory);
            prefix = EditorGUILayout.TextField("MessagePrefix", prefix);

            if (GUILayout.Button("Gen LuaAnnotation"))
            {
                GenLuaAnnotation(protoDirectory, luaDirectory, prefix);
                Debug.Log("Gen LuaAnnotation success!");
            }
        }
        
        public static void GenLuaAnnotation(string protoDirectory, string luaDirectory, string prefix = "")
        {
            foreach (var filePath in Directory.GetFiles(protoDirectory, "*.proto", SearchOption.AllDirectories))
            {
                string directory = luaDirectory + Path.GetDirectoryName(filePath).Substring(protoDirectory.Length);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, Path.ChangeExtension(Path.GetFileName(filePath), "lua")), BuildLuaAnnotation(filePath, prefix));
            }
            AssetDatabase.Refresh();
        }

        public static string BuildLuaAnnotation(string protoPath, string prefix = "")
        {
            var protoData = ProtoParser.Parse(File.ReadAllText(protoPath));
            List<string> enumNames = new List<string>();
            string result = "";
            foreach (var enumData in protoData.EnumDataList)
            {
                //加上自定义前缀
                var name = GetCustomFieldType(protoData.Package, enumData.Name, prefix);
                enumNames.Add(name);
                result += string.Format(ClassAnnotationTemplate, name);
                var fieldsContent = "{";
                foreach (var typeInfo in enumData.TypeList)
                {
                    string comment = string.IsNullOrEmpty(typeInfo.comment) ? "" : $" --{typeInfo.comment}";
                    fieldsContent += string.Format(FieldContentTemplate, typeInfo.name, typeInfo.value, comment);
                }
                fieldsContent += "\n}";
                result += string.Format(ClassTemplate, name, fieldsContent) + "\n";
            }
            foreach (var messageData in protoData.MessageDataList)
            {
                //加上自定义前缀
                var name = GetCustomFieldType(protoData.Package, messageData.Name, prefix);
                result += string.Format(ClassAnnotationTemplate, name);
                foreach (var typeInfo in messageData.TypeList)
                {
                    string comment = string.IsNullOrEmpty(typeInfo.comment) ? "" : $" @{typeInfo.comment}";
                    result += string.Format(FieldAnnotationTemplate, typeInfo.name, GetFieldType(protoData.Package, typeInfo.isMap, typeInfo.type, typeInfo.modifier, prefix), comment);
                }
                result += string.Format(ClassTemplate, name, "nil") + "\n";
            }

            if (enumNames.Count > 0)
            {
                string returnContent = "";
                foreach (var enumName in enumNames)
                    returnContent += string.Format(ReturnContentTemplate, enumName);
                returnContent += "\n";
                result += string.Format(ReturnTemplate, returnContent);
            }
            
            return result;
        }

        public static string GetFieldType(string package, bool isMap, string type, string modifier, string prefix = "")
        {
            if (!isMap)
            {
                type = GetFieldType(package, type, prefix);
                if (modifier == "repeated")
                    type += "[]";
            }
            else
            {
                string keyType = GetFieldType(package, modifier, prefix);
                string valueType = GetFieldType(package, type, prefix);
                type = $"table<{keyType}, {valueType}>";
            }
            return type;
        }

        public static string GetFieldType(string package, string protoType, string prefix)
        {
            //proto type => lua type
            switch (protoType)
            {
                case ProtoFieldType.Double:
                case ProtoFieldType.Float:
                case ProtoFieldType.Int32:
                case ProtoFieldType.Int64:
                case ProtoFieldType.UIt32:
                case ProtoFieldType.UIt64:
                case ProtoFieldType.SInt32:
                case ProtoFieldType.SInt64:
                case ProtoFieldType.Fixed32:
                case ProtoFieldType.Fixed64:
                case ProtoFieldType.SFixed32:
                case ProtoFieldType.SFixed64:
                    return "number";
                case ProtoFieldType.Bool:
                    return "boolean";
                case ProtoFieldType.String:
                    return "string";
                case ProtoFieldType.Bytes:
                    return "string";
                default:
                    //可能会出现package.messageName的字段类型，需要拆解一下
                    var dotIndex = protoType.IndexOf('.');
                    if (dotIndex >= 0)
                    {
                        package = protoType.Substring(0, dotIndex);
                        protoType = protoType.Substring(dotIndex + 1);
                    }
                    return GetCustomFieldType(package, protoType, prefix);
            }
        }

        public static string GetCustomFieldType(string package, string protoType, string prefix)
        {
            return prefix + package + "_" + protoType;
        }
        
        private static string DrawPath(string label, string showPath)
        {
            EditorGUILayout.BeginHorizontal();
            showPath = EditorGUILayout.TextField(label, showPath);
            var pathRect = GUILayoutUtility.GetLastRect();
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var path = EditorUtility.OpenFolderPanel(label, showPath, "");
                if (!string.IsNullOrEmpty(path))
                    showPath = path;
            }
            if (GUILayout.Button("Open", GUILayout.Width(60)))
                EditorUtility.RevealInFinder(showPath);
            EditorGUILayout.EndHorizontal();
            
            //DragDrop
            if (Event.current.type == EventType.DragUpdated && pathRect.Contains(Event.current.mousePosition))
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            else if (Event.current.type == EventType.DragPerform && DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && pathRect.Contains(Event.current.mousePosition))
                showPath = DragAndDrop.paths[0];
            return showPath;
        }
    }
}