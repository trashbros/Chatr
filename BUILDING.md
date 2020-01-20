# Building Chatr

Chatr can be built from source using the following methods:
- Visual Studio 2019 with .NET Core SDK 3.0 or higher
- .NET Core SDK 3.0 or higher command-line interface (CLI)
- Docker

## Visual Studio

1. Open `src\Chatr.sln` with Visual Studio.
2. Choose **Release** from the **Solution Configurations** list on the toolbar.
3. **Build** > **Rebuild Solution**
4. Find the built binaries in the `ChatrConsole\bin\Release\netcoreapp3.0\` directory.

## .NET Core SDK command-line interface (CLI)

1. Open a console and navigate to the Chatr `src` directory.
2. `dotnet publish -c release -o out --self-contained false`
3. Find the built binaries in the `out` directory.

## Docker

### Docker on Windows using Linux Containers

1. Open PowerShell and navigate to the Chatr `src` directory.
2. `docker run --rm -v ${pwd}:/app -w /app mcr.microsoft.com/dotnet/core/sdk:3.0 dotnet publish -c release -o out --self-contained false -r win-x64`
3. Find the built binaries in the `out` directory.

### Docker on Windows using Windows Containers

1. Open PowerShell and navigate to the Chatr `src` directory.
2. `docker run --rm -v ${pwd}:/app -w /app mcr.microsoft.com/dotnet/core/sdk:3.0 dotnet publish -c release -o out --self-contained false`
3. Find the built binaries in the `out` directory.

### Docker on Linux

1. Open a console and navigate to the Chatr `src` directory.
2. `docker run --rm -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/core/sdk:3.0 dotnet publish -c release -o out --self-contained false`
3. Find the built binaries in the `out` directory.

### Docker on macOS

1. Open a console and navigate to the Chatr `src` directory.
2. `docker run --rm -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/core/sdk:3.0 dotnet publish -c release -o out --self-contained false -r osx-x64`
3. Find the built binaries in the `out` directory.
