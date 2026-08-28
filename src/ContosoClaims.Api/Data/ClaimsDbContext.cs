using ContosoClaims.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoClaims.Api.Data;

public class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : base(options)
    {
    }

    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Adjuster> Adjusters => Set<Adjuster>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimNote> ClaimNotes => Set<ClaimNote>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Policy>(entity =>
        {
            entity.ToTable("policies");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PolicyNumber).HasColumnName("policy_number").HasMaxLength(20).IsRequired();
            entity.Property(e => e.HolderName).HasColumnName("holder_name").HasMaxLength(120).IsRequired();
            entity.Property(e => e.HolderEmail).HasColumnName("holder_email").HasMaxLength(160).IsRequired();
            entity.Property(e => e.ProductType).HasColumnName("product_type")
                .HasColumnType("enum('auto','home','travel','liability')").IsRequired();
            entity.Property(e => e.CoverageLimit).HasColumnName("coverage_limit").HasColumnType("decimal(12,2)");
            entity.Property(e => e.Deductible).HasColumnName("deductible").HasColumnType("decimal(10,2)");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date").HasColumnType("date");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date").HasColumnType("date");
            entity.Property(e => e.Status).HasColumnName("status")
                .HasColumnType("enum('active','lapsed','cancelled')").IsRequired();
            entity.HasIndex(e => e.PolicyNumber).IsUnique();
        });

        modelBuilder.Entity<Adjuster>(entity =>
        {
            entity.ToTable("adjusters");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EmployeeCode).HasColumnName("employee_code").HasMaxLength(12).IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(120).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(160).IsRequired();
            entity.Property(e => e.Region).HasColumnName("region")
                .HasColumnType("enum('north','south','east','west')").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
        });

        modelBuilder.Entity<Claim>(entity =>
        {
            entity.ToTable("claims");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClaimNumber).HasColumnName("claim_number").HasMaxLength(20).IsRequired();
            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
            entity.Property(e => e.AssignedAdjusterId).HasColumnName("assigned_adjuster_id");
            entity.Property(e => e.Status).HasColumnName("status")
                .HasColumnType("enum('submitted','under_review','approved','rejected','paid')").IsRequired();
            entity.Property(e => e.IncidentDate).HasColumnName("incident_date").HasColumnType("date");
            entity.Property(e => e.ReportedAt).HasColumnName("reported_at").HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnName("description").HasColumnType("text").IsRequired();
            entity.Property(e => e.ClaimedAmount).HasColumnName("claimed_amount").HasColumnType("decimal(12,2)");
            entity.Property(e => e.ApprovedAmount).HasColumnName("approved_amount").HasColumnType("decimal(12,2)");
            entity.Property(e => e.DecidedByAdjusterId).HasColumnName("decided_by_adjuster_id");
            entity.Property(e => e.DecidedAt).HasColumnName("decided_at").HasColumnType("datetime");
            entity.HasIndex(e => e.ClaimNumber).IsUnique();
            entity.HasIndex(e => e.Status).HasDatabaseName("idx_claims_status");
            entity.HasIndex(e => e.PolicyId).HasDatabaseName("idx_claims_policy");
            entity.HasIndex(e => e.AssignedAdjusterId).HasDatabaseName("idx_claims_assigned");

            entity.HasOne(e => e.Policy)
                .WithMany(p => p.Claims)
                .HasForeignKey(e => e.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedAdjuster)
                .WithMany(a => a.AssignedClaims)
                .HasForeignKey(e => e.AssignedAdjusterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DecidedByAdjuster)
                .WithMany(a => a.DecidedClaims)
                .HasForeignKey(e => e.DecidedByAdjusterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ClaimNote>(entity =>
        {
            entity.ToTable("claim_notes");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClaimId).HasColumnName("claim_id");
            entity.Property(e => e.AuthorAdjusterId).HasColumnName("author_adjuster_id");
            entity.Property(e => e.Body).HasColumnName("body").HasColumnType("text").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("datetime");

            entity.HasOne(e => e.Claim)
                .WithMany(c => c.Notes)
                .HasForeignKey(e => e.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AuthorAdjuster)
                .WithMany()
                .HasForeignKey(e => e.AuthorAdjusterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClaimId).HasColumnName("claim_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at").HasColumnType("datetime");
            entity.Property(e => e.Method).HasColumnName("method")
                .HasColumnType("enum('bank_transfer','cheque','card')").IsRequired();
            entity.Property(e => e.Reference).HasColumnName("reference").HasMaxLength(40).IsRequired();
            entity.HasIndex(e => e.Reference).IsUnique();

            entity.HasOne(e => e.Claim)
                .WithMany(c => c.Payments)
                .HasForeignKey(e => e.ClaimId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
