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
| GitHub (Polls)       | No<sup>3</sup> | ?                      | Yes       | [GitHub Discussion Polls](https://github.blog/changelog/2022-04-12-discussions-polls/) | 8                         | ?                          | ?                              | ?                            |
| Discord              | No             | ?                      | ?         | ?                                                                                      | ?                         | ?                          | ?                              | ?                            |
| Web                  | No             | No                     | Yes       | HTML form                                                                              | Infinite                  | HTML form                  | Disable voting                 | Delete poll                  |

> <sup>1</sup> User-identifiers are colected via some voting channels to identify duplicate votes, and to enable vote editing and deletion. Twitter does not offer any of these features.

> <sup>2</sup> Telegram has an anonymous polls feature which I'm not using because it makes it significantly harder to monitor poll results.

> <sup>3</sup> I'm (mis)using GitHub Discussions for polls because the Discussion Polls feature is not yet available via the API.

## Features
- [x] Creating polls.
- [ ] Concluding polls.
- [ ] Deleting polls.
- [x] Voting.
- [x] Changing votes.
- [x] Deleting polls.
- [x] Real-time updates from channels that support it.
- [x] Automatic results refresh for channels that do not support real-time events. 
- [ ] Real-time result view updates via SignalR.

## Screenshots

```csharp 
// TODO
```

## Running locally

0. Prerequisites
- Git
- .NET 6 SDK (Set up for local HTTPS development).
- Ngrok
- All applicable credentaials (see [appsettings.json](https://github.com/sixpeteunder/tally/tree/main/Web/appsetings.json)).

1. Clone the repo:

```bash
git clone https://github.com/sixpeteunder/tally.git
cd Tally
```

Or (GitHub CLI):

```bash
gh repo clone sixpeteunder/tally
cd Tally
```

2. Start ngrok, pointing to port 8443:

> ⚠️ Telegram requires the use of port `8443`, so most others won't work.

```bash 
ngrok http https://127.0.0.1:8443
```

3. Fill in required credentials and host (ngrok) address:

> The default values provided in appsettings.json are fake sample credentials representing what each field expects.

```bash 
nano ./Web/appsettings.json
```

Or (Sublime Text):

```bash
subl ./Web/appsettings.json
```

Or (Visual Studio Code):

```bash
code ./Web/appsettings.json
```

4. Navigate to https://127.0.0.1:8443 in your favourite browser.

```bash
xdg-open https://127.0.0.1:8443
```

Or (Windows):

```cmd
start https://127.0.0.1:8443
```

## Webhooks

> The application logs relevant webhook events, but GitHub and [webhook.site](https://webhook.site) also provide excellent tooling for testing webhooks.

You do not need to set up anything webhook-related. The application will automatically set up webhooks on Telegram and GitHub for you, and will dispose of them when it is shutting down.

