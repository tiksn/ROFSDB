using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Parquet;
using Parquet.Data;

namespace TIKSN.ROFSDB.Serialization;

public class ParquetSerialization : ISerialization
{
    private static readonly IEnumerable<string> fileExtensions = [".parquet"];

    public IEnumerable<string> FileExtensions => fileExtensions;

    public async IAsyncEnumerable<T> GetDocumentsAsync<T>(
        Stream stream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where T : class, new()
    {
        using var parquetReader = await ParquetReader.CreateAsync(stream, cancellationToken: cancellationToken);
        var schema = parquetReader.Schema;
        var dataFields = schema.GetDataFields();

        var rowGroupCount = parquetReader.RowGroupCount;
        for (int rowGroupIndex = 0; rowGroupIndex < rowGroupCount; rowGroupIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var rowGroupReader = parquetReader.OpenRowGroupReader(rowGroupIndex);
            var rowCountInGroup = rowGroupReader.RowCount;

            var columns = new Dictionary<string, DataColumn>();
            foreach (var field in dataFields)
            {
                var column = await rowGroupReader.ReadColumnAsync(field, cancellationToken);
                columns[field.Name] = column;
            }

            for (int rowIndex = 0; rowIndex < rowCountInGroup; rowIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var instance = new T();
                var type = typeof(T);

                foreach (var field in dataFields)
                {
                    var property = type.GetProperty(field.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (property != null && property.CanWrite)
                    {
                        var column = columns[field.Name];
                        var value = GetValueFromDataColumn(column, rowIndex, property.PropertyType);
                        if (value != null)
                        {
                            property.SetValue(instance, value);
                        }
                    }
                }

                yield return instance;
            }
        }
    }

    private static object GetValueFromDataColumn(DataColumn column, int rowIndex, Type targetType)
    {
        if (rowIndex >= column.Data.Length)
        {
            return null;
        }

        var value = column.Data.GetValue(rowIndex);
        if (value == null)
        {
            return null;
        }

        if (targetType.IsAssignableFrom(value.GetType()))
        {
            return value;
        }

        if (targetType == typeof(string))
        {
            return value.ToString();
        }

        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            if (value == null || value is DBNull)
            {
                return null;
            }
            return Convert.ChangeType(value, underlyingType);
        }

        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return value;
        }
    }
}