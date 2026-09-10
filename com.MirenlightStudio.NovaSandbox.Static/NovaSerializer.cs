using System.Reflection;
using System.Text;

namespace com.MirenlightStudio.NovaSandbox.Static
{
    public static partial class NovaSerializer
    {
        public static string Serialize<T>(T obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            Type type = typeof(T);

            NovaSerializerModelAttribute? modelAttr = type.GetCustomAttribute<NovaSerializerModelAttribute>();
            string modelName = modelAttr?.Name ?? type.Name.ToLower();

            string idValue = string.Empty;
            StringBuilder sb = new StringBuilder();

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                if (prop.GetCustomAttribute<NovaSerializerIdAttribute>() != null)
                {
                    idValue = prop.GetValue(obj)?.ToString() ?? string.Empty;
                    continue;
                }

                NovaSerializerFieldOrPropertyAttribute? fieldAttr = prop.GetCustomAttribute<NovaSerializerFieldOrPropertyAttribute>();
                if (fieldAttr != null)
                {
                    object? val = prop.GetValue(obj);
                    string strVal = val?.ToString() ?? string.Empty;

                    sb.Append($"{fieldAttr.Name}={strVal};");
                }
            }
            return $"{modelName}<{idValue}>/{sb}";
        }
        public static T? Deserialize<T>(string data) where T : new()
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return default;
            }

            string[] parts = data.Split('/');
            if (parts.Length != 2)
            {
                throw new FormatException("Invalid data format");
            }

            string header = parts[0];
            string body = parts[1];

            int idStart = header.IndexOf('<');
            int idEnd = header.IndexOf('>');
            if (idStart == -1 || idEnd == -1 || idEnd <= idStart)
            {
                throw new FormatException("Invalid header format (missing ID brackets)");
            }

            string idValue = header.Substring(idStart + 1, idEnd - idStart - 1);

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(body))
            {
                string[] fields = body.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string field in fields)
                {
                    int equalsIndex = field.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string key = field.Substring(0, equalsIndex);
                        string val = field.Substring(equalsIndex + 1);
                        keyValuePairs[key] = val;
                    }
                }
            }

            T obj = new T();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<NovaSerializerIdAttribute>() != null)
                {
                    SetValue(obj, prop, idValue);
                    continue;
                }

                var fieldAttr = prop.GetCustomAttribute<NovaSerializerFieldOrPropertyAttribute>();
                if (fieldAttr != null && keyValuePairs.TryGetValue(fieldAttr.Name, out var strVal))
                {
                    SetValue(obj, prop, strVal);
                }
            }
            return obj;
        }
        private static void SetValue(object obj, PropertyInfo prop, string stringValue)
        {
            Type targetType = prop.PropertyType;
            try
            {
                if (targetType == typeof(Guid))
                {
                    prop.SetValue(obj, Guid.Parse(stringValue));
                }
                else if (targetType == typeof(bool))
                {
                    bool bVal = stringValue == "1" || (bool.TryParse(stringValue, out bool b) && b);
                    prop.SetValue(obj, bVal);
                }
                else
                {
                    object convVal = Convert.ChangeType(stringValue, targetType);
                    prop.SetValue(obj, convVal);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}