# Chatter
Internal server-less chat client for communicating between development machines.
Currently, only runs in a console and only provides conversation between all running clients indiscriminantly.

## Build Instructions
- Requires .NET Core 3.0 SDK
- Visual Studio 2019
- Startup project is ChatterConsole

## Use Instructions
- Run the EXE
- Start chatting in the console window

## Commands
The following is a list of commands that may be used in the console.

| Command                    | Description                                        |
|----------------------------|----------------------------------------------------|
| `/quit`, `/q`              | Quit applicaiton                                   |
| `/pm [username] [message]` | Send a private message to  *username*              |
| `/users`                   | List active users                                  |
| `/help`, `/h`              | Display helpful informaion                         |
| `/name [username]`         | Change current username to *username*              |
| `/multicast [ip]`          | Change Multicast IP address to *ip*                |
| `/port [port]`             | Change port number to *port*                       |
