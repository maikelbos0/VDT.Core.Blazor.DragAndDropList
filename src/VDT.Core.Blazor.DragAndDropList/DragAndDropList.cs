using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;

namespace VDT.Core.Blazor.DragAndDropList;

public class DragAndDropList<TItem> : ComponentBase {
    [Parameter] public List<TItem> Items { get; set; } = new();

    [Parameter] public RenderFragment<ItemContext<TItem>> ItemTemplate { get; set; } = itemContext => builder => {};

    protected override void BuildRenderTree(RenderTreeBuilder builder) {
        builder.OpenElement(1, "div");
        builder.AddAttribute(2, "class", "drag-and-drop-list");
        builder.OpenRegion(3);

        foreach (var item in Items) {
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "class", "drag-and-drop-list-item");
            builder.AddContent(6, ItemTemplate(new ItemContext<TItem>(item)));
            builder.CloseElement();
        }

        builder.CloseRegion();
        builder.CloseElement();
    }
}
