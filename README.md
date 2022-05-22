# tally
Real-time polling application that integrates with Telegram, Twitter, GitHub and Discord.

Tally makes use of REST APIs, polling, webhooks, GraphQL and websockets to create polls across several channels and aggregate results to be viewable in one central location.

Creating a poll on Tally automatically creates it on Tally itself, as well as on Telegram, Twitter, GitHub and Discord, and automatically tracks results - in real-time - across all channels. 
Concluding the poll blocks additional results from coming in, and deleting the poll deletes it across all channels.

## Implementation Details

| Channel              | Implemented?   | Anonymous?<sup>1</sup> | Is Vote Editable? | Poll Implementation                                                                    | Maximum number of Options | Voting Implementation            | "Conclude Poll" Implementation | "Delete Poll" Implementation |
|----------------------|----------------|------------------------|-------------------|----------------------------------------------------------------------------------------|---------------------------|----------------------------------|--------------------------------|------------------------------|
| Telegram             | Yes            | No<sup>2</sup>         | Yes               | [Telegram Polls](https://telegram.org/blog/polls-2-0-vmq)                              | 10                        | Webhooks, polling                | Native "Stop poll"             | Delete message               |
| Twitter              | Yes            | Yes                    | No                | [Twitter Polls](https://help.twitter.com/en/using-twitter/twitter-polls)               | 4                         | Polling                          | Timeout                        | Delete tweet                 |
| GitHub (Discussions) | Yes            | No                     | Yes               | [GitHub Discussions](https://docs.github.com/en/discussions)                           | Infinite                  | Webhooks, GraphQL, polling       | Native "Lock conversation"     | Delete discussion            |
| GitHub (Polls)       | No<sup>3</sup> | ?                      | Yes               | [GitHub Discussion Polls](https://github.blog/changelog/2022-04-12-discussions-polls/) | 8                         | ?                                | Native "Lock conversation"     | Delete discussion            |
| Discord              | Yes            | No                     | Yes               | Reaction polls                                                                         | Infinite<sup>4</sup>      | Webhooks, websockets<sup>5</sup> | Remove reaction                | Delete message               |
| Web                  | Yes            | No                     | Yes               | HTML page                                                                              | Infinite                  | Websockets (via SignalR)         | Disable voting                 | Delete poll                  |

> <sup>1</sup> User-identifiers are collected when voting via some channels to identify duplicate votes, and to enable vote editing and deletion. Twitter does not offer any of these features.

> <sup>2</sup> Telegram has an anonymous polls feature which I'm not using because it makes it significantly harder to monitor poll results.

> <sup>3</sup> I'm (mis)using GitHub Discussions for polls because the Discussion Polls feature is not yet available via the API.

> <sup>4</sup> Options in reaction polls are limited by the number of available reactions.

> <sup>5</sup> Discord provides support of outgoing webhooks ([Interactions](https://discord.com/developers/docs/interactions/receiving-and-responding)), but the Interaction URL endpoint must be set manually. The alternative is to maintain a constant websocket connection ([Gateway](https://discord.com/developers/docs/topics/gateway)) to Discord servers.

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

> **Warning**
> [Telegram requires the use of port `443`, `80`, `88` or `8443`](https://core.telegram.org/bots/webhooks), so most others won't work.

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

> **Note**
> I use MSBuild to transpile TypeScript and bundle the resultant JS. This process is completely transparent and needs no additional setup.
> See [TypeScript](#typescript) below for more information.

```shell
# Get rid of old built assets (Optional, recommended) 
dotnet clean 

# Build new assets and run the app
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

## Webhooks and websockets

> The application logs relevant webhook events, but GitHub and [webhook.site](https://webhook.site) also provide excellent tooling for testing webhooks.

> I also use [WatchDog](https://nuget.org/packages/WatchDog.NET) to monitor HTTP activity. The dashboard is accessible at https://localhost:8443/watchdog.

You do not need to set up anything webhook-related. The application will automatically set up webhooks on Telegram and GitHub for you, and will dispose of them when it is shutting down.

Similarly, it will also set up a websocket ([Gateway](https://discord.com/developers/docs/topics/gateway)) connection to Discord on startup and dispose of it during shutdown.

The web channel also uses websockets, via [SignalR](https://dotnet.microsoft.com/en-us/apps/aspnet/signalr), to submit and retract votes.

## Channel Poll Identifiers

Tally stores a pair of "Channel Poll Identifiers" for each poll on each channel, that allows it to find and manage the linked poll.

> A "Primary" and "Auxiliary" identifier is stored for each channel because the relevant APIs sometimes require different identifiers for different actions.

The table below shows exactly what is stored for each platform.

| Channel              | Primary Identifier   | Auxiliary Identifier |
|----------------------|----------------------|----------------------|
| Telegram<sup>1</sup> | Message ID           | Poll ID              |
| Twitter              | Tweet ID             | Tweet ID             |
| GitHub               | Discussion Node ID   | Discussion Number    |
| Discord<sup>2</sup>  | Message ID           | Message ID           |
| Web                  | Locally Generated ID | Locally Generated ID |

> <sup>1</sup> For Telegram, the Chat ID is also stored, to identify the Chat the poll message was sent to.

> <sup>2</sup> Similarly, for Discord, the Server ID and Channel are stored.

## TypeScript

> **Note**
> TypeScript is automatically transpiled and bundled when you run `dotnet build`.

Anything in the `Resources/scripts` directory will automatically be transpiled into JavaScript and stored under `wwwroot/js` (maintaining folder structure).

To additionally bundle the generated JavaScript file with Webpack, add it to the entry key in `webpack.config.js`, and a bundled file named [filename].g.js will be generated alongside the original file.
