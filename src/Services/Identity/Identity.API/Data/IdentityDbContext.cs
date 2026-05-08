namespace Identity.API.Data;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<UserRefreshToken> RefreshTokens => Set<UserRefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>(builder =>
        {
            builder.ToTable("Users");
            builder.HasKey(user => user.Id);
            builder.Ignore(user => user.DomainEvents);
            builder.HasIndex(user => user.Email).IsUnique();
            builder.HasIndex(user => user.UserName).IsUnique();
            builder.Property(user => user.UserName).HasMaxLength(100).IsRequired();
            builder.Property(user => user.Email).HasMaxLength(256).IsRequired();
            builder.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
        });

        modelBuilder.Entity<UserRefreshToken>(builder =>
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(token => token.Id);
            builder.Ignore(token => token.DomainEvents);
            builder.HasIndex(token => token.UserId);
            builder.HasIndex(token => token.TokenHash).IsUnique();
            builder.Property(token => token.TokenHash).HasMaxLength(256).IsRequired();
        });
    }
}
