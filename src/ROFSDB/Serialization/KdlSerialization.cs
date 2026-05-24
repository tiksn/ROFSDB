using KdlSharp;
using KdlSharp.Exceptions;
using KdlSharp.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TIKSN.ROFSDB.Serialization;

public class KdlSerialization : ISerialization
{
    private static readonly IEnumerable<string> fileExtensions = [".kdl"];
    private static readonly KdlParserSettings parserSettings = new()
    {
        TargetVersion = KdlVersion.V2,
        AllowDuplicateProperties = false
    };

    public IEnumerable<string> FileExtensions => fileExtensions;

    public async IAsyncEnumerable<T> GetDocumentsAsync<T>(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where T : class, new()
    {
        KdlDocument document;
        try
        {
            document = await KdlDocument.ParseStreamAsync(stream, parserSettings, leaveOpen: true, cancellationToken);
        }
        catch (KdlException ex)
        {
            throw new FormatException("Could not parse as KDL.", ex);
        }

        var expectedNodeName = ToKebabCase(typeof(T).Name);
        var properties = GetSerializableProperties<T>();

        foreach (var node in document.Nodes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.Equals(node.Name, expectedNodeName, StringComparison.Ordinal))
            {
                throw new FormatException(
                    $"Unexpected KDL node '{node.Name}'. Expected '{expectedNodeName}' for '{typeof(T).Name}'.");
            }

            yield return DeserializeNode<T>(node, properties);
        }
    }

    private static object ConvertValue(KdlValue value, Type targetType, string memberName)
    {
        var nullableType = Nullable.GetUnderlyingType(targetType);
        if (value.ValueType == KdlValueType.Null)
        {
            if (!targetType.IsValueType || nullableType != null)
            {
                return null;
            }

            throw new FormatException($"KDL member '{memberName}' cannot be null.");
        }

        var conversionType = nullableType ?? targetType;

        if (conversionType == typeof(string))
        {
            return RequireValue(value.ValueType == KdlValueType.String, value.AsString(), memberName, conversionType);
        }

        if (conversionType == typeof(char))
        {
            var text = RequireValue(value.ValueType == KdlValueType.String, value.AsString(), memberName, conversionType);
            if (text.Length == 1)
            {
                return text[0];
            }

            throw new FormatException($"KDL member '{memberName}' must contain a single character.");
        }

        if (conversionType == typeof(bool))
        {
            return RequireBoolean(value, memberName, conversionType);
        }

        if (conversionType.IsEnum)
        {
            return ConvertEnumValue(value, conversionType, memberName);
        }

        if (IsIntegralType(conversionType))
        {
            var number = RequireNumber(value, memberName, conversionType);
            return ConvertIntegralValue(number, conversionType, memberName);
        }

        if (conversionType == typeof(decimal))
        {
            return RequireNumber(value, memberName, conversionType);
        }

        if (conversionType == typeof(double))
        {
            return RequireDouble(value, memberName, conversionType);
        }

        if (conversionType == typeof(float))
        {
            var number = RequireDouble(value, memberName, conversionType);
            return (float)number;
        }

        if (conversionType == typeof(Guid))
        {
            var text = RequireValue(value.ValueType == KdlValueType.String, value.AsString(), memberName, conversionType);
            return Guid.Parse(text);
        }

        if (conversionType == typeof(DateTime))
        {
            var text = RequireValue(value.ValueType == KdlValueType.String, value.AsString(), memberName, conversionType);
            return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        if (conversionType == typeof(DateTimeOffset))
        {
            var text = RequireValue(value.ValueType == KdlValueType.String, value.AsString(), memberName, conversionType);
            return DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        if (conversionType == typeof(TimeSpan))
        {
            var text = RequireValue(value.ValueType == KdlValueType.String, value.AsString(), memberName, conversionType);
            return TimeSpan.Parse(text, CultureInfo.InvariantCulture);
        }

        throw new FormatException($"KDL member '{memberName}' has unsupported target type '{targetType.Name}'.");
    }

    private static object ConvertEnumValue(KdlValue value, Type enumType, string memberName)
    {
        if (value.ValueType == KdlValueType.String)
        {
            var text = RequireValue(true, value.AsString(), memberName, enumType);
            return Enum.Parse(enumType, text, ignoreCase: false);
        }

        if (value.ValueType == KdlValueType.Number)
        {
            var number = RequireNumber(value, memberName, enumType);
            var enumBaseType = Enum.GetUnderlyingType(enumType);
            return Enum.ToObject(enumType, ConvertIntegralValue(number, enumBaseType, memberName));
        }

        throw new FormatException($"KDL member '{memberName}' cannot be converted to enum type '{enumType.Name}'.");
    }

    private static object ConvertIntegralValue(decimal number, Type targetType, string memberName)
    {
        if (decimal.Truncate(number) != number)
        {
            throw new FormatException($"KDL member '{memberName}' must be an integer.");
        }

        try
        {
            return Convert.ChangeType(number, targetType, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is InvalidCastException or OverflowException)
        {
            throw new FormatException($"KDL member '{memberName}' cannot be converted to '{targetType.Name}'.", ex);
        }
    }

    private static T DeserializeNode<T>(KdlNode node, IReadOnlyDictionary<string, PropertyInfo> properties)
        where T : class, new()
    {
        if (node.Arguments.Count != 0)
        {
            throw new FormatException($"KDL node '{node.Name}' must not contain positional arguments.");
        }

        T model = new();
        var seenMembers = new HashSet<string>(StringComparer.Ordinal);

        foreach (var property in node.Properties)
        {
            SetMember(model, properties, seenMembers, property.Key, property.Value);
        }

        foreach (var child in node.Children)
        {
            if (child.Arguments.Count != 1 || child.Properties.Count != 0 || child.Children.Count != 0)
            {
                throw new FormatException(
                    $"KDL child member '{child.Name}' must contain exactly one argument and no properties or children.");
            }

            SetMember(model, properties, seenMembers, child.Name, child.Arguments[0]);
        }

        return model;
    }

    private static Dictionary<string, PropertyInfo> GetSerializableProperties<T>()
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(static property => property.CanWrite && property.SetMethod?.IsPublic == true)
            .ToArray();

        var result = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
        foreach (var property in properties)
        {
            if (!IsSupportedType(property.PropertyType))
            {
                throw new FormatException(
                    $"KDL member '{property.Name}' has unsupported target type '{property.PropertyType.Name}'.");
            }

            var memberName = ToKebabCase(property.Name);
            if (!result.TryAdd(memberName, property))
            {
                throw new FormatException($"KDL member name '{memberName}' maps to more than one property.");
            }
        }

        return result;
    }

    private static bool IsIntegralType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type == typeof(byte) ||
            type == typeof(sbyte) ||
            type == typeof(short) ||
            type == typeof(ushort) ||
            type == typeof(int) ||
            type == typeof(uint) ||
            type == typeof(long) ||
            type == typeof(ulong);
    }

    private static bool IsSupportedType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type == typeof(string) ||
            type == typeof(char) ||
            type == typeof(bool) ||
            type == typeof(decimal) ||
            type == typeof(double) ||
            type == typeof(float) ||
            type == typeof(Guid) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) ||
            type.IsEnum ||
            IsIntegralType(type);
    }

    private static bool RequireBoolean(KdlValue value, string memberName, Type targetType)
    {
        var boolean = value.AsBoolean();
        if (value.ValueType == KdlValueType.Boolean && boolean.HasValue)
        {
            return boolean.Value;
        }

        throw new FormatException($"KDL member '{memberName}' cannot be converted to '{targetType.Name}'.");
    }

    private static double RequireDouble(KdlValue value, string memberName, Type targetType)
    {
        var number = value.AsDouble();
        if (value.ValueType == KdlValueType.Number && number.HasValue)
        {
            return number.Value;
        }

        throw new FormatException($"KDL member '{memberName}' cannot be converted to '{targetType.Name}'.");
    }

    private static decimal RequireNumber(KdlValue value, string memberName, Type targetType)
    {
        var number = value.AsNumber();
        if (value.ValueType == KdlValueType.Number && number.HasValue)
        {
            return number.Value;
        }

        throw new FormatException($"KDL member '{memberName}' cannot be converted to '{targetType.Name}'.");
    }

    private static T RequireValue<T>(bool isExpectedValueType, T value, string memberName, Type targetType)
    {
        if (isExpectedValueType && value != null)
        {
            return value;
        }

        throw new FormatException($"KDL member '{memberName}' cannot be converted to '{targetType.Name}'.");
    }

    private static void SetMember<T>(
        T model,
        IReadOnlyDictionary<string, PropertyInfo> properties,
        HashSet<string> seenMembers,
        string memberName,
        KdlValue value)
        where T : class, new()
    {
        if (!properties.TryGetValue(memberName, out var property))
        {
            throw new FormatException($"Unknown KDL member '{memberName}' for '{typeof(T).Name}'.");
        }

        if (!seenMembers.Add(memberName))
        {
            throw new FormatException($"Duplicate KDL member '{memberName}' for '{typeof(T).Name}'.");
        }

        property.SetValue(model, ConvertValue(value, property.PropertyType, memberName));
    }

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var result = new List<char>(value.Length + 8);

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            if (i > 0 &&
                char.IsUpper(current) &&
                (char.IsLower(value[i - 1]) ||
                char.IsDigit(value[i - 1]) ||
                (i + 1 < value.Length && char.IsUpper(value[i - 1]) && char.IsLower(value[i + 1]))))
            {
                result.Add('-');
            }

            result.Add(char.ToLowerInvariant(current));
        }

        return new string([.. result]);
    }
}
