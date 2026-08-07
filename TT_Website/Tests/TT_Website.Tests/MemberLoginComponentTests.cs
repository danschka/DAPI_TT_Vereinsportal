using Bunit;
using TT_Website.Components.Pages.Public;

namespace TT_Website.Tests;

public class MemberLoginComponentTests : BunitContext
{
    [Fact]
    public void MemberLogin_RendersProtectedPostForm()
    {
        var component = Render<MemberLogin>();

        var form = component.Find("form");
        Assert.Equal("post", form.GetAttribute("method"));
        Assert.Equal("/mitglieder/auth/login", form.GetAttribute("action"));
        Assert.Equal("password", component.Find("input[name=password]").GetAttribute("type"));
    }
}
