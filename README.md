# AUREON Notepad

<img width="793" height="596" alt="image" src="https://github.com/user-attachments/assets/5cd0cbdc-f5dd-4a77-baad-3416c14bcc93" />

**English** | [Українська](README.uk.md)

![Platform](https://img.shields.io/badge/platform-Windows-0078D6?logo=windows)
![.NET](https://img.shields.io/badge/.NET-6%2B-512BD4?logo=dotnet)
![License](https://img.shields.io/badge/license-MIT-green)

A fast, lightweight and good-looking text editor for Windows, written in C# with Windows Forms.
It replaces the classic Notepad with a custom dark interface, a built-in find & replace bar,
a distraction-free "Zen mode" and automatic crash recovery, while staying simple and plain-text only.

<!-- Add a screenshot to docs/screenshot.png and uncomment the line below -->
<!-- ![Screenshot](docs/screenshot.png) -->

## Features

- **Custom borderless UI** with its own title bar, menus and status bar
- **Theming**: several theme presets, accent colors, text colors and a custom text color picker
- **Zen mode** (`F11`): hide everything except the text, `Esc` to return
- **Find & replace bar**: incremental search, match counter, case-sensitive option,
  find next / previous, wrap-around, replace one / replace all (undoable with `Ctrl+Z`)
- **Go to line**
- **Duplicate line**
- **Smart encoding detection**: UTF-8 (with and without BOM), UTF-16 LE/BE,
  and automatic fallback to Windows-1251 for old Cyrillic files
- **Autosave recovery**: a backup copy is written every 30 seconds;
  after a crash the editor offers to restore your unsaved text
- **Status bar**: line and column, selection length, word and character count, zoom level, encoding
- **Plain-text paste**: formatting is stripped on paste, rich-text shortcuts (`Ctrl+B/I/U`, …) are disabled
- **Drag & drop** files into the window, or open a file via *Open with…* / the command line
- **Printing** with page setup and multi-page output
- **Zoom**, word wrap and font selection
- **Remembers your settings**: window size, theme, font, zoom, word wrap and status bar visibility
- Unsaved-changes protection when opening, creating or closing

## Requirements

- Windows 10 or Windows 11
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or newer (to build from source)

## Getting started

### Build from source

```bash
git clone https://github.com/<CapitanSashko>/<repository-name>.git
cd <repository-name>
dotnet build -c Release
```

Run it:

```bash
dotnet run -c Release
```

### Publish a standalone build

```bash
dotnet publish -c Release -r win-x64 --self-contained false -o publish
```

The result is in the `publish` folder. To run it without installing .NET, use `--self-contained true` instead
(the output is larger).

### Open files from the command line

```bash
NotePad.exe "C:\path\to\file.txt"
```

## Usage

| Action | Shortcut |
| --- | --- |
| Zen mode on / off | `F11` |
| Close search bar / leave Zen mode | `Esc` |
| Paste as plain text | `Ctrl+V` |
| Undo | `Ctrl+Z` |

All other commands are available from the menu, where their shortcuts are shown.

## Where settings are stored

Settings and recovery copies live in `%LOCALAPPDATA%\NotePadPro`:

- `settings.ini` — theme, font, zoom, window size
- `recovery_<id>.txt` — autosaved backup of unsaved text (removed on normal exit)

## Contributing

Issues and pull requests are welcome. For larger changes please open an issue first
to discuss what you would like to change.

## License

Released under the [MIT License](LICENSE).
