using FluentValidation;
using JetBrains.Annotations;

namespace Raspo_Stempelkarten_Backend.Dtos;

[UsedImplicitly]
public class StampCardUpdateDtoValidator : AbstractValidator<StampCardUpdateDto>
{
    public StampCardUpdateDtoValidator()
    {
        RuleFor(dto => dto.Id).NotNull().NotEmpty();
        RuleFor(dto => dto.Team).NotNull().NotEmpty();
        RuleFor(dto => dto.Season).NotNull().NotEmpty().Matches(@"\d{4}/\d{2}");
        RuleFor(dto => dto.Recipient).NotNull().NotEmpty();
        RuleFor(dto => dto.MinStamps).GreaterThan(0).LessThanOrEqualTo(dto => dto.MaxStamps);
        RuleFor(dto => dto.MaxStamps).GreaterThanOrEqualTo(dto => dto.MinStamps);
    }
}