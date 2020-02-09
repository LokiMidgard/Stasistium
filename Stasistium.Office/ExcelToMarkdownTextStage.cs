using Stasistium.Documents;
using System;
using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.Linq;
using System.Text;
using Stasistium.Stages;

namespace Stasistium.Stages
{
    public class ExcelToMarkdownTextStage<TInCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<Stream, TInCache, string>
    where TInCache : class
    {

        private enum TableAlignment
        {
            Undefined,
            Left,
            Right,
            Center,
        }


        private readonly bool hasHeader;
        public ExcelToMarkdownTextStage(StageBase<Stream, TInCache> inputSingle0, bool hasHeader, IGeneratorContext context, string? name) : base(inputSingle0, context, name)
        {
            this.hasHeader = hasHeader;
        }

        protected override Task<IDocument<string>> Work(IDocument<Stream> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var sheetIndex = 0;
            using (var stream = input.Value)
            using (var excel = new ExcelPackage(stream))
            {

                if (sheetIndex >= excel.Workbook.Worksheets.Count)
                    throw this.Context.Exception($"Sheet does not exists index: {sheetIndex} count: {excel.Workbook.Worksheets.Count}");

                var sheet = excel.Workbook.Worksheets[sheetIndex];

                var dimension = sheet.Dimension;

                if (dimension is null)
                    return Task.FromResult(input.With(string.Empty, string.Empty));


                var rowCount = dimension?.Rows ?? 0;
                var columnCount = dimension?.Columns ?? 0;

                var table = new (string value, TableAlignment alignment)[columnCount, rowCount];

                for (int row = 0; row < rowCount; row++)
                    for (int colum = 0; colum < columnCount; colum++)
                    {
                        var cell = sheet.Cells[row, colum].FirstOrDefault();
                        var alignment = cell.Style.HorizontalAlignment switch
                        {
                            OfficeOpenXml.Style.ExcelHorizontalAlignment.Center => TableAlignment.Center,
                            OfficeOpenXml.Style.ExcelHorizontalAlignment.Fill => TableAlignment.Center,
                            OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify => TableAlignment.Center,
                            OfficeOpenXml.Style.ExcelHorizontalAlignment.CenterContinuous => TableAlignment.Center,
                            OfficeOpenXml.Style.ExcelHorizontalAlignment.Distributed => TableAlignment.Undefined,
                            OfficeOpenXml.Style.ExcelHorizontalAlignment.General => TableAlignment.Undefined,
                            OfficeOpenXml.Style.ExcelHorizontalAlignment.Left => TableAlignment.Left,
                            OfficeOpenXml.Style.ExcelHorizontalAlignment.Right => TableAlignment.Right,
                            _ => TableAlignment.Undefined,
                        };
                        table[colum, row] = (cell.Value?.ToString() ?? "", alignment);
                    }



                var builder = new StringBuilder();


                int[] columnSize = new int[columnCount];

                for (int column = 0; column < columnCount; column++)
                    for (int row = 0; row < rowCount; row++)
                    {
                        var cell = table[column, row].value;
                        columnSize[column] = Math.Max(columnSize[column], cell.Length);
                    }


                static void WriteLine(StringBuilder builder, int[] columnSize, bool isHeader = false)
                {
                    foreach (var column in columnSize)
                    {
                        builder.Append("+");
                        builder.Append(isHeader ? '=' : '-', column + 4);
                    }
                    builder.AppendLine("+");
                }

                bool firstLine = true;
                WriteLine(builder, columnSize);
                for (int row = 0; row < rowCount; row++)
                {
                    builder.Append("|");
                    for (int column = 0; column < columnSize.Length; column++)
                    {
                        var (value, alignment) = table[column, row];
                        var whitespace = columnSize[column] - value.Length;
                        var odd = whitespace % 2 == 1;
                        switch (alignment)
                        {
                            case TableAlignment.Left:
                                builder.Append(':');
                                builder.Append(' ');
                                builder.Append(value);
                                builder.Append(' ', whitespace + 2);
                                builder.Append('|');
                                break;
                            case TableAlignment.Right:
                                builder.Append(' ', whitespace + 2);
                                builder.Append(value);
                                builder.Append(' ');
                                builder.Append(':');
                                builder.Append('|');
                                break;
                            case TableAlignment.Center:
                                builder.Append(':');
                                builder.Append(' ');
                                builder.Append(' ', whitespace + 1);
                                builder.Append(value);
                                builder.Append(' ', whitespace + 1 + (odd ? 1 : 0));
                                builder.Append(':');
                                builder.Append('|');
                                break;
                            case TableAlignment.Undefined:
                            default:
                                builder.Append(' ');
                                builder.Append(' ');
                                builder.Append(' ', whitespace + 1);
                                builder.Append(value);
                                builder.Append(' ', whitespace + 1 + (odd ? 1 : 0));
                                builder.Append(' ');
                                builder.Append('|');
                                break;
                        }

                    }
                    builder.AppendLine();
                    WriteLine(builder, columnSize, this.hasHeader && firstLine);
                    firstLine = false;
                }
                var result = builder.ToString();
                return Task.FromResult(input.With(result, input.Context.GetHashForString(result)));
            }
        }
    }
}

namespace Stasistium
{
    public static class OfficeStageExtension
    {
        public static ExcelToMarkdownTextStage<T> ExcelToMarkdownText<T>(this StageBase<Stream, T> input, bool hasHeader = true, string? name = null)
        where T : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return new ExcelToMarkdownTextStage<T>(input, hasHeader, input.Context, name);
        }
    }
}