using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace TIKSN.ROFSDB.Serialization
{
    public class YamlSerialization : ISerialization
    {
        private static readonly IEnumerable<string> fileExtensions = new[] { ".yml", ".yaml" };

        public IEnumerable<string> FileExtensions => fileExtensions;

        public async IAsyncEnumerable<T> GetDocumentsAsync<T>(
            Stream stream,
            [EnumeratorCancellation] CancellationToken cancellationToken)
            where T : class, new()
        {
            var streamReader = new StreamReader(stream);
            var scanner = new Scanner(streamReader);
            var deserializer = new DeserializerBuilder().Build();

            var parser = new Parser(scanner);

            parser.Consume<StreamStart>();

            while (parser.Accept(out DocumentStart documentStart))
            {
                var doc = deserializer.Deserialize<T>(parser);

                yield return doc;
            }
        }
    }
}