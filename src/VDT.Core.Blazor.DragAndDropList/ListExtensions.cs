using System.Collections.Generic;

namespace VDT.Core.Blazor.DragAndDropList;

/// <summary>
/// Provides static methods for manipulating objects that implement <see cref="IList{T}"/>
/// </summary>
public static class ListExtensions {
    /// <summary>
    /// Reorders a list according to the given event arguments
    /// </summary>
    /// <typeparam name="TItem">Item type</typeparam>
    /// <param name="source">List to reorder</param>
    /// <param name="args">Event arguments according to which the list should be reordered</param>
    public static void Reorder<TItem>(this IList<TItem> source, DropItemEventArgs args) {
        var item = source[args.OriginalItemIndex];
        source.RemoveAt(args.OriginalItemIndex);
        source.Insert(args.NewItemIndex, item);
    }
}
