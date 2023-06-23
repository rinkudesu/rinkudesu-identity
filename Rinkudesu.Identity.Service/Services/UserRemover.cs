using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Rinkudesu.Identity.Service.MessageQueues;
using Rinkudesu.Identity.Service.Models;
using Rinkudesu.Identity.Service.Repositories;
using Rinkudesu.Kafka.Dotnet.Base;

namespace Rinkudesu.Identity.Service.Services;

/// <summary>
/// Helper service used for removing user accounts.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserRemover
{
    private readonly UserManager<User> _userManager;
    private readonly SessionTicketRepository _sessionTicketRepository;
    private readonly IKafkaProducer _kafkaProducer;

#pragma warning disable CS1591
    public UserRemover(UserManager<User> userManager, SessionTicketRepository sessionTicketRepository, IKafkaProducer kafkaProducer)
#pragma warning restore CS1591
    {
        _userManager = userManager;
        _sessionTicketRepository = sessionTicketRepository;
        _kafkaProducer = kafkaProducer;
    }

    /// <summary>
    /// Removes given user, invalidates sessions and send kafka message to remove user data.
    /// </summary>
    /// <returns><c>true</c> when successful.</returns>
    public async Task<bool> RemoveUser(User user)
    {
        var result = await _userManager.DeleteAsync(user);
        //send kafka message regardless, to at least remove all user data
        await _sessionTicketRepository.RemoveUserSessionTickets(user.Id);
        await _kafkaProducer.ProduceUserDeleted(user.Id);
        return result.Succeeded;
    }
}
