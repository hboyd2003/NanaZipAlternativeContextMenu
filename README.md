# Alternative Context Menu entries for [NanaZip](https://github.com/M2Team/NanaZip)
A flat alternative to the default [NanaZip](https://github.com/M2Team/NanaZip) context menu using [ShellAnything](https://github.com/end2endzone/ShellAnything). It also adds an entry to decompress a file and then recycle the file.

## Why?
Currently [NanaZip](https://github.com/M2Team/NanaZip) context menu items are inside of a sub-context menu, which I find slow. Additionally, the developer has no intention of adding an option to not place them in a sub menu. 

## How?
Using [ShellAnything](https://github.com/end2endzone/ShellAnything) you can easily implement custom context menu entries using XML, however, it is not all powerful and to create a better implementation of the context menu entries I have a created a simple companion app.

## Problems with the current implementation
The issues surrounding the current implementation are due to the way that privileges are detected and how the program reacts. Currently the program reads the permissions of the folder you are in and checks to see what permissions apply to the user and the groups the user is in. It then does this with every file we are using. This method of detection has the issue that if you do not have permissions to read the permissions it can not detect your permissions. Additionally it does not take into account if requesting admin permissions will actually allow the program to read/write.

## Admin Permissions
The program will prompt you for administrator when needed. Either NanaZip will prompt you directly or the context menu handler will prompt you depending on the scenario. The context menu handler is not signed so windows may give a warning about running it.

## Installation
1. Install [ShellAnything](https://github.com/end2endzone/ShellAnything)
2. From the [latest release](https://github.com/hboyd2003/NanaZipAlternativeContextMenu/releases/latest) download the XML file and the exe for your architecture.
3. Place the XML file in your root ShellAnything folder (%USERPROFILE%/ShellAnything).
4. Create a "Tools" folder inside your ShellAnything folder (%USERPROFILE%/ShellAnything/Tools) and place the exe inside.
5. Rename the exe to just "NanaZipContext.exe" 
3. Either install [CMDUtils using Chocolatey](https://community.chocolatey.org/packages/cmdutils) or download it [directly](http://www.maddogsw.com/cmdutils/) and place the "recycle.exe" into the tools folder.
3. Enjoy!
