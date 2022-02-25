using System.Collections.Generic;

namespace ProtoBuf2LuaAnnotation
{
    public class ProtoData
    {
        public string Package;
        public List<ProtoEnumData> EnumDataList = new List<ProtoEnumData>();
        public List<ProtoMessageData> MessageDataList = new List<ProtoMessageData>();

        public ProtoData(string package)
        {
            Package = package;
        }
    }
    
    public class ProtoEnumData
    {
        public string Name;
        public List<(string name, int value, string comment)> TypeList;

        public ProtoEnumData()
        {
            TypeList = new List<(string, int, string)>();
        }

        public ProtoEnumData(string name) : this()
        {
            Name = name;
        }
    }

    public class ProtoMessageData
    {
        public string Name;
        //isMap true:
        //modifier => keyType
        //type => valueType
        public List<(bool isMap, string name, string modifier, string type, string comment)> TypeList;
        
        public ProtoMessageData()
        {
            TypeList = new List<(bool, string, string, string, string)>();
        }

        public ProtoMessageData(string name) : this()
        {
            Name = name;
        }
    }
}