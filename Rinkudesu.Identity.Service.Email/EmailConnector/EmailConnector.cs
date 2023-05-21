using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mail;

namespace Rinkudesu.Identity.Service.Email.EmailConnector;

[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
internal class EmailConnector : IEmailConnector
{
    private readonly Lazy<SmtpClient> client = new Lazy<SmtpClient>(()
        => new SmtpClient("127.0.0.1", 2525) //todo: read from env
        {
            EnableSsl = false,
            Credentials = new NetworkCredential("username", "password"),
        });

    /// <summary>
    /// Returns an instance of <see cref="SmtpClient"/> that can be used to send emails.
    /// Please note that this instance should not be disposed of in calling code as it's managed by this class.
    /// </summary>
    public SmtpClient Client => GetClientSafe();

    private readonly object _disposalMutex = new object();
    private bool isDisposed;

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_disposalMutex)
            {
                if (client.IsValueCreated)
                    client.Value.Dispose();
                isDisposed = true;
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // This funky disposal handling is needed as we might conceivably request disposal before client was created
    // in such a case calling client.Value.Dispose() would lead to the object creation just for it to be disposed immediately after,
    // but at the same time, unprotected if could make it so that the object would get "disposed" while at the same time getting Client.
    // While unlikely, such a scenario would lead to resource leaks, so let's protect against it.
    private SmtpClient GetClientSafe()
    {
        lock (_disposalMutex)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(EmailConnector));
            return client.Value;
        }
    }
}
