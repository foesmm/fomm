using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;

/*  Author:  Michael A. McCloskey
 *  Company: Schematrix
 *  Version: 20040714
 *  
 *  Personal Comments:
 *  I created this unrar wrapper class for personal use 
 *  after running into a number of issues trying to use
 *  another COM unrar product via COM interop.  I hope it 
 *  proves as useful to you as it has to me and saves you
 *  some time in building your own products.
 */

namespace Fomm.PackageManager {
    #region Event Delegate Definitions

    /// <summary>
    /// Represents the method that will handle extraction progress events
    /// </summary>
    internal delegate void ExtractionProgressHandler(object sender, ExtractionProgressEventArgs e);

    #endregion

    /// <summary>
    /// Wrapper class for unrar DLL supplied by RARSoft.  
    /// Calls unrar DLL via platform invocation services (pinvoke).
    /// DLL is available at http://www.rarlab.com/rar/UnRARDLL.exe
    /// </summary>
    internal class Unrar : IDisposable {
        #region Unrar DLL enumerations

        /// <summary>
        /// Mode in which archive is to be opened for processing.
        /// </summary>
        internal enum OpenMode {
            /// <summary>
            /// Open archive for listing contents only
            /// </summary>
            List=0,
            /// <summary>
            /// Open archive for testing or extracting contents
            /// </summary>
            Extract=1
        }

        private enum RarError : uint {
            EndOfArchive=10,
            InsufficientMemory=11,
            BadData=12,
            BadArchive=13,
            UnknownFormat=14,
            OpenError=15,
            CreateError=16,
            CloseError=17,
            ReadError=18,
            WriteError=19,
            BufferTooSmall=20,
            UnknownError=21
        }

        private enum Operation : uint {
            Skip=0,
            Test=1,
            Extract=2
        }

        private enum VolumeMessage : uint {
            Ask=0,
            Notify=1
        }

        [Flags]
        private enum ArchiveFlags : uint {
            Volume=0x1,										// Volume attribute (archive volume)
            CommentPresent=0x2,						// Archive comment present
            Lock=0x4,											// Archive lock attribute
            SolidArchive=0x8,							// Solid attribute (solid archive)
            NewNamingScheme=0x10,					// New volume naming scheme ('volname.partN.rar')
            AuthenticityPresent=0x20,			// Authenticity information present
            RecoveryRecordPresent=0x40,		// Recovery record present
            EncryptedHeaders=0x80,				// Block headers are encrypted
            FirstVolume=0x100							// 0x0100  - First volume (set only by RAR 3.0 and later)
        }

        private enum CallbackMessages : uint {
            VolumeChange=0,
            ProcessData=1,
            NeedPassword=2
        }

        #endregion

        #region Unrar DLL structure definitions
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct RARHeaderDataEx {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=512)]
            internal string ArcName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=1024)]
            internal string ArcNameW;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=512)]
            internal string FileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=1024)]
            internal string FileNameW;
            internal uint Flags;
            internal uint PackSize;
            internal uint PackSizeHigh;
            internal uint UnpSize;
            internal uint UnpSizeHigh;
            internal uint HostOS;
            internal uint FileCRC;
            internal uint FileTime;
            internal uint UnpVer;
            internal uint Method;
            internal uint FileAttr;
            [MarshalAs(UnmanagedType.LPStr)]
            internal string CmtBuf;
            internal uint CmtBufSize;
            internal uint CmtSize;
            internal uint CmtState;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=1024)]
            internal uint[] Reserved;

            internal void Initialize() {
                this.CmtBuf=new string((char)0, 65536);
                this.CmtBufSize=65536;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RAROpenArchiveDataEx {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string ArcName;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string ArcNameW;
            internal uint OpenMode;
            internal uint OpenResult;
            [MarshalAs(UnmanagedType.LPStr)]
            internal string CmtBuf;
            internal uint CmtBufSize;
            internal uint CmtSize;
            internal uint CmtState;
            internal uint Flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
            internal uint[] Reserved;

            internal void Initialize() {
                this.CmtBuf=new string((char)0, 65536);
                this.CmtBufSize=65536;
                this.Reserved=new uint[32];
            }
        }

        #endregion

        #region Unrar function declarations
        [DllImport(@"fomm\unrar.dll")]
        private static extern IntPtr RAROpenArchiveEx(ref RAROpenArchiveDataEx archiveData);

        [DllImport(@"fomm\unrar.dll")]
        private static extern int RARCloseArchive(IntPtr hArcData);

        [DllImport(@"fomm\unrar.dll")]
        private static extern int RARReadHeaderEx(IntPtr hArcData, ref RARHeaderDataEx headerData);

        [DllImport(@"fomm\unrar.dll")]
        private static extern int RARProcessFile(IntPtr hArcData, int operation,
            [MarshalAs(UnmanagedType.LPStr)] string destPath,
            [MarshalAs(UnmanagedType.LPStr)] string destName);

        [DllImport(@"fomm\unrar.dll")]
        private static extern void RARSetCallback(IntPtr hArcData, UNRARCallback callback, int userData);

        // Unrar callback delegate signature
        private delegate int UNRARCallback(uint msg, int UserData, IntPtr p1, int p2);

        #endregion

        #region internal event declarations

        /// <summary>
        /// Event that is raised to indicate extraction progress
        /// </summary>
        internal event ExtractionProgressHandler ExtractionProgress;

        #endregion

        #region Private fields

        private string archivePathName=string.Empty;
        private IntPtr archiveHandle=new IntPtr(0);
        private bool retrieveComment=true;
        private RARHeaderDataEx header=new RARHeaderDataEx();
        private RARFileInfo currentFile;
        private UNRARCallback callback;

        #endregion

        #region Object lifetime procedures

        internal Unrar() {
            this.callback=new UNRARCallback(RARCallback);
        }

        internal Unrar(string archivePathName)
            : this() {
            this.archivePathName=archivePathName;
        }

        ~Unrar() {
            if(this.archiveHandle!=IntPtr.Zero) {
                Unrar.RARCloseArchive(this.archiveHandle);
                this.archiveHandle=IntPtr.Zero;
            }
        }

        public void Dispose() {
            if(this.archiveHandle!=IntPtr.Zero) {
                Unrar.RARCloseArchive(this.archiveHandle);
                this.archiveHandle=IntPtr.Zero;
            }
        }

        #endregion

        #region internal Properties

        /// <summary>
        /// Path and name of RAR archive to open
        /// </summary>
        internal string ArchivePathName {
            get {
                return this.archivePathName;
            }
            set {
                this.archivePathName=value;
            }
        }

        #endregion

        #region internal Methods

        /// <summary>
        /// Close the currently open archive
        /// </summary>
        /// <returns></returns>
        internal void Close() {
            // Exit without exception if no archive is open
            if(this.archiveHandle==IntPtr.Zero)
                return;

            // Close archive
            int result=Unrar.RARCloseArchive(this.archiveHandle);

            // Check result
            if(result!=0) {
                ProcessFileError(result);
            } else {
                this.archiveHandle=IntPtr.Zero;
            }
        }

        /// <summary>
        /// Opens archive specified by the ArchivePathName property with a specified mode
        /// </summary>
        /// <param name="openMode">Mode in which archive should be opened</param>
        internal void Open(OpenMode openMode) {
            if(this.ArchivePathName.Length==0)
                throw new IOException("Archive name has not been set.");
            this.Open(this.ArchivePathName, openMode);
        }

        /// <summary>
        /// Opens specified archive using the specified mode.  
        /// </summary>
        /// <param name="archivePathName">Path of archive to open</param>
        /// <param name="openMode">Mode in which to open archive</param>
        internal void Open(string archivePathName, OpenMode openMode) {
            IntPtr handle=IntPtr.Zero;

            // Close any previously open archives
            if(this.archiveHandle!=IntPtr.Zero)
                this.Close();

            // Prepare extended open archive struct
            this.ArchivePathName=archivePathName;
            RAROpenArchiveDataEx openStruct=new RAROpenArchiveDataEx();
            openStruct.Initialize();
            openStruct.ArcName=this.archivePathName+"\0";
            openStruct.ArcNameW=this.archivePathName+"\0";
            openStruct.OpenMode=(uint)openMode;
            if(this.retrieveComment) {
                openStruct.CmtBuf=new string((char)0, 65536);
                openStruct.CmtBufSize=65536;
            } else {
                openStruct.CmtBuf=null;
                openStruct.CmtBufSize=0;
            }

            // Open archive
            handle=Unrar.RAROpenArchiveEx(ref openStruct);

            // Check for success
            if(openStruct.OpenResult!=0) {
                switch((RarError)openStruct.OpenResult) {
                case RarError.InsufficientMemory:
                    throw new OutOfMemoryException("Insufficient memory to perform operation.");

                case RarError.BadData:
                    throw new IOException("Archive header broken");

                case RarError.BadArchive:
                    throw new IOException("File is not a valid archive.");

                case RarError.OpenError:
                    throw new IOException("File could not be opened.");
                }
            }

            // Save handle and flags
            this.archiveHandle=handle;

            // Set callback
            Unrar.RARSetCallback(this.archiveHandle, this.callback, this.GetHashCode());
        }

        /// <summary>
        /// Reads the next archive header and populates CurrentFile property data
        /// </summary>
        /// <returns></returns>
        internal bool ReadHeader() {
            // Throw exception if archive not open
            if(this.archiveHandle==IntPtr.Zero)
                throw new IOException("Archive is not open.");

            // Initialize header struct
            this.header=new RARHeaderDataEx();
            header.Initialize();

            // Read next entry
            currentFile=null;
            int result=Unrar.RARReadHeaderEx(this.archiveHandle, ref this.header);

            // Check for error or end of archive
            if((RarError)result==RarError.EndOfArchive)
                return false;
            else if((RarError)result==RarError.BadData)
                throw new IOException("Archive data is corrupt.");

            // Determine if new file
            if(((header.Flags & 0x01) != 0) && currentFile!=null) {

            } else {
                // New file, prepare header
                currentFile=new RARFileInfo();
                currentFile.FileName=header.FileNameW.ToString();
                if(header.UnpSizeHigh != 0) currentFile.UnpackedSize=(header.UnpSizeHigh * 0x100000000) + header.UnpSize;
                else currentFile.UnpackedSize=header.UnpSize;
                currentFile.BytesExtracted=0;
            }

            // Return success
            return true;
        }

        /// <summary>
        /// Extracts the current file to a specified directory without renaming file
        /// </summary>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        internal void ExtractToDirectory(string destinationPath) {
            this.Extract(destinationPath, string.Empty);
        }

        #endregion

        #region Private Methods

        private void Extract(string destinationPath, string destinationName) {
            int result=Unrar.RARProcessFile(this.archiveHandle, (int)Operation.Extract, destinationPath, destinationName);

            // Check result
            if(result!=0) {
                ProcessFileError(result);
            }
        }

        private static void ProcessFileError(int result) {
            switch((RarError)result) {
            case RarError.UnknownFormat:
                throw new OutOfMemoryException("Unknown archive format.");

            case RarError.BadData:
                throw new IOException("File CRC Error");

            case RarError.BadArchive:
                throw new IOException("File is not a valid archive.");

            case RarError.OpenError:
                throw new IOException("File could not be opened.");

            case RarError.CreateError:
                throw new IOException("File could not be created.");

            case RarError.CloseError:
                throw new IOException("File close error.");

            case RarError.ReadError:
                throw new IOException("File read error.");

            case RarError.WriteError:
                throw new IOException("File write error.");
            }
        }

        private int RARCallback(uint msg, int UserData, IntPtr p1, int p2) {
            int result=-1;

            switch((CallbackMessages)msg) {
            case CallbackMessages.VolumeChange:
                if((VolumeMessage)p2==VolumeMessage.Notify)
                    result=1;
                else if((VolumeMessage)p2==VolumeMessage.Ask) {
                    result=-1;
                }
                break;

            case CallbackMessages.ProcessData:
                result=OnDataAvailable(p2);
                break;
            case CallbackMessages.NeedPassword:
                result=-1;
                break;
            }
            return result;
        }

        private int OnDataAvailable(int p2) {
            int result=1;
            if(this.currentFile!=null) this.currentFile.BytesExtracted+=p2;
            if((this.ExtractionProgress!=null) && (this.currentFile!=null)) {
                ExtractionProgressEventArgs e = new ExtractionProgressEventArgs();
                e.FileName=this.currentFile.FileName;
                e.FileSize=this.currentFile.UnpackedSize;
                e.BytesExtracted=this.currentFile.BytesExtracted;
                e.PercentComplete=this.currentFile.PercentComplete;
                this.ExtractionProgress(this, e);
                if(!e.ContinueOperation)
                    result=-1;
            }
            return result;
        }

        #endregion
    }

    #region Event Argument Classes

    internal class ExtractionProgressEventArgs {
        internal string FileName;
        internal long FileSize;
        internal long BytesExtracted;
        internal double PercentComplete;
        internal bool ContinueOperation=true;
    }

    internal class RARFileInfo {
        internal string FileName;
        internal long UnpackedSize;
        internal long BytesExtracted;

        internal double PercentComplete {
            get {
                if(this.UnpackedSize!=0)
                    return (((double)this.BytesExtracted/(double)this.UnpackedSize) * (double)100.0);
                else
                    return (double)0;
            }
        }
    }

    #endregion
}