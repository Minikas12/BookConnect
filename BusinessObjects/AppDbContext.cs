using BusinessObjects.Models;
using BusinessObjects.Models.Ads;
using BusinessObjects.Models.Authorization;
using BusinessObjects.Models.Creative;
using BusinessObjects.Models.E_com.Rating;
using BusinessObjects.Models.E_com.Trading;
using BusinessObjects.Models.Ecom;
using BusinessObjects.Models.Ecom.Base;
using BusinessObjects.Models.Ecom.Payment;
using BusinessObjects.Models.Ecom.Rating;
using BusinessObjects.Models.Social;
using BusinessObjects.Models.Trading;
using BusinessObjects.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BusinessObjects
{
	public class AppDbContext: DbContext
	{
        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) { }

        //Base services DbSets
        public virtual DbSet<AppUser> AppUsers { get; set; }
        public virtual DbSet<Agency> Agencies { get; set; } 
        public virtual DbSet<Basket> Baskets { get; set; } 
        public virtual DbSet<Cart> Carts { get; set; } 
        public virtual DbSet<Inventory> Inventories { get; set; } 
        public virtual DbSet<Order> Orders { get; set; } 
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<BanRecord> BanRecords { get; set; }
        public virtual DbSet<HeartRecord> HeartRecords { get; set; }
        public virtual DbSet<UserTargetedCategory> UserTargetedCategories { get; set; }
        public virtual DbSet<OrderItemStatus> OrderItemStatuses { get; set; }

        //Subscribtion services DbSets
        public virtual DbSet<Tier> Tiers { get; set; } 
        public virtual DbSet<Subscription> Subscriptions { get; set; } 
        public virtual DbSet<SubRecord> SubRecords { get; set; } 

        //Rating services DbSets
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<RatingRecord> RatingRecords { get; set; } 

        //Payment service DbSets 
        //public virtual DbSet<PaymentDetails> PaymentDetails { get; set; } = null!;
        public virtual DbSet<TransactionRecord> Transactions { get; set; } 
        //public virtual Db

        //Social services DbSets
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<PostInterester> PostInteresters { get; set; }
        public virtual DbSet<CommentOfPost> CommentOfPosts { get; set; }
        public virtual DbSet<TradeDetails> TradeDetails { get; set; }
        public virtual DbSet<UserSavedPost> UserSavedPosts { get; set; }

        //Utility DbSets
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<RoleRecord> RoleRecords { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; } 
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CategoryList> CategoryLists { get; set; }
        public virtual DbSet<BookGroup> BookGroups { get; set; }
        public virtual DbSet<ListBookGroup> ListBookGroups { get; set; }
        public virtual DbSet<Address> Addresses { get; set; } 
        public virtual DbSet<Statistic> Statistics { get; set; }
        public virtual DbSet<NIC_Data> NIC_Datas { get; set; }
        public virtual DbSet<Advertisement> Advertisements { get; set; }
        public virtual DbSet<AdParticipateRecord> AdParticipateRecords { get; set; }
        public virtual DbSet<BookCheckList> BookCheckLists { get; set; }


        //Creative services DbSet
        public virtual DbSet<Reply> Replies { get; set; } //SONDB
        public virtual DbSet<ChatMessage> ChatMessages { get; set; } //SONDB
     
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfigurationRoot config = builder.Build();
            optionsBuilder.UseSqlServer(config.GetConnectionString("Default"));
            optionsBuilder.EnableSensitiveDataLogging(); //for: The instance of entity type ? cannot be tracked because another instance with the same key value
                                                         //for ? is already being tracked. 
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Role>().HasData(
                new Role {
                    RoleId = Guid.Parse("2da9143d-559c-40b5-907d-0d9c8d714c6c"),
                    RoleName = "BaseUser",
                    Description = "Role for base user?"
                },
                  new Role
                   {
                       RoleId = Guid.Parse("325c7657-7391-40ea-98ba-29580d3f7e74"),
                       RoleName = "Admin",
                       Description = ""
                   },
                 new Role
                 {
                     RoleId = Guid.Parse("439e2d3c-6050-4480-a7d5-ab4b23425992"),
                     RoleName = "Seller",
                     Description = ""
                 },
                 new Role
                 {
                      RoleId = Guid.Parse("3b22f2b7-5938-4cbe-8cf6-82886f4853d4"),
                      RoleName = "Staff",
                      Description = ""
                 }
             );
            base.OnModelCreating(builder); 
            builder.Entity<SubRecord>()
           .HasOne(sr => sr.Subscription)
           .WithMany()
           .HasForeignKey(sr => sr.SubscriptionId)
           .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Agency>()
            .HasOne(a => a.PostAddress)
            .WithMany()
            .HasForeignKey(a => a.PostAddressId)
            .OnDelete(DeleteBehavior.Restrict);

              builder.Entity<PostInterester>()
            .HasOne(p => p.AppUser)
            .WithMany()
            .HasForeignKey(a => a.InteresterId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PostInterester>()
            .HasOne(p => p.Post)
            .WithMany()
            .HasForeignKey(a => a.PostId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Comment>()
            .HasOne(p => p.AppUser)
            .WithMany()
            .HasForeignKey(a => a.CommenterId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<HeartRecord>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<HeartRecord>()
            .HasOne(r => r.Post)
            .WithMany()
            .HasForeignKey(a => a.PostId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserSavedPost>()
            .HasOne(us => us.Post)
            .WithMany()
            .HasForeignKey(us => us.PostId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reply>() //SONDB
                .HasOne(r => r.RatingRecord)
                .WithMany()
                .HasForeignKey(r => r.RatingRecordId) // Use RatingRecordId as the foreign key
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ChatMessage>(entity => //SONDB
            {

                entity.HasOne(d => d.Sender)
                    .WithMany()
                    .HasForeignKey(d => d.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Receiver)
                    .WithMany()
                    .HasForeignKey(d => d.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<OrderItemStatus>()  //SONDB
               .HasOne(o => o.Basket) // Define the navigation property
               .WithMany() // Define the inverse navigation property
               .HasForeignKey(o => o.BasketId) // Specify the foreign key
               .OnDelete(DeleteBehavior.Restrict); // Set delete behavior

            // Optionally, configure other relationships like Agency, Order, and Book
            builder.Entity<OrderItemStatus>() //SONDB
                .HasOne(o => o.Agency)
                .WithMany()
                .HasForeignKey(o => o.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItemStatus>() //SONDB
                .HasOne(o => o.Order)
                .WithMany()
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItemStatus>() //SONDB
                .HasOne(o => o.Book)
                .WithMany()
                .HasForeignKey(o => o.BookId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}

