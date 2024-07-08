using System;

namespace VDT.Core.Blazor.DragAndDropList;

/// <summary>
/// Supplies information about a start drag event that is being raised from a <see cref="DragAndDropList{TItem}"/>
/// </summary>
/// <typeparam name="TItem">Item type</typeparam>
public class DragItemEventArgs<TItem> : EventArgs {
    /// <summary>
    /// Gets or sets the item initiating the drag and drop action
    /// </summary>
    public TItem Item { get; set; } = default!;

    /// <summary>
    /// Gets or sets the original index of the item initiating the drag and drop action
    /// </summary>
    public int OriginalItemIndex { get; set; }
}
