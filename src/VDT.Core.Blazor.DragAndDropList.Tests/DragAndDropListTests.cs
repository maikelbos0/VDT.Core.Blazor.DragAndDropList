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
    public async Task StartDragging() {
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz"],
            ModuleReference = Substitute.For<IJSObjectReference>()
        };

        subject.ModuleReference.InvokeAsync<List<double>>("getElementHeights", Arg.Any<object?[]?>()).Returns([25, 35, 45]);

        await subject.StartDragging("Bar", new MouseEventArgs() { PageY = 123 });

        Assert.Equal(1, subject.DraggingItemIndex);
        Assert.Equal(123, subject.StartY);
        Assert.Equal([25, 35, 45], subject.Heights);
    }

    [Fact]
    public void Drag() {
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz"],
            DraggingItemIndex = 1
        };

        subject.Drag(new MouseEventArgs() { PageY = 234 });

        Assert.Equal(234, subject.CurrentY);
    }

    [Fact]
    public void Drag_Without_DraggingItemIndex() {
        var subject = new DragAndDropList<string>() {
            Items = ["Foo", "Bar", "Baz"],
            DraggingItemIndex = -1
        };

        subject.Drag(new MouseEventArgs() { PageY = 234 });

        Assert.Equal(0, subject.CurrentY);
    }
}
