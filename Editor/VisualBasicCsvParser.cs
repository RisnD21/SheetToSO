using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

namespace SheetToSO
{
    /// <summary>
    /// CSV 解析器，使用 VisualBasic 的 TextFieldParser
    /// </summary>
    public class VisualBasicCsvParser : ICsvParser
    {
        public IEnumerable<string[]> Parse(string path, int linesToSkip)
        {
            using (TextFieldParser parser = new (path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                for (int i = 0; i < linesToSkip; i++)
                {
                    if (parser.EndOfData) yield break;
                    parser.ReadLine();
                }
                
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields == null || fields.Length == 0) continue;

                    yield return fields;
                }
            }
        }
    }
}