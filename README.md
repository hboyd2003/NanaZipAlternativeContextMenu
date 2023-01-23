# Alternative Context Menu entries for [NanaZip](https://github.com/M2Team/NanaZip)
A flat alternative to the default [NanaZip](https://github.com/M2Team/NanaZip) context menu using [ShellAnything](https://github.com/end2endzone/ShellAnything). It also adds an entry to decompress a file and then recycle the file.

## Why?
Currently [NanaZip](https://github.com/M2Team/NanaZip) context menu items are inside of a sub-context menu, which I find slow. Additionally, the developer has no intention of adding an option to not place them in a sub menu. 

## How?
Using [ShellAnything](https://github.com/end2endzone/ShellAnything) you can easily impliment custom context menu entries using XML, however, it is not all powerfull and to create a better implementation of the context menu intries I have a created a simple companion app.

## Problems with the current implementation
The issues surrouding the current implementation are due to the way that privladges are detect and how the program reacts. Currently the program reads the permissions of the folder you are in and checks to see what permisions applies to the user and the groups the user is in. It then does this with every file we are using. This method of detection has the issue that if you do not have permisions to read the permissions it can not detect your permisions. Additionally it does not take into account if requesting admin permissions will actually allow the program to read/write.
