using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using NSubstitute;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace VDT.Core.Blazor.DragAndDropList.Tests;

public class DragAndDropListTests {
    [Fact]
    public void ModuleLocation_Is_Correct() {
        var fileName = Path.GetFileName(DragAndDropList<string>.ModuleLocation);

        var expectedFilePath = Directory.GetFiles(Path.Combine("..", "..", "..", "..", "VDT.Core.Blazor.DragAndDropList", "wwwroot"), "draganddroplist.*.js").Single();
        var expectedFileName = Path.GetFileName(expectedFilePath);

        Assert.Equal(expectedFileName, fileName);
    }

    [Fact]
    public void Module_Has_Correct_Fingerprint() {
        var filePath = Directory.GetFiles(Path.Combine("..", "..", "..", "..", "VDT.Core.Blazor.DragAndDropList", "wwwroot"), "draganddroplist.*.js").Single();
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        var fingerprintFinder = new Regex("draganddroplist\\.([a-f0-9]+)\\.js$", RegexOptions.IgnoreCase);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
        var fingerprint = fingerprintFinder.Match(filePath).Groups[1].Value;
        var fileContents = File.ReadAllBytes(filePath).Where(b => b != '\r').ToArray(); // Normalize newlines between Windows and Linux
        var expectedFingerprint = string.Join("", SHA256.HashData(fileContents).Take(5).Select(b => b.ToString("x2")));

        Assert.Equal(expectedFingerprint, fingerprint);
    }

    [Fact]
    public void DeltaY() {
        var subject = new DragAndDropList<string>() {
            StartY = 700,
            CurrentY = 300
        };

        Assert.Equal(-400, subject.DeltaY);
    }

    [Theory]
    [InlineData(-1, 100, -1)]
    [InlineData(2, 0, 2)]
    [InlineData(2, -25, 2)]
    [InlineData(2, -26, 1)]
    [InlineData(2, -65, 1)]
    [InlineData(2, -66, 0)]
    [InlineData(2, 15, 2)]
    [InlineData(2, 16, 3)]
    [InlineData(2, 55, 3)]
    [InlineData(2, 56, 4)]
    [InlineData(3, -35, 3)]
    [InlineData(3, -36, 2)]
    [InlineData(3, 25, 3)]
    [InlineData(3, 26, 4)]
    [InlineData(0, -100, 0)]
    [InlineData(5, 100, 5)]
    public void NewItemIndex(int draggingItemIndex, double deltaY, int expectedNewItemIndex) {
        var subject = new DragAndDropList<string>() {
            Heights = [30, 50, 70, 30, 50, 70],
            OriginalItemIndex = draggingItemIndex,
            StartY = 0,
            CurrentY = deltaY
        };

        Assert.Equal(expectedNewItemIndex, subject.NewItemIndex);
    }

    [Fact]
    public async Task StartDragging() {
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz"],
            ModuleReference = Substitute.For<IJSObjectReference>()
        };

        subject.ModuleReference.InvokeAsync<List<double>>("getElementHeights", Arg.Any<object?[]?>()).Returns([25, 35, 45]);

        await subject.StartDragging("Bar", new MouseEventArgs() { PageY = 123 });

        Assert.Equal(1, subject.OriginalItemIndex);
        Assert.Equal(123, subject.StartY);
        Assert.Equal(123, subject.CurrentY);
        Assert.Equal([25, 35, 45], subject.Heights);
    }

    [Fact]
    public void Drag() {
        var subject = new DragAndDropList<string>() {
            OriginalItemIndex = 1
        };

        subject.Drag(new MouseEventArgs() { PageY = 234 });

        Assert.Equal(234, subject.CurrentY);
    }

    [Fact]
    public void Drag_Without_DraggingItemIndex() {
        var subject = new DragAndDropList<string>() {
            OriginalItemIndex = -1
        };

        subject.Drag(new MouseEventArgs() { PageY = 234 });

        Assert.Equal(0, subject.CurrentY);
    }

    [Fact]
    public async Task StopDragging() {
        DropItemEventArgs<string>? receivedArgs = null;
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux"],
            Heights = [100, 100, 100, 100],
            OriginalItemIndex = 1,
            StartY = 100,
            CurrentY = 340,
            OnDropItem = EventCallback.Factory.Create<DropItemEventArgs<string>>(this, args => receivedArgs = args)
        };

        await subject.StopDragging(new MouseEventArgs() { PageY = 200 });

        Assert.Equal(-1, subject.OriginalItemIndex);
        Assert.Equal(0, subject.StartY);
        Assert.Equal(0, subject.CurrentY);

        Assert.NotNull(receivedArgs);
        Assert.Equal(1, receivedArgs.OriginalItemIndex);
        Assert.Equal(2, receivedArgs.NewItemIndex);
        Assert.Equal(["Foo", "Baz", "Bar", "Qux"], receivedArgs.ReorderedItems);
    }

    [Theory]
    [InlineData(0, "")]
    [InlineData(1, "z-index: 1000; margin-top: 240px; margin-bottom: -240px")]
    [InlineData(2, "margin-top: -100px; margin-bottom: 100px")]
    [InlineData(3, "margin-top: -100px; margin-bottom: 100px")]
    [InlineData(4, "")]
    public void GetStyle(int itemIndex, string expectedStyle) {
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux", "Quux"],
            Heights = [90, 100, 110, 110, 110],
            OriginalItemIndex = 1,
            StartY = 100,
            CurrentY = 340
        };

        Assert.Equal(expectedStyle, subject.GetStyle(itemIndex));
    }

    [Fact]
    public void GetStyle_Without_OriginalItemIndex() {
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux", "Quux"],
            Heights = [100, 100, 100, 100],
            OriginalItemIndex = -1,
            StartY = 100,
            CurrentY = 340
        };

        Assert.Equal("", subject.GetStyle(2));
    }

    [Fact]
    public void GetStyle_Without_Delta() {
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux", "Quux"],
            Heights = [100, 100, 100, 100],
            OriginalItemIndex = 1,
            StartY = 100,
            CurrentY = 100
        };

        Assert.Equal("", subject.GetStyle(2));
    }
}
