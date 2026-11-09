using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Tarhal_FYP.Models;

public partial class TarhalDbContext : DbContext
{
    public TarhalDbContext()
    {
    }

    public TarhalDbContext(DbContextOptions<TarhalDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Accommodation> Accommodations { get; set; }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Attraction> Attractions { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Plan> Plans { get; set; }

    public virtual DbSet<PlanAccommodation> PlanAccommodations { get; set; }

    public virtual DbSet<PlanAccommodationView> PlanAccommodationViews { get; set; }

    public virtual DbSet<PlanActivity> PlanActivities { get; set; }

    public virtual DbSet<PlanActivityView> PlanActivityViews { get; set; }

    public virtual DbSet<PlanAttraction> PlanAttractions { get; set; }

    public virtual DbSet<PlanAttractionView> PlanAttractionViews { get; set; }

    public virtual DbSet<PlanRestaurant> PlanRestaurants { get; set; }

    public virtual DbSet<PlanRestaurantView> PlanRestaurantViews { get; set; }

    public virtual DbSet<Restaurant> Restaurants { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceProvider> ServiceProviders { get; set; }

    public virtual DbSet<Tourist> Tourists { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TarhalDB;Integrated Security=True;Pooling=False;Encrypt=True;Trust Server Certificate=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accommodation>(entity =>
        {
            entity.HasKey(e => e.AccommodationId).HasName("PK__Accommod__004EC325F4237010");

            entity.ToTable("Accommodation");

            entity.Property(e => e.AccommodationId)
                .ValueGeneratedNever()
                .HasColumnName("accommodation_id");
            entity.Property(e => e.AccommodationAvailable)
                .HasDefaultValue(1)
                .HasColumnName("accommodation_available");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.Amenities)
                .HasColumnType("text")
                .HasColumnName("amenities");
            entity.Property(e => e.BookingAvailability).HasColumnName("bookingAvailability");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(18, 15)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(18, 15)")
                .HasColumnName("longitude");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
            entity.Property(e => e.PricePerNight)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("price_per_night");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.Rate).HasColumnName("rate");
            entity.Property(e => e.RatesCount).HasColumnName("rates_count");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.Url)
                .HasMaxLength(455)
                .IsUnicode(false)
                .HasColumnName("url");

            entity.HasOne(d => d.AccommodationNavigation).WithOne(p => p.Accommodation)
                .HasForeignKey<Accommodation>(d => d.AccommodationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_accommodation_service");
        });

        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.ActivityId).HasName("PK__Activity__482FBD63EBE1A301");

            entity.ToTable("Activity");

            entity.Property(e => e.ActivityId)
                .ValueGeneratedNever()
                .HasColumnName("activity_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.BookingAvailability).HasColumnName("bookingAvailability");
            entity.Property(e => e.Category)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("category");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(18, 15)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(18, 15)")
                .HasColumnName("longitude");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Policies)
                .HasColumnType("text")
                .HasColumnName("policies");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.Rate)
                .HasDefaultValue(0.0)
                .HasColumnName("rate");
            entity.Property(e => e.RatesCount).HasColumnName("rates_count");
            entity.Property(e => e.Schedule)
                .HasMaxLength(350)
                .IsUnicode(false)
                .HasColumnName("schedule");
            entity.Property(e => e.TimeTaken).HasColumnName("time_taken");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.ActivityNavigation).WithOne(p => p.Activity)
                .HasForeignKey<Activity>(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_activity_service");
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admin__43AA4141FAF3C7BE");

            entity.ToTable("Admin");

            entity.Property(e => e.AdminId)
                .ValueGeneratedNever()
                .HasColumnName("admin_id");

            entity.HasOne(d => d.AdminNavigation).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Admin_user_id_foreign");
        });

        modelBuilder.Entity<Attraction>(entity =>
        {
            entity.HasKey(e => e.AttractionId).HasName("PK__Attracti__537C1CF36F5516AA");

            entity.ToTable("Attraction");

            entity.Property(e => e.AttractionId)
                .ValueGeneratedNever()
                .HasColumnName("attraction_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.AdmissionFee)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("admission_fee");
            entity.Property(e => e.BookingAvailability).HasColumnName("bookingAvailability");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(18, 15)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(18, 15)")
                .HasColumnName("longitude");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.OpeningHours)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("opening_hours");
            entity.Property(e => e.PopularTimes)
                .HasColumnType("text")
                .HasColumnName("popular_times");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.Rate).HasColumnName("rate");
            entity.Property(e => e.RatesCount).HasColumnName("rates_count");
            entity.Property(e => e.TimeTaken).HasColumnName("time_taken");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.AttractionNavigation).WithOne(p => p.Attraction)
                .HasForeignKey<Attraction>(d => d.AttractionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_attraction_service");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => new { e.TouristId, e.ServiceId }).HasName("PK__Bookings__603D49435E043450");

            entity.Property(e => e.TouristId).HasColumnName("tourist_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");

            entity.HasOne(d => d.Service).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bookings_service_id_foreign");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__tmp_ms_x__7516F4EC6DA2CE38");

            entity.ToTable("Image");

            entity.Property(e => e.ImageId).HasColumnName("ImageID");
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

            entity.HasOne(d => d.Service).WithMany(p => p.Images)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("Image_service_id_foreign");
        });

        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__tmp_ms_x__BE9F8F1D504DA81F");

            entity.ToTable("Plan");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.Budget)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("budget");
            entity.Property(e => e.DateBegin).HasColumnName("date_begin");
            entity.Property(e => e.DateEnd).HasColumnName("date_end");
            entity.Property(e => e.Destination)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("destination");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Preferences)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("preferences");
            entity.Property(e => e.TouristId).HasColumnName("tourist_id");
            entity.Property(e => e.TravelTime).HasColumnName("travel_time");
            entity.Property(e => e.TripType)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("trip_type");

            entity.HasOne(d => d.Tourist).WithMany(p => p.Plans)
                .HasForeignKey(d => d.TouristId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("plan_tourist_id_foreign");
        });

        modelBuilder.Entity<PlanAccommodation>(entity =>
        {
            entity.HasKey(e => new { e.PlanId, e.DayNum }).HasName("PK__Plan_Acc__C7BB74DE9A3D597D");

            entity.ToTable("Plan_Accommodation");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.DayNum).HasColumnName("day_num");
            entity.Property(e => e.AccommodationId).HasColumnName("accommodation_id");

            entity.HasOne(d => d.Accommodation).WithMany(p => p.PlanAccommodations)
                .HasForeignKey(d => d.AccommodationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_accomodation_plan_Accomodation");

            entity.HasOne(d => d.Plan).WithMany(p => p.PlanAccommodations)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Plan_Accommodation_foreign");
        });

        modelBuilder.Entity<PlanAccommodationView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("planAccommodation_view");

            entity.Property(e => e.Accommodation31).HasColumnName("Accommodation_31");
            entity.Property(e => e.Accommodation32).HasColumnName("Accommodation_32");
            entity.Property(e => e.Accommodation33).HasColumnName("Accommodation_33");
            entity.Property(e => e.Accommodation34).HasColumnName("Accommodation_34");
            entity.Property(e => e.Accommodation35).HasColumnName("Accommodation_35");
            entity.Property(e => e.Accommodation36).HasColumnName("Accommodation_36");
            entity.Property(e => e.Accommodation37).HasColumnName("Accommodation_37");
            entity.Property(e => e.Accommodation38).HasColumnName("Accommodation_38");
            entity.Property(e => e.Accommodation39).HasColumnName("Accommodation_39");
            entity.Property(e => e.Accommodation40).HasColumnName("Accommodation_40");
            entity.Property(e => e.Accommodation41).HasColumnName("Accommodation_41");
            entity.Property(e => e.Accommodation42).HasColumnName("Accommodation_42");
            entity.Property(e => e.Accommodation43).HasColumnName("Accommodation_43");
            entity.Property(e => e.Accommodation44).HasColumnName("Accommodation_44");
            entity.Property(e => e.Accommodation45).HasColumnName("Accommodation_45");
            entity.Property(e => e.Budget)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("budget");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.TravelTime).HasColumnName("travel_time");
            entity.Property(e => e.TripType)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("trip_type");
        });

        modelBuilder.Entity<PlanActivity>(entity =>
        {
            entity.HasKey(e => new { e.PlanId, e.DayNum }).HasName("PK__Plan_Act__C7BB74DE4EB70EFB");

            entity.ToTable("Plan_Activity");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.DayNum).HasColumnName("day_num");
            entity.Property(e => e.ActivityId).HasColumnName("activity_id");

            entity.HasOne(d => d.Activity).WithMany(p => p.PlanActivities)
                .HasForeignKey(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_activity_plan_Activity");

            entity.HasOne(d => d.Plan).WithMany(p => p.PlanActivities)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Plan_Activity_foreign");
        });

        modelBuilder.Entity<PlanActivityView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("planActivity_view");

            entity.Property(e => e.Activity16).HasColumnName("Activity_16");
            entity.Property(e => e.Activity17).HasColumnName("Activity_17");
            entity.Property(e => e.Activity18).HasColumnName("Activity_18");
            entity.Property(e => e.Activity19).HasColumnName("Activity_19");
            entity.Property(e => e.Activity20).HasColumnName("Activity_20");
            entity.Property(e => e.Activity21).HasColumnName("Activity_21");
            entity.Property(e => e.Activity22).HasColumnName("Activity_22");
            entity.Property(e => e.Activity23).HasColumnName("Activity_23");
            entity.Property(e => e.Activity24).HasColumnName("Activity_24");
            entity.Property(e => e.Activity25).HasColumnName("Activity_25");
            entity.Property(e => e.Activity26).HasColumnName("Activity_26");
            entity.Property(e => e.Activity27).HasColumnName("Activity_27");
            entity.Property(e => e.Activity28).HasColumnName("Activity_28");
            entity.Property(e => e.Activity29).HasColumnName("Activity_29");
            entity.Property(e => e.Activity30).HasColumnName("Activity_30");
            entity.Property(e => e.Budget)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("budget");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.TouristPreferences)
                .HasMaxLength(8000)
                .IsUnicode(false)
                .HasColumnName("tourist_preferences");
            entity.Property(e => e.TravelTime).HasColumnName("travel_time");
            entity.Property(e => e.TripType)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("trip_type");
        });

        modelBuilder.Entity<PlanAttraction>(entity =>
        {
            entity.HasKey(e => new { e.PlanId, e.AttractionId, e.DayNum }).HasName("PK__Plan_Att__AC3A016E12AE742A");

            entity.ToTable("Plan_Attraction");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.AttractionId).HasColumnName("attraction_id");
            entity.Property(e => e.DayNum).HasColumnName("day_num");

            entity.HasOne(d => d.Attraction).WithMany(p => p.PlanAttractions)
                .HasForeignKey(d => d.AttractionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_attraction_plan_Attraction");

            entity.HasOne(d => d.Plan).WithMany(p => p.PlanAttractions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Plan_Attraction_foreign");
        });

        modelBuilder.Entity<PlanAttractionView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("planAttraction_view");

            entity.Property(e => e.Attraction1).HasColumnName("Attraction_1");
            entity.Property(e => e.Attraction10).HasColumnName("Attraction_10");
            entity.Property(e => e.Attraction11).HasColumnName("Attraction_11");
            entity.Property(e => e.Attraction12).HasColumnName("Attraction_12");
            entity.Property(e => e.Attraction13).HasColumnName("Attraction_13");
            entity.Property(e => e.Attraction14).HasColumnName("Attraction_14");
            entity.Property(e => e.Attraction15).HasColumnName("Attraction_15");
            entity.Property(e => e.Attraction2).HasColumnName("Attraction_2");
            entity.Property(e => e.Attraction3).HasColumnName("Attraction_3");
            entity.Property(e => e.Attraction4).HasColumnName("Attraction_4");
            entity.Property(e => e.Attraction5).HasColumnName("Attraction_5");
            entity.Property(e => e.Attraction6).HasColumnName("Attraction_6");
            entity.Property(e => e.Attraction7).HasColumnName("Attraction_7");
            entity.Property(e => e.Attraction8).HasColumnName("Attraction_8");
            entity.Property(e => e.Attraction9).HasColumnName("Attraction_9");
            entity.Property(e => e.Budget)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("budget");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.TouristPreferences)
                .HasMaxLength(8000)
                .IsUnicode(false)
                .HasColumnName("tourist_preferences");
            entity.Property(e => e.TravelTime).HasColumnName("travel_time");
            entity.Property(e => e.TripType)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("trip_type");
        });

        modelBuilder.Entity<PlanRestaurant>(entity =>
        {
            entity.HasKey(e => new { e.PlanId, e.DayNum, e.Arrange }).HasName("PK__Plan_Res__40C1D5841C1143A8");

            entity.ToTable("Plan_Restaurant");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.DayNum).HasColumnName("day_num");
            entity.Property(e => e.Arrange).HasColumnName("arrange");
            entity.Property(e => e.RestaurantId).HasColumnName("restaurant_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.PlanRestaurants)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Plan_Restaurant_foreign");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.PlanRestaurants)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_restaurant_plan_Restaurant");
        });

        modelBuilder.Entity<PlanRestaurantView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("planRestaurant_view");

            entity.Property(e => e.Budget)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("budget");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.Restaurant46).HasColumnName("Restaurant_46");
            entity.Property(e => e.Restaurant47).HasColumnName("Restaurant_47");
            entity.Property(e => e.Restaurant48).HasColumnName("Restaurant_48");
            entity.Property(e => e.Restaurant49).HasColumnName("Restaurant_49");
            entity.Property(e => e.Restaurant50).HasColumnName("Restaurant_50");
            entity.Property(e => e.Restaurant51).HasColumnName("Restaurant_51");
            entity.Property(e => e.Restaurant52).HasColumnName("Restaurant_52");
            entity.Property(e => e.Restaurant53).HasColumnName("Restaurant_53");
            entity.Property(e => e.Restaurant54).HasColumnName("Restaurant_54");
            entity.Property(e => e.Restaurant55).HasColumnName("Restaurant_55");
            entity.Property(e => e.Restaurant56).HasColumnName("Restaurant_56");
            entity.Property(e => e.Restaurant57).HasColumnName("Restaurant_57");
            entity.Property(e => e.Restaurant58).HasColumnName("Restaurant_58");
            entity.Property(e => e.Restaurant59).HasColumnName("Restaurant_59");
            entity.Property(e => e.TravelTime).HasColumnName("travel_time");
            entity.Property(e => e.TripType)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("trip_type");
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.RestaurantId).HasName("PK__Restaura__3B0FAA913278FB45");

            entity.ToTable("Restaurant");

            entity.Property(e => e.RestaurantId)
                .ValueGeneratedNever()
                .HasColumnName("restaurant_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.BookingAvailability).HasColumnName("bookingAvailability");
            entity.Property(e => e.CuisineType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("cuisine_type");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(18, 15)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(18, 15)")
                .HasColumnName("longitude");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PriceRange)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("price_range");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.Rate).HasColumnName("rate");
            entity.Property(e => e.RatesCount).HasColumnName("rates_count");
            entity.Property(e => e.Url)
                .HasMaxLength(455)
                .IsUnicode(false)
                .HasColumnName("url");

            entity.HasOne(d => d.RestaurantNavigation).WithOne(p => p.Restaurant)
                .HasForeignKey<Restaurant>(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_restaurant_service");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Service__3E0DB8AFCF1B6E4E");

            entity.ToTable("Service");

            entity.Property(e => e.ServiceId)
                .ValueGeneratedNever()
                .HasColumnName("service_id");
            entity.Property(e => e.Destination)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Provider).WithMany(p => p.Services)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("service_provider_id_foreign");
        });

        modelBuilder.Entity<ServiceProvider>(entity =>
        {
            entity.HasKey(e => e.ProviderId).HasName("PK__ServiceP__00E21310232FD3CE");

            entity.ToTable("ServiceProvider");

            entity.HasIndex(e => e.Email, "serviceprovider_email_unique").IsUnique();

            entity.Property(e => e.ProviderId)
                .ValueGeneratedNever()
                .HasColumnName("provider_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone_number");

            entity.HasOne(d => d.Provider).WithOne(p => p.ServiceProvider)
                .HasForeignKey<ServiceProvider>(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ServiceProvider_user_id_foreign");
        });

        modelBuilder.Entity<Tourist>(entity =>
        {
            entity.HasKey(e => e.TouristId).HasName("PK__Tourist__83DD92C9A62CD138");

            entity.ToTable("Tourist");

            entity.HasIndex(e => e.Email, "tourist_email_unique").IsUnique();

            entity.Property(e => e.TouristId)
                .ValueGeneratedNever()
                .HasColumnName("tourist_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.TravelTime).HasColumnName("travel_time");

            entity.HasOne(d => d.TouristNavigation).WithOne(p => p.Tourist)
                .HasForeignKey<Tourist>(d => d.TouristId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Tourist_user_id_foreign");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370F6EC30413");

            entity.ToTable("User");

            entity.HasIndex(e => e.Username, "user_username_unique").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.UserType)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("user_type");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserInfo__3214EC07D28A761D");

            entity.ToTable("UserInfo");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Un)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("un");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
