using LinqToTwitter;
using LinqToTwitter.OAuth;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Web.Channels;
using Web.Data;
using Web.Models.Configuration;
using Web.Services;
using User = Web.Models.User;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var telegramBotConfig = builder.Configuration.GetSection("TelegramBotConfiguration").Get<TelegramBotConfiguration>();
var twitterBotConfig = builder.Configuration.GetSection("TwitterBotConfiguration").Get<TwitterBotConfiguration>();

// Telegram Webhooks
builder.Services.AddHostedService<ConfigureWebhook>();
builder.Services.AddHttpClient("tgwebhook")
    .AddTypedClient<ITelegramBotClient>(client => new TelegramBotClient(telegramBotConfig.BotToken, client));

// Twitter Context
builder.Services.AddScoped(_ => new TwitterContext(new SingleUserAuthorizer
{
    CredentialStore = new SingleUserInMemoryCredentialStore
    {
        ConsumerKey = twitterBotConfig.ConsumerKey,
        ConsumerSecret = twitterBotConfig.ConsumerSecret,
        AccessToken = twitterBotConfig.AccessToken,
        AccessTokenSecret = twitterBotConfig.AccessTokenSecret
    }
}));

builder.Services.AddDbContext<TallyContext>(options => options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<TallyContext>();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddRazorPages();

builder.Services.AddScoped<HandleUpdateService>();

builder.Services.AddScoped<TelegramChannel>();
builder.Services.AddScoped<TwitterChannel>();

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
    var defaults = new {controller = "Webhook", action = "Post"};
    endpoints.MapControllerRoute(name: "tgwebhook", pattern: $"bot/{telegramBotConfig.BotToken}", defaults: defaults);
    endpoints.MapControllers();
});
app.MapRazorPages();

app.Run();