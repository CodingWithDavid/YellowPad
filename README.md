# YellowPad

YellowPad is a local-first todo application for Windows, built with WPF and a Blazor Hybrid user interface. It looks and behaves like a simple yellow legal pad while providing native window controls, persistent SQLite storage, keyboard-friendly editing, completed-task history, and Windows speech recognition.

## Features

- Create, complete, edit, and delete tasks.
- Check spelling while entering or editing task titles.
- Double-click a task title to edit it in place.
- Right-click a task title to set its text to red, blue, green, pink, or the default color.
- Use Enter or F2 to edit a focused task and Escape to cancel an edit.
- View completed tasks with their local completion dates.
- Delete completed tasks or clear the entire pad from the application menu.
- Dictate a task and create it with a spoken command.
- Keep every change in a local SQLite database between sessions.
- Resize, minimize, maximize, and close the custom borderless Windows shell.
- Display the YellowPad application icon in the window and taskbar.

## Requirements

- Windows 10 version 1809 or later
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`global.json` accepts SDK `10.0.300` or a newer .NET 10 feature band)
- Microsoft Edge WebView2 Runtime
- Visual Studio with the **.NET desktop development** workload, or the .NET CLI
- A Windows desktop speech recognizer and microphone for voice input

## Quick start

### Visual Studio

1. Open `YellowPad.sln`.
2. Set `YellowPad` as the startup project if it is not already selected.
3. Press F5.

### .NET CLI

```powershell
dotnet restore YellowPad.sln
dotnet run --project YellowPad\YellowPad.csproj
```

## Using YellowPad

### Tasks

1. Type a title in the **New task** row and press Enter.
2. Select the circle beside a task to mark it complete or incomplete.
3. Double-click a title to edit it. Press Enter to save or Escape to cancel.
4. Select the × button to delete a task.
5. Right-click a task title and choose a text color to mark its priority.

Task titles are limited to 500 characters. On startup, the main pad hides tasks that were already completed; tasks completed during the current session remain visible until the next restart. All completed tasks remain saved in the completed-task history.

Right-click a misspelled word while entering or editing a task to see spelling suggestions.

### Voice input

1. Select the microphone button beside the new-task field.
2. Say the task title followed by **Enter**, **Create task**, or **Create new task**.

For example, saying “Buy milk, Enter” creates a task named “Buy milk.” YellowPad uses the installed Windows speech recognizer and the default audio input device. The microphone button stops listening after a task is created.

### Completed tasks

Open the ••• menu and select **View completed tasks**. The dialog lists completed titles and the date each task was completed, converted from stored UTC time to the computer's local date.

The same menu provides options to delete completed tasks, delete every task after confirmation, and view application information.

## Data storage

YellowPad stores its SQLite database at:

```text
%LocalAppData%\YellowPad\yellowpad.db
```

The directory and database are created automatically on first launch. No account or remote database is required.

## Commands

| Command | Purpose |
| --- | --- |
| `dotnet restore YellowPad.sln` | Restore NuGet packages |
| `dotnet build YellowPad.sln` | Build the application and tests |
| `dotnet run --project YellowPad\YellowPad.csproj` | Run YellowPad |
| `dotnet test YellowPad.sln` | Run the complete test suite |
| `dotnet publish YellowPad\YellowPad.csproj -c Release` | Create a Release publish output |

## Architecture

YellowPad uses a small native WPF host around a Blazor WebView. Razor components own the user interface, an injected service layer owns task operations, and Entity Framework Core persists tasks to SQLite.

```text
WPF window
  └─ BlazorWebView
      └─ Razor components
          └─ ITodoService
              └─ EF Core + SQLite
```

Voice input is implemented separately through `System.Speech`, which keeps microphone and recognizer lifecycle concerns out of the Razor components.

### Project structure

| Path | Responsibility |
| --- | --- |
| `YellowPad/Components` | Razor UI and small presentation helpers |
| `YellowPad/Services` | Task, voice-command, and native-window services |
| `YellowPad/Data` | EF Core context, database path, and initialization |
| `YellowPad/Models` | Persisted task model |
| `YellowPad/wwwroot/css/yellowpad.css` | Legal-pad layout, dialogs, responsive styling, and interaction states |
| `YellowPad/Assets/YellowPad.ico` | Application and taskbar icon |
| `YellowPad.Tests` | Unit and SQLite-backed service tests |

## Troubleshooting

### The application opens without web content

Install or repair the Microsoft Edge WebView2 Runtime, then restart YellowPad.

### Voice input is unavailable

- Confirm that a microphone is connected and selected as the Windows default input device.
- Confirm that Windows has a desktop speech recognizer installed for the current language.
- Close other applications that may have exclusive access to the microphone.
- Restart YellowPad after changing audio or speech settings.

## Framework references

- [Build a WPF Blazor Hybrid app](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/tutorials/wpf?view=aspnetcore-10.0)
- [EF Core DbContext factories](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#use-a-dbcontext-factory)
- [Microsoft.EntityFrameworkCore.Sqlite](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)
- [System.Speech recognition APIs](https://learn.microsoft.com/en-us/dotnet/api/system.speech.recognition)
