using Microsoft.EntityFrameworkCore;
using Octokit.Webhooks.AspNetCore;
using Web.Channels;
using Web.Data;
using Web.Models.Configuration;
using Web.Services;
using User = Web.Models.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var telegramBotConfig = builder.Configuration.GetSection(nameof(TelegramBotConfiguration)).Get<TelegramBotConfiguration>();
var twitterBotConfig = builder.Configuration.GetSection(nameof(TwitterBotConfiguration)).Get<TwitterBotConfiguration>();
var gitHubBotConfig = builder.Configuration.GetSection(nameof(GitHubBotConfiguration)).Get<GitHubBotConfiguration>();

builder.Services.AddDbContext<TallyContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
 
builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<TallyContext>();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddRazorPages();
 
builder.Services.AddTelegram(telegramBotConfig);
builder.Services.AddTwitter(twitterBotConfig);
builder.Services.AddGitHub(gitHubBotConfig);
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

app.UseEndpoints(endpoints =>
{
    // Configure custom endpoint per Telegram API recommendations:
    // REF: https://core.telegram.org/bots/api#setwebhook
    endpoints.MapControllerRoute(
        name: "webhooks.telegram", 
        pattern: $"webhooks/telegram/{telegramBotConfig.BotToken}", 
        defaults: new { controller = "TelegramWebhook", action = "Post" }
        );
    endpoints.MapGitHubWebhooks("/webhooks/github", gitHubBotConfig.WebHookSecret);
    endpoints.MapControllers();
});
app.MapRazorPages();

app.Run();