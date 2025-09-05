using FluentValidation;
using JetBrains.Annotations;

namespace Raspo_Stempelkarten_Backend.Dtos;

[UsedImplicitly]
public class StempelkartenCreateDtoValidator : AbstractValidator<StempelkartenCreateDto>
{
    public StempelkartenCreateDtoValidator()
    {
        RuleFor(dto => dto.Team).NotNull().NotEmpty().Matches(@".+ \d{4}");
        RuleFor(dto => dto.Season).NotNull().NotEmpty().Matches(@"\d{4}/\d{2}");
        RuleFor(dto => dto.Recipient).NotNull().NotEmpty();
        RuleFor(dto => dto.MinStamps).GreaterThan(0).LessThanOrEqualTo(dto => dto.MaxStamps);
        RuleFor(dto => dto.MaxStamps).GreaterThanOrEqualTo(dto => dto.MinStamps);
    }
}