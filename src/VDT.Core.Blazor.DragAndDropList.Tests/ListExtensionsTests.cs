using Xunit;
using System.Collections.Generic;

namespace VDT.Core.Blazor.DragAndDropList.Tests;

public class ListExtensionsTests {
    [Fact]
    public void Reorder() {
        var subject = new List<string>() { "Foo", "Bar", "Baz", "Qux", "Quux" };

        subject.Reorder(new DropItemEventArgs() { OriginalItemIndex = 1, NewItemIndex =3  });

        Assert.Equal(["Foo", "Baz", "Qux", "Bar", "Quux"], subject);
    }
}
