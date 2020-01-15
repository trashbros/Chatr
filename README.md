# Chatr
Internal server-less chat client for communicating between development machines.
Currently, only runs in a console and only provides conversation between all running clients indiscriminantly.

## Build Instructions
- Requires .NET Core 3.0 SDK
- Visual Studio 2019
- Startup project is ChatrConsole

## Usage Instructions
- Run the ChatrConsole executable
- Start chatting in the console window

## Commands
The following is a list of commands that may be used in the console.

| Command                    | Description                                        |
|----------------------------|----------------------------------------------------|
| `/help`, `/h`              | Display helpful informaion                         |
| `/quit`, `/q`              | Quit applicaiton                                   |
| `/pm [username] [message]` | Send a private message to *username*               |
| `/users`                   | List active users                                  |
| `/name [username]`         | Change current username to *username*              |
| `/multicast [ip]`          | Change Multicast IP address to *ip*                |
| `/port [port]`             | Change port number to *port*                       |
| `/channel [channel]`       | Change active channel to *channel*                 |
| `/add [channel settings]`  | Add a new channel using *channel settings*         |
| `/list`                    | Get a listing of connected channels                |
| `/info [channel]`          | Display information about *channel*                |
| `/connect [channel]`       | Connect to *channel*                               |
