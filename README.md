# WinLZW

This repo is a Windows C#/WPF application for the LZW compression functionality found in my LZW repo.
The most recent build can be found in the Downloads section of this repo.

The core functionality is the same as the LZW repo; after modifying the original code slightly
to agree with the C++/CLI compiler and adding a managed wrapper for the unmanaged LZW code, the
original code could be called from C#.

## Compiling

Open the solution WinLZW.sln in Visual Studio to build it.

Note that the WinLZW solution contain an executable assembly called RegistryAdmin as well as the WinLZW
project. This very small executable creates or removes a registry key that adds an option to the Windows
Explorer context menu to compress or decompress files. Editing the registry can only be done by an
administrator, so this registry-editing assembly runs as an administrator while bulk of WinLZW does not.

## Running

Run WinLZW with WinLZW.exe. In the main window, files can be compressed and decompressed by selecting a file
and clicking "Go". Files are selected with an open-file-dialog or by dragging them onto the window.

An options dialog can be accessed from the menu bar. Here you can change the way WinLZW encodes files
and create or remove a registry key to enable WinLZW functionality from the Windows shell context menus.