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

        await subject.StartDragging(new MouseEventArgs() { PageY = 123 });

        Assert.Equal(1, list.OriginalItemIndex);
        Assert.Equal(-1, list.TouchIdentifier);
        Assert.Equal(123, list.StartY);
        Assert.Equal(123, list.CurrentY);
        Assert.Equal([25, 35, 45], list.Heights);
    }

    [Fact]
    public async Task StartDragging() {
        var list = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz"],
            ModuleReference = Substitute.For<IJSObjectReference>()
        };
        var subject = new ItemContext<string>(list, "Bar");

        list.ModuleReference.InvokeAsync<List<double>>("getElementHeights", Arg.Any<object?[]?>()).Returns([25, 35, 45]);

        await subject.StartDragging(new TouchEventArgs() {
            ChangedTouches = [
                new TouchPoint() { PageY = 123, Identifier = 1 },
                new TouchPoint() { PageY = 234, Identifier = 2 }
            ]
        });

        Assert.Equal(1, list.OriginalItemIndex);
        Assert.Equal(1, list.TouchIdentifier);
        Assert.Equal(123, list.StartY);
        Assert.Equal(123, list.CurrentY);
        Assert.Equal([25, 35, 45], list.Heights);
    }
}
