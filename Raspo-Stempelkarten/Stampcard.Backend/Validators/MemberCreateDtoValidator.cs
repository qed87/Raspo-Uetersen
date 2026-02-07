using FluentValidation;
using JetBrains.Annotations;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Validators;

[UsedImplicitly]
public class MemberCreateDtoValidator : AbstractValidator<PlayerCreateDto>
{
    public MemberCreateDtoValidator()
    {
        RuleFor(dto => dto.FirstName).NotNull().NotEmpty();
        RuleFor(dto => dto.LastName).NotNull().NotEmpty();
        RuleFor(dto => dto.Birthdate).NotNull().NotEmpty();
    }
}