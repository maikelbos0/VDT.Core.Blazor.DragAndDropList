using Xunit;

namespace VDT.Core.Blazor.DragAndDropList.Tests;

public class DropItemEventArgsTests {
    [Fact]
    public void IndexDelta() {
        var subject = new DropItemEventArgs<string>() { OriginalItemIndex = 3, NewItemIndex = 1 };

        Assert.Equal(-2, subject.IndexDelta);
    }
}
