using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;

namespace VDT.Core.Blazor.DragAndDropList;

public class DragAndDropList<TItem> : ComponentBase {
    internal double StartY { get; set; } = 0;
    internal double CurrentY { get; set; } = 0;
    internal int DraggingItemIndex { get; set; } = -1;

    [Parameter] public List<TItem> Items { get; set; } = new();

    [Parameter] public RenderFragment<ItemContext<TItem>> ItemTemplate { get; set; } = itemContext => builder => { };

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
}
