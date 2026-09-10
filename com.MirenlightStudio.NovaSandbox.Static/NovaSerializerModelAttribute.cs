namespace com.MirenlightStudio.NovaSandbox.Static
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NovaSerializerModelAttribute : Attribute
    {
        public string Name { get; }
        public NovaSerializerModelAttribute(string name)
        {
            Name = name;
        }
    }
}
