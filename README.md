# Chatter
Internal server-less client for communicating between development machines. Currently uses a console log and only provides conversation between all running clients indiscriminantly.

## Build Instructions
- Requires .NET Core 3.1 SDK
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

    


## To-Do
- Set up as background console notifications rather than full control app
- Able to connect in and get notifications on repo pushes
- Setup a release pipeline
- Console text coloring for message push?