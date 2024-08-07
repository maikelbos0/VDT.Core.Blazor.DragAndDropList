﻿using Microsoft.AspNetCore.Components;
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
        DragItemEventArgs<string>? receivedArgs = null;
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz"],
            ModuleReference = Substitute.For<IJSObjectReference>(),
            OnDragItem = EventCallback.Factory.Create<DragItemEventArgs<string>>(this, args => receivedArgs = args)
        };

        subject.ModuleReference.InvokeAsync<List<double>>("getElementHeights", Arg.Any<object?[]?>()).Returns([25, 35, 45]);

        await subject.StartDragging("Bar", 100, 1);

        Assert.Equal(1, subject.OriginalItemIndex);
        Assert.Equal(1, subject.CurrentTouchIdentifier);
        Assert.Equal(100, subject.StartY);
        Assert.Equal(100, subject.CurrentY);
        Assert.Equal([25, 35, 45], subject.Heights);

        Assert.NotNull(receivedArgs);
        Assert.Equal("Bar", receivedArgs.Item);
        Assert.Equal(1, receivedArgs.OriginalItemIndex);
    }

    [Fact]
    public void Drag_MouseEventArgs() {
        var subject = new DragAndDropList<string>() {
            OriginalItemIndex = 1,
            CurrentTouchIdentifier = -1
        };

        subject.Drag(new MouseEventArgs() { PageY = 200 });

        Assert.Equal(200, subject.CurrentY);
    }

    [Fact]
    public void Drag_MouseEventArgs_With_Current_Touch() {
        var subject = new DragAndDropList<string>() {
            OriginalItemIndex = -1,
            CurrentTouchIdentifier = 2
        };

        subject.Drag(new MouseEventArgs() { PageY = 200 });

        Assert.Equal(0, subject.CurrentY);
    }

    [Fact]
    public void Drag_MouseEventArgs_Without_OriginalItemIndex() {
        var subject = new DragAndDropList<string>() {
            OriginalItemIndex = -1,
            CurrentTouchIdentifier = -1
        };

        subject.Drag(new MouseEventArgs() { PageY = 200 });

        Assert.Equal(0, subject.CurrentY);
    }

    [Fact]
    public void Drag_TouchEventArgs() {
        var subject = new DragAndDropList<string>() {
            OriginalItemIndex = 1,
            CurrentTouchIdentifier = 2
        };

        subject.Drag(new TouchEventArgs() {
            ChangedTouches = [
                new TouchPoint() { PageY = 100, Identifier = 1 },
                new TouchPoint() { PageY = 200, Identifier = 2 }
            ]
        });

        Assert.Equal(200, subject.CurrentY);
    }

    [Fact]
    public void Drag_TouchEventArgs_Without_Current_Touch() {
        var subject = new DragAndDropList<string>() {
            OriginalItemIndex = -1,
            CurrentTouchIdentifier = -1
        };

        subject.Drag(new TouchEventArgs() {
            ChangedTouches = [
                new TouchPoint() { PageY = 100, Identifier = 1 },
                new TouchPoint() { PageY = 200, Identifier = 2 }
            ]
        });

        Assert.Equal(0, subject.CurrentY);
    }

    [Fact]
    public void Drag_TouchEventArgs_Without_OriginalItemIndex() {
        var subject = new DragAndDropList<string>() {
            OriginalItemIndex = -1,
            CurrentTouchIdentifier = 2
        };

        subject.Drag(new TouchEventArgs() {
            ChangedTouches = [
                new TouchPoint() { PageY = 100, Identifier = 1 },
                new TouchPoint() { PageY = 200, Identifier = 2 }
            ]
        });

        Assert.Equal(0, subject.CurrentY);
    }

    [Fact]
    public async Task StopDragging_MouseEventArgs() {
        DropItemEventArgs<string>? receivedArgs = null;
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux"],
            Heights = [100, 100, 100, 100],
            OriginalItemIndex = 1,
            CurrentTouchIdentifier = -1,
            StartY = 100,
            CurrentY = 340,
            OnDropItem = EventCallback.Factory.Create<DropItemEventArgs<string>>(this, args => receivedArgs = args)
        };

        await subject.StopDragging(new MouseEventArgs() { PageY = 200 });

        Assert.Equal(-1, subject.OriginalItemIndex);
        Assert.Equal(-1, subject.CurrentTouchIdentifier);
        Assert.Equal(0, subject.StartY);
        Assert.Equal(0, subject.CurrentY);

        Assert.NotNull(receivedArgs);
        Assert.Equal("Bar", receivedArgs.Item);
        Assert.Equal(1, receivedArgs.OriginalItemIndex);
        Assert.Equal(2, receivedArgs.NewItemIndex);
    }

    [Fact]
    public async Task StopDragging_MouseEventArgs_With_CurrentTouchIdentifier() {
        DropItemEventArgs<string>? receivedArgs = null;
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux"],
            Heights = [100, 100, 100, 100],
            OriginalItemIndex = 1,
            CurrentTouchIdentifier = 2,
            OnDropItem = EventCallback.Factory.Create<DropItemEventArgs<string>>(this, args => receivedArgs = args)
        };

        await subject.StopDragging(new MouseEventArgs() { PageY = 200 });

        Assert.Null(receivedArgs);
        Assert.Equal(1, subject.OriginalItemIndex);
        Assert.Equal(2, subject.CurrentTouchIdentifier);
    }

    [Fact]
    public async Task StopDragging_MouseEventArgs_Without_OriginalItemIndex() {
        DropItemEventArgs<string>? receivedArgs = null;
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux"],
            Heights = [100, 100, 100, 100],
            OriginalItemIndex = -1,
            CurrentTouchIdentifier = 2,
            OnDropItem = EventCallback.Factory.Create<DropItemEventArgs<string>>(this, args => receivedArgs = args)
        };

        await subject.StopDragging(new MouseEventArgs() { PageY = 200 });

        Assert.Null(receivedArgs);
        Assert.Equal(-1, subject.OriginalItemIndex);
        Assert.Equal(2, subject.CurrentTouchIdentifier);
    }

    [Fact]
    public async Task StopDragging_TouchEventArgs() {
        DropItemEventArgs<string>? receivedArgs = null;
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux"],
            Heights = [100, 100, 100, 100],
            OriginalItemIndex = 1,
            CurrentTouchIdentifier = 2,
            StartY = 100,
            CurrentY = 340,
            OnDropItem = EventCallback.Factory.Create<DropItemEventArgs<string>>(this, args => receivedArgs = args)
        };

        await subject.StopDragging(new TouchEventArgs() {
            ChangedTouches = [
                new TouchPoint() { PageY = 100, Identifier = 1 },
                new TouchPoint() { PageY = 200, Identifier = 2 }
            ]
        });

        Assert.Equal(-1, subject.OriginalItemIndex);
        Assert.Equal(-1, subject.CurrentTouchIdentifier);
        Assert.Equal(0, subject.StartY);
        Assert.Equal(0, subject.CurrentY);

        Assert.NotNull(receivedArgs);
        Assert.Equal(1, receivedArgs.OriginalItemIndex);
        Assert.Equal(2, receivedArgs.NewItemIndex);
    }

    [Fact]
    public async Task StopDragging_TouchEventArgs_Without_CurrentTouchIdentifier() {
        DropItemEventArgs<string>? receivedArgs = null;
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux"],
            Heights = [100, 100, 100, 100],
            OriginalItemIndex = 1,
            CurrentTouchIdentifier = -1,
            OnDropItem = EventCallback.Factory.Create<DropItemEventArgs<string>>(this, args => receivedArgs = args)
        };

        await subject.StopDragging(new TouchEventArgs() {
            ChangedTouches = [
                new TouchPoint() { PageY = 100, Identifier = 1 },
                new TouchPoint() { PageY = 200, Identifier = 2 }
            ]
        });

        Assert.Null(receivedArgs);
        Assert.Equal(1, subject.OriginalItemIndex);
        Assert.Equal(-1, subject.CurrentTouchIdentifier);
    }

    [Fact]
    public async Task StopDragging_TouchEventArgs_Without_OriginalItemIndex() {
        DropItemEventArgs<string>? receivedArgs = null;
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux"],
            Heights = [100, 100, 100, 100],
            OriginalItemIndex = -1,
            CurrentTouchIdentifier = 1,
            OnDropItem = EventCallback.Factory.Create<DropItemEventArgs<string>>(this, args => receivedArgs = args)
        };

        await subject.StopDragging(new TouchEventArgs() {
            ChangedTouches = [
                new TouchPoint() { PageY = 100, Identifier = 1 },
                new TouchPoint() { PageY = 200, Identifier = 2 }
            ]
        });

        Assert.Null(receivedArgs);
        Assert.Equal(-1, subject.OriginalItemIndex);
        Assert.Equal(1, subject.CurrentTouchIdentifier);
    }

    [Theory]
    [InlineData(0, "display: flex; position: relative")]
    [InlineData(1, "display: flex; position: relative; z-index: 1000; top: 240px")]
    [InlineData(2, "display: flex; position: relative; top: -100px")]
    [InlineData(3, "display: flex; position: relative; top: -100px")]
    [InlineData(4, "display: flex; position: relative")]
    public void GetItemStyle(int itemIndex, string expectedStyle) {
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux", "Quux"],
            Heights = [90, 100, 110, 110, 110],
            OriginalItemIndex = 1,
            StartY = 100,
            CurrentY = 340
        };

        Assert.Equal(expectedStyle, subject.GetItemStyle(itemIndex));
    }

    [Fact]
    public void GetItemStyle_Without_OriginalItemIndex() {
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz", "Qux", "Quux"],
            Heights = [100, 100, 100, 100],
            OriginalItemIndex = -1,
            StartY = 100,
            CurrentY = 340
        };

        Assert.Equal("display: flex; position: relative", subject.GetItemStyle(2));
    }
}
