using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VDT.Core.Blazor.DragAndDropList;

public class DragAndDropList<TItem> : ComponentBase, IAsyncDisposable {
    internal const string ModuleLocation = "./_content/VDT.Core.Blazor.DragAndDropList/draganddroplist.2d284769ef.js";

    private IJSObjectReference? moduleReference;

    internal double StartY { get; set; } = 0;
    internal double CurrentY { get; set; } = 0;
    internal int DraggingItemIndex { get; set; } = -1;
    internal IJSObjectReference ModuleReference {
        get => moduleReference ?? throw new InvalidOperationException($"{nameof(ModuleReference)} is only available after the list has rendered");
        set => moduleReference = value;
    }

    [Inject] internal IJSRuntime JSRuntime { get; set; } = null!;

    [Parameter] public List<TItem> Items { get; set; } = new();

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
        
        builder.OpenRegion(3);
        foreach (var item in Items) {
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "drag-and-drop-list-item");
            builder.AddContent(6, ItemTemplate(new ItemContext<TItem>(this, item)));
            builder.CloseElement();
        }
        builder.CloseRegion();

        builder.OpenComponent<GlobalEventHandler.GlobalEventHandler>(7);
        builder.AddAttribute(8, nameof(GlobalEventHandler.GlobalEventHandler.OnMouseMove), EventCallback.Factory.Create<MouseEventArgs>(this, Drag));
        builder.AddAttribute(9, nameof(GlobalEventHandler.GlobalEventHandler.OnMouseUp), EventCallback.Factory.Create<MouseEventArgs>(this, StopDragging));
        builder.CloseComponent();

        builder.CloseElement();
    }

    internal void StartDragging(TItem itemToDrag, MouseEventArgs args) {
        DraggingItemIndex = Items.IndexOf(itemToDrag);
        StartY = args.PageY;
    }

    internal void Drag(MouseEventArgs args) {
        if (DraggingItemIndex != -1) {
            CurrentY = args.PageY;
            Console.WriteLine(CurrentY);
        }
    }

    internal void StopDragging(MouseEventArgs args) {
        if (DraggingItemIndex != -1) {
            DraggingItemIndex = -1;
            Console.WriteLine("Done");
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync() {
        if (moduleReference != null) {
            await moduleReference.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}
