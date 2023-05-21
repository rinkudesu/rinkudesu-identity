using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Identity.Service.Email;

[ExcludeFromCodeCoverage]
public class EmailOptions
{
    public string To { get; private set; }
    public string? Cc { get; private set; }
    public string? Bcc { get; private set; }
    public string Subject { get; private set; }
    public string Content { get; private set; }
    public bool IsContentHtml { get; private set; }

    public EmailOptions(string to, string subject, string content, bool isHtml = true, string? cc = null, string? bcc = null)
    {
        To = to;
        Cc = cc;
        Bcc = bcc;
        Subject = subject;
        Content = content;
        IsContentHtml = isHtml;
    }
}
