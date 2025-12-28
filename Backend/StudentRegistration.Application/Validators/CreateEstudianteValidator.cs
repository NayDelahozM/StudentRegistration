using FluentValidation;
using StudentRegistration.Application.DTOs.Estudiante;
using System;

namespace StudentRegistration.Application.Validators
{
    public class CreateEstudianteValidator : AbstractValidator<CreateEstudianteDto>
    {
        public CreateEstudianteValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre solo puede contener letras");

            RuleFor(x => x.Apellido)
                .NotEmpty().WithMessage("El apellido es obligatorio")
                .MaximumLength(100).WithMessage("El apellido no puede exceder 100 caracteres")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El apellido solo puede contener letras");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es obligatorio")
                .EmailAddress().WithMessage("El formato del email es inválido")
                .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres");

            RuleFor(x => x.Telefono)
                .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres")
                .Matches(@"^[\d\s\-\+\(\)]*$").WithMessage("El formato del teléfono es inválido")
                .When(x => !string.IsNullOrEmpty(x.Telefono));

            RuleFor(x => x.FechaNacimiento)
                .LessThan(DateTime.Now).WithMessage("La fecha de nacimiento debe ser anterior a hoy")
                .GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("La fecha de nacimiento no es válida")
                .When(x => x.FechaNacimiento.HasValue);

            RuleFor(x => x.Direccion)
                .MaximumLength(200).WithMessage("La dirección no puede exceder 200 caracteres");
        }
    }
}
