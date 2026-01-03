using FluentValidation;
using JetBrains.Annotations;

namespace Raspo_Stempelkarten_Backend.Dtos;

[UsedImplicitly]
public class PlayerCreateDtoValidator : AbstractValidator<PlayerCreateDto>
{
    public PlayerCreateDtoValidator()
    {
        RuleFor(dto => dto.FirstName).NotNull().NotEmpty();
        RuleFor(dto => dto.Surname).NotNull().NotEmpty();
        RuleFor(dto => dto.Birthdate).NotNull().NotEmpty();
    }
}