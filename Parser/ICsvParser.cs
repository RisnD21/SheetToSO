using System.Collections.Generic;

namespace SheetToSO
{
    public interface ICsvParser
    {
        IEnumerable<string[]> Parse(string path, int lineToSkip);
    }
}