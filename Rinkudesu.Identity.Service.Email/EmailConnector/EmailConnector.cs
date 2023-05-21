#pragma warning disable CA1812
using System.Net;
using System.Net.Mail;
using RInkudesu.Identity.Service.Common.Utilities;

namespace Rinkudesu.Identity.Service.Email.EmailConnector;

internal sealed class EmailConnector : IEmailConnector
{
    private readonly Lazy<SmtpClient> client = new Lazy<SmtpClient>(()
        => new SmtpClient
        (
            EnvironmentalVariablesReader.GetRequiredVariable(EnvironmentalVariablesReader.EmailServerHost),
            EnvironmentalVariablesReader.GetRequiredIntVariable(EnvironmentalVariablesReader.EmailServerPort)
        )
        {
            EnableSsl = EnvironmentalVariablesReader.IsSet(EnvironmentalVariablesReader.EmailServerEnableSsl),
            Credentials = new NetworkCredential
                (
                    EnvironmentalVariablesReader.GetRequiredVariable(EnvironmentalVariablesReader.EmailServerUsername),
                    EnvironmentalVariablesReader.GetRequiredVariable(EnvironmentalVariablesReader.EmailServerPassword)
                ),
        });

    /// <summary>
    /// Returns an instance of <see cref="SmtpClient"/> that can be used to send emails.
    /// Please note that this instance should not be disposed of in calling code as it's managed by this class.
    /// </summary>
    public SmtpClient Client => GetClientSafe();

    private readonly object _disposalMutex = new object();
    private bool isDisposed;

    public void Dispose()
    {
        lock (_disposalMutex)
        {
            if (client.IsValueCreated)
                client.Value.Dispose();
            isDisposed = true;
        }
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
