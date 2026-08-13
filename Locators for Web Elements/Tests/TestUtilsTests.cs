using Locators_for_Web_Elements.Core;

namespace Locators_for_Web_Elements.Tests;

public class TestUtilsTests
{
    [Fact]
    public void SanitizeFileName_RemovesInvalidWindowsCharacters()
    {
        var input = "Locators_for_Web_Elements.Tests.EpamTests.Task1_ValidatePositionSearch(keyword: \"JavaScript\", country: \"United States\")";

        var result = TestUtils.SanitizeFileName(input);

        Assert.True(result.Length > 0);
        Assert.DoesNotContain('"', result);
        Assert.DoesNotContain(':', result);
        Assert.DoesNotContain('<', result);
        Assert.DoesNotContain('>', result);
        Assert.DoesNotContain('|', result);
        Assert.DoesNotContain('*', result);
        Assert.DoesNotContain('?', result);
        Assert.Equal(-1, result.IndexOfAny(Path.GetInvalidFileNameChars()));
    }
}
