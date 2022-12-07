# ✨ Little Starter™ Config Yaml
Create or edit a file name `LittelStarterConfig.yaml` placed in the same directory as the executable `LittleStarter.exe` of this app.

The file's content needs to be set to something like this:
```yaml
version: 1.0

actions:

- name: Powershell
  filename: pwsh.exe
  isSelectedByDefault: true

- name: Text File Editing
  filename: C:\path\to\your\text\file.txt
  verb: open
  useShellExecute: true
  isSelectedByDefault: true

log:
  path: .\log

```

`version` needs to be specified as `1.0`.

`actions` is the list of actions which will be presented and executed by the App.

Note:
You can edit this file while the App is running, to get live feedback.

## Actions

Each Action object has to have a `name` and a `filename`.

The `name` is a human-readable name, only used for displaying purpose in the app.

The `filename` is the path or filename to the executable or file to be opened.
For files, it is recommended to specify the full file system path.
For executables, if no path is specified, the default search behavior of the operating system will try to find the executable.

The variable `isSelectedByDefault` is a boolean value, and controls whether or not this action will be selected by default when the app starts.

The variable `icon` specifies the path to an image file name to be used as icon.
You cannot specify an executable or a dll file.
You must use and ico file or any other square image file.
Recommended formats are `.ico` and `.png` for their capability of transparency.

Further variables control details of how the processes will be spawned by the App:

* `ArgumentList`
* `WorkingDirectory`
* `Verb`
* `UseShellExecute`

You can find details on how those work in the [Microsoft online documentation of `System.Diagnostics.ProcessStartInfo`](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo?view=net-6.0).

## Sample Action - Start a Program
The simplest action is to just invoke a program, similar to what most application shortcuts do:

```yaml
- name: Powershell
  filename: pwsh.exe
```

Give it a `name` and specify the `filename` of the executable you want to start.
Either us the full path or just the file name and let the operating system find the executable.

Relative paths might work, but they will be evaluated to the App's own working directory, which is not necessarily the directory the App's executable is stored in.
Relative paths are thus often not intuitive and are thus not recommended.

## Sample Action - Open a File
You can open a file in the default application associated with the file type:

```yaml
- name: Text File Editing
  filename: C:\path\to\your\text\file.txt
  verb: open
  useShellExecute: true
```

The `filename` should be an absolute path.

The `verb` triggers the `open` behavior of the respective file type.

`useShellExecute: true` is required to force spawning the new process from the operating system's shell, instead of directly spawning the process from the App's process.
This is required to have the `verb` being evaluated.
Without it `filename` is always assumed to be an executable itself.

## Log Files
You can include a log path to enable writing log files:

```yaml
log:
  path: .\log
```

A relative path will be evaluated to the directory hosting the app's executable files.

Only the ten newest log files will be kept.
Older log files will be automatically deleted.
