using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Core.Blazor.DragAndDropList.Tests;

public class ItemContextTests {
    [Fact]
    public async Task StartDragging_MouseEventArgs() {
        var list = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz"],
            ModuleReference = Substitute.For<IJSObjectReference>()
        };
        var subject = new ItemContext<string>(list, "Bar");

        list.ModuleReference.InvokeAsync<List<double>>("getElementHeights", Arg.Any<object?[]?>()).Returns([25, 35, 45]);

        await subject.StartDragging(new MouseEventArgs() { PageY = 100 });

        Assert.Equal(1, list.OriginalItemIndex);
        Assert.Equal(-1, list.CurrentTouchIdentifier);
        Assert.Equal(100, list.StartY);
        Assert.Equal(100, list.CurrentY);
        Assert.Equal([25, 35, 45], list.Heights);
    }

    [Fact]
    public async Task StartDragging_TouchEventArgs() {
        var list = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz"],
            ModuleReference = Substitute.For<IJSObjectReference>()
        };
        var subject = new ItemContext<string>(list, "Bar");

        list.ModuleReference.InvokeAsync<List<double>>("getElementHeights", Arg.Any<object?[]?>()).Returns([25, 35, 45]);

        await subject.StartDragging(new TouchEventArgs() {
            ChangedTouches = [
                new TouchPoint() { PageY = 100, Identifier = 1 },
                new TouchPoint() { PageY = 200, Identifier = 2 }
            ]
        });

        Assert.Equal(1, list.OriginalItemIndex);
        Assert.Equal(1, list.CurrentTouchIdentifier);
        Assert.Equal(100, list.StartY);
        Assert.Equal(100, list.CurrentY);
        Assert.Equal([25, 35, 45], list.Heights);
    }
}
