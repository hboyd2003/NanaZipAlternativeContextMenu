﻿using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;


//Needs two arguments and a none empty file list
if (args.Length != 2 || args[1] == "") Environment.Exit(1);

var files = args[1].Split(",");

var outputPath =
    Path.GetDirectoryName(files[0]) + "\\" ??
    throw new Exception(
        "Failed to get directory name"); //Should be impossible for you to run the command on multiple files in different directories.

var isAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent())
    .IsInRole(WindowsBuiltInRole.Administrator);

var needWriteAccess = false;
var needReadAccess = false;

if (!isAdministrator)
    needWriteAccess =
        CheckPermissions(outputPath) != "Write"; //Checks if the folder we are writing the files to needs admin.


switch (args[0])
{
    case "decompress":

        if (files.Length > 1 && !isAdministrator) //Only self elevate if there are multiple files to avoid UAC prompt spam.
        {
            if (needWriteAccess) SelfElevate();

            foreach (var file in files)
                if (CheckPermissions(file) != "Write") //Checks if file we are decompressing needs admin to write (to delete).
                    SelfElevate();
        }

        foreach (var file in files)
        {
            if (!isAdministrator)
                needReadAccess = !needWriteAccess && CheckPermissions(file) == "None"; //Make sure we can read the current file.

            var nanaZipG = NewNanaZipGProcess("x \"" + file + "\" -o\"" +
                                              outputPath + Path.GetFileNameWithoutExtension(file) + "\"",
                needReadAccess || needWriteAccess); //Arguments: x "[file path]" -o"[directory path]"
            Process.Start(nanaZipG);
        }

        break;
    case "decompressAndRecycle":
        if (!isAdministrator)
        {
            if (needWriteAccess) SelfElevate();
            //Even if a single file needs elevation to decompress/recycle elevate the whole program to avoid UAC prompting for both NanaZip and Recycle.
            foreach (var file in files)
                if (CheckPermissions(file) != "Write") //Checks if file we are decompressing needs admin to write (to delete).
                    SelfElevate();
        }


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
        if (!needWriteAccess && !isAdministrator)
            if (files.Any(file => !needWriteAccess && CheckPermissions(file) == "None"))
                needReadAccess = true;

        //NanaZip takes files separated by spaces.
        var filesAsArgument = "";
        foreach (var file in files) filesAsArgument += "\"" + file + "\" ";

        var directoryName = string.Join("\\", outputPath.Split('\\')[^2..])[..^1];

        var nanaZipGArchive = NewNanaZipGProcess("a \"" + directoryName + "\" -ad -saa -- " + filesAsArgument,
            needReadAccess || needWriteAccess); //Arguments: a "[directory name]" -ad -saa -- "[file path 1]" "[file path 2]" "[file path ... x]"
        nanaZipGArchive.WorkingDirectory = outputPath;

        Process.Start(nanaZipGArchive);
        break;
    default: //No valid command
        Environment.Exit(1);
        break;
}


//Needed otherwise arguments will be overwritten
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


//Needed otherwise arguments will be overwritten
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
    //Gets info needed to check permissions
    var currentUser = WindowsIdentity.GetCurrent();
    var currentPrincipal = new WindowsPrincipal(currentUser);
    var accessRules = new DirectoryInfo(directory).GetAccessControl()
        .GetAccessRules(true, true, typeof(SecurityIdentifier));


    if (currentUser.User == null) throw new Exception("Current user is Null!");


    //A deny permission takes precedent over a allow permission
    var writeDeny = false;
    var writeAllow = false;
    var readAllow = false;

    foreach (FileSystemAccessRule accessRule in accessRules)
    {
        if (!currentUser.User.Equals(accessRule.IdentityReference) &&
            !currentPrincipal.IsInRole(
                (SecurityIdentifier)accessRule.IdentityReference))
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
                throw new ArgumentOutOfRangeException();
        }
    }

    if (!writeDeny && writeAllow)
        return "Write";
    return readAllow ? "Read" : "None";
}


void SelfElevate()
{
    var selfElevate = new ProcessStartInfo
    {
        UseShellExecute = true,
        Verb = "runas",
        WorkingDirectory = Environment.CurrentDirectory,
        FileName = Environment.ProcessPath,
        Arguments = "\"" + args[0] + "\"  \"" + args[1] + "\""
    };
    Process.Start(selfElevate);
    Environment.Exit(0);
}