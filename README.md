# Alternative Context Menu entries for [NanaZip](https://github.com/M2Team/NanaZip)
A flat alternative to the default [NanaZip](https://github.com/M2Team/NanaZip) context menu using [NileSoft Shell](https://github.com/moudey/Shell).
### Context Menu Entries
- Extract to .\<Filename>
- Extract to .\<Filename> and recycle
- Add to archive
- Create archive from folder

## Why?
Currently, [NanaZip](https://github.com/M2Team/NanaZip) context menu items are inside a sub-context menu which is slow and cumbersome and the developer has no intention of adding an option to not place them in a sub menu. Additionally, when extracting multiple zip files it does so synchronously which on SSDs is slower than extracting them asynchronously.

## How?
[NileSoft Shell](https://github.com/moudey/Shell) allows for the theming, creation, removal and modification of the context menu and its entries without having to mess with the registry. A helper program to handle more advanced functionality.

## Problems with the current implementation
The issues surrounding the current implementation are due to the way that privileges are detected and how the program reacts. Currently, the program reads the permissions of the folder you are in and checks to see what permissions apply to the user and the groups the user is in. It then does this with every file we are using. This method of detection has the issue that if you do not have permissions to read the permissions it can not detect your permissions. Additionally it does not take into account if requesting admin permissions will actually allow the program to read/write.

## Admin Permissions
The program will prompt you for administrator when needed. In most cases NanaZip will prompt you directly but in some cases either Windows Sudo (if enabled) or the context menu handler program will prompt you directly. The context menu handler is not signed so windows may give a warning about running it.

## Installation
1. Install [NanaZip](https://github.com/M2Team/NanaZip)
2. From the [latest release](https://github.com/hboyd2003/NanaZipAlternativeContextMenu/releases/latest) download the NSS file, icons and the exe for your architecture.
3. Place the NSS file in the NileSoft Shell Imports folder (```C:\Program Files\Nilesoft Shell\imports```).
4. Create an "Icons" folder inside the NileSoft Shell folder (```%USERPROFILE%/ShellAnything/Icons```) and place the icons inside it.
5. Create a "Tools" folder inside your ShellAnything folder (%USERPROFILE%/ShellAnything/Tools) and place the exe inside.
6. Rename the exe to just "NanaZipContext.exe"
7. Either install [CMDUtils using Chocolatey](https://community.chocolatey.org/packages/cmdutils) or download it [directly](http://www.maddogsw.com/cmdutils/) and place the "recycle.exe" into the tools folder.
8. Enjoy!