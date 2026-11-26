using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Items.Commands.ClearCart;

/// <summary>«Убрать купленные»: everything in the cart goes to history now. A repeat clears nothing.</summary>
public sealed record ClearCartCommand : ICommand<Result<ClearCartResult>>;
