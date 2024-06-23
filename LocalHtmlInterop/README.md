# LocalHtmlInterop
Utility to interact with locally generated, file-hosted, and locally viewed html pages.
Custom protocol url schema to trigger functionality from within the Html pages in a system-consistent manner.
<!-- START INCLUDE IN PACKAGE README -->

Workflow:
* Fundamentally, a local html page can call a (native) command to be executed on the local computer, by issuing a custom protocol url.
* If the command is known, the respective application is triggered, and it's output and exit code are captured.
* The app opens a websocket server and accepts a connection with an id matching the issued custom protocol url
  * The websocket port is configured globally.
  * The websocket host can timeout if no incoming connection is established in time.
* The html page quickly opens the websocket connection (avoid timeout) and waits to receive the results.
* When the child process completes, filtered results are sent via the websocket connection to the html page.
* The html page can then react, usually, by refreshing itself or displaying a message.
* The core app hosting the websocket is a single-instance application and can host multiple requests at the same time.
  * This is necessary because the hosted websocket server must listen on the one specific port.


## Configuration

`PORT` to be used by the app to host the websocket server on.

`TIMEOUT` to be used by the app's websocket server to wait for incoming connections.

`COMMANDS` configured commands which can be invoked via the custom URL protocol.

Trying to execute unknown commands will result in an error.

Commands also configure a list of parameter names.
Only those parameters will be used when executing the command.
Additional parameters will be ignored.
