# tally
Real-time polling application that integrates with Telegram, Twitter, GitHub and Discord.

Tally makes use of REST APIs, polling, webhooks, GraphQL and websockets to create polls across several channels and aggregate results to be viewable in one central location.

Creating a poll on Tally automatically creates it on Tally itself, as well as on Telegram, Twitter, GitHub and Discord, and automatically tracks results - in real-time - across all channels. 
Concluding the poll blocks additional results from coming in, and deleting the poll deletes it across all channels.

## Implementation Details

| Channel              | Implemented?   | Anonymous?<sup>1</sup> | Editable? | Poll Implementation                                                                    | Maximum number of Options | Voting Implementation      | "Conclude Poll" Implementation | "Delete Poll" Implementation |
|----------------------|----------------|------------------------|-----------|----------------------------------------------------------------------------------------|---------------------------|----------------------------|--------------------------------|------------------------------|
| Telegram             | Yes            | No<sup>2</sup>         | No        | [Telegram Polls](https://telegram.org/blog/polls-2-0-vmq)                              | 10                        | Webhooks, polling          | Native "Stop poll"             | Delete message               |
| Twitter              | Yes            | Yes                    | No        | [Twitter Polls](https://help.twitter.com/en/using-twitter/twitter-polls)               | 4                         | Polling                    | Timeout                        | Delete tweet                 |
| GitHub (Discussions) | Yes            | No                     | Yes       | [GitHub Discussions](https://docs.github.com/en/discussions)                           | Infinite                  | Webhooks, GraphQL, polling | Native "Lock conversation"     | Delete discussion            |
| GitHub (Polls)       | No<sup>3</sup> | ?                      | Yes       | [GitHub Discussion Polls](https://github.blog/changelog/2022-04-12-discussions-polls/) | 8                         | ?                          | Native "Lock conversation"     | Delete discussion            |
| Discord              | No             | No                     | Yes       | Reaction polls                                                                         | Infinite<sup>4</sup>      | Webhooks, Websockets       | Remove reaction                | Delete message               |
| Web                  | No             | No                     | Yes       | HTML form                                                                              | Infinite                  | HTML form                  | Disable voting                 | Delete poll                  |

> <sup>1</sup> User-identifiers are collected via some voting channels to identify duplicate votes, and to enable vote editing and deletion. Twitter does not offer any of these features.

> <sup>2</sup> Telegram has an anonymous polls feature which I'm not using because it makes it significantly harder to monitor poll results.

> <sup>3</sup> I'm (mis)using GitHub Discussions for polls because the Discussion Polls feature is not yet available via the API.

> <sup>4</sup> Discord

## Features
- [x] Creating polls.
- [x] Concluding polls.
- [x] Deleting polls.
- [x] Voting.
- [x] Changing votes.
- [x] Deleting polls.
- [x] Real-time updates from channels that support it.
- [x] Automatic results refresh for channels that do not support real-time events. 
- [x] Real-time result view updates via SignalR.
- [ ] Home-page feed
- [ ] Channel selection
- [ ] Poll configuration
- [ ] Channel-specific configuration
- [ ] Poll scheduling.
- [ ] OAuth Apps

## Screenshots

```csharp 
// TODO
```

## Running locally

0. Prerequisites
- Git
- [.NET 6 SDK](https://get.dot.net/6) (Set up for local HTTPS development).
- Ngrok
- All applicable credentials (see [appsettings.json](https://github.com/sixpeteunder/tally/tree/main/Web/appsetings.json)).

1. Clone the repo:

```shell
git clone https://github.com/sixpeteunder/tally.git
cd Tally
```

Or (GitHub CLI):

```shell
gh repo clone sixpeteunder/tally
cd Tally
```

2. Start ngrok, pointing to port 8443:

> ⚠️ [Telegram requires the use of port `443`, `80`, `88` or `8443`](https://core.telegram.org/bots/webhooks), so most others won't work.

```shell 
ngrok http https://localhost:8443
```

3. Using your favourite editor, fill required credentials in secrets.json, and host (ngrok) address in appsettings.json:

> The default values provided in `secrets.sample.json` are fake sample credentials representing what each field expects.

Tally uses the [.NET Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) tool to manage secrets during development. To get started, make a copy of secrets.sample.json, populate it with the required secrets, and then persist them to Secret Manager.

```shell
cp secrets.sample.json secrets.json
code ./Web/secrets.json
code ./Web/appsettings.json
cat ./secrets.json | dotnet user-secrets set
```

Or (Windows):

```cmd
copy secrets.sample.json secrets.json
code .\Web\secrets.json
code .\Web\appsettings.json
type .\secrets.json | dotnet user-secrets set
```
4. Run the program

> Moving to TypeScript has complicated the build process a little. 
> You will now need to ensure TypeScript files are built and bundled before running the app, while I figure out a better way to do this.
> (Looking into MS Build and npm watch.)

> These commands must be run in exactly this order, because TypeScript compilation is handled by the MS Build pipeline, and bundling is handled by webpack.

```shell
# Get rid of old built assets (Optional, recommended) 
dotnet clean 
npm run clean

# Build new assets
dotnet build
npm run build
```
Run the app

```shell
dotnet run
```

5. Navigate to https://localhost:8443 in your favourite browser.

```shell
xdg-open https://localhost:8443
```

Or (Windows):

```cmd
start https://localhost:8443
```

## Webhooks

> The application logs relevant webhook events, but GitHub and [webhook.site](https://webhook.site) also provide excellent tooling for testing webhooks.

> I also use [WatchDog](https://nuget.org/packages/WatchDog.NET) to monitor HTTP activity. The dashboard is accessible at https://localhost:8443/watchdog.

You do not need to set up anything webhook-related. The application will automatically set up webhooks on Telegram and GitHub for you, and will dispose of them when it is shutting down.

## Channel Poll Identifiers

Tally stores a pair of "Channel Poll Identifiers" for each poll on each channel, that allows it to find and manage the linked poll.

> A "Primary" and "Auxiliary" identifier is stored for each channel because the relevant APIs sometimes require different identifiers for different actions.

The table below shows exactly what is stored for each platform.

| Channel              | Primary Identifier   | Auxiliary Identifier |
|----------------------|----------------------|----------------------|
| Telegram<sup>1</sup> | Message ID           | Poll Id              |
| Twitter              | Tweet ID             | Tweet ID             |
| GitHub               | Discussion Node ID   | Discussion Number    |
| Discord              | ?                    | ?                    |
| Web                  | Locally Generated ID | Locally Generated ID |

<sup>1</sup> For Telegram, the Chat ID is also stored, to identify the Chat the poll message was sent to.
