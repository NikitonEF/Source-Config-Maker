using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace SourceConfigMaker.Views;

public class SourceSyntaxColorizer : DocumentColorizingTransformer
{
    private static readonly Regex CommentRegex = new(@"//.*$", RegexOptions.Compiled);
    private static readonly Regex StringRegex = new("\"[^\"]*\"", RegexOptions.Compiled);
    private static readonly Regex KeywordRegex = new(@"\balias\b", RegexOptions.Compiled);
    private static readonly Regex CommandRegex = new(@"[+-][A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);
    private static readonly Regex NumberRegex = new(@"(?<![\w""])\d+(\.\d+)?(?![\w""])", RegexOptions.Compiled);

    protected override void ColorizeLine(DocumentLine line)
    {
        string text = CurrentContext.Document.GetText(line);
        int lineStart = line.Offset;

        // Порядок важен: комментарий должен перекрашивать всё, что внутри него, последним
        Apply(NumberRegex, text, lineStart, Brushes.Orange);
        Apply(CommandRegex, text, lineStart, Brushes.MediumSpringGreen);
        Apply(KeywordRegex, text, lineStart, Brushes.DeepSkyBlue);
        Apply(StringRegex, text, lineStart, Brushes.Goldenrod);
        Apply(CommentRegex, text, lineStart, Brushes.Gray);
    }

    private void Apply(Regex regex, string text, int lineStart, IBrush brush)
    {
        foreach (Match m in regex.Matches(text))
        {
            ChangeLinePart(
                lineStart + m.Index,
                lineStart + m.Index + m.Length,
                el => el.TextRunProperties.SetForegroundBrush(brush));
        }
    }
}