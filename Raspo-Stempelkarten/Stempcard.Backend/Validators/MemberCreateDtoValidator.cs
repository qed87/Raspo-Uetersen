using FluentValidation;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Validators;

[UsedImplicitly]
public class MemberCreateDtoValidator : AbstractValidator<MemberCreateDto>
{
    public MemberCreateDtoValidator()
    {
        RuleFor(dto => dto.FirstName).NotNull().NotEmpty();
        RuleFor(dto => dto.LastName).NotNull().NotEmpty();
        RuleFor(dto => dto.Birthdate).NotNull().NotEmpty();
    }
}