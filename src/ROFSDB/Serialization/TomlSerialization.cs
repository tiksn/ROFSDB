using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Tomlyn;

namespace TIKSN.ROFSDB.Serialization
{
    public class TomlSerialization : ISerialization
    {
        private static readonly IEnumerable<string> fileExtensions = new[] { ".toml" };

        public IEnumerable<string> FileExtensions => fileExtensions;

        public async IAsyncEnumerable<T> GetDocumentsAsync<T>(
            Stream stream,
            [EnumeratorCancellation] CancellationToken cancellationToken)
            where T : class, new()
        {
            using var streamReader = new StreamReader(stream);

            string content = streamReader.ReadToEnd();

            yield return Toml.ToModel<T>(content);
        }
    }
}