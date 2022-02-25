using System.Text.RegularExpressions;

namespace ProtoBuf2LuaAnnotation
{
    public static class ProtoParser
    {
        public const string UnusedCommentPattern = @"^( *//).*|/\*(.|[\r\n])*?\*/";
        
        public const string PackagePattern = @"package(?: | *[\r\n] *)+(\w+)(?: | *[\r\n] *)*;";

        public const string EnumBodyPattern = @"enum(?: | *[\r\n] *)+(\w+)(?: | *[\r\n] *)*\{(?:.|[\r\n])*?\}";
        public const string EnumFieldPattern = @"(\w+)(?: | *[\r\n] *)*=(?: | *[\r\n] *)*([0-9]+)(?: | *[\r\n] *)*; *(?://)*(.*)";
        
        public const string MessageBodyPattern = @"message(?: | *[\r\n] *)+(\w+)(?: | *[\r\n] *)*\{(?:.|[\r\n])*?\}";
        public const string MessageFieldPattern = @"(\w+)(?: | *[\r\n] *)+(\w+)(?: | *[\r\n] *)+(\w*)(?: | *[\r\n] *)*=(?: | *[\r\n] *)*[0-9]+(?: | *[\r\n] *)*; *(?://)*(.*)";
        public const string MessageMapFieldPattern = @"map(?: | *[\r\n] *)*<(?: | *[\r\n] *)*(\w+)(?: | *[\r\n] *)*,(?: | *[\r\n] *)*(\w+)(?: | *[\r\n] *)*>(?: | *[\r\n] *)*(\w+)(?: | *[\r\n] *)*=(?: | *[\r\n] *)*[0-9]+(?: | *[\r\n] *)*; *(?://)*(.*)";

        public static ProtoData Parse(string protoContent)
        {
            //去除无用的注释，避免后续的正则匹配到注释里的内容
            protoContent = Regex.Replace(protoContent, UnusedCommentPattern, "");
            //Package
            ProtoData protoData = new ProtoData(Regex.Match(protoContent, PackagePattern).Groups[1].Value);
            //解析enum
            foreach (Match bodyMatch in Regex.Matches(protoContent, EnumBodyPattern))
            {
                string enumName = bodyMatch.Groups[1].Value;
                var protoEnumData = new ProtoEnumData(enumName);
                protoData.EnumDataList.Add(protoEnumData);
                foreach (Match fieldMatch in Regex.Matches(bodyMatch.Value, EnumFieldPattern))
                {
                    string fieldName = fieldMatch.Groups[1].Value;
                    int fieldValue = int.Parse(fieldMatch.Groups[2].Value);
                    string fieldComment = fieldMatch.Groups[3].Value;
                    protoEnumData.TypeList.Add((fieldName, fieldValue, fieldComment));
                }
            }
            //解析message
            foreach (Match bodyMatch in Regex.Matches(protoContent, MessageBodyPattern))
            {
                string messageName = bodyMatch.Groups[1].Value;
                var protoMessageData = new ProtoMessageData(messageName);
                protoData.MessageDataList.Add(protoMessageData);
                foreach (Match fieldMatch in Regex.Matches(bodyMatch.Value, MessageFieldPattern))
                {
                    //[modifier] type name = x; //comment
                    string fieldName = fieldMatch.Groups[3].Value;
                    string fieldModifier;
                    string fieldType;
                    string fieldComment = fieldMatch.Groups[4].Value;
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        fieldModifier = fieldMatch.Groups[1].Value;
                        fieldType = fieldMatch.Groups[2].Value;
                    }
                    else
                    {
                        fieldModifier = "";
                        fieldType = fieldMatch.Groups[1].Value;
                        fieldName = fieldMatch.Groups[2].Value;
                    }
                    protoMessageData.TypeList.Add((false, fieldName, fieldModifier, fieldType, fieldComment));
                }

                foreach (Match mapFieldMatch in Regex.Matches(bodyMatch.Value, MessageMapFieldPattern))
                {
                    string keyType = mapFieldMatch.Groups[1].Value;
                    string valueType = mapFieldMatch.Groups[2].Value;
                    string fieldName = mapFieldMatch.Groups[3].Value;
                    string fieldComment = mapFieldMatch.Groups[4].Value;
                    protoMessageData.TypeList.Add((true, fieldName, keyType, valueType, fieldComment));
                }
            }
            return protoData;
        }
    }
}