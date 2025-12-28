using FluentValidation;
using StudentRegistration.Application.DTOs.Inscripcion;
using System.Linq;

namespace StudentRegistration.Application.Validators
{
    public class CreateInscripcionValidator : AbstractValidator<CreateInscripcionDto>
    {
        public CreateInscripcionValidator()
        {
            RuleFor(x => x.EstudiantId)
                .GreaterThan(0).WithMessage("El ID del estudiante es inválido");

            RuleFor(x => x.MateriaIds)
                .NotEmpty().WithMessage("Debe seleccionar al menos una materia")
                .Must(x => x.Count <= 3).WithMessage("No puede seleccionar más de 3 materias")
                .Must(x => x.Distinct().Count() == x.Count).WithMessage("No puede seleccionar materias duplicadas");
        }
    }
}
