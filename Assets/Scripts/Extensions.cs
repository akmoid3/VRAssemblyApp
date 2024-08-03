using System.Reflection;

public static class Extensions
{
    public static void CopyPropertiesAndFields(this object source, object destination)
    {
        // Copy all properties
        var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.CanWrite)
            {
                try
                {
                    property.SetValue(destination, property.GetValue(source));
                }
                catch
                {
                    // Ignore properties that cannot be copied
                }
            }
        }

        // Copy all fields
        var fields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            try
            {
                field.SetValue(destination, field.GetValue(source));
            }
            catch
            {
                // Ignore fields that cannot be copied
            }
        }
    }
}
