using Microsoft.AspNetCore.Components.Web;

namespace VDT.Core.Blazor.DragAndDropList;

public class ItemContext<TItem> {
    private readonly DragAndDropList<TItem> dragAndDropList;

    public TItem Item { get; }

    public ItemContext(DragAndDropList<TItem> dragAndDropList, TItem item) {
        this.dragAndDropList = dragAndDropList;
        Item = item;
    }

    public void StartDragging(MouseEventArgs args) {
        dragAndDropList.StartDragging(this.Item, args);
    }
}
