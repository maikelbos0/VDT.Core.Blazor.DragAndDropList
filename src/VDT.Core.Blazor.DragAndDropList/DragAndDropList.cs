using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VDT.Core.Blazor.DragAndDropList;

public class DragAndDropList<TItem> : ComponentBase, IAsyncDisposable {
    internal const string ModuleLocation = "./_content/VDT.Core.Blazor.DragAndDropList/draganddroplist.31d575906d.js";

    private IJSObjectReference? moduleReference;
    private ElementReference containerReference;

    internal double StartY { get; set; } = 0;
    internal double CurrentY { get; set; } = 0;
    internal double DeltaY => CurrentY - StartY;
    internal int OriginalItemIndex { get; set; } = -1;
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

    [Parameter] public List<TItem> Items { get; set; } = new();

    [Parameter] public EventCallback<DropItemEventArgs<TItem>> OnDropItem { get; set; }

    [Parameter] public RenderFragment<ItemContext<TItem>> ItemTemplate { get; set; } = itemContext => builder => { };

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            moduleReference = await JSRuntime.InvokeAsync<IJSObjectReference>("import", ModuleLocation);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder) {
        builder.OpenElement(1, "div");
        builder.AddAttribute(2, "class", "drag-and-drop-list");
        builder.AddElementReferenceCapture(3, containerReference => this.containerReference = containerReference);

        builder.OpenRegion(4);
        for (var i = 0; i < Items.Count; i++) {
            builder.OpenElement(5, "div");
            builder.AddAttribute(6, "class", "drag-and-drop-list-item");
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

    internal async Task StartDragging(TItem itemToDrag, MouseEventArgs args) {
        OriginalItemIndex = Items.IndexOf(itemToDrag);
        StartY = args.PageY;
        CurrentY = args.PageY;
        Heights = await ModuleReference.InvokeAsync<List<double>>("getElementHeights", containerReference);
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
            return $"z-index: 1000; margin-top: {DeltaY}px; margin-bottom: {-DeltaY}px";
        }

        var newItemIndex = NewItemIndex;

        if (OriginalItemIndex < newItemIndex && OriginalItemIndex < itemIndex && newItemIndex >= itemIndex) {
            return $"margin-top: {-Heights[OriginalItemIndex]}px; margin-bottom: {Heights[OriginalItemIndex]}px";
        }

        if (OriginalItemIndex > newItemIndex && OriginalItemIndex > itemIndex && newItemIndex <= itemIndex) {
            return $"margin-top: {Heights[OriginalItemIndex]}px; margin-bottom: {-Heights[OriginalItemIndex]}px";
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
