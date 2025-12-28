using FluentValidation;
using StudentRegistration.Application.DTOs.Estudiante;

namespace StudentRegistration.Application.Validators
{
    public class UpdateEstudianteValidator : AbstractValidator<UpdateEstudianteDto>
    {
        public UpdateEstudianteValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.Apellido)
                .NotEmpty().WithMessage("El apellido es obligatorio")
                .MaximumLength(100).WithMessage("El apellido no puede exceder 100 caracteres");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio")
                .EmailAddress().WithMessage("El formato del email es inválido")
                .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");
        }
    }
}
