using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project.Data.Relation;
using Project.Enums;
using Project.Tables;


namespace DataBase.Data
{
    public class AppDbContext : IdentityDbContext<Person>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }


        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<Product>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Entity.CalculateSellPrice();
                }
            }
            return base.SaveChanges();
        }


        public DbSet<Person> Persons { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Merchant> Merchants { get; set; }
        public DbSet<DeliveryRep> DeliveryReps { get; set; }



        public DbSet<Product> Products { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }


        public DbSet<EditDelivery> EditDeliveries { get; set; }
        public DbSet<EditCustomer> EditCustomers { get; set; }
        public DbSet<EditMerchant> EditMerchants { get; set; }
        public DbSet<EditOrder> EditOrders { get; set; }
        public DbSet<FavMerchant> FavMerchants { get; set; }
        public DbSet<FavProduct> FavProducts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Image> Images { get; set; }

        public DbSet<ProductDetail> ProductDetails { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet<FeedbackComments> FeedbackComments { get; set; }

        public DbSet<History> Histories { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //---------All Relations be Restrict
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }



            modelBuilder.Entity<Person>()
            .HasIndex(p => p.Email)
            .IsUnique(); //

            modelBuilder.Entity<Merchant>()
            .HasIndex(p => p.NationalId)
            .IsUnique(); //

            modelBuilder.Entity<Admin>()
           .HasIndex(p => p.NationalId)
           .IsUnique(); //

            modelBuilder.Entity<DeliveryRep>()
           .HasIndex(p => p.NationalId)
           .IsUnique(); //


            //---------Custom Identity Tables Configurations
            modelBuilder.Entity<IdentityUserLogin<string>>()
                .HasKey(l => new { l.LoginProvider, l.ProviderKey });

            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasKey(r => new { r.UserId, r.RoleId });

            modelBuilder.Entity<IdentityUserToken<string>>()
                .HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            modelBuilder.Entity<IdentityUserClaim<string>>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<IdentityRoleClaim<string>>()
                .HasKey(rc => rc.Id);

            //---------Other Entities
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Merchant>().ToTable("Merchants");
            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<DeliveryRep>().ToTable("DeliveryReps");

            // Default Date Time
            modelBuilder.Entity<EditMerchant>()
                .Property(em => em.EditDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<EditOrder>()
                .Property(eo => eo.EditDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<FeedbackComments>()
                .Property(eo => eo.DateCreate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<EditCustomer>()
                .Property(em => em.EditDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<EditDelivery>()
                .Property(em => em.EditDate)
                .HasDefaultValueSql("GETUTCDATE()");

            //---------For Default Account Status 
           

            modelBuilder.Entity<Person>()
                .Property(p => p.Status)
                .HasConversion<string>();

            // Composite Keys
            modelBuilder.Entity<EditDelivery>()
                .HasKey(ed => new { ed.adminId, ed.deliveryId });

            modelBuilder.Entity<EditCustomer>()
                .HasKey(ec => new { ec.adminId, ec.customerId });

            modelBuilder.Entity<EditMerchant>()
                .HasKey(em => new { em.adminId, em.merchantId });

            modelBuilder.Entity<EditOrder>()
                .HasKey(eo => new { eo.adminId, eo.orderId });

        //    modelBuilder.Entity<FavMerchant>()
            //    .HasKey(fm => new { fm.merchantId, fm.customerId });

         //   modelBuilder.Entity<FavProduct>()
              //  .HasKey(fm => new { fm.productId, fm.customerId });

            modelBuilder.Entity<Notification>()
                .HasKey(n => new { n.productId, n.customerId });

            //modelBuilder.Entity<OrderItem>()
            //    .HasKey(oi => new { oi.Id, oi.sizeId, oi.colorId, oi.orderId, oi.productId ,oi.MerchantId });

            modelBuilder.Entity<Image>()
                .HasKey(i => new { i.productId, i.colorId , i.ImageData });

            //modelBuilder.Entity<ProductDetail>()
            //    .HasKey(pd => new { pd.Id,pd.productId, pd.colorId, pd.sizeId });

            // modelBuilder.Entity<Cart>()
            //    .HasKey(c => new { c.productId, c.colorId, c.sizeId, c.customerId });
            
            modelBuilder.Entity<Feedback>()
                .HasKey(fc => new { fc.productId , fc.customerId });

            modelBuilder.Entity<FeedbackComments>()
                .HasKey(fc => new { fc.customerId, fc.productId , fc.OriginalComment});

            modelBuilder.Entity<History>()
                .HasKey(d => new { d.productId, d.customerId,d.event_type });

        }




    }
}
