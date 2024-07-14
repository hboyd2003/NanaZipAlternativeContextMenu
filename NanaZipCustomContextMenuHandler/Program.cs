// This is the NanaZip Alternative Context Menu.
// Copyright © 2023-2024 Harrison Boyd
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>


using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Win32;

const string sudoEnabledRegistryPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Sudo";

//Needs two arguments and a none empty file list
if (args.Length != 2 || args[1] == "") Environment.Exit(1);

var files = args[1].Split(",");

var outputPath =
    Path.GetDirectoryName(files[0]) + "\\" ??
    throw new Exception(
        "Failed to get directory name"); //Should be impossible for you to run the command on multiple files in different directories.

var needFolderWriteAccess = false;
var needFileReadAccess = false;
var needFileWriteAccess = false;

if (!new WindowsPrincipal(WindowsIdentity.GetCurrent())
        .IsInRole(WindowsBuiltInRole.Administrator))
{
    needFolderWriteAccess =
        CheckPermissions(outputPath) != "Write"; //Checks if the folder we are writing the files to needs admin.
    if (!needFolderWriteAccess)
        foreach (var file in
                 files) //Checking if every file as read/write permissions in inefficient but simplifies the code a lot.
        {
            if (needFileReadAccess && needFileWriteAccess) break;
            var permission = CheckPermissions(file);
            needFileReadAccess = permission is not ("Write" or "Read");
            needFileWriteAccess = permission != "Write";
        }
}


switch (args[0])
{
    case "decompress":
        if (files.Length > 1 && (needFolderWriteAccess || needFileReadAccess))
            SelfElevate(); //Only self elevate if there are multiple files to avoid UAC prompt spam.

        foreach (var file in files)
        {
            var nanaZipG = NewNanaZipGProcess("x \"" + file + "\" -o\"" +
                                              outputPath + Path.GetFileNameWithoutExtension(file) + "\"",
                needFolderWriteAccess || needFileReadAccess); //Arguments: x "[file path]" -o"[directory path]"
            Process.Start(nanaZipG);
        }

        break;
    case "decompressAndRecycle":
        if (needFileReadAccess || needFileWriteAccess || needFolderWriteAccess) SelfElevate();

        foreach (var file in files)
            new Thread(
                () => //Thread is so we can keep starting new decompression jobs while each threads waits for NanaZip to finish.
                {
                    var nanaZipG = NewNanaZipGProcess("x \"" + file + "\" -o\"" +
                                                      outputPath +
                                                      Path.GetFileNameWithoutExtension(file) + "\"",
                        false); //Arguments: x "[file path]" -o"[directory path]"
                    using var proc = Process.Start(nanaZipG) ?? throw new Exception("NanaZip failed to start");

                    //Don't delete files if NanaZip failed to decompress.
                    proc.WaitForExit();
                    var exitCode = proc.ExitCode;
                    if (exitCode != 0) return; //End if NanaZip didn't finish correctly.

                    var recycle = NewRecycleProcess("\"" + file + "\"");
                    Process.Start(recycle);
                }).Start();

        break;
    case "addToArchive":
        //NanaZip takes files separated by spaces.
        var filesAsArgument = files.Aggregate("", (current, file) => current + "\"" + file + "\" ");

        var directoryName = string.Join("\\", outputPath.Split('\\')[^2..])[..^1];

        var nanaZipGArchive = NewNanaZipGProcess("a \"" + directoryName + "\" -ad -saa -- " + filesAsArgument,
            needFolderWriteAccess ||
            needFileReadAccess); //Arguments: a "[directory name]" -ad -saa -- "[file path 1]" "[file path 2]" "[file path ... x]"
        nanaZipGArchive.WorkingDirectory = outputPath;

        Process.Start(nanaZipGArchive);
        break;
    default: //No valid command
        Environment.Exit(1);
        break;
}

return;


// Needed otherwise arguments will be overwritten
static ProcessStartInfo NewNanaZipGProcess(string arguments, bool admin)
{
    var nanaZipG = new ProcessStartInfo
    {
        FileName = "NanaZipG",
        Arguments = arguments
    };
    if (!admin) return nanaZipG;

    //Changes needed for Admin
    nanaZipG.UseShellExecute = true;
    nanaZipG.Verb = "runas";
    return nanaZipG;
}


// Needed otherwise arguments will be overwritten
static ProcessStartInfo NewRecycleProcess(string arguments)
{
    var recycle = new ProcessStartInfo
    {
        FileName = "Recycle",
        WindowStyle = ProcessWindowStyle.Hidden,
        CreateNoWindow = true,
        Arguments = arguments
    };

    return recycle;
}


static string CheckPermissions(string directory)
{
    // Gets info needed to check permissions
    var currentUser = WindowsIdentity.GetCurrent();
    var currentPrincipal = new WindowsPrincipal(currentUser);
    var accessRules = new DirectoryInfo(directory).GetAccessControl()
        .GetAccessRules(true, true, typeof(SecurityIdentifier));


    if (currentUser.User == null) throw new Exception("Current user is Null!");


    // A deny permission takes precedent over an allow permission
    var writeDeny = false;
    var writeAllow = false;
    var readAllow = false;

    foreach (FileSystemAccessRule accessRule in accessRules)
    {
        if (!currentUser.User.Equals(accessRule.IdentityReference) &&
            !currentPrincipal.IsInRole((SecurityIdentifier)accessRule.IdentityReference))
            continue; //Skips to next loop if rule does not apply to the current user

        switch (accessRule.AccessControlType)
        {
            case AccessControlType.Deny:
            {
                if ((accessRule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write) writeDeny = true;
                if ((accessRule.FileSystemRights & FileSystemRights.Read) == FileSystemRights.Read)
                    return "None"; //If you can't read then you can presume you can't write
                break;
            }
            case AccessControlType.Allow:
            {
                if ((accessRule.FileSystemRights & FileSystemRights.Write) == FileSystemRights.Write) writeAllow = true;
                if ((accessRule.FileSystemRights & FileSystemRights.Read) == FileSystemRights.Read) readAllow = true;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException($"Unexpected Access control type. Expected either Allow or Deny but got {accessRule.AccessControlType}!");
        }
    }

    if (!writeDeny && writeAllow)
        return "Write";
    return readAllow ? "Read" : "None";
}

void SelfElevate()
{
    try
    {
        using var key = Registry.LocalMachine.OpenSubKey(sudoEnabledRegistryPath) ??
                        throw new FileNotFoundException(
                            $"Failed to open \"{sudoEnabledRegistryPath}\" registry key. Is Sudo not installed?");

        // Verify if the key is the expected type
        var keyType = key.GetValueKind("Enabled");
        if (!keyType.Equals(RegistryValueKind.DWord))
        {
            throw new FormatException(
                $"Expected Sudo \"Enabled\" registry key at \"{sudoEnabledRegistryPath}\" to be DWORD but it was {keyType}");
        }

        // Get value and verify it's a bool
        var sudoEnabled = key.GetValue("Enabled") as bool? ?? throw new FormatException(
            $"Failed to convert \"Enabled\" registry key at \"{sudoEnabledRegistryPath}\" to a bool. Key value should be either a 1 or 0.");


        if (sudoEnabled) SelfElevateWithSudo();
        else SelfElevateWithRunas();
    }
    catch (Exception)
    {
        SelfElevateWithRunas();
    }

    Environment.Exit(0);
}

// Self elevates with Sudo so that the pop-up shows as "Sudo" instead of the unsigned exe asking for elevation
void SelfElevateWithSudo()
{
    var sudoProcessStartInfo = new ProcessStartInfo
    {
        UseShellExecute = true,
        WindowStyle = ProcessWindowStyle.Hidden,
        WorkingDirectory = Environment.CurrentDirectory,
        FileName = "sudo",
        Arguments = $"--inline \"sudo\" --inline \"{Environment.ProcessPath}\" \"{args[0]}\" \"{args[1]}\""
    };
    Process.Start(sudoProcessStartInfo);
}

void SelfElevateWithRunas()
{
    var selfElevate = new ProcessStartInfo
    {
        UseShellExecute = true,
        Verb = "runas",
        WorkingDirectory = Environment.CurrentDirectory,
        FileName = Environment.ProcessPath,
        Arguments = $"\"{args[0]}\" \"{args[1]}\""
    };
    Process.Start(selfElevate);
}