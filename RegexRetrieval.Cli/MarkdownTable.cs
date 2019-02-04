using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RegexRetrieval.Cli
{
    internal class MarkdownTable
    {
        private readonly List<Column> Columns = new List<Column>();

        public TextWriter Writer { get; }


        public MarkdownTable() : this(Console.Out) { }
        public MarkdownTable(TextWriter writer)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }


        public void AddColumn(string title, int length, TextAlignment align = TextAlignment.Right, Func<object, string> formatter = null)
        {
            if (title != null) length = Math.Max(length, title.Length);

            Columns.Add(new Column
            {
                Title = title,
                Length = length,
                Alignment = align,
                Formatter = formatter ?? (o => o?.ToString())
            });
        }

        public void WriteHeader()
        {
            WriteRowImpl(Columns, c =>
            {
                var title = c.Title ?? "";
                Writer.Write(title);
                Writer.Write(new string(' ', c.Length - title.Length));
            });

            WriteRowImpl(Columns, c =>
            {
                switch (c.Alignment)
                {
                    case TextAlignment.Left:
                        Writer.Write(':');
                        Writer.Write(new string('-', c.Length - 1));
                        break;
                    case TextAlignment.Center:
                        Writer.Write(':');
                        Writer.Write(new string('-', c.Length - 2));
                        Writer.Write(':');
                        break;
                    case TextAlignment.Right:
                        Writer.Write(new string('-', c.Length - 1));
                        Writer.Write(':');
                        break;
                }
            });
        }

        public void WriteRow(params object[] values)
        {
            if (values.Length != Columns.Count)
                throw new ArgumentException(nameof(values));

            WriteRowImpl(Columns, (c, i) =>
            {
                var value = c.Formatter(values[i]) ?? "";

                if (value.Length >= c.Length)
                {
                    Writer.Write(value);
                }
                else
                {
                    switch (c.Alignment)
                    {
                        case TextAlignment.Left:
                            Writer.Write(value);
                            Writer.Write(new string(' ', c.Length - value.Length));
                            break;
                        case TextAlignment.Center:
                            var leftOffset = c.Length - value.Length;
                            leftOffset = leftOffset / 2;
                            Writer.Write(new string(' ', leftOffset));
                            Writer.Write(value);
                            Writer.Write(new string(' ', c.Length - value.Length - leftOffset));
                            break;
                        case TextAlignment.Right:
                            Writer.Write(new string(' ', c.Length - value.Length));
                            Writer.Write(value);
                            break;
                    }
                }
            });
        }
        public void WriteRow(IEnumerable values)
            => WriteRow(values.Cast<object>().ToArray());

        public void WriteEmptyRow()
        {
            WriteRowImpl(Columns, c => Writer.Write(new string(' ', c.Length)));
        }

        private void WriteRowImpl<T>(IEnumerable<T> collection, Action<T, int> writeItem)
        {
            Writer.Write("| ");

            int i = 0;
            foreach (var item in collection)
            {
                if (i > 0) Writer.Write(" | ");

                writeItem(item, i);
                i++;
            }
            Writer.WriteLine(" |");
        }
        private void WriteRowImpl<T>(IEnumerable<T> collection, Action<T> writeItem)
        {
            WriteRowImpl(collection, (c, i) => writeItem(c));
        }


        private class Column
        {
            public string Title { get; set; }
            public int Length { get; set; }
            public TextAlignment Alignment { get; set; }
            public Func<object, string> Formatter { get; set; }
        }
    }

    internal enum TextAlignment
    {
        Left,
        Center,
        Right
    }
}
