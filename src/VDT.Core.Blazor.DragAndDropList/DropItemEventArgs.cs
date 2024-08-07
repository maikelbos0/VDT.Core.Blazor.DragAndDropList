﻿using System;

namespace VDT.Core.Blazor.DragAndDropList;

/// <summary>
/// Supplies information about a drop event that is being raised from a <see cref="DragAndDropList{TItem}"/>
/// </summary>
/// <typeparam name="TItem">Item type</typeparam>
public class DropItemEventArgs<TItem> : EventArgs {
    /// <summary>
    /// Gets or sets the item initiating the drag and drop action
    /// </summary>
    public TItem Item { get; set; } = default!;

    /// <summary>
    /// Gets or sets the original index of the item initiating the drag and drop action
    /// </summary>
    public int OriginalItemIndex { get; set; }

    /// <summary>
    /// Gets or sets the suggested new index for the item initiating the drag and drop action
    /// </summary>
    public int NewItemIndex { get; set; }

    /// <summary>
    /// Gets the difference in index between <see cref="NewItemIndex"/> and <see cref="OriginalItemIndex"/>
    /// </summary>
    public int IndexDelta => NewItemIndex - OriginalItemIndex;
}
