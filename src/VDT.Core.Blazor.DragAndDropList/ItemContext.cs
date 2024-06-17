namespace VDT.Core.Blazor.DragAndDropList;

public class ItemContext<TItem> {
    public TItem Item { get; }

    public ItemContext(TItem item) {
        Item = item;
    }
}
