using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace VDT.Core.Blazor.DragAndDropList.Tests {
    public class DragAndDropListTests {
        [Fact]
        public void StartDragging() {
            var subject = new DragAndDropList<string>() {
                Items = ["Foo", "Bar", "Baz"]
            };

            subject.StartDragging("Bar", new MouseEventArgs() { PageY = 123 });

            Assert.Equal(1, subject.DraggingItemIndex);
            Assert.Equal(123, subject.StartY);
        }

        [Fact]
        public void Drag() {
            var subject = new DragAndDropList<string>() {
                Items = ["Foo", "Bar", "Baz"],
                DraggingItemIndex = -1
            };

            subject.Drag(new MouseEventArgs() { PageY = 234 });

            Assert.Equal(123, subject.CurrentY);
        }
    }
}
