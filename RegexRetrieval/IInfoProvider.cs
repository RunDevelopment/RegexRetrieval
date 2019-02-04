using System.Collections.Generic;

namespace RegexRetrieval
{
    public interface IInfoProvider
    {
        IReadOnlyList<InfoMeta> InfoMetadata { get; }
        bool TryGetInfo(string name, out object value);
    }

    public class InfoMeta
    {
        public string Name { get; }
        public int Length { get; }

        public InfoMeta(string name, int length)
        {
            Name = name;
            Length = length;
        }
    }
}
