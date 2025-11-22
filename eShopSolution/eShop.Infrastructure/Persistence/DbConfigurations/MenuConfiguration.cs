namespace eShop.Infrastructure.Persistence.DbConfigurations
{
    public class MenuConfiguration : IEntityTypeConfiguration<Menu>
    {
        public void Configure(EntityTypeBuilder<Menu> builder)
        {
            // Table name (optional — can remove if same as class name)
            builder.ToTable("Menus");

            // Primary key
            builder.HasKey(m => m.Id);
            
            builder.Property(b => b.Id)
               .IsRequired()
               .ValueGeneratedOnAdd()
               .HasColumnType("int");

            // Properties
            builder.Property(m => m.Title)
                    .IsRequired()
                   .HasMaxLength(50)
                   .HasColumnType("nvarchar(50)");

            builder.Property(m => m.Link).IsRequired()
                .HasMaxLength(300)
                .HasColumnType("nvarchar(300)");

            builder.Property(m => m.Type).IsRequired()
                .HasMaxLength(20);

            builder.Property(m => m.ParentId);

            builder.Property(m => m.Order)
                .IsRequired();


            // Optional self-relation if you need nested menus
            builder.HasOne<Menu>()
                .WithMany()
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
