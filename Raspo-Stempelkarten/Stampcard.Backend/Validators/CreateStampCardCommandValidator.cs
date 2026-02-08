using FluentValidation;
using StampCard.Backend.Commands.CreateStampCard;

namespace StampCard.Backend.Validators;

/// <inheritdoc />
public class CreateStampCardCommandValidator : AbstractValidator<CreateStampCardCommand>
{
    /// <inheritdoc />
    public CreateStampCardCommandValidator()
    {
        RuleFor(command => command.Team).NotEmpty().Matches("team-[0-9a-fA-F]{32}");
        RuleFor(command => command.AccountingYear).GreaterThan((short) DateTime.UtcNow.AddYears(-25).Year)
            .WithMessage("Geschäftsjahr darf nicht mehr als 25 Jahre in der Vergangenheit liegen.");
        RuleFor(command => command.Flag).IsEnumName(typeof(StampCardCreateFlags), false)
            .WithMessage("Ungültige Command flag.");
    }
}

public enum StampCardCreateFlags
{
    Manual,
    Auto
}