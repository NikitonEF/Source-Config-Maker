using System;
using Avalonia;
using AvaloniaEdit;

namespace SourceConfigMaker.Views;

public static class AvaloniaEditTextBehavior
{
    public static readonly AttachedProperty<string?> TextProperty =
        AvaloniaProperty.RegisterAttached<TextEditor, string?>("Text", typeof(AvaloniaEditTextBehavior));

    private static readonly AttachedProperty<bool> IsHookedProperty =
        AvaloniaProperty.RegisterAttached<TextEditor, bool>("IsHooked", typeof(AvaloniaEditTextBehavior));

    static AvaloniaEditTextBehavior()
    {
        TextProperty.Changed.AddClassHandler<TextEditor>(OnTextPropertyChanged);
    }

    public static string? GetText(TextEditor editor) => editor.GetValue(TextProperty);
    public static void SetText(TextEditor editor, string? value) => editor.SetValue(TextProperty, value);

    private static void OnTextPropertyChanged(TextEditor editor, AvaloniaPropertyChangedEventArgs e)
    {
        if (!editor.GetValue(IsHookedProperty))
        {
            editor.SetValue(IsHookedProperty, true);
            editor.TextChanged += (_, _) =>
            {
                if (GetText(editor) != editor.Text)
                    SetText(editor, editor.Text);
            };
        }

        var incoming = e.NewValue as string ?? string.Empty;
        if (editor.Text != incoming)
        {
            var caret = editor.CaretOffset;
            editor.Text = incoming;
            editor.CaretOffset = Math.Min(caret, editor.Text.Length);
        }
    }
}