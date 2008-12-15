//--------------------------------------------------------------------------------------
// File: DXMUTData.cs
//
// DirectX SDK Managed Direct3D sample framework data class
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
using System;
using System.Windows.Forms;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Microsoft.Samples.DirectX.UtilityToolkit
{

    #region Framework Interfaces and Eventargs classes
    /// <summary>Interface that the framework will use to call into samples</summary>
    public interface IFrameworkCallback
    {
        void OnFrameMove(Device device, double totalTime, float elapsedTime);
        void OnFrameRender(Device device, double totalTime, float elapsedTime);
    }
    
    /// <summary>Interface that the framework will use to determine if a device is acceptable</summary>
    public interface IDeviceCreation
    {
        bool IsDeviceAcceptable(Caps caps, Format adapterFormat, Format backBufferFormat, bool isWindowed);
        void ModifyDeviceSettings(DeviceSettings settings, Caps caps);
    }

    /// <summary>Event arguments for device creation/reset</summary>
    public class DeviceEventArgs : EventArgs
    {
        // Class data
        public Device Device;
        public SurfaceDescription BackBufferDescription;

        public DeviceEventArgs(Device d, SurfaceDescription desc) 
        {
            Device = d;
            BackBufferDescription = desc;
        }
    }
    /// <summary>Event Handler delegate for device creation/reset</summary>
    public delegate void DeviceEventHandler(object sender, DeviceEventArgs e);
    #endregion

    #region Device Settings
    /// <summary>
    /// Holds the settings for creating a device
    /// </summary>
    public class DeviceSettings : ICloneable
    {
        public uint AdapterOrdinal;
        public DeviceType DeviceType;
        public Format AdapterFormat;
        public CreateFlags BehaviorFlags;
        public PresentParameters presentParams;

        #region ICloneable Members
        /// <summary>Clone this object</summary>
        public DeviceSettings Clone()
        {
            DeviceSettings clonedObject = new DeviceSettings();
            clonedObject.presentParams = (PresentParameters)this.presentParams.Clone();
            clonedObject.AdapterFormat = this.AdapterFormat;
            clonedObject.AdapterOrdinal = this.AdapterOrdinal;
            clonedObject.BehaviorFlags = this.BehaviorFlags;
            clonedObject.DeviceType = this.DeviceType;

            return clonedObject;
        }
        /// <summary>Clone this object</summary>
        object ICloneable.Clone() { throw new NotSupportedException("Use the strongly typed overload instead."); }
        #endregion
    }
    #endregion

    #region User Timers
    /// <summary>Stores timer callback information</summary>
    public struct TimerData
    {
        public TimerCallback callback;
        public float TimeoutInSecs;
        public float Countdown;
        public bool IsEnabled;
    }
    #endregion

    #region Callback methods 
    public delegate IntPtr WndProcCallback(IntPtr hWnd, NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam, ref bool NoFurtherProcessing);
    public delegate void TimerCallback(uint eventId);
    #endregion

    #region Matching Options
    /// <summary>
    /// Used when finding valid device settings
    /// </summary>
    public enum MatchType
    {
        IgnoreInput, // Use the closest valid value to a default 
        PreserveInput, // Use input without change, but may cause no valid device to be found
        ClosestToInput // Use the closest valid value to the input 
    }

    /// <summary>
    /// Options on how to match items
    /// </summary>
    public struct MatchOptions
    {
        public MatchType AdapterOrdinal;
        public MatchType DeviceType;
        public MatchType Windowed;
        public MatchType AdapterFormat;
        public MatchType VertexProcessing;
        public MatchType Resolution;
        public MatchType BackBufferFormat;
        public MatchType BackBufferCount;
        public MatchType MultiSample;
        public MatchType SwapEffect;
        public MatchType DepthFormat;
        public MatchType StencilFormat;
        public MatchType PresentFlags;
        public MatchType RefreshRate;
        public MatchType PresentInterval;
    };
    #endregion

    #region Framework's data
    /// <summary>
    /// Holds data for the Framework class, and all of the properties
    /// </summary>
    class FrameworkData
    {
        #region Instance Data
        private Device device; // the D3D rendering device

        private DeviceSettings  currentDeviceSettings; // current device settings
        private SurfaceDescription backBufferSurfaceDesc; // back buffer surface description
        private Caps caps; // D3D caps for current device

        private System.Windows.Forms.Control windowFocus; // the main app focus window
        private System.Windows.Forms.Control windowDeviceFullScreen; // the main app device window in fullscreen mode
        private System.Windows.Forms.Control windowDeviceWindowed; // the main app device window in windowed mode
        private IntPtr adapterMonitor; // the monitor of the adapter 
        private double currentTime; // current time in seconds
        private float elapsedTime; // time elapsed since last frame

        private System.Windows.Forms.FormStartPosition defaultStartingLocation; // default starting location of the window
        private System.Drawing.Rectangle clientRect; // client rect of window
        private System.Drawing.Rectangle fullScreenClientRect; // client rect of window when fullscreen
        private System.Drawing.Rectangle windowBoundsRect; // window rect of window
        private System.Drawing.Point windowLocation; // Location of the window
        
        private System.Windows.Forms.MainMenu windowMenu; // menu of app
        private double lastStatsUpdateTime; // last time the stats were updated
        private uint lastStatsUpdateFrames; // frames count since last time the stats were updated
        private float frameRate; // frames per second
        private int currentFrameNumber; // the current frame number

        private bool isHandlingDefaultHotkeys; // if true, the sample framework will handle some default hotkeys
        private bool isShowingMsgBoxOnError; // if true, then msgboxes are displayed upon errors
        private bool isClipCursorWhenFullScreen; // if true, then the sample framework will keep the cursor from going outside the window when full screen
        private bool isShowingCursorWhenFullScreen; // if true, then the sample framework will show a cursor when full screen
        private bool isConstantFrameTime; // if true, then elapsed frame time will always be 0.05f seconds which is good for debugging or automated capture
        private float timePerFrame; // the constant time per frame in seconds, only valid if isConstantFrameTime==true
        private bool canAutoChangeAdapter; // if true, then the adapter will automatically change if the window is different monitor
        private bool isWindowCreatedWithDefaultPositions; // if true, then default was used and the window should be moved to the right adapter
        private int applicationExitCode; // the exit code to be returned to the command line

        private bool isInited; // if true, then Init() has succeeded
        private bool wasWindowCreated; // if true, then CreateWindow() or SetWindow() has succeeded
        private bool wasDeviceCreated; // if true, then CreateDevice*() or SetDevice() has succeeded

        private bool isInitCalled; // if true, then Init() was called
        private bool isWindowCreateCalled; // if true, then CreateWindow() or SetWindow() was called
        private bool isDeviceCreateCalled; // if true, then CreateDevice*() or SetDevice() was called

        private bool isDeviceObjectsCreated; // if true, then DeviceCreated callback has been called (if non-NULL)
        private bool isDeviceObjectsReset; // if true, then DeviceReset callback has been called (if non-NULL)
        private bool isInsideDeviceCallback; // if true, then the framework is inside an app device callback
        private bool isInsideMainloop; // if true, then the framework is inside the main loop
        private bool isActive; // if true, then the app is the active top level window
        private bool isTimePaused; // if true, then time is paused
        private bool isRenderingPaused; // if true, then rendering is paused
        private int pauseRenderingCount; // pause rendering ref count
        private int pauseTimeCount; // pause time ref count
        private bool isDeviceLost; // if true, then the device is lost and needs to be reset
        private bool isMinimized; // if true, then the window is minimized
        private bool isMaximized; // if true, then the window is maximized
        private bool isSizeChangesIgnored; // if true, the sample framework won't reset the device upon window size change (for public use only)
        private bool isNotifyOnMouseMove; // if true, include WM_MOUSEMOVE in mousecallback

        private int overrideAdapterOrdinal; // if != -1, then override to use this adapter ordinal
        private bool overrideWindowed; // if true, then force to start windowed
        private bool overrideFullScreen; // if true, then force to start full screen
        private int overrideStartX; // if != -1, then override to this X position of the window
        private int overrideStartY; // if != -1, then override to this Y position of the window
        private int overrideWidth; // if != 0, then override to this width
        private int overrideHeight; // if != 0, then override to this height
        private bool overrideForceHAL; // if true, then force to Hardware device (failing if one doesn't exist)
        private bool overrideForceREF; // if true, then force to Reference device (failing if one doesn't exist)
        private bool overrideForcePureHWVP; // if true, then force to use pure Hardware VertexProcessing (failing if device doesn't support it)
        private bool overrideForceHWVP; // if true, then force to use Hardware VertexProcessing (failing if device doesn't support it)
        private bool overrideForceSWVP; // if true, then force to use Software VertexProcessing 
        private bool overrideConstantFrameTime; // if true, then force to constant frame time
        private float overrideConstantTimePerFrame; // the constant time per frame in seconds if overrideConstantFrameTime==true
        private int overrideQuitAfterFrame; // if != 0, then it will force the app to quit after that frame

        private IDeviceCreation deviceCallback; // Callback for device creation and acceptability
        private IFrameworkCallback frameworkCallback; // Framework callback interface
        private WndProcCallback wndFunc; // window messages callback

        private SettingsDialog settings; // The settings dialog
        private bool isShowingD3DSettingsDlg; // if true, then show the settings dialog

        private ArrayList timerList = new ArrayList(); // list of TimerData structs
        private string staticFrameStats; // static part of frames stats 
        private string frameStats; // frame stats (fps, width, etc)
        private string deviceStats; // device stats (description, device type, etc)
        private string windowTitle; // window title

        #endregion

        #region Properties
        public Device Device { get { return device; } set {device = value; } }
        public DeviceSettings CurrentDeviceSettings { get { return currentDeviceSettings; } set {currentDeviceSettings = value; } }
        public SurfaceDescription BackBufferSurfaceDesc { get { return backBufferSurfaceDesc; } set {backBufferSurfaceDesc = value; } }
        public Caps Caps { get { return caps; } set {caps = value; } }

        public System.Windows.Forms.Control WindowFocus { get { return windowFocus; } set {windowFocus = value; } }
        public System.Windows.Forms.Control WindowDeviceFullScreen { get { return windowDeviceFullScreen; } set {windowDeviceFullScreen = value; } }
        public System.Windows.Forms.Control WindowDeviceWindowed { get { return windowDeviceWindowed; } set {windowDeviceWindowed = value; } }
        public IntPtr AdapterMonitor { get { return adapterMonitor; } set {adapterMonitor = value; } }
        public double CurrentTime { get { return currentTime; } set {currentTime = value; } }
        public float ElapsedTime { get { return elapsedTime; } set {elapsedTime = value; } }

        public System.Windows.Forms.FormStartPosition DefaultStartingLocation { get { return defaultStartingLocation; } set {defaultStartingLocation = value; } }
        public System.Drawing.Rectangle ClientRectangle { get { return clientRect; } set {clientRect = value; } }
        public System.Drawing.Rectangle FullScreenClientRectangle { get { return fullScreenClientRect; } set {fullScreenClientRect = value; } }
        public System.Drawing.Rectangle WindowBoundsRectangle { get { return windowBoundsRect; } set {windowBoundsRect = value; } }
        public System.Drawing.Point ClientLocation { get { return windowLocation; } set {windowLocation = value; } }
        public System.Windows.Forms.MainMenu Menu { get { return windowMenu; } set {windowMenu = value; } }
        public double LastStatsUpdateTime { get { return lastStatsUpdateTime; } set {lastStatsUpdateTime = value; } }
        public uint LastStatsUpdateFrames { get { return lastStatsUpdateFrames; } set {lastStatsUpdateFrames = value; } }
        public float CurrentFrameRate { get { return frameRate; } set {frameRate = value; } }
        public int CurrentFrameNumber { get { return currentFrameNumber; } set {currentFrameNumber = value; } }

        public bool IsHandlingDefaultHotkeys { get { return isHandlingDefaultHotkeys; } set {isHandlingDefaultHotkeys = value; } }
        public bool IsShowingMsgBoxOnError { get { return isShowingMsgBoxOnError; } set {isShowingMsgBoxOnError = value; } }
        public bool IsCursorClippedWhenFullScreen { get { return isClipCursorWhenFullScreen; } set {isClipCursorWhenFullScreen = value; } }
        public bool IsShowingCursorWhenFullScreen { get { return isShowingCursorWhenFullScreen; } set {isShowingCursorWhenFullScreen = value; } }
        public bool IsUsingConstantFrameTime { get { return isConstantFrameTime; } set {isConstantFrameTime = value; } }
        public float TimePerFrame { get { return timePerFrame; } set {timePerFrame = value; } }
        public bool CanAutoChangeAdapter { get { return canAutoChangeAdapter; } set {canAutoChangeAdapter = value; } }
        public bool IsWindowCreatedWithDefaultPositions { get { return isWindowCreatedWithDefaultPositions; } set {isWindowCreatedWithDefaultPositions = value; } }
        public int ApplicationExitCode { get { return applicationExitCode; } set {applicationExitCode = value; } }

        public bool IsInited { get { return isInited; } set {isInited = value; } }
        public bool WasWindowCreated { get { return wasWindowCreated; } set {wasWindowCreated = value; } }
        public bool WasDeviceCreated { get { return wasDeviceCreated; } set {wasDeviceCreated = value; } }

        public bool WasInitCalled { get { return isInitCalled; } set {isInitCalled = value; } }
        public bool WasWindowCreateCalled { get { return isWindowCreateCalled; } set {isWindowCreateCalled = value; } }
        public bool WasDeviceCreateCalled { get { return isDeviceCreateCalled; } set {isDeviceCreateCalled = value; } }

        public bool AreDeviceObjectsCreated { get { return isDeviceObjectsCreated; } set {isDeviceObjectsCreated = value; } }
        public bool AreDeviceObjectsReset { get { return isDeviceObjectsReset; } set {isDeviceObjectsReset = value; } }
        public bool IsInsideDeviceCallback { get { return isInsideDeviceCallback; } set {isInsideDeviceCallback = value; } }
        public bool IsInsideMainloop { get { return isInsideMainloop; } set {isInsideMainloop = value; } }
        public bool IsActive { get { return isActive; } set {isActive = value; } }
        public bool IsTimePaused { get { return isTimePaused; } set {isTimePaused = value; } }
        public bool IsRenderingPaused { get { return isRenderingPaused; } set {isRenderingPaused = value; } }
        public int PauseRenderingCount { get { return pauseRenderingCount; } set {pauseRenderingCount = value; } }
        public int PauseTimeCount { get { return pauseTimeCount; } set {pauseTimeCount = value; } }
        public bool IsDeviceLost { get { return isDeviceLost; } set {isDeviceLost = value; } }
        public bool IsMinimized { get { return isMinimized; } set {isMinimized = value; } }
        public bool IsMaximized { get { return isMaximized; } set {isMaximized = value; } }
        public bool AreSizeChangesIgnored { get { return isSizeChangesIgnored; } set {isSizeChangesIgnored = value; } }
        public bool IsNotifiedOnMouseMove { get { return isNotifyOnMouseMove; } set {isNotifyOnMouseMove = value; } }

        public int OverrideAdapterOrdinal { get { return overrideAdapterOrdinal; } set {overrideAdapterOrdinal = value; } }
        public bool IsOverridingWindowed { get { return overrideWindowed; } set {overrideWindowed = value; } }
        public bool IsOverridingFullScreen { get { return overrideFullScreen; } set {overrideFullScreen = value; } }
        public int OverrideStartX { get { return overrideStartX; } set {overrideStartX = value; } }
        public int OverrideStartY { get { return overrideStartY; } set {overrideStartY = value; } }
        public int OverrideWidth { get { return overrideWidth; } set {overrideWidth = value; } }
        public int OverrideHeight { get { return overrideHeight; } set {overrideHeight = value; } }
        public bool IsOverridingForceHardware { get { return overrideForceHAL; } set {overrideForceHAL = value; } }
        public bool IsOverridingForceReference { get { return overrideForceREF; } set {overrideForceREF = value; } }
        public bool IsOverridingForcePureHardwareVertexProcessing { get { return overrideForcePureHWVP; } set {overrideForcePureHWVP = value; } }
        public bool IsOverridingForceHardwareVertexProcessing { get { return overrideForceHWVP; } set {overrideForceHWVP = value; } }
        public bool IsOverridingForceSoftwareVertexProcessing { get { return overrideForceSWVP; } set {overrideForceSWVP = value; } }
        public bool IsOverridingConstantFrameTime { get { return overrideConstantFrameTime; } set {overrideConstantFrameTime = value; } }
        public float OverrideConstantTimePerFrame { get { return overrideConstantTimePerFrame; } set {overrideConstantTimePerFrame = value; } }
        public int OverrideQuitAfterFrame { get { return overrideQuitAfterFrame; } set {overrideQuitAfterFrame = value; } }

        public IDeviceCreation DeviceCreationInterface { get { return deviceCallback; } set { deviceCallback = value; } }
        public IFrameworkCallback CallbackInterface { get { return frameworkCallback; } set {frameworkCallback = value; } }
        public WndProcCallback WndProcFunction { get { return wndFunc; } set {wndFunc = value; } }
        
        public SettingsDialog Settings { get { return settings; } set {settings = value; } }
        public bool IsD3DSettingsDialogShowing { get { return isShowingD3DSettingsDlg; } set {isShowingD3DSettingsDlg = value; } }

        public ArrayList Timers { get { return timerList; } set {timerList = value; } }
        public string StaticFrameStats { get { return staticFrameStats; } set {staticFrameStats = value; } }
        public string FrameStats { get { return frameStats; } set {frameStats = value; } }
        public string DeviceStats { get { return deviceStats; } set {deviceStats = value; } }
        public string WindowTitle { get { return windowTitle; } set {windowTitle = value; } }
        #endregion

        /// <summary>
        /// Initialize data
        /// </summary>
        public FrameworkData()
        {
            // Set some initial data
            overrideStartX = -1;
            overrideStartY = -1;
            overrideAdapterOrdinal = -1;
            canAutoChangeAdapter = true;
            isShowingMsgBoxOnError = true;
            isActive = true;
            defaultStartingLocation = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
        }
    }
    #endregion

    #region Framework's Default Window
    /// <summary>
    /// The main window that will be used for the sample framework
    /// </summary>
    public class GraphicsWindow : System.Windows.Forms.Form
    {
        private Framework frame = null;
        public GraphicsWindow(Framework f)
        {
            frame = f;
            this.MinimumSize = Framework.MinWindowSize;
        }

        /// <summary>
        /// Will call into the sample framework's window proc
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            frame.WindowsProcedure(ref m);
            base.WndProc (ref m);
        }


    }
    #endregion
}