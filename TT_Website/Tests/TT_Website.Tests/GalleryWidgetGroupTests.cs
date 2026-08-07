using System.Text.Json;
using TT_Website.Models;

namespace TT_Website.Tests;

public class GalleryWidgetGroupTests
{
    [Fact]
    public void FromGroups_CreatesCycleFreeSerializableModel()
    {
        var group = new GalleryGroup { Id = 4, Name = "Turnier" };
        var image = new GalleryImage { Id = 7, Title = "Finale", ImagePath = "/uploads/finale.jpg" };
        group.Images.Add(new GalleryGroupImage { GalleryGroup = group, GalleryImage = image, SortOrder = 1 });

        var widgetGroups = GalleryWidgetGroup.FromGroups([group]);
        var json = JsonSerializer.Serialize(widgetGroups);

        Assert.Contains("Turnier", json);
        Assert.Contains("finale.jpg", json);
        Assert.DoesNotContain("GalleryGroup", json);
    }
}
