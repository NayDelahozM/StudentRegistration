using AutoMapper;
using StudentRegistration.Application.DTOs.Estudiante;
using StudentRegistration.Application.DTOs.Inscripcion;
using StudentRegistration.Domain.Entities;
using System.Linq;

namespace StudentRegistration.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Estudiante mappings
            CreateMap<Estudiante, EstudianteDto>()
                .ForMember(dest => dest.CreditosTotales,
                    opt => opt.MapFrom(src => src.Inscripciones.Sum(i => i.Materia.Creditos)));

            // Fix Problema #8: Mapeo para DTO simplificado (solo info pública no sensible)
            CreateMap<Estudiante, EstudianteSummaryDto>();

            CreateMap<CreateEstudianteDto, Estudiante>();
            CreateMap<UpdateEstudianteDto, Estudiante>();

            // Inscripcion mappings
            CreateMap<Inscripcion, InscripcionDto>()
                .ForMember(dest => dest.MateriaNombre,
                    opt => opt.MapFrom(src => src.Materia.Nombre))
                .ForMember(dest => dest.MateriaCode,
                    opt => opt.MapFrom(src => src.Materia.Codigo))
                .ForMember(dest => dest.ProfesorNombre,
                    opt => opt.MapFrom(src => $"{src.Profesor.Nombre} {src.Profesor.Apellido}"))
                .ForMember(dest => dest.FechaInscripcion,
                    opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}
