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
    internal long CurrentTouchIdentifier { get; set; } = -1;
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
    [EditorRequired, Parameter] public IList<TItem> Items { get; set; } = new List<TItem>();

    /// <summary>
    /// Gets or sets the method for selecting unique keys for the items in the list; defaults to selecting the items themselves
    /// </summary>
    [Parameter] public Func<TItem, object?> KeySelector { get; set; } = item => item;

    /// <summary>
    /// Gets or sets the callback that will be invoked when an item is dropped after dragging
    /// </summary>
    [EditorRequired, Parameter] public EventCallback<DropItemEventArgs> OnDropItem { get; set; }

    /// <summary>
    /// Gets or sets the layout template for rendering an item
    /// </summary>
    [EditorRequired, Parameter] public RenderFragment<ItemContext<TItem>> ItemTemplate { get; set; } = itemContext => builder => { };

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            moduleReference = await JSRuntime.InvokeAsync<IJSObjectReference>("import", ModuleLocation);
        }
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder) {
        builder.OpenElement(1, "div");
        builder.AddAttribute(2, "style", "touch-action: none");
        builder.AddAttribute(3, "class", "drag-and-drop-list");
        builder.AddElementReferenceCapture(4, containerReference => this.containerReference = containerReference);

        builder.OpenRegion(5);
        for (var i = 0; i < Items.Count; i++) {
            builder.OpenElement(6, "div");
            builder.SetKey(KeySelector(Items[i]));
            builder.AddAttribute(7, "class", i == OriginalItemIndex ? "drag-and-drop-list-item drag-and-drop-list-item-active" : "drag-and-drop-list-item");
            builder.AddAttribute(8, "style", GetItemStyle(i));
            builder.OpenElement(9, "div");
            builder.AddAttribute(10, "style", "width: 100%");
            builder.AddContent(11, ItemTemplate(new ItemContext<TItem>(this, Items[i])));
            builder.CloseElement();
            builder.CloseElement();
        }
        builder.CloseRegion();

        builder.OpenComponent<GlobalEventHandler.GlobalEventHandler>(12);
        builder.AddAttribute(13, nameof(GlobalEventHandler.GlobalEventHandler.OnMouseMove), EventCallback.Factory.Create<MouseEventArgs>(this, Drag));
        builder.AddAttribute(14, nameof(GlobalEventHandler.GlobalEventHandler.OnTouchMove), EventCallback.Factory.Create<TouchEventArgs>(this, Drag));
        builder.AddAttribute(15, nameof(GlobalEventHandler.GlobalEventHandler.OnMouseUp), EventCallback.Factory.Create<MouseEventArgs>(this, StopDragging));
        builder.AddAttribute(16, nameof(GlobalEventHandler.GlobalEventHandler.OnTouchEnd), EventCallback.Factory.Create<TouchEventArgs>(this, StopDragging));
        builder.CloseComponent();

        builder.CloseElement();
    }

    internal async Task StartDragging(TItem itemToDrag, double pageY, long touchIdentifier = -1) {
        if (OriginalItemIndex == -1) {
            OriginalItemIndex = Items.IndexOf(itemToDrag);
            CurrentTouchIdentifier = touchIdentifier;
            StartY = pageY;
            CurrentY = pageY;
            Heights = await ModuleReference.InvokeAsync<List<double>>("getElementHeights", containerReference);
        }
    }

    internal void Drag(MouseEventArgs args) => Drag(args.PageY);

    internal void Drag(TouchEventArgs args) {
        var touch = args.ChangedTouches.SingleOrDefault(touch => touch.Identifier == CurrentTouchIdentifier);

        if (touch != null) {
            Drag(touch.PageY, touch.Identifier);
        }
    }

    private void Drag(double currentY, long touchIdentifier = -1) {
        if (OriginalItemIndex != -1 && touchIdentifier == CurrentTouchIdentifier) {
            CurrentY = currentY;
        }
    }

    internal Task StopDragging(MouseEventArgs args) => StopDragging(args.PageY);

    internal async Task StopDragging(TouchEventArgs args) {
        var touch = args.ChangedTouches.SingleOrDefault(touch => touch.Identifier == CurrentTouchIdentifier);

        if (touch != null) {
            await StopDragging(touch.PageY, touch.Identifier);
        }
    }

    internal async Task StopDragging(double currentY, long touchIdentifier = -1) {
        if (OriginalItemIndex != -1 && touchIdentifier == CurrentTouchIdentifier) {
            CurrentY = currentY;

            var args = new DropItemEventArgs() {
                OriginalItemIndex = OriginalItemIndex,
                NewItemIndex = NewItemIndex
            };

            OriginalItemIndex = -1;
            CurrentTouchIdentifier = -1;
            StartY = 0;
            CurrentY = 0;

            await OnDropItem.InvokeAsync(args);
        }
    }

    internal string GetItemStyle(int itemIndex) {
        const string defaultStyle = "display: flex; position: relative";

        if (OriginalItemIndex == -1) {
            return defaultStyle;
        }

        if (itemIndex == OriginalItemIndex) {
            return FormattableString.Invariant($"{defaultStyle}; z-index: 1000; top: {DeltaY}px");
        }

        var newItemIndex = NewItemIndex;

        if (OriginalItemIndex < newItemIndex && OriginalItemIndex < itemIndex && newItemIndex >= itemIndex) {
            return FormattableString.Invariant($"{defaultStyle}; top: {-Heights[OriginalItemIndex]}px");
        }

        if (OriginalItemIndex > newItemIndex && OriginalItemIndex > itemIndex && newItemIndex <= itemIndex) {
            return FormattableString.Invariant($"{defaultStyle}; top: {Heights[OriginalItemIndex]}px");
        }

        return defaultStyle;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync() {
        if (moduleReference != null) {
            await moduleReference.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}
