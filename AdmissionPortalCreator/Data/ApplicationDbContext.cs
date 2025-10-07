using AdmissionPortalCreator.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdmissionPortalCreator.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSet properties
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<FormSection> FormSections { get; set; }
        public DbSet<FormField> FormFields { get; set; }
        public DbSet<FieldOption> FieldOptions { get; set; }
        public DbSet<FormSubmission> FormSubmissions { get; set; }
        public DbSet<FormAnswer> FormAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser configuration
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.SetNull);

            // Tenant configuration
            builder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.TenantId);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Email).IsRequired();
            });

            // Form configuration
            builder.Entity<Form>(entity =>
            {
                entity.HasKey(e => e.FormId);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.FormSections)
                    .WithOne(s => s.Form)
                    .HasForeignKey(s => s.FormId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FormSection configuration
            builder.Entity<FormSection>(entity =>
            {
                entity.HasKey(e => e.SectionId);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();

                entity.HasOne(e => e.Form)
                    .WithMany(f => f.FormSections)
                    .HasForeignKey(e => e.FormId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.FormFields)
                    .WithOne(f => f.FormSection)
                    .HasForeignKey(f => f.SectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FormField configuration
            builder.Entity<FormField>(entity =>
            {
                entity.HasKey(e => e.FieldId);
                entity.Property(e => e.Label).HasMaxLength(200).IsRequired();
                entity.Property(e => e.FieldType).HasMaxLength(50).IsRequired();

                entity.HasOne(e => e.FormSection)
                    .WithMany(s => s.FormFields)
                    .HasForeignKey(e => e.SectionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.FieldOptions)
                    .WithOne(o => o.FormField)
                    .HasForeignKey(o => o.FieldId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FieldOption configuration
            builder.Entity<FieldOption>(entity =>
            {
                entity.HasKey(e => e.OptionId);
                entity.Property(e => e.OptionValue).HasMaxLength(200).IsRequired();

                entity.HasOne(e => e.FormField)
                    .WithMany(f => f.FieldOptions)
                    .HasForeignKey(e => e.FieldId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FormSubmission configuration
            builder.Entity<FormSubmission>(entity =>
            {
                entity.HasKey(e => e.SubmissionId);

                entity.HasOne(e => e.Form)
                    .WithMany()
                    .HasForeignKey(e => e.FormId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.FormAnswers)
                    .WithOne(a => a.FormSubmission)
                    .HasForeignKey(a => a.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FormAnswer configuration
            builder.Entity<FormAnswer>(entity =>
            {
                entity.HasKey(e => e.AnswerId);
                entity.Property(e => e.FilePath).HasMaxLength(500);

                entity.HasOne(e => e.FormSubmission)
                    .WithMany(s => s.FormAnswers)
                    .HasForeignKey(e => e.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.FormField)
                    .WithMany()
                    .HasForeignKey(e => e.FieldId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}