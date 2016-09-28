namespace ITweakMyBuild
{
    using System.Runtime.Serialization;

    [DataContract]
    public class Settings
    {
        [DataMember]
        public Property[] Properties { get; set; }
    }

    [DataContract]
    public class Property
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}
