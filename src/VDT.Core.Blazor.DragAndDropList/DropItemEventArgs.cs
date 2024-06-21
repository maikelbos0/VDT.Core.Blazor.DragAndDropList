using System;
using System.Collections.Generic;

namespace VDT.Core.Blazor.DragAndDropList;

public class DropItemEventArgs<TItem> : EventArgs {
    public int OriginalItemIndex { get; set; }

    public int NewItemIndex { get; set; }

    public List<TItem> ReorderedItems { get; set; } = default!;
}
