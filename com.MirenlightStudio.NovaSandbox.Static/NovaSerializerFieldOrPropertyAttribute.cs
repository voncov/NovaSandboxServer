namespace com.MirenlightStudio.NovaSandbox.Static
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NovaSerializerFieldOrPropertyAttribute : Attribute
    {
        public string Name { get; }
        public NovaSerializerFieldOrPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}
