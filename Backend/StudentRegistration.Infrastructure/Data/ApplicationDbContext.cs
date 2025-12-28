using Microsoft.EntityFrameworkCore;
using StudentRegistration.Domain.Entities;
using System;

namespace StudentRegistration.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Profesor> Profesores { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<ProfesorMateria> ProfesorMaterias { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Query filters para soft delete
            modelBuilder.Entity<Estudiante>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Profesor>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Materia>().HasQueryFilter(m => !m.IsDeleted);
            // Importante: como ProfesorMateria depende de Materia, también filtramos la tabla puente
            // para no traer relaciones hacia Materias soft-deleted (evita warnings y resultados raros).
            modelBuilder.Entity<ProfesorMateria>().HasQueryFilter(pm => !pm.IsDeleted && !pm.Materia.IsDeleted);
            modelBuilder.Entity<Inscripcion>().HasQueryFilter(i => !i.IsDeleted);
            modelBuilder.Entity<Usuario>().HasQueryFilter(u => !u.IsDeleted);

            // Configuración de Estudiante
            modelBuilder.Entity<Estudiante>(entity =>
            {
                entity.ToTable("Estudiantes");
                entity.HasKey(e => e.EstudiantId);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique().HasFilter("[IsDeleted] = 0");
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.Activo).HasDefaultValue(true);
            });

            // Configuración de Profesor
            modelBuilder.Entity<Profesor>(entity =>
            {
                entity.ToTable("Profesores");
                entity.HasKey(p => p.ProfesorId);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(p => p.Email).IsUnique().HasFilter("[IsDeleted] = 0");
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.IsDeleted).HasDefaultValue(false);
            });

            // Configuración de Materia
            modelBuilder.Entity<Materia>(entity =>
            {
                entity.ToTable("Materias");
                entity.HasKey(m => m.MateriaId);
                entity.Property(m => m.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Codigo).IsRequired().HasMaxLength(20);
                entity.HasIndex(m => m.Codigo).IsUnique().HasFilter("[IsDeleted] = 0");
                entity.Property(m => m.Creditos).HasDefaultValue(3);
                entity.Property(m => m.Descripcion).HasMaxLength(500);
                entity.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(m => m.IsDeleted).HasDefaultValue(false);
            });

            // Configuración de ProfesorMateria
            modelBuilder.Entity<ProfesorMateria>(entity =>
            {
                entity.ToTable("ProfesorMaterias");
                entity.HasQueryFilter(pm => !pm.IsDeleted);
                entity.HasKey(pm => pm.ProfesorMateriaId);
                entity.HasIndex(pm => new { pm.ProfesorId, pm.MateriaId })
                    .IsUnique().HasDatabaseName("UK_ProfesorMateria").HasFilter("[IsDeleted] = 0");
                entity.HasOne(pm => pm.Profesor).WithMany(p => p.ProfesorMaterias)
                    .HasForeignKey(pm => pm.ProfesorId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(pm => pm.Materia).WithMany(m => m.ProfesorMaterias)
                    .HasForeignKey(pm => pm.MateriaId).OnDelete(DeleteBehavior.Restrict);
                entity.Property(pm => pm.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(pm => pm.IsDeleted).HasDefaultValue(false);
            });

            // Configuración de Inscripcion
            modelBuilder.Entity<Inscripcion>(entity =>
            {
                entity.ToTable("Inscripciones");
                entity.HasKey(i => i.InscripcionId);
                entity.HasIndex(i => new { i.EstudiantId, i.MateriaId })
                    .IsUnique().HasDatabaseName("UK_EstudianteMateria").HasFilter("[IsDeleted] = 0");
                entity.HasIndex(i => i.EstudiantId);
                entity.HasIndex(i => i.MateriaId);
                entity.HasIndex(i => i.ProfesorId);
                entity.HasOne(i => i.Estudiante).WithMany(e => e.Inscripciones)
                    .HasForeignKey(i => i.EstudiantId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(i => i.Materia).WithMany(m => m.Inscripciones)
                    .HasForeignKey(i => i.MateriaId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(i => i.Profesor).WithMany(p => p.Inscripciones)
                    .HasForeignKey(i => i.ProfesorId).OnDelete(DeleteBehavior.Restrict);
                entity.Property(i => i.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(i => i.IsDeleted).HasDefaultValue(false);
            });

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(u => u.UsuarioId);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
                entity.Property(u => u.Rol).IsRequired().HasMaxLength(20).HasDefaultValue("Estudiante");
                entity.HasIndex(u => u.Username).IsUnique().HasFilter("[IsDeleted] = 0");
                entity.HasIndex(u => u.Email).IsUnique().HasFilter("[IsDeleted] = 0");
                entity.HasOne(u => u.Estudiante).WithMany()
                    .HasForeignKey(u => u.EstudiantId).OnDelete(DeleteBehavior.SetNull);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(u => u.IsDeleted).HasDefaultValue(false);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var now = DateTime.UtcNow;

            // Seed Profesores
            modelBuilder.Entity<Profesor>().HasData(
                new Profesor { ProfesorId = 1, Nombre = "Carlos", Apellido = "Rodríguez", Email = "carlos.rodriguez@universidad.edu", CreatedAt = now, IsDeleted = false },
                new Profesor { ProfesorId = 2, Nombre = "María", Apellido = "González", Email = "maria.gonzalez@universidad.edu", CreatedAt = now, IsDeleted = false },
                new Profesor { ProfesorId = 3, Nombre = "José", Apellido = "Martínez", Email = "jose.martinez@universidad.edu", CreatedAt = now, IsDeleted = false },
                new Profesor { ProfesorId = 4, Nombre = "Ana", Apellido = "López", Email = "ana.lopez@universidad.edu", CreatedAt = now, IsDeleted = false },
                new Profesor { ProfesorId = 5, Nombre = "Pedro", Apellido = "Sánchez", Email = "pedro.sanchez@universidad.edu", CreatedAt = now, IsDeleted = false }
            );

            // Seed Materias
            modelBuilder.Entity<Materia>().HasData(
                new Materia { MateriaId = 1, Nombre = "Programación I", Codigo = "PROG101", Creditos = 3, Descripcion = "Fundamentos de programación", CreatedAt = now, IsDeleted = false },
                new Materia { MateriaId = 2, Nombre = "Bases de Datos", Codigo = "BD102", Creditos = 3, Descripcion = "Diseño y gestión de BD", CreatedAt = now, IsDeleted = false },
                new Materia { MateriaId = 3, Nombre = "Estructuras de Datos", Codigo = "ED103", Creditos = 3, Descripcion = "Algoritmos y estructuras", CreatedAt = now, IsDeleted = false },
                new Materia { MateriaId = 4, Nombre = "Desarrollo Web", Codigo = "WEB104", Creditos = 3, Descripcion = "Aplicaciones web", CreatedAt = now, IsDeleted = false },
                new Materia { MateriaId = 5, Nombre = "Ingeniería de Software", Codigo = "IS105", Creditos = 3, Descripcion = "Metodologías", CreatedAt = now, IsDeleted = false },
                new Materia { MateriaId = 6, Nombre = "Redes de Computadoras", Codigo = "RED106", Creditos = 3, Descripcion = "Fundamentos de redes", CreatedAt = now, IsDeleted = false },
                new Materia { MateriaId = 7, Nombre = "Sistemas Operativos", Codigo = "SO107", Creditos = 3, Descripcion = "Arquitectura de SO", CreatedAt = now, IsDeleted = false },
                new Materia { MateriaId = 8, Nombre = "Inteligencia Artificial", Codigo = "IA108", Creditos = 3, Descripcion = "Introducción a IA", CreatedAt = now, IsDeleted = false },
                new Materia { MateriaId = 9, Nombre = "Seguridad Informática", Codigo = "SEG109", Creditos = 3, Descripcion = "Seguridad en sistemas", CreatedAt = now, IsDeleted = false },
                new Materia { MateriaId = 10, Nombre = "Arquitectura de Software", Codigo = "ARQ110", Creditos = 3, Descripcion = "Patrones", CreatedAt = now, IsDeleted = false }
            );

            // Seed ProfesorMaterias
            modelBuilder.Entity<ProfesorMateria>().HasData(
                new ProfesorMateria { ProfesorMateriaId = 1, ProfesorId = 1, MateriaId = 1, CreatedAt = now, IsDeleted = false },
                new ProfesorMateria { ProfesorMateriaId = 2, ProfesorId = 1, MateriaId = 2, CreatedAt = now, IsDeleted = false },
                new ProfesorMateria { ProfesorMateriaId = 3, ProfesorId = 2, MateriaId = 3, CreatedAt = now, IsDeleted = false },
                new ProfesorMateria { ProfesorMateriaId = 4, ProfesorId = 2, MateriaId = 4, CreatedAt = now, IsDeleted = false },
                new ProfesorMateria { ProfesorMateriaId = 5, ProfesorId = 3, MateriaId = 5, CreatedAt = now, IsDeleted = false },
                new ProfesorMateria { ProfesorMateriaId = 6, ProfesorId = 3, MateriaId = 6, CreatedAt = now, IsDeleted = false },
                new ProfesorMateria { ProfesorMateriaId = 7, ProfesorId = 4, MateriaId = 7, CreatedAt = now, IsDeleted = false },
                new ProfesorMateria { ProfesorMateriaId = 8, ProfesorId = 4, MateriaId = 8, CreatedAt = now, IsDeleted = false },
                new ProfesorMateria { ProfesorMateriaId = 9, ProfesorId = 5, MateriaId = 9, CreatedAt = now, IsDeleted = false },
                new ProfesorMateria { ProfesorMateriaId = 10, ProfesorId = 5, MateriaId = 10, CreatedAt = now, IsDeleted = false }
            );

            // Seed Usuario Admin
            modelBuilder.Entity<Usuario>().HasData(new Usuario
            {
                UsuarioId = 1,
                Username = "admin",
                Email = "admin@universidad.edu",
                // Credenciales demo (Swagger): username=admin, password=Admin123*
            // Hash: SHA256(UTF8) -> Base64
                // Nota: en producción deberías usar un hash con salt (PBKDF2/bcrypt/Argon2). Para la prueba basta.
                PasswordHash = "ClvD40JDLxutkv/VG3hTQ+xykGzbpqJhMQYLAI54ZlY=",
                Rol = "Admin",
                CreatedAt = now,
                IsDeleted = false
            });
        }
    }
}