using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;

namespace VDT.Core.Blazor.DragAndDropList;

public class ItemContext<TItem> {
    private readonly DragAndDropList<TItem> dragAndDropList;

    public TItem Item { get; }

    public ItemContext(DragAndDropList<TItem> dragAndDropList, TItem item) {
        this.dragAndDropList = dragAndDropList;
        Item = item;
    }

    public Task StartDragging(MouseEventArgs args)
        => dragAndDropList.StartDragging(Item, args);
}
