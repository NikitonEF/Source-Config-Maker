using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SourceConfigMaker.Models;

namespace SourceConfigMaker.Views
{
    public partial class PresetsView : UserControl
    {
        private Avalonia.Point _dragStartPoint;
        private PresetItem? _dragSourcePreset;
        private PointerPressedEventArgs? _dragStartEvent;

        public PresetsView()
        {
            InitializeComponent();
        }

        private void Preset_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Control control && control.DataContext is PresetItem preset)
            {
                var point = e.GetCurrentPoint(control);
                if (point.Properties.IsLeftButtonPressed)
                {
                    _dragStartPoint = point.Position;
                    _dragSourcePreset = preset;
                    _dragStartEvent = e;

                    control.PointerMoved += Preset_PointerMoved;
                }
            }
        }

        private async void Preset_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (_dragSourcePreset == null || sender is not Control control) return;

            var point = e.GetCurrentPoint(control);
            if (!point.Properties.IsLeftButtonPressed) return;

            var diff = point.Position - _dragStartPoint;

            if (Math.Abs(diff.X) > 3 || Math.Abs(diff.Y) > 3)
            {
                control.PointerMoved -= Preset_PointerMoved;

                var displayName = _dragSourcePreset.Name;
                var scriptContent = _dragSourcePreset.Content;

                // Умный поиск реальной команды внутри скрипта
                var actualCommand = GetCommandFromAliasScript(scriptContent);

                _dragSourcePreset = null;

                // В сессию для назначения на клавишу кладем именно найденную команду (например, +bhop)
                DragSession.Command = actualCommand;
                DragSession.Key = null;

                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                // Создаем призрака с оригинальным красивым именем (для пользователя)
                var ghost = new DragGhost(topLevel, displayName, e.GetPosition(topLevel));

                EventHandler<DragEventArgs> dragOverHandler = (_, args) => ghost.Update(args.GetPosition(topLevel));
                topLevel.AddHandler(DragDrop.DragOverEvent, dragOverHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);

                var dragData = new DataTransfer();
                if (_dragStartEvent != null)
                {
                    await DragDrop.DoDragDropAsync(_dragStartEvent, dragData, DragDropEffects.Copy);
                }

                topLevel.RemoveHandler(DragDrop.DragOverEvent, dragOverHandler);
                ghost.Dispose();
            }
        }

        // Вспомогательный метод для извлечения имени алиаса из скрипта
        private string GetCommandFromAliasScript(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;

            // Ищем первую строку, которая начинается с "alias"
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("alias", StringComparison.OrdinalIgnoreCase))
                {
                    // Разбиваем строку: alias +bhop "..." -> берем второе слово
                    var parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        // Убираем возможные кавычки вокруг имени алиаса
                        return parts[1].Trim('"', '\'');
                    }
                }
            }

            // Fallback: Если слова alias нет внутри, берем самое первое слово в скрипте 
            // (на случай, если пресет - это просто команда вроде "say Hello")
            var firstWord = content.Split(new[] { ' ', '\t', '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
            return firstWord.Length > 0 ? firstWord[0] : string.Empty;
        }
    }
}