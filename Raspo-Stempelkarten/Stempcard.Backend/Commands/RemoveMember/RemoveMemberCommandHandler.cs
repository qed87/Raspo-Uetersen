using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.RemoveMember;

/// <inheritdoc />
[UsedImplicitly]
public class RemoveMemberCommandHandler(IServiceProvider serviceProvider) 
    : CommandHandlerBase<RemoveMemberCommand, Guid>(serviceProvider)
{
    /// <inheritdoc />
    protected override async Task<Result<Guid>> ApplyCommandToModel(RemoveMemberCommand command, ITeamAggregate model)
    {
        return await model.RemoveMemberAsync(command.MemberId);
    }
}