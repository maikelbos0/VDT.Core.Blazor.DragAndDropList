using System;
using System.Collections.Generic;

namespace VDT.Core.Blazor.DragAndDropList;

/// <summary>
/// Supplies information about a drop event that is being raised from a <see cref="DragAndDropList{TItem}"/>
/// </summary>
/// <typeparam name="TItem">Item type</typeparam>
public class DropItemEventArgs<TItem> : EventArgs {
    /// <summary>
    /// Gets or sets the original index of the item initiating the drag and drop action
    /// </summary>
    public int OriginalItemIndex { get; set; }

    /// <summary>
    /// Gets or sets the suggested new index for the item initiating the drag and drop action
    /// </summary>
    public int NewItemIndex { get; set; }

    /// <summary>
    /// Gets or sets the list as it will be after reordering according to the drag and drop action
    /// </summary>
    public List<TItem> ReorderedItems { get; set; } = default!;
}
