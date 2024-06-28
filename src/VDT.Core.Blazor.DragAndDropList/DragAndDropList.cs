using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VDT.Core.Blazor.DragAndDropList;

/// <summary>
/// List component that allows reordering using drag and drop
/// </summary>
/// <typeparam name="TItem">Type of list item</typeparam>
public class DragAndDropList<TItem> : ComponentBase, IAsyncDisposable {
    internal const string ModuleLocation = "./_content/VDT.Core.Blazor.DragAndDropList/draganddroplist.31d575906d.js";

    private IJSObjectReference? moduleReference;
    private ElementReference containerReference;

    internal double StartY { get; set; } = 0;
    internal double CurrentY { get; set; } = 0;
    internal double DeltaY => CurrentY - StartY;
    internal int OriginalItemIndex { get; set; } = -1;
    internal long TouchIdentifier { get; set; } = -1;
    internal List<double> Heights { get; set; } = new();
    internal int NewItemIndex {
        get {
            if (OriginalItemIndex == -1) {
                return -1;
            }

            var index = OriginalItemIndex;
            var deltaY = DeltaY;

            if (deltaY < 0) {
                while (index > 0 && deltaY < -Heights[index - 1] / 2) {
                    deltaY += Heights[--index];
                }
            }
            else {
                while (index < Heights.Count - 1 && deltaY > Heights[index + 1] / 2) {
                    deltaY -= Heights[++index];
                }
            }

            return index;
        }
    }

    internal IJSObjectReference ModuleReference {
        get => moduleReference ?? throw new InvalidOperationException($"{nameof(ModuleReference)} is only available after the list has rendered");
        set => moduleReference = value;
    }

    [Inject] internal IJSRuntime JSRuntime { get; set; } = null!;

    /// <summary>
    /// Gets or sets the items in the list
    /// </summary>
    [Parameter] public List<TItem> Items { get; set; } = new();

    /// <summary>
    /// Gets or sets the method for selecting unique keys for the items in the list; defaults to selecting the items themselves
    /// </summary>
    [Parameter] public Func<TItem, object?> KeySelector { get; set; } = item => item;

    /// <summary>
    /// Gets or sets the callback that will be invoked when an item is dropped after dragging
    /// </summary>
    [Parameter] public EventCallback<DropItemEventArgs<TItem>> OnDropItem { get; set; }

    /// <summary>
    /// Gets or sets the layout template for rendering an item
    /// </summary>
    [Parameter] public RenderFragment<ItemContext<TItem>> ItemTemplate { get; set; } = itemContext => builder => { };

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            moduleReference = await JSRuntime.InvokeAsync<IJSObjectReference>("import", ModuleLocation);
        }
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder) {
        builder.OpenElement(1, "div");
        builder.AddAttribute(2, "class", "drag-and-drop-list");
        builder.AddElementReferenceCapture(3, containerReference => this.containerReference = containerReference);

        builder.OpenRegion(4);
        for (var i = 0; i < Items.Count; i++) {
            builder.OpenElement(5, "div");
            builder.SetKey(KeySelector(Items[i]));
            builder.AddAttribute(6, "class", i == OriginalItemIndex ? "drag-and-drop-list-item drag-and-drop-list-item-active" : "drag-and-drop-list-item");
            builder.AddAttribute(7, "style", GetStyle(i));
            builder.AddContent(8, ItemTemplate(new ItemContext<TItem>(this, Items[i])));
            builder.CloseElement();
        }
        builder.CloseRegion();

        builder.OpenComponent<GlobalEventHandler.GlobalEventHandler>(9);
        builder.AddAttribute(10, nameof(GlobalEventHandler.GlobalEventHandler.OnMouseMove), EventCallback.Factory.Create<MouseEventArgs>(this, Drag));
        builder.AddAttribute(11, nameof(GlobalEventHandler.GlobalEventHandler.OnMouseUp), EventCallback.Factory.Create<MouseEventArgs>(this, StopDragging));
        builder.CloseComponent();

        builder.CloseElement();
    }

    internal Task StartDragging(TItem itemToDrag, MouseEventArgs args)
        => StartDragging(itemToDrag, args.PageY);

    internal Task StartDragging(TItem itemToDrag, TouchEventArgs args) {
        var touch = args.ChangedTouches.First();

        return StartDragging(itemToDrag, touch.PageY, touch.Identifier);
    }

    internal async Task StartDragging(TItem itemToDrag, double pageY, long touchIdentifier = -1) {
        if (OriginalItemIndex == -1) {
            OriginalItemIndex = Items.IndexOf(itemToDrag);
            TouchIdentifier = touchIdentifier;
            StartY = pageY;
            CurrentY = pageY;
            Heights = await ModuleReference.InvokeAsync<List<double>>("getElementHeights", containerReference);
        }
    }

    internal void Drag(MouseEventArgs args) {
        if (OriginalItemIndex != -1) {
            CurrentY = args.PageY;
        }
    }

    internal async Task StopDragging(MouseEventArgs args) {
        if (OriginalItemIndex != -1) {
            CurrentY = args.PageY;

            var dropEventArgs = new DropItemEventArgs<TItem>() {
                OriginalItemIndex = OriginalItemIndex,
                NewItemIndex = NewItemIndex,
                ReorderedItems = new List<TItem>(Items)
            };
            var item = dropEventArgs.ReorderedItems[OriginalItemIndex];

            dropEventArgs.ReorderedItems.RemoveAt(OriginalItemIndex);
            dropEventArgs.ReorderedItems.Insert(NewItemIndex, item);

            OriginalItemIndex = -1;
            StartY = 0;
            CurrentY = 0;

            await OnDropItem.InvokeAsync(dropEventArgs);
        }
    }

    internal string GetStyle(int itemIndex) {
        if (OriginalItemIndex == -1 || DeltaY == 0) {
            return "";
        }

        if (itemIndex == OriginalItemIndex) {
            return FormattableString.Invariant($"z-index: 1000; position: relative; top: {DeltaY}px");
        }

        var newItemIndex = NewItemIndex;

        if (OriginalItemIndex < newItemIndex && OriginalItemIndex < itemIndex && newItemIndex >= itemIndex) {
            return FormattableString.Invariant($"position: relative; top: {-Heights[OriginalItemIndex]}px");
        }

        if (OriginalItemIndex > newItemIndex && OriginalItemIndex > itemIndex && newItemIndex <= itemIndex) {
            return FormattableString.Invariant($"position: relative; top: {Heights[OriginalItemIndex]}px");
        }

        return "";
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync() {
        if (moduleReference != null) {
            await moduleReference.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}
