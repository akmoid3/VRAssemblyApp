using System;
using System.Reflection;
using UnityEngine;

public static class Extensions
{
    public static void CopyPropertiesAndFields(this object source, object destination)
    {
        var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.CanWrite)
            {
                try
                {
                    property.SetValue(destination, property.GetValue(source));
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to copy property '{property.Name}': {ex.Message}");
                }
            }
        }

        var fields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            try
            {
                field.SetValue(destination, field.GetValue(source));
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to copy field '{field.Name}': {ex.Message}");
            }
        }
    }
}
