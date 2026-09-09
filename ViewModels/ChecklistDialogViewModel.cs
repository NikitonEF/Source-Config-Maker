using conmaker.Models;
using System.Collections.Generic;
using System.Linq;

namespace conmaker.ViewModels;

public class ChecklistRowViewModel
{
    public string Command { get; }
    public bool IsBound { get; }
    public IReadOnlyList<string> Keys { get; }

    public ChecklistRowViewModel(ChecklistItem item)
    {
        Command = item.Command;
        IsBound = item.IsBound;
        Keys = item.Keys;
    }
}

public class ChecklistDialogViewModel : ViewModelBase
{
    public IReadOnlyList<ChecklistRowViewModel> Rows { get; }

    public ChecklistDialogViewModel(ConfigState state)
    {
        Rows = ChecklistBuilder.Build(state)
            .Select(item => new ChecklistRowViewModel(item))
            .ToList();
    }
}