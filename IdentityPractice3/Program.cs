using IdentityPractice3.Data;
using IdentityPractice3.Helpers;
using IdentityPractice3.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DataBaseContext>(p=>p.UseSqlServer("Server=.;Database=IdentityDB3;Integrated Security=True;TrustServerCertificate=True;"));
builder.Services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<DataBaseContext>()
                .AddDefaultTokenProviders()
                .AddRoles<Role>()
                .AddErrorDescriber<CustomIdentityError>()
                .AddPasswordValidator<MyPasswordValidator>();

//identity change default setting
builder.Services.Configure<IdentityOptions>(option =>
{
    option.User.RequireUniqueEmail = true;
    option.Password.RequireLowercase = false;
    option.Password.RequireNonAlphanumeric = false;
    option.Password.RequireUppercase = false;
    option.Password.RequiredLength = 3;
    option.Password.RequiredUniqueChars = 1;
});


builder.Services.AddScoped<IClaimsTransformation, AddClaim>();
builder.Services.AddSingleton<IAuthorizationHandler, UserCreditHandler>();

//authoriz by  policy || customClaim 
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BuyerPolicy", policy =>
    {
        policy.RequireClaim("Buyer");
    });
    options.AddPolicy("BloodType", policy =>
    {
        policy.RequireClaim("Blood", "A", "O");
    }
    );
    options.AddPolicy("Cradit", policy =>
    {
        policy.Requirements.Add(new UserCreditRequerment(10000));
    });
});

//builder.Services.AddAuthentication()
//                .AddGoogle(options =>
//                {
//                    options.ClientId = "  " ;
//                    options.ClientSecret = "   " ;
//                }
//                );

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//library for xss attack
//app.UseCsp(Option =>
//{
//    //just run host inside scripts
//    Option.ScriptSources(d => d.Self().CustomSources();
//});


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
