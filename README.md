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

## Special Commands
- Application Quit
  
    `/q`
    `/quit`

- Private Message

    `/pm [username] [message]`

- Active User List
  
    `/users`

- Help

    `/help` or `/h`

- Name Change

    `/name [new name]`

- Multicast IP Change

    `/multicast [new ip]`

- Port Change

    `/port [new port]`
