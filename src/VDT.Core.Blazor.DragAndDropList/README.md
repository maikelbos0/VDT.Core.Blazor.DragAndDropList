# VDT.Core.Blazor.DragAndDropList

Blazor component that allows users to reorder items in a list by dragging and dropping.

## Features

- Drag and drop items in a list to change the order
- Fully customizable item layout template

## Usage

To use this components, there are two steps that must be taken.

- Inside the item template, there needs to be an element that has an `@onmousedown` event callback to `context.StartDragging`, passing the `MouseEventArgs`,
  to start the dragging action
- When the dragging item is dropped, the component `OnDropItem` event is triggered which provides an object of type `DropItemEventArgs<TItem>`; subscribe to
  this event to handle the reordering of your list

Please note that reordering must be done in client code since the component should not change its input property `Items`. To handle reordering you can either
perform manual switching using the `OriginalItemIndex` and `NewItemIndex` properties of the event arguments parameter, or use the `ReorderedItems` property
which contains the reordered list.

### Example

```
<DragAndDropList Items="Items" OnDropItem="(DropItemEventArgs<Item> args) => ItemDropped(args)">
    <ItemTemplate>
        <div class="mt-3 p-3 bg-light border rounded d-flex justify-content-between align-items-center">
            <div class="overflow-hidden">
                <h5 class="text-truncate">@context.Item.Text</h5>
                <div class="text-muted text-truncate">@context.Item.Id</div>
            </div>
            <div>
                <button class="btn btn-primary"><span>&varr;</span><span class="ps-2 d-none d-lg-inline" @onmousedown="context.StartDragging">Move</span></button>
            </div>
        </div>
    </ItemTemplate>
</DragAndDropList>

@code {
    private record Item(Guid Id, string Text);

    private List<Item> Items { get; set; } = Enumerable.Range(1, 8).Select(i => new Item(Guid.NewGuid(), $"Item {i}")).ToList();

    private void ItemDropped(DropItemEventArgs<Item> args) {
        Items = args.ReorderedItems;
    }
}
```

## Style

The only styles that get automatically applied to the list and its items are those that are needed to render the dragging and switching effects for the list
items. Further styles can be applied either directly on the item layout template, or using below CSS classes.

- `drag-and-drop-list` for the list container
- `drag-and-drop-list-item` for each list item
- `drag-and-drop-list-item-active` for the list item that is currently being dragged

### Example

```
/* Display a shadow around the currently active element's content */
.drag-and-drop-list-item-active > div {
    box-shadow: 0 0 0.75rem #dee2e6;
}

/* Create smooth switching animations */
.drag-and-drop-list-item:not(.drag-and-drop-list-item-active) {
    transition: margin 0.4s ease-in-out;
}

```
