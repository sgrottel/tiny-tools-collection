# ToggleDisplay

<!-- START INCLUDE IN PACKAGE README -->
## Preparation of Display Configuration
In order to enable/disable/toggle a display, the resulting configuration must be known to windows.
E.g. if you have three display enabled and want to disable one of them, you must have manually set that configuration with two displays before, using the Windows OS settings.
The reason for this is, that this tool does not "place" displays, but only enables or disables them.
The placement is stored, selected and restored by the Windows OS.

## Referencing Displays
When calling `ToggleDisplay.exe LIST` display will be shown with three identifiers:

```
\\.\DISPLAY1 -> DELL P2415Q (\\?\DISPLAY#DELA0BE#5&8fda59d&0&UID257#{e6f07b5f-ee97-4a90-b076-33f57bf4eaa7})  [enabled]
    (w: 3840; h: 2160; x: 0; y: 0)
```

First: \
`\\.\DISPLAY1` is the display name.

This one is **dynamically assigned by Windows** and not persistent.

It is **not recommended** to use this identifier in scripts.

Second: \
`DELL P2415Q` is the device name.

This is the hardware device name of your display and will not change.
This can be a easily, human-readable name.

However, if you have multiple displays of the same type attached to your system, you will likely have multiple displays with the same device name, and therefore this entry will **not be unique**.

Third:
`\\?\DISPLAY#DELA0BE#5&8fda59d&0&UID257#{e6f07b5f-ee97-4a90-b076-33f57bf4eaa7}` is the device path.

This path represents the graphics adapter, the connector, and the display device.
As such this path is as close to the actual hardware as it gets.

For use in scripts, this is the **recommended** way of identifying a display to enable/disable/toggle.
