using Backend.Shared;
using Domain;
using Domain.EventSourcing;
using Microsoft.AspNetCore.Mvc;

namespace Payment.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public abstract class DomainController<T, TId, TCommand, TEvent> : ControllerBase
    where T : IDomainClient<TId, TCommand, TEvent>
    where TCommand : DomainCommand
    where TEvent : DomainEvent
    where TId : ValueRecord<string>
{
    private readonly TId? _id;
    private readonly IMerchantService _merchantService;

    protected IBackendClient<TId, TCommand, TEvent> ClusterClientBase { get; }
    protected DomainController(TId id, IBackendClient<TId, TCommand, TEvent> clusterClientBase, IMerchantService merchantService)
    {
        _id = id;
        ClusterClientBase = clusterClientBase;
        _merchantService = merchantService;
    }

    protected DomainController(IBackendClient<TId, TCommand, TEvent> clusterClientBase, IMerchantService merchantService)
    {
        ClusterClientBase = clusterClientBase;
        _merchantService = merchantService;
    }

    protected virtual async Task<IActionResult> ExecuteCommand(TCommand command, int MerchantId) =>
        (await ClusterClientBase.Execute(_id, command, MerchantId))
        .Match<IActionResult>(val => Ok(), err => BadRequest(err));

    protected virtual async Task<IActionResult> ExecuteCommand(TId id, TCommand command, int MerchantId) =>
        (await ClusterClientBase.Execute(id, command, MerchantId))
        .Match<IActionResult>(val => Ok(), err => BadRequest($"InvalidRequest - {err.FirstOrDefault().Message}"));

    protected int GetMerchantId() => _merchantService.GetMerchantIntegerId(User.Claims.First(x => x.Type == "Merchant_Id").Value).Result;

    protected string GetUserId() => User.Claims.First(c => c.Type.ToLower() == "user_id").Value;
}
