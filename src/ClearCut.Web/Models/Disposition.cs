using System.Text.Json.Serialization;

namespace ClearCut.Web.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Disposition
{
    Dismiss,
    Investigate,
    Replace,
    License
}

public static class DispositionExtensions
{
    public static string GetDescription(this Disposition disposition)
    {
        return disposition switch
        {
            Disposition.Dismiss => "Dismiss: No further action after human review.",
            Disposition.Investigate => "Investigate: More research or professional review is required.",
            Disposition.Replace => "Replace: Remove or substitute the material.",
            Disposition.License => "License: Seek permission or licensing.",
            _ => throw new ArgumentOutOfRangeException(nameof(disposition), disposition, null)
        };
    }
}
