using Octopus.CoreParsers.Hcl;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace TIKSN.ROFSDB.Serialization
{
    public class HclSerialization : ISerialization
    {
        private static readonly IEnumerable<string> fileExtensions = new[] { ".hcl" };

        public IEnumerable<string> FileExtensions => fileExtensions;

        public async IAsyncEnumerable<T> GetDocumentsAsync<T>(
            Stream stream,
            [EnumeratorCancellation] CancellationToken cancellationToken)
            where T : class, new()
        {
            var streamReader = new StreamReader(stream);
            var template = await streamReader.ReadToEndAsync(cancellationToken);
            var parsed = HclParser.HclTemplate.Parse(template);
            yield return Deserialize<T>(parsed);
        }

        private static T Deserialize<T>(HclElement parsed) where T : class, new()
        {
            T model = new();
            var properties = typeof(T).GetProperties().ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase);

            foreach (var child in parsed.Children)
            {
                var property = properties[child.Name];
                SetValue(model, property, child.ProcessedValue);
            }

            return model;
        }

        private static object GetValue(Type propertyType, string processedValue)
        {
            if (propertyType == typeof(string))
            {
                return processedValue;
            }

            var stringAndFormatterMethod = propertyType
                .GetMethods()
                .Single(x =>
                    x.Name.Equals("Parse", StringComparison.Ordinal) &&
                    x.IsStatic && x.IsPublic &&
                    PickParseMethodByParameters(x.GetParameters()));

            return stringAndFormatterMethod.Invoke(null, new object[] { processedValue, CultureInfo.InvariantCulture });
        }

        private static bool PickParseMethodByParameters(ParameterInfo[] parameterInfos)
        {
            return parameterInfos.Length == 2 &&
                parameterInfos[0].ParameterType == typeof(string) &&
                parameterInfos[1].ParameterType == typeof(IFormatProvider);
        }

        private static void SetValue<T>(T model, PropertyInfo property, string processedValue) where T : class, new()
        {
            property.SetValue(model, GetValue(property.PropertyType, processedValue));
        }
    }
}