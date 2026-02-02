using DispatchR.Abstractions.Send;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Raspo_Stempelkarten_Backend.Authorization;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.AddMember;

/// <inheritdoc />
[UsedImplicitly]
public class AddMemberCommandHandler(
    IServiceProvider serviceProvider,
    IHttpContextAccessor httpContextAccessor,
    IAuthorizationService authorizationService,
    IValidator<AddMemberCommand> validator) 
    : CommandHandlerBase<AddMemberCommand, Guid>(serviceProvider)
{
    /// <inheritdoc />
    protected override async Task<Result<Guid>> ApplyCommandToModel(AddMemberCommand command, ITeamAggregate model)
    { 
        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid) return validationResult.ToResult();
        var authorizationResult = await authorizationService.AuthorizeAsync(
            httpContextAccessor.HttpContext!.User, 
            model, 
            "TeamCoachOnly");
        if (!authorizationResult.Succeeded)
            return authorizationResult.ToResult();
        var result = await model.AddMemberAsync(command.FirstName, command.LastName, 
            command.Birthdate, command.Birthplace);
        return result;
    }
}