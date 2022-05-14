using Microsoft.EntityFrameworkCore;
using Web.Channels;
using Web.Data;
using Web.Hubs;
using Web.Models.Configuration;
using Web.Services;
using User = Web.Models.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var telegramBotConfig = builder.Configuration.GetSection(nameof(TelegramBotConfiguration)).Get<TelegramBotConfiguration>();
var twitterBotConfig = builder.Configuration.GetSection(nameof(TwitterBotConfiguration)).Get<TwitterBotConfiguration>();
var gitHubBotConfig = builder.Configuration.GetSection(nameof(GitHubBotConfiguration)).Get<GitHubBotConfiguration>();
var discordBotConfig = builder.Configuration.GetSection(nameof(DiscordBotConfiguration)).Get<DiscordBotConfiguration>();

// var watchdogConfig = builder.Configuration.GetSection(nameof(WatchDogConfiguration)).Get<WatchDogConfiguration>();

builder.Services.AddDbContext<TallyContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
 
builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<TallyContext>();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// builder.Services.AddWatchDogServices(opt =>
// {
//     opt.IsAutoClear = true;
//     opt.ClearTimeSchedule = WatchDogAutoClearScheduleEnum.Monthly;
// });
 
builder.Services.AddTelegram(telegramBotConfig);
builder.Services.AddTwitter(twitterBotConfig);
builder.Services.AddGitHub(gitHubBotConfig);
builder.Services.AddDiscord(discordBotConfig);
builder.Services.AddScoped<ChannelWrapper>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// app.UseWatchDogExceptionLogger();
// app.UseWatchDog(options =>
// {
//     options.WatchPageUsername = watchdogConfig.WatchPageUsername;
//     options.WatchPagePassword = watchdogConfig.WatchPagePassword;
// });

app.UseEndpoints(endpoints =>
{
    endpoints.MapTelegramWebHooks(telegramBotConfig);
    endpoints.MapGitHubWebHooks(gitHubBotConfig);
    // endpoints.MapControllers();
});
app.MapRazorPages();
app.MapHub<TallyHub>("/tally");

app.Run();