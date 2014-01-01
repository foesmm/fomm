Name: Fallout Mod Manager
Version: 0.11.0
Category: Utilities
Author: Timeslip, Q
Source: http://sourceforge.net/project/showfiles.php?group_id=247976
Forum: http://sourceforge.net/forum/?group_id=247976
Homepage: http://sourceforge.net/projects/fomm/

Description
===========
A collection of utilities for mod users and creators. Included are tools for one click mod installation and removal, load order manipulation, esp editing, bsa creation and extraction, sdp editing and more.

Manual Install
==============
First extract the .7z archive to a location of your choice. (Which presumably you've already done, or you wouldn't be reading this. :p) Make sure you keep the directory structure intact.

If you only have a single, retail DVD install of fallout 3 you can run fomm from anywhere; the only restriction is that it needs to be somewhere that it has write access to. If you have multiple installations, or you are running a downloaded version of fallout from D2D or steam, you will need to either install fomm into the same directory as the fallout3.exe of the game you want to run it with, or manually edit the settings.xml file to add the node '<strValue name="FalloutDir">add the full path to your fallout directory here</strValue>'.

If fomm crashes on startup with a generic windows error then you likely have a problem with your .NET installation; fomm requires .NET 2.0 to run. Among the other third party gunk that the fallout 3 installer installs alongside the actual game is the original prepatched version of .NET 3.0, which under some circumstances can cause .NET 2 applications to break. In this case visiting windows update and installing the .NET 3 patch it offers you is sufficient to get fomm working.

Help on the actual usage of fomm is available from the wiki at http://fomm.wiki.sourceforge.net/, which is also available from inside fomm by clicking 'help' on the plugins list.

Uninstall
=========
No special uninstallation is required; just delete fomm. (Or run the uninstaller from the start menu, if you used the installer to install in the first place,) Before you do so you'll probably want to undo any changes that have been made with it, such as deactivating fomods or removing archive invalidation. All fomods you have installed will be saved in a 'mods' directory in the same folder as fomm.exe, which you may or may not want to delete along with fomm.

Contact
=======
You can find me on the official Elder Scrolls forums or sourceforge as 'Timeslip'.

Credits
=======
Thanks to Bethesda for creating Fallout 3.
Thanks to LHammonds for the Readme Generator this file was based on.
Thanks to Claviticus for the code that the auto load order sorting functionality is based on.
Thanks to AliTheLord for an updated load order template
Thanks to Quarn for ArchiveInvalidation Invalidated, which fomm uses as its archive invalidation method.
Thanks to the long list of people who helped out with obmm development, which fomm is partially based on.

Licensing/Legal
===============
This mod is released under the GNU General Public License.
The GPL can be found here: http://www.gnu.org/licenses/gpl.txt

This mod uses the library SevenZipSharp, which is released under the GNU Lesser General Public License.
The LGPL can be found here: http://www.gnu.org/licenses/lgpl.txt

This mod uses the 7zip library, which is released under the GNU Lesser General Public License, version 2.1, with an unRAR restriction (see below).
The LGPL can be found here: http://www.gnu.org/licenses/lgpl.txt
7-Zip Copyright (C) 1999-2010 Igor Pavlov.

unRAR restriction
-----------------
    The decompression engine for RAR archives was developed using source 
    code of unRAR program.
    All copyrights to original unRAR code are owned by Alexander Roshal.

    The license for original unRAR code has the following restriction:

      The unRAR sources cannot be used to re-create the RAR compression algorithm, 
      which is proprietary. Distribution of modified unRAR sources in separate form 
      or as a part of other software is permitted, provided that it is clearly
      stated in the documentation and source comments that the code may
      not be used to develop a RAR (WinRAR) compatible archiver.


Troubleshooting
===============
Q. I get the "Unable to get write permissions for fallout's installation directory" error. How do I fix it?
A. This happens when you are running Vista or Windows 7 and have installed Fallout 3 in the Program Files folder. You need to do one of the following:
1) Disable UAC (not recommended).
2) Move Fallout 3 outside of the Program Files folder (e.g., C:\Games\Bethesda\Fallout 3). This may require a reinstall. I know with Oblivion you could just copy the game folder to a new location, run the game, and all would be well. I'm not sure if that works with Fallout 3.
3) In theory you should also be able to run FOMM as administrator. You can try this by right-clicking on the FOMM shortcut and selecting "Run as administrator." Alternatively, right-click on the shortcut, select Properties->Compatibility and check "Run this program as an administrator."

The best thing to do in order to avoid other problems, and the generally recommended solution, is to install Fallout outside of the Program Files folder.

