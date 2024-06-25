using Microsoft.AspNetCore.Components.Web;
using System.Linq;
using System.Threading.Tasks;

namespace VDT.Core.Blazor.DragAndDropList;

/// <summary>
/// Context for an item in a <see cref="DragAndDropList{TItem}"/>
/// </summary>
/// <typeparam name="TItem">Type of list item</typeparam>
public class ItemContext<TItem> {
    private readonly DragAndDropList<TItem> dragAndDropList;

    /// <summary>
    /// Gets the item that this context represents
    /// </summary>
    public TItem Item { get; }

    /// <summary>
    /// Create an item context
    /// </summary>
    /// <param name="dragAndDropList">List this item context belongs to</param>
    /// <param name="item">Item that this context represents</param>
    public ItemContext(DragAndDropList<TItem> dragAndDropList, TItem item) {
        this.dragAndDropList = dragAndDropList;
        Item = item;
    }

    /// <summary>
    /// Initiate dragging the item in the list
    /// </summary>
    /// <param name="args">Mouse event information</param>
    /// <returns>A <see cref="Task"/> which completes asynchronously once the drag operation has started</returns>
    public Task StartDragging(MouseEventArgs args)
        => dragAndDropList.StartDragging(Item, args);
}
