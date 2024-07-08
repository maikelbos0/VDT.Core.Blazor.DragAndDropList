using Xunit;
using System.Collections.Generic;

namespace VDT.Core.Blazor.DragAndDropList.Tests;

public class ListExtensionsTests {
    [Fact]
    public void Reorder() {
        var subject = new List<string>() { "Foo", "Bar", "Baz", "Qux", "Quux" };

        subject.Reorder(new DropItemEventArgs<string>() { Item = "Bar", OriginalItemIndex = 1, NewItemIndex =3  });

        Assert.Equal(["Foo", "Baz", "Qux", "Bar", "Quux"], subject);
    }

    [Fact]
    public void RevertOrder() {
        var subject = new List<string>() { "Foo", "Baz", "Qux", "Bar", "Quux" };

        subject.RevertOrder(new DropItemEventArgs<string>() { Item = "Bar", OriginalItemIndex = 1, NewItemIndex =3  });

        Assert.Equal(["Foo", "Bar", "Baz", "Qux", "Quux"], subject);
    }
}
