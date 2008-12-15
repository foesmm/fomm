//--------------------------------------------------------------------------------------
// File: DXMUT.cs
//
// DirectX SDK Managed Direct3D sample framework
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
using System;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Microsoft.Samples.DirectX.UtilityToolkit
{
    /// <summary>
    /// Managed Utility Framework Class
    /// </summary>
    public sealed class Framework : IDisposable
    {

        #region Class Data
        // Constants for command line arguments
        private const string CmdAdapter = "adapter";
        private const string CmdWindowed = "windowed";
        private const string CmdFullscreen = "fullscreen";
        private const string CmdForceHardware = "forcehal";
        private const string CmdForceRef = "forceref";
        private const string CmdPureHwVp = "forcepurehwvp";
        private const string CmdForceHwVp = "forcehwvp";
        private const string CmdForceSwVp = "forceswvp";
        private const string CmdWidth = "width";
        private const string CmdHeight = "height";
        private const string CmdStartx = "startx";
        private const string CmdStarty = "starty";
        private const string CmdConstantFrame = "constantframetime";
        private const string CmdQuitAfterFrame = "quitafterframe";
        private const string CmdNoErrorBoxes = "noerrormsgboxes";

        // Delegate used for disposing application
        private delegate void DisposeDelegate();

        // Constants for minimum window size
        private const int MinimumWindowSizeX = 200;
        private const int MinimumWindowSizeY = 200;
        public const int DefaultSizeWidth = 640;
        public const int DefaultSizeHeight = 480;
        // Default starting size
        private static readonly System.Drawing.Size DefaultStartingSize = new System.Drawing.Size(DefaultSizeWidth, DefaultSizeHeight);
        internal static readonly System.Drawing.Size MinWindowSize = new System.Drawing.Size(MinimumWindowSizeX, MinimumWindowSizeY);
        public static readonly IntPtr TrueIntPtr = new IntPtr(1);
        private const string WindowClassName = "ManagedDirect3DWindowClass";

        // What about the stored data?
        private FrameworkData State;
        // Last shown error
        private Exception lastDisplayedMessage = null;
        // Has the framework been disposed already
        private bool isDisposed = false;
        // Did the toggle fullscreen happen when maximized?
        private bool toggleMaximized = false;
        #endregion

        #region Creation 
        /// <summary>
        /// Constructor, init data here
        /// </summary>
        public Framework()
        {
            State = new FrameworkData();
        }
        #endregion

        #region Setting up callbacks
        // Interface 'callbacks'
        public void SetCallbackInterface(IFrameworkCallback callback) { State.CallbackInterface = callback; }
        public void SetDeviceCreationInterface(IDeviceCreation callback) { State.DeviceCreationInterface = callback; }

        // Event 'callbacks'

        /// <summary>The event fired for disposing of the device</summary>
        public event EventHandler Disposing; 
        /// <summary>The event fired for when the device is lost</summary>
        public event EventHandler DeviceLost; 
        /// <summary>The event fired for when the device is created</summary>
        public event DeviceEventHandler DeviceCreated; 
        /// <summary>The event fired for when the device is reset</summary>
        public event DeviceEventHandler DeviceReset; 

        public void SetWndProcCallback(WndProcCallback callback) { State.WndProcFunction = callback; }
        #endregion

        /// <summary>
        /// Optionally parses the command line and sets if default hotkeys are handled
        /// Possible command line parameters are:
        /// -adapter:#              forces app to use this adapter # (fails if the adapter doesn't exist)
        /// -windowed               forces app to start windowed
        /// -fullscreen             forces app to start full screen
        /// -forceHardware          forces app to use Hardware (fails if Hardware doesn't exist)
        /// -forceReference         forces app to use Reference (fails if Reference doesn't exist)
        /// -forcepurehwvp          forces app to use pure HWVP (fails if device doesn't support it)
        /// -forcehwvp              forces app to use HWVP (fails if device doesn't support it)
        /// -forceswvp              forces app to use SWVP 
        /// -width:#                forces app to use # for width. for full screen, it will pick the closest possible supported mode
        /// -height:#               forces app to use # for height. for full screen, it will pick the closest possible supported mode
        /// -startx:#               forces app to use # for the x coord of the window position for windowed mode
        /// -starty:#               forces app to use # for the y coord of the window position for windowed mode
        /// -constantframetime:#    forces app to use constant frame time, where # is the time/frame in seconds
        /// -quitafterframe:x       forces app to quit after # frames
        /// -noerrormsgboxes        prevents the display of message boxes generated by the framework so the application can be run without user interaction
        /// 
        /// Hotkeys handled by default are:
        /// ESC                 app exits
        /// Alt-Enter           toggle between full screen & windowed
        /// F2 			        device selection dialog
        /// F3 			        toggle Hardware/Reference
        /// F8                  toggle wire-frame mode
        /// Pause               pauses time
        /// </summary>
        public void Initialize(bool handleDefaultKeys, bool showMessageBoxOnError)
        {
            State.WasInitCalled = true;

            // Turn off event handling for the MDX runtime, this will
            // increase performance and remove potential working set issues
            Device.IsUsingEventHandlers = false;

            // Increase

            // Store this data
            State.IsShowingMsgBoxOnError = showMessageBoxOnError;
            State.IsHandlingDefaultHotkeys = handleDefaultKeys;

            // Reset the timer period (ignore any failures)
            try
            { NativeMethods.timeBeginPeriod(1); }
            catch { System.Diagnostics.Debugger.Log(9, string.Empty, "Could not set time period.\r\n" ); }

            // Reset the timer
            FrameworkTimer.Reset();
            State.IsInited = true;

        }

        /// <summary>
        /// Creates a window with the specified title, icon, menu, and starting position.  If Init hasn't been
        /// called, this will call with default params.  Instead of calling this, you can call SetWindow
        /// to use an existing window
        /// </summary>
        public void CreateWindow(string windowTitle, System.Drawing.Icon icon, 
            System.Windows.Forms.MainMenu menu, int x, int y)
        {
            if (State.IsInsideDeviceCallback)
                throw new InvalidOperationException("You cannot create a window from inside a callback.");

            State.WasWindowCreateCalled = true;
            // Are we inited?
            if (!State.IsInited)
            {
                // If Init was already called and failed, fail again
                if (State.WasInitCalled)
                {
                    throw new InvalidOperationException("Initialize was already called and failed.");
                }

                // Call initialize with default params
                Initialize(true, true);
            }

            // Is there already a window created?
            if (State.WindowFocus == null)
            {
                // Override the window's initial size and position if there were cmd line args
                if (State.OverrideStartX != -1)
                {
                    State.DefaultStartingLocation = System.Windows.Forms.FormStartPosition.Manual;
                    x = State.OverrideStartX;
                }
                if (State.OverrideStartY != -1)
                {
                    State.DefaultStartingLocation = System.Windows.Forms.FormStartPosition.Manual;
                    y = State.OverrideStartY;
                }

                // Create the new window
                GraphicsWindow renderWindow = new GraphicsWindow(this);
                // Hook the events
                HookEvents(renderWindow);
                // Are we in the default start location?
                if (State.DefaultStartingLocation == System.Windows.Forms.FormStartPosition.WindowsDefaultLocation)
                {
                    State.IsWindowCreatedWithDefaultPositions = true;
                    renderWindow.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultLocation;
                }
                else
                {
                    State.IsWindowCreatedWithDefaultPositions = false;
                    renderWindow.Location = new System.Drawing.Point(x, y);
                }

                // Calculate the window size
                System.Drawing.Size windowSize = DefaultStartingSize;
                if (State.OverrideWidth != 0)
                    windowSize.Width = State.OverrideWidth;
                if (State.OverrideHeight != 0)
                    windowSize.Height = State.OverrideHeight;
                // Set the size
                renderWindow.ClientSize = windowSize;

                // Store the window title
                State.WindowTitle = windowTitle;
                renderWindow.Text = windowTitle;

                // Set current cursor
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

                // Get the window's rectangles
                renderWindow.Visible = false;
                renderWindow.CreateControl();
                State.ClientRectangle = renderWindow.ClientRectangle;
                State.WindowBoundsRectangle = renderWindow.Bounds;

                // Update state
                State.WasWindowCreated = true;
                State.WindowFocus = renderWindow;
                State.WindowDeviceFullScreen = renderWindow;
                State.WindowDeviceWindowed = renderWindow;
            }
        }

        /// <summary>
        /// Calls into CreateWindow with default paramters
        /// </summary>
        public void CreateWindow(string windowTitle)
        {
            CreateWindow(windowTitle, LoadFirstIconFromResource(), null, -1, -1);
        }
        /// <summary>
        /// Sets a previously created window for the framework to use.  If Init has
        /// not already been called, it will call it with default parameters.  Instead
        /// of calling this you can call CreateWindow to create a new window.
        /// </summary>
        public void SetWindow(System.Windows.Forms.Control windowFocus, System.Windows.Forms.Control windowDeviceFullscreen,
            System.Windows.Forms.Control windowDeviceWindowed, bool handleMessages)
        {
            if (State.IsInsideDeviceCallback)
                throw new InvalidOperationException("You cannot create a window from inside a callback.");

            State.WasWindowCreateCalled = true;

            // To avoid confusion, we do not allow any window handles to be null here.  The
            // caller must pass in valid window handles for all three parameters.  The same
            // window handles may be used for more than one parameter.
            if ((windowDeviceFullscreen == null) || (windowDeviceWindowed == null)
                || (windowFocus == null))
            {
                throw new InvalidOperationException("You must pass in valid window handles.");
            }

            if (handleMessages)
            {
                // Use the static windows procedure defined here
                HookEvents(windowFocus);
            }

            // Are we inited?
            if (!State.IsInited)
            {
                // If Init was already called and failed, fail again
                if (State.WasInitCalled)
                {
                    throw new InvalidOperationException("Initialize was already called and failed.");
                }

                // Call initialize with default params
                Initialize(true, true);
            }

            // Get the window's rectangles
            State.ClientRectangle = windowDeviceWindowed.ClientRectangle;
            State.WindowBoundsRectangle = windowDeviceWindowed.Bounds;

            // Update state
            State.WasWindowCreated = true;
            State.WindowFocus = windowFocus;
            State.WindowDeviceFullScreen = windowDeviceFullscreen;
            State.WindowDeviceWindowed = windowDeviceWindowed;
        }


        /// <summary>
        /// Sets a previously created window for the framework to use.  If Init has
        /// not already been called, it will call it with default parameters.  Instead
        /// of calling this you can call CreateWindow to create a new window.
        /// </summary>
        public void SetWinformWindow(System.Windows.Forms.Control windowFocus, System.Windows.Forms.Control windowDeviceFullscreen,
            System.Windows.Forms.Control windowDeviceWindowed)
        {
            if (State.IsInsideDeviceCallback)
                throw new InvalidOperationException("You cannot create a window from inside a callback.");

            State.WasWindowCreateCalled = true;

            // To avoid confusion, we do not allow any controls to be null here.  The
            // caller must pass in valid windows for all three parameters.  The same
            // window may be used for more than one parameter.
            if ((windowDeviceFullscreen == null) || (windowDeviceWindowed == null)
                || (windowFocus == null))
            {
                throw new InvalidOperationException("You must pass in valid window handles.");
            }

            // Are we inited?
            if (!State.IsInited)
            {
                // If Init was already called and failed, fail again
                if (State.WasInitCalled)
                {
                    throw new InvalidOperationException("Initialize was already called and failed.");
                }

                // Call initialize with default params
                Initialize(true, true);
            }

            // Update state
            State.WasWindowCreated = true;
            State.WindowFocus = windowFocus;
            State.WindowDeviceFullScreen = windowDeviceFullscreen;
            State.WindowDeviceWindowed = windowDeviceWindowed;
        }

        /// <summary>
        /// Creates a Direct3D device.  If CreateWindow or SetWindow has not already
        /// been called, it will call CreateWindow with default parameters.
        /// Instead of calling this, you can call SetDevice or CreateDeviceFromSettings
        /// </summary>
        public void CreateDevice(uint adapterOridinal, bool windowed, int suggestedWidth, int suggestedHeight, IDeviceCreation callback)
        {
            if (State.IsInsideDeviceCallback)
                throw new InvalidOperationException("You cannot create a window from inside a callback.");

            // Store callbacks in global state
            SetDeviceCreationInterface(callback);

            // Update device create being called
            State.WasDeviceCreateCalled = true;

            // Was the window created? If not, create it now
            if (!State.WasWindowCreated)
            {
                // If CreateWindow or SetWindow was already called and failed, then fail again.
                if (State.WasWindowCreateCalled)
                {
                    throw new InvalidOperationException("CreateWindow was already called and failed.");
                }

                // Create a default window
                CreateWindow("Direct3D Window", null, null, -1, -1);
            }

            // Force an enumeration with the updated Device Acceptable callback
            Enumeration.Enumerate(State.DeviceCreationInterface);

            // Set up the match options
            MatchOptions match = new MatchOptions();
            match.AdapterOrdinal = MatchType.PreserveInput;
            match.DeviceType = MatchType.IgnoreInput;
            match.Windowed = MatchType.PreserveInput;
            match.AdapterFormat = MatchType.IgnoreInput;
            match.VertexProcessing = MatchType.IgnoreInput;
            match.Resolution = MatchType.ClosestToInput;
            match.BackBufferFormat = MatchType.IgnoreInput;
            match.BackBufferCount = MatchType.IgnoreInput;
            match.MultiSample = MatchType.IgnoreInput;
            match.SwapEffect = MatchType.IgnoreInput;
            match.DepthFormat = MatchType.IgnoreInput;
            match.StencilFormat = MatchType.IgnoreInput;
            match.PresentFlags = MatchType.IgnoreInput;
            match.RefreshRate = MatchType.IgnoreInput;
            match.PresentInterval = MatchType.IgnoreInput;

            // Get the device settings
            DeviceSettings settings = new DeviceSettings();
            settings.presentParams = new PresentParameters();
            settings.AdapterOrdinal = adapterOridinal;
            settings.presentParams.Windowed = windowed;
            settings.presentParams.BackBufferWidth = suggestedWidth;
            settings.presentParams.BackBufferHeight = suggestedHeight;

            // Override with settings for command line
            if (State.OverrideWidth != 0)
                settings.presentParams.BackBufferWidth = State.OverrideWidth;
            if (State.OverrideHeight != 0)
                settings.presentParams.BackBufferHeight = State.OverrideHeight;
            if (State.OverrideAdapterOrdinal != -1)
                settings.AdapterOrdinal = (uint)State.OverrideAdapterOrdinal;
            
            if (State.IsOverridingFullScreen)
            {
                settings.presentParams.Windowed = false;
                if ((State.OverrideWidth == 0) && (State.OverrideHeight == 0))
                    match.Resolution = MatchType.IgnoreInput;
            }
            if (State.IsOverridingWindowed)
                settings.presentParams.Windowed = true;

            if (State.IsOverridingForceHardware)
            {
                settings.DeviceType = DeviceType.Hardware;
                match.DeviceType = MatchType.PreserveInput;
            }
            if (State.IsOverridingForceReference)
            {
                settings.DeviceType = DeviceType.Reference;
                match.DeviceType = MatchType.PreserveInput;
            }
            if (State.IsOverridingForcePureHardwareVertexProcessing)
            {
                settings.BehaviorFlags = CreateFlags.HardwareVertexProcessing | CreateFlags.PureDevice;
                match.VertexProcessing = MatchType.PreserveInput;
            }
            if (State.IsOverridingForceHardwareVertexProcessing)
            {
                settings.BehaviorFlags = CreateFlags.HardwareVertexProcessing;
                match.VertexProcessing = MatchType.PreserveInput;
            }
            if (State.IsOverridingForceSoftwareVertexProcessing)
            {
                settings.BehaviorFlags = CreateFlags.SoftwareVertexProcessing;
                match.VertexProcessing = MatchType.PreserveInput;
            }

            try
            {
                settings = FindValidDeviceSettings(settings, match);
            }
            catch(Exception e)
            {
                DisplayErrorMessage(e);
                throw;
            }

            // If the modify device callback isn't null, call it to
            // let the app change the settings
            if (State.DeviceCreationInterface != null)
            {
                Caps c = Manager.GetDeviceCaps((int)settings.AdapterOrdinal, settings.DeviceType);
                State.DeviceCreationInterface.ModifyDeviceSettings(settings, c);
            }

            // Change to a Direct3D device created from the new device settings
            // If there is an existing device, either reset or recreate
            ChangeDevice(settings, null, false);
        }


        /// <summary>
        /// Creates a Direct3D device.  If CreateWindow or SetWindow has not already
        /// been called, it will call CreateWindow with default parameters.
        /// Instead of calling this, you can call SetDevice or CreateDeviceFromSettings
        /// This is the CLS compliant version
        /// </summary>
        public void CreateDevice(int adapterOridinal, bool windowed, int suggestedWidth, int suggestedHeight, IDeviceCreation callback)
        {
            CreateDevice((uint)adapterOridinal, windowed, suggestedWidth,
            suggestedHeight, callback);
        }


        /// <summary>
        /// Passes a previously created Direct3D device for use by the framework.  
        /// If CreateWindow() has not already been called, it will call it with the 
        /// default parameters.  Instead of calling this, you can call CreateDevice() or 
        /// CreateDeviceFromSettings() 
        /// </summary>
        public void SetDevice(Device device)
        {
            if (device == null)
                throw new ArgumentNullException("device", "You cannot pass in a null device to SetDevice");

            if (State.IsInsideDeviceCallback)
                throw new InvalidOperationException("You cannot set a device from inside a callback.");

            // Was the window created? If not, create it now
            if (!State.WasWindowCreated)
            {
                // If CreateWindow or SetWindow was already called and failed, then fail again.
                if (State.WasWindowCreateCalled)
                {
                    throw new InvalidOperationException("CreateWindow was already called and failed.");
                }

                // Create a default window
                CreateWindow("Direct3D Window", null, null, -1, -1);
            }

            DeviceSettings deviceSettings = new DeviceSettings();

            // Get the present parameters from the swap chain
            using(Surface backBuffer = device.GetBackBuffer(0, 0, BackBufferType.Mono))
            {
                using (SwapChain swap = backBuffer.GetContainer(InterfaceGuid.SwapChain) as SwapChain)
                {
                    deviceSettings.presentParams = swap.PresentParameters;
                    System.Diagnostics.Debug.Assert(deviceSettings.presentParams != null, "You must have valid present parameters here.");
                }
            }

            DeviceCreationParameters creationParams = device.CreationParameters;

            // Fill out the device settings structure now
            deviceSettings.AdapterOrdinal = (uint)creationParams.AdapterOrdinal;    
            deviceSettings.DeviceType = creationParams.DeviceType;
            deviceSettings.AdapterFormat = FindAdapterFormat(deviceSettings.AdapterOrdinal,
                deviceSettings.DeviceType, deviceSettings.presentParams.BackBufferFormat,
                deviceSettings.presentParams.Windowed);
            deviceSettings.BehaviorFlags = creationParams.Behavior.Value;

            // Change to the Direct3D device passed in
            ChangeDevice(deviceSettings, device, false);
        }

        /// <summary>
        /// Tells the framework to change to a device created from the passed in device settings
        /// If CreateWindow() has not already been called, it will call it with the 
        /// default parameters.  Instead of calling this, you can call CreateDevice() 
        /// or SetDevice() 
        /// </summary>
        public void CreateDeviceFromSettings(DeviceSettings deviceSettings, bool preserveInput)
        {
            // Set the state since this was called
            State.WasDeviceCreateCalled = true;

            // Was the window created? If not, create it now
            if (!State.WasWindowCreated)
            {
                // If CreateWindow or SetWindow was already called and failed, then fail again.
                if (State.WasWindowCreateCalled)
                {
                    throw new InvalidOperationException("CreateWindow was already called and failed.");
                }

                // Create a default window
                CreateWindow("Direct3D Window", null, null, -1, -1);
            }

            if (!preserveInput)
            {
                // If not preserving the input, the find the closest valid to it
                MatchOptions match = new MatchOptions();
                match.AdapterOrdinal = MatchType.ClosestToInput;
                match.DeviceType = MatchType.ClosestToInput;
                match.Windowed = MatchType.ClosestToInput;
                match.AdapterFormat = MatchType.ClosestToInput;
                match.VertexProcessing = MatchType.ClosestToInput;
                match.Resolution = MatchType.ClosestToInput;
                match.BackBufferFormat = MatchType.ClosestToInput;
                match.BackBufferCount = MatchType.ClosestToInput;
                match.MultiSample = MatchType.ClosestToInput;
                match.SwapEffect = MatchType.ClosestToInput;
                match.DepthFormat = MatchType.ClosestToInput;
                match.StencilFormat = MatchType.ClosestToInput;
                match.PresentFlags = MatchType.ClosestToInput;
                match.RefreshRate = MatchType.ClosestToInput;
                match.PresentInterval = MatchType.ClosestToInput;

                try
                {
                    deviceSettings = FindValidDeviceSettings(deviceSettings, match);
                }
                catch(Exception e)
                {
                    // Display any error message
                    DisplayErrorMessage(e);
                    throw;
                }
                // Change to a Direct3D device created from the new device settings.  
                // If there is an existing device, then either reset or recreate the scene
                ChangeDevice(deviceSettings, null, false);
            }
        }
        /// <summary>
        /// Tells the framework to change to a device created from the passed in device settings
        /// If CreateWindow() has not already been called, it will call it with the 
        /// default parameters.  Instead of calling this, you can call CreateDevice() 
        /// or SetDevice() 
        /// </summary>
        public void CreateDeviceFromSettings(DeviceSettings deviceSettings) { CreateDeviceFromSettings(deviceSettings, false); }

        /// <summary>
        /// Toggle between full screen and windowed
        /// </summary>
        public void ToggleFullscreen()
        {
            // Pause the application
            Pause(true, true);

            // Get the current device settings and flip the windowed state then
            // find the closest valid device settings with this change
            DeviceSettings currentSettings = State.CurrentDeviceSettings.Clone();
            currentSettings.presentParams.Windowed = !currentSettings.presentParams.Windowed;

            MatchOptions match = new MatchOptions();
            match.AdapterOrdinal = MatchType.PreserveInput;
            match.DeviceType = MatchType.ClosestToInput;
            match.Windowed = MatchType.PreserveInput;
            match.AdapterFormat = MatchType.IgnoreInput;
            match.VertexProcessing = MatchType.ClosestToInput;
            match.BackBufferFormat = MatchType.IgnoreInput;
            match.BackBufferCount = MatchType.ClosestToInput;
            match.MultiSample = MatchType.ClosestToInput;
            match.SwapEffect = MatchType.ClosestToInput;
            match.DepthFormat = MatchType.ClosestToInput;
            match.StencilFormat = MatchType.ClosestToInput;
            match.PresentFlags = MatchType.ClosestToInput;
            match.RefreshRate = MatchType.IgnoreInput;
            match.PresentInterval = MatchType.IgnoreInput;

            System.Drawing.Rectangle windowClient;
            if (currentSettings.presentParams.Windowed)
                windowClient = State.ClientRectangle;
            else
                windowClient = State.FullScreenClientRectangle;

            if (windowClient.Width > 0 && windowClient.Height > 0)
            {
                match.Resolution = MatchType.ClosestToInput;
                currentSettings.presentParams.BackBufferWidth = windowClient.Width;
                currentSettings.presentParams.BackBufferHeight = windowClient.Height;
            }
            else
            {
                match.Resolution = MatchType.IgnoreInput;
            }

            try
            {
                if ( (!currentSettings.presentParams.Windowed) && (State.IsMaximized))
                {
                    if (WindowForm != null)
                    {
                        WindowForm.WindowState = System.Windows.Forms.FormWindowState.Normal;
                    }
                    toggleMaximized = true;
                }
                currentSettings = FindValidDeviceSettings(currentSettings, match);
                ChangeDevice(currentSettings, null, false);
                Window.Size = State.WindowBoundsRectangle.Size;                
                Window.Location = State.WindowBoundsRectangle.Location;                
                if (currentSettings.presentParams.Windowed)
                {
                    if (toggleMaximized)
                    {
                        if (WindowForm != null)
                        {
                            WindowForm.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                        }
                    }
                    toggleMaximized = false;
                }
            }
            finally
            {
                // Well, unpause no matter what
                Pause(false, false);
                State.CurrentDeviceSettings = currentSettings;
            }
        }

        
        /// <summary>
        /// Toggle between Hardware and Reference
        /// </summary>
        public void ToggleReference()
        {
#if !DEBUG
            return;
#endif
            // Get the current device settings 
            DeviceSettings currentSettings = State.CurrentDeviceSettings.Clone();
            if (currentSettings.DeviceType == DeviceType.Hardware)
                currentSettings.DeviceType = DeviceType.Reference;
            else if (currentSettings.DeviceType == DeviceType.Reference)
                currentSettings.DeviceType = DeviceType.Hardware;

            MatchOptions match = new MatchOptions();
            match.AdapterOrdinal = MatchType.PreserveInput;
            match.DeviceType = MatchType.PreserveInput;
            match.Windowed = MatchType.ClosestToInput;
            match.AdapterFormat = MatchType.ClosestToInput;
            match.VertexProcessing = MatchType.ClosestToInput;
            match.Resolution = MatchType.ClosestToInput;
            match.BackBufferFormat = MatchType.ClosestToInput;
            match.BackBufferCount = MatchType.ClosestToInput;
            match.MultiSample = MatchType.ClosestToInput;
            match.SwapEffect = MatchType.ClosestToInput;
            match.DepthFormat = MatchType.ClosestToInput;
            match.StencilFormat = MatchType.ClosestToInput;
            match.PresentFlags = MatchType.ClosestToInput;
            match.RefreshRate = MatchType.ClosestToInput;
            match.PresentInterval = MatchType.ClosestToInput;

            try
            {
                currentSettings = FindValidDeviceSettings(currentSettings, match);
                ChangeDevice(currentSettings, null, false);
            }
#if(DEBUG)
            catch (Exception e)
            {
                // In debug mode show this error (maybe - depending on settings)
                DisplayErrorMessage(e);
#else
            catch
            {
                // In release mode fail silently
#endif
            }
            finally
            {
                // Update the current settings
                State.CurrentDeviceSettings = currentSettings;
            }
        }

        /// <summary>
        /// This function tries to find valid device settings based upon the input device settings 
        /// struct and the match options.  For each device setting a match option in the 
        /// MatchOptions struct specifies how the function makes decisions.  For example, if 
        /// the caller wants a hardware device with a back buffer format of A2B10G10R10 but the 
        /// hardware device on the system does not support A2B10G10R10 however a reference device is 
        /// installed that does, then the function has a choice to either use the reference device
        /// or to change to a back buffer format to compatible with the hardware device.  The match options lets the 
        /// caller control how these choices are made.
        /// 
        /// Each match option must be one of the following types: 
        /// MatchType.IgnoreInput: Uses the closest valid value to a default 
        /// MatchType.PreserveInput: Uses the input without change, but may cause no valid device to be found
        /// MatchType.ClosestToInput: Uses the closest valid value to the input 
        /// </summary>
        private DeviceSettings FindValidDeviceSettings(DeviceSettings settings, MatchOptions match)
        {
            // Build an optimal device settings structure based upon the match 
            // options.  If the match option is set to ignore, then a optimal default value is used.
            // The default value may not exist on the system, but later this will be taken 
            // into account.
            DeviceSettings optimalSettings = BuildOptimalDeviceSettings(settings, match);
            float bestRanking = -1.0f;
            EnumDeviceSettingsCombo bestDeviceSettingsCombo = new EnumDeviceSettingsCombo();

            // Find the best combination of:
            //      Adapter Ordinal
            //      Device Type
            //      Adapter Format
            //      Back Buffer Format
            //      Windowed
            // given what's available on the system and the match options combined with the device settings input.
            // This combination of settings is encapsulated by the EnumDeviceSettingsCombo class.
            DisplayMode adapterDesktopDisplayMode;
            for (int iAdapter = 0; iAdapter < Enumeration.AdapterInformationList.Count; iAdapter++)
            {
                EnumAdapterInformation adapterInfo = Enumeration.AdapterInformationList[iAdapter] as EnumAdapterInformation;
                
                // Get the desktop display mode of the adapter
                adapterDesktopDisplayMode = Manager.Adapters[(int)adapterInfo.AdapterOrdinal].CurrentDisplayMode;

                // Enum all the device types supported by this adapter to find the best device settings
                for (int iDeviceInfo = 0; iDeviceInfo < adapterInfo.deviceInfoList.Count; iDeviceInfo++)
                {
                    EnumDeviceInformation deviceInfo = adapterInfo.deviceInfoList[iDeviceInfo] as EnumDeviceInformation;
                    for (int iDeviceCombo = 0; iDeviceCombo<deviceInfo.deviceSettingsList.Count; iDeviceCombo++)
                    {
                        EnumDeviceSettingsCombo deviceSettings = deviceInfo.deviceSettingsList[iDeviceCombo] as EnumDeviceSettingsCombo;
                        // If windowed mode the adapter format has to be the same as the desktop 
                        // display mode format so skip any that don't match
                        if (deviceSettings.IsWindowed && (deviceSettings.AdapterFormat != adapterDesktopDisplayMode.Format))
                            continue;

                        // Skip any combo that doesn't meet the preserve match options
                        if(!DoesDeviceComboMatchPreserveOptions(deviceSettings, settings, match))
                            continue;           

                        // Get a ranking number that describes how closely this device combo matches the optimal combo
                        float curRanking = RankDeviceCombo(deviceSettings, optimalSettings, adapterDesktopDisplayMode);

                        // If this combo better matches the input device settings then save it
                        if (curRanking > bestRanking )
                        {
                            bestDeviceSettingsCombo = deviceSettings;
                            bestRanking = curRanking;
                        }                
                    }
                }
            }

            // If no best device combination was found then fail
            if (bestRanking == -1.0f)
            {
                throw new NoCompatibleDevicesException();
            }

            // Using the best device settings combo found, build valid device settings taking heed of 
            // the match options and the input device settings
            return BuildValidDeviceSettings(bestDeviceSettingsCombo, settings, match);
        }
        /// <summary>
        /// Calls FindValidDeviceSettings with default match options (all ignore)
        /// </summary>
        private DeviceSettings FindValidDeviceSettings(DeviceSettings settings)
        {
            return FindValidDeviceSettings(settings, new MatchOptions());
        }

        /// <summary>
        /// public helper function to build a device settings structure based upon the match 
        /// options.  If the match option is set to ignore, then a optimal default value is used.
        /// The default value may not exist on the system, but later this will be taken 
        /// into account.
        /// </summary>
        private DeviceSettings BuildOptimalDeviceSettings(DeviceSettings settings, MatchOptions match)
        {
            DeviceSettings optimal = new DeviceSettings(); // This will be what we return
            optimal.presentParams = new PresentParameters();

            //---------------------
            // Adapter ordinal
            //---------------------    
            if (match.AdapterOrdinal == MatchType.IgnoreInput)
                optimal.AdapterOrdinal = 0; 
            else
                optimal.AdapterOrdinal = settings.AdapterOrdinal;      

            //---------------------
            // Device type
            //---------------------
            if (match.DeviceType == MatchType.IgnoreInput)
                optimal.DeviceType = DeviceType.Hardware; 
            else
                optimal.DeviceType = settings.DeviceType;

            //---------------------
            // Windowed
            //---------------------
            if (match.Windowed == MatchType.IgnoreInput)
                optimal.presentParams.Windowed = true; 
            else
                optimal.presentParams.Windowed = settings.presentParams.Windowed;

            //---------------------
            // Adapter format
            //---------------------
            if (match.AdapterFormat == MatchType.IgnoreInput)
            {
                // If windowed, default to the desktop display mode
                // If fullscreen, default to the desktop display mode for quick mode change or 
                // default to Format.X8R8G8B8 if the desktop display mode is < 32bit
                DisplayMode adapterDesktopMode = Manager.Adapters[(int)optimal.AdapterOrdinal].CurrentDisplayMode;

                if (optimal.presentParams.Windowed || 
                    ManagedUtility.GetColorChannelBits(adapterDesktopMode.Format) >= 8 )
                    optimal.AdapterFormat = adapterDesktopMode.Format;
                else
                    optimal.AdapterFormat = Format.X8R8G8B8;
            }
            else
            {
                optimal.AdapterFormat = settings.AdapterFormat;
            }

            //---------------------
            // Vertex processing
            //---------------------
            if (match.VertexProcessing == MatchType.IgnoreInput)
                optimal.BehaviorFlags = CreateFlags.HardwareVertexProcessing; 
            else
                optimal.BehaviorFlags = settings.BehaviorFlags;

            //---------------------
            // Resolution
            //---------------------
            if (match.Resolution == MatchType.IgnoreInput)
            {
                // If windowed, default to 640x480
                // If fullscreen, default to the desktop res for quick mode change
                if (optimal.presentParams.Windowed )
                {
                    optimal.presentParams.BackBufferWidth = DefaultSizeWidth;
                    optimal.presentParams.BackBufferHeight = DefaultSizeHeight;
                }
                else
                {
                    DisplayMode adapterDesktopMode = Manager.Adapters[(int)optimal.AdapterOrdinal].CurrentDisplayMode;

                    optimal.presentParams.BackBufferWidth = adapterDesktopMode.Width;
                    optimal.presentParams.BackBufferHeight = adapterDesktopMode.Height;
                }
            }
            else
            {
                optimal.presentParams.BackBufferWidth = settings.presentParams.BackBufferWidth;
                optimal.presentParams.BackBufferHeight = settings.presentParams.BackBufferHeight;
            }

            //---------------------
            // Back buffer format
            //---------------------
            if (match.BackBufferFormat == MatchType.IgnoreInput)
                optimal.presentParams.BackBufferFormat = optimal.AdapterFormat; // Default to match the adapter format
            else
                optimal.presentParams.BackBufferFormat = settings.presentParams.BackBufferFormat;

            //---------------------
            // Back buffer count
            //---------------------
            if (match.BackBufferCount == MatchType.IgnoreInput)
                optimal.presentParams.BackBufferCount = 2; // Default to triple buffering for perf gain
            else
                optimal.presentParams.BackBufferCount = settings.presentParams.BackBufferCount;
   
            //---------------------
            // Multisample
            //---------------------
            if (match.MultiSample == MatchType.IgnoreInput)
                optimal.presentParams.MultiSampleQuality = 0; // Default to no multisampling 
            else
                optimal.presentParams.MultiSampleQuality = settings.presentParams.MultiSampleQuality;

            //---------------------
            // Swap effect
            //---------------------
            if (match.SwapEffect == MatchType.IgnoreInput)
                optimal.presentParams.SwapEffect = SwapEffect.Discard; 
            else
                optimal.presentParams.SwapEffect = settings.presentParams.SwapEffect;

            //---------------------
            // Depth stencil 
            //---------------------
            if (match.DepthFormat == MatchType.IgnoreInput &&
                match.StencilFormat == MatchType.IgnoreInput)
            {
                uint backBufferBits = ManagedUtility.GetColorChannelBits(optimal.presentParams.BackBufferFormat);
                if (backBufferBits >= 8)
                    optimal.presentParams.AutoDepthStencilFormat = DepthFormat.D32; 
                else
                    optimal.presentParams.AutoDepthStencilFormat = DepthFormat.D16; 
            }
            else
            {
                optimal.presentParams.AutoDepthStencilFormat = settings.presentParams.AutoDepthStencilFormat;
            }

            //---------------------
            // Present flags
            //---------------------
            if (match.PresentFlags == MatchType.IgnoreInput)
                optimal.presentParams.PresentFlag = PresentFlag.DiscardDepthStencil;
            else
                optimal.presentParams.PresentFlag = settings.presentParams.PresentFlag;

            //---------------------
            // Refresh rate
            //---------------------
            if (match.RefreshRate == MatchType.IgnoreInput)
                optimal.presentParams.FullScreenRefreshRateInHz = 0;
            else
                optimal.presentParams.FullScreenRefreshRateInHz = settings.presentParams.FullScreenRefreshRateInHz;

            //---------------------
            // Present interval
            //---------------------
            if (match.PresentInterval == MatchType.IgnoreInput)
            {
                // For windowed, default to PresentInterval.Immediate
                // which will wait not for the vertical retrace period to prevent tearing, 
                // but may introduce tearing.
                // For full screen, default to PresentInterval.Default 
                // which will wait for the vertical retrace period to prevent tearing.
                if (optimal.presentParams.Windowed )
                    optimal.presentParams.PresentationInterval = PresentInterval.Immediate;
                else
                    optimal.presentParams.PresentationInterval = PresentInterval.Default;
            }
            else
            {
                optimal.presentParams.PresentationInterval = settings.presentParams.PresentationInterval;
            }

            return optimal;
        }


        /// <summary>
        /// Builds valid device settings using the match options, the input device settings, and the 
        /// best device settings combo found.
        /// </summary>
        private DeviceSettings BuildValidDeviceSettings(EnumDeviceSettingsCombo deviceCombo, DeviceSettings settings, MatchOptions match)
        {
            DeviceSettings validSettings = new DeviceSettings();
            DisplayMode adapterDesktopDisplayMode = Manager.Adapters[(int)deviceCombo.AdapterOrdinal].CurrentDisplayMode;

            // For each setting pick the best, taking into account the match options and 
            // what's supported by the device

            //---------------------
            // Adapter Ordinal
            //---------------------
            // Just using deviceCombo.AdapterOrdinal

            //---------------------
            // Device Type
            //---------------------
            // Just using deviceCombo.DeviceType

            //---------------------
            // Windowed 
            //---------------------
            // Just using deviceCombo.Windowed

            //---------------------
            // Adapter Format
            //---------------------
            // Just using deviceCombo.AdapterFormat

            //---------------------
            // Vertex processing
            //---------------------
            CreateFlags bestBehaviorFlags = 0;
            if (match.VertexProcessing == MatchType.PreserveInput )   
            {
                bestBehaviorFlags = settings.BehaviorFlags;
            }
            else if (match.VertexProcessing == MatchType.IgnoreInput )    
            {
                // The framework defaults to HWVP if available otherwise use SWVP
                if (deviceCombo.deviceInformation.Caps.DeviceCaps.SupportsHardwareTransformAndLight)
                    bestBehaviorFlags |= CreateFlags.HardwareVertexProcessing;
                else
                    bestBehaviorFlags |= CreateFlags.SoftwareVertexProcessing;
            }
            else 
            {
                // Default to input, and fallback to SWVP if HWVP not available 
                bestBehaviorFlags = settings.BehaviorFlags;
                if ((!deviceCombo.deviceInformation.Caps.DeviceCaps.SupportsHardwareTransformAndLight) && 
                    ( (bestBehaviorFlags & CreateFlags.HardwareVertexProcessing ) != 0 || 
                    (bestBehaviorFlags & CreateFlags.MixedVertexProcessing) != 0) )
                {
                    bestBehaviorFlags &= ~CreateFlags.HardwareVertexProcessing ;
                    bestBehaviorFlags &= ~CreateFlags.MixedVertexProcessing;
                    bestBehaviorFlags |= CreateFlags.SoftwareVertexProcessing;
                }

                // One of these must be selected
                if ((bestBehaviorFlags & CreateFlags.HardwareVertexProcessing ) == 0 &&
                    (bestBehaviorFlags & CreateFlags.MixedVertexProcessing) == 0 &&
                    (bestBehaviorFlags & CreateFlags.SoftwareVertexProcessing) == 0 )
                {
                    if (deviceCombo.deviceInformation.Caps.DeviceCaps.SupportsHardwareTransformAndLight)
                        bestBehaviorFlags |= CreateFlags.HardwareVertexProcessing ;
                    else
                        bestBehaviorFlags |= CreateFlags.SoftwareVertexProcessing;
                }
            }

            //---------------------
            // Resolution
            //---------------------
            DisplayMode bestDisplayMode = new DisplayMode();  
            if (match.Resolution == MatchType.PreserveInput )   
            {
                bestDisplayMode.Width = settings.presentParams.BackBufferWidth;
                bestDisplayMode.Height = settings.presentParams.BackBufferHeight;
            }
            else 
            {
                DisplayMode displayModeIn = new DisplayMode();  
                if (match.Resolution == MatchType.ClosestToInput &&
                    (settings.presentParams.BackBufferWidth != 0 && settings.presentParams.BackBufferWidth != 0) )   
                {
                    displayModeIn.Width = settings.presentParams.BackBufferWidth;
                    displayModeIn.Height = settings.presentParams.BackBufferHeight;
                }
                else // if (match.Resolution == MatchType.IgnoreInput )   
                {
                    if (deviceCombo.IsWindowed )
                    {
                        // The framework defaults to 640x480 for windowed
                        displayModeIn.Width = DefaultSizeWidth;
                        displayModeIn.Height = DefaultSizeHeight;
                    }
                    else
                    {
                        // The framework defaults to desktop resolution for fullscreen to try to avoid slow mode change
                        displayModeIn.Width = adapterDesktopDisplayMode.Width;
                        displayModeIn.Height = adapterDesktopDisplayMode.Height;
                    }
                }

                // Call a helper function to find the closest valid display mode to the optimal 
                bestDisplayMode = FindValidResolution(deviceCombo, displayModeIn);
            }

            //---------------------
            // Back Buffer Format
            //---------------------
            // Just using deviceCombo.BackBufferFormat

            //---------------------
            // Back buffer count
            //---------------------
            uint bestBackBufferCount;
            if (match.BackBufferCount == MatchType.PreserveInput )   
            {
                bestBackBufferCount = (uint)settings.presentParams.BackBufferCount;
            }
            else if (match.BackBufferCount == MatchType.IgnoreInput )   
            {
                // The framework defaults to triple buffering 
                bestBackBufferCount = 2;
            }
            else // if (match.BackBufferCount == MatchType.ClosestToInput )   
            {
                bestBackBufferCount = (uint)settings.presentParams.BackBufferCount;
                if (bestBackBufferCount > 3 )
                    bestBackBufferCount = 3;
                if (bestBackBufferCount < 1 )
                    bestBackBufferCount = 1;
            }
    
            //---------------------
            // Multisample
            //---------------------
            MultiSampleType bestMultiSampleType;
            uint bestMultiSampleQuality;
            if (settings.presentParams.SwapEffect != SwapEffect.Discard)
            {
                // Swap effect is not set to discard so multisampling has to off
                bestMultiSampleType = MultiSampleType.None;
                bestMultiSampleQuality = 0;
            }
            else
            {
                if (match.BackBufferCount == MatchType.PreserveInput )   
                {
                    bestMultiSampleType    = settings.presentParams.MultiSample;
                    bestMultiSampleQuality = (uint)settings.presentParams.MultiSampleQuality;
                }
                else if (match.BackBufferCount == MatchType.IgnoreInput )   
                {
                    // Default to no multisampling (always supported)
                    bestMultiSampleType = MultiSampleType.None;
                    bestMultiSampleQuality = 0;
                }
                else if (match.BackBufferCount == MatchType.ClosestToInput )   
                {
                    // Default to no multisampling (always supported)
                    bestMultiSampleType = MultiSampleType.None;
                    bestMultiSampleQuality = 0;

                    for (int i = 0; i < deviceCombo.multiSampleTypeList.Count; i++)
                    {
                        MultiSampleType tempType = (MultiSampleType)deviceCombo.multiSampleTypeList[i];
                        uint tempQuality = (uint)(int)deviceCombo.multiSampleQualityList[i];

                        // Check whether supported type is closer to the input than our current best
                        if (Math.Abs((int)tempType -  (int)settings.presentParams.MultiSample) < Math.Abs((int)bestMultiSampleType - (int)settings.presentParams.MultiSample) )
                        {
                            bestMultiSampleType = tempType;
                            bestMultiSampleQuality = (uint)Math.Min(tempQuality-1, settings.presentParams.MultiSampleQuality);
                        }
                    }
                }
                else // Error case
                {
                    // Default to no multisampling (always supported) 
                    bestMultiSampleType = MultiSampleType.None;
                    bestMultiSampleQuality = 0;
                }
            }

            //---------------------
            // Swap effect
            //---------------------
            SwapEffect bestSwapEffect;
            if (match.SwapEffect == MatchType.PreserveInput )   
            {
                bestSwapEffect = settings.presentParams.SwapEffect;
            }
            else if (match.SwapEffect == MatchType.IgnoreInput )   
            {
                bestSwapEffect = SwapEffect.Discard;
            }
            else // if (match.SwapEffect == MatchType.ClosestToInput )   
            {
                bestSwapEffect = settings.presentParams.SwapEffect;

                // Swap effect has to be one of these 3
                if (bestSwapEffect != SwapEffect.Discard &&
                    bestSwapEffect != SwapEffect.Flip &&
                    bestSwapEffect != SwapEffect.Copy )
                {
                    bestSwapEffect = SwapEffect.Discard;
                }
            }

            //---------------------
            // Depth stencil 
            //---------------------
            DepthFormat bestDepthStencilFormat;
            bool bestEnableAutoDepthStencil;

            int[] depthStencilRanking = new int[deviceCombo.depthStencilFormatList.Count];

            uint backBufferBitDepth = ManagedUtility.GetColorChannelBits( deviceCombo.BackBufferFormat );       
            uint inputDepthBitDepth = ManagedUtility.GetDepthBits( settings.presentParams.AutoDepthStencilFormat );

            for( int i=0; i<deviceCombo.depthStencilFormatList.Count; i++ )
            {
                DepthFormat curDepthStencilFmt = (DepthFormat)deviceCombo.depthStencilFormatList[i];
                uint curDepthBitDepth = ManagedUtility.GetDepthBits( curDepthStencilFmt );
                int ranking;

                if (match.DepthFormat == MatchType.PreserveInput )
                {                       
                    // Need to match bit depth of input
                    if(curDepthBitDepth == inputDepthBitDepth)
                        ranking = 0;
                    else
                        ranking = 10000;
                }
                else if (match.DepthFormat == MatchType.IgnoreInput )
                {
                    // Prefer match of backbuffer bit depth
                    ranking = Math.Abs((int)curDepthBitDepth - (int)(backBufferBitDepth*4));
                }
                else // if (match.DepthFormat == MatchType.ClosestToInput )
                {
                    // Prefer match of input depth format bit depth
                    ranking = Math.Abs((int)curDepthBitDepth - (int)inputDepthBitDepth);
                }

                depthStencilRanking[i] = ranking;
            }

            uint inputStencilBitDepth = ManagedUtility.GetStencilBits( settings.presentParams.AutoDepthStencilFormat );

            for( int i=0; i<deviceCombo.depthStencilFormatList.Count; i++ )
            {
                DepthFormat curDepthStencilFmt = (DepthFormat)deviceCombo.depthStencilFormatList[i];
                int ranking = depthStencilRanking[i];
                uint curStencilBitDepth = ManagedUtility.GetStencilBits( curDepthStencilFmt );

                if (match.StencilFormat == MatchType.PreserveInput )
                {                       
                    // Need to match bit depth of input
                    if(curStencilBitDepth == inputStencilBitDepth)
                        ranking += 0;
                    else
                        ranking += 10000;
                }
                else if (match.StencilFormat == MatchType.IgnoreInput )
                {
                    // Prefer 0 stencil bit depth
                    ranking += (int)curStencilBitDepth;
                }
                else // if (match.StencilFormat == MatchType.ClosestToInput )
                {
                    // Prefer match of input stencil format bit depth
                    ranking += Math.Abs((int)curStencilBitDepth - (int)inputStencilBitDepth);
                }

                depthStencilRanking[i] = ranking;
            }

            int bestRanking = 100000;
            int bestIndex = -1;
            for( int i=0; i<depthStencilRanking.Length; i++ )
            {
                if (depthStencilRanking[i] < bestRanking )
                {
                    bestRanking = depthStencilRanking[i];
                    bestIndex = i;
                }
            }

            if (bestIndex >= 0 )
            {
                bestDepthStencilFormat = (DepthFormat)deviceCombo.depthStencilFormatList[bestIndex];
                bestEnableAutoDepthStencil = true;
            }
            else
            {
                bestDepthStencilFormat = DepthFormat.Unknown;
                bestEnableAutoDepthStencil = false;
            }


            //---------------------
            // Present flags
            //---------------------
            PresentFlag bestPresentFlag;
            if (match.PresentFlags == MatchType.PreserveInput )   
            {
                bestPresentFlag = settings.presentParams.PresentFlag;
            }
            else if (match.PresentFlags == MatchType.IgnoreInput )   
            {
                bestPresentFlag = 0;
                if (bestEnableAutoDepthStencil )
                    bestPresentFlag = PresentFlag.DiscardDepthStencil;            
            }
            else // if (match.PresentFlags == MatchType.ClosestToInput )   
            {
                bestPresentFlag = settings.presentParams.PresentFlag;
                if (bestEnableAutoDepthStencil )
                    bestPresentFlag |= PresentFlag.DiscardDepthStencil;
            }

            //---------------------
            // Refresh rate
            //---------------------
            if (deviceCombo.IsWindowed )
            {
                // Must be 0 for windowed
                bestDisplayMode.RefreshRate = 0;
            }
            else
            {
                if (match.RefreshRate == MatchType.PreserveInput )   
                {
                    bestDisplayMode.RefreshRate = settings.presentParams.FullScreenRefreshRateInHz;
                }
                else 
                {
                    uint refreshRateMatch;
                    if (match.RefreshRate == MatchType.ClosestToInput )   
                    {
                        refreshRateMatch = (uint)settings.presentParams.FullScreenRefreshRateInHz;
                    }
                    else // if (match.RefreshRate == MatchType.IgnoreInput )   
                    {
                        refreshRateMatch = (uint)adapterDesktopDisplayMode.RefreshRate;
                    }

                    bestDisplayMode.RefreshRate = 0;

                    if (refreshRateMatch != 0 )
                    {
                        int bestRefreshRanking = 100000;
                        for( int iDisplayMode=0; iDisplayMode<deviceCombo.adapterInformation.displayModeList.Count; iDisplayMode++ )
                        {
                            DisplayMode displayMode = (DisplayMode)deviceCombo.adapterInformation.displayModeList[iDisplayMode];                
                            if (displayMode.Format != deviceCombo.AdapterFormat || 
                                displayMode.Height != bestDisplayMode.Height ||
                                displayMode.Width != bestDisplayMode.Width )
                                continue; // Skip display modes that don't match 

                            // Find the delta between the current refresh rate and the optimal refresh rate 
                            int currentRefreshRanking = Math.Abs((int)displayMode.RefreshRate - (int)refreshRateMatch);
                                        
                            if (currentRefreshRanking < bestRefreshRanking )
                            {
                                bestDisplayMode.RefreshRate = displayMode.RefreshRate;
                                bestRefreshRanking = currentRefreshRanking;

                                // Stop if perfect match found
                                if (bestRefreshRanking == 0 )
                                    break;
                            }
                        }
                    }
                }
            }

            //---------------------
            // Present interval
            //---------------------
            PresentInterval bestPresentInterval;
            if (match.PresentInterval == MatchType.PreserveInput )   
            {
                bestPresentInterval = settings.presentParams.PresentationInterval;
            }
            else if (match.PresentInterval == MatchType.IgnoreInput )   
            {
                if (deviceCombo.IsWindowed )
                {
                    // For windowed, the framework defaults to PresentInterval.Immediate
                    // which will wait not for the vertical retrace period to prevent tearing, 
                    // but may introduce tearing
                    bestPresentInterval = PresentInterval.Immediate;
                }
                else
                {
                    // For full screen, the framework defaults to PresentInterval.Default 
                    // which will wait for the vertical retrace period to prevent tearing
                    bestPresentInterval = PresentInterval.Default;
                }
            }
            else // if (match.PresentInterval == MatchType.ClosestToInput )   
            {
                if (deviceCombo.presentIntervalList.Contains( settings.presentParams.PresentationInterval ) )
                {
                    bestPresentInterval = settings.presentParams.PresentationInterval;
                }
                else
                {
                    if (deviceCombo.IsWindowed )
                        bestPresentInterval = PresentInterval.Immediate;
                    else
                        bestPresentInterval = PresentInterval.Default;
                }
            }

            // Fill the device settings struct
            validSettings.AdapterOrdinal = deviceCombo.AdapterOrdinal;
            validSettings.DeviceType = deviceCombo.DeviceType;
            validSettings.AdapterFormat = deviceCombo.AdapterFormat;
            validSettings.BehaviorFlags = bestBehaviorFlags;
            validSettings.presentParams = new PresentParameters();
            validSettings.presentParams.BackBufferWidth = bestDisplayMode.Width;
            validSettings.presentParams.BackBufferHeight = bestDisplayMode.Height;
            validSettings.presentParams.BackBufferFormat = deviceCombo.BackBufferFormat;
            validSettings.presentParams.BackBufferCount = (int)bestBackBufferCount;
            validSettings.presentParams.MultiSample = bestMultiSampleType;  
            validSettings.presentParams.MultiSampleQuality = (int)bestMultiSampleQuality;
            validSettings.presentParams.SwapEffect = bestSwapEffect;
            validSettings.presentParams.DeviceWindow = deviceCombo.IsWindowed ? State.WindowDeviceWindowed : State.WindowDeviceFullScreen;
            validSettings.presentParams.Windowed = deviceCombo.IsWindowed;
            validSettings.presentParams.EnableAutoDepthStencil = bestEnableAutoDepthStencil;  
            validSettings.presentParams.AutoDepthStencilFormat = bestDepthStencilFormat;
            validSettings.presentParams.PresentFlag = bestPresentFlag;                   
            validSettings.presentParams.FullScreenRefreshRateInHz  = bestDisplayMode.RefreshRate;
            validSettings.presentParams.PresentationInterval = bestPresentInterval;
            validSettings.presentParams.ForceNoMultiThreadedFlag = true;

            return validSettings;
        }

        
        /// <summary>
        /// Returns false for any device combo that doesn't meet the preserve
        /// match options
        /// </summary>
        private bool DoesDeviceComboMatchPreserveOptions(EnumDeviceSettingsCombo deviceCombo, DeviceSettings settings, MatchOptions match)
        {
            //---------------------
            // Adapter ordinal
            //---------------------
            if (match.AdapterOrdinal == MatchType.PreserveInput && 
                (deviceCombo.AdapterOrdinal != settings.AdapterOrdinal) )
                return false;

            //---------------------
            // Device type
            //---------------------
            if (match.DeviceType == MatchType.PreserveInput && 
                (deviceCombo.DeviceType != settings.DeviceType) )
                return false;

            //---------------------
            // Windowed
            //---------------------
            if (match.Windowed == MatchType.PreserveInput && 
                (deviceCombo.IsWindowed != settings.presentParams.Windowed) )
                return false;

            //---------------------
            // Adapter format
            //---------------------
            if (match.AdapterFormat == MatchType.PreserveInput && 
                (deviceCombo.AdapterFormat != settings.AdapterFormat) )
                return false;

            //---------------------
            // Vertex processing
            //---------------------
            // If keep VP and input has HWVP, then skip if this combo doesn't have HWTL 
            if (match.VertexProcessing == MatchType.PreserveInput && 
                ((settings.BehaviorFlags & CreateFlags.HardwareVertexProcessing) != 0) && 
                (deviceCombo.deviceInformation.Caps.DeviceCaps.SupportsHardwareTransformAndLight) )
                return false;

            //---------------------
            // Resolution
            //---------------------
            // If keep resolution then check that width and height supported by this combo
            if (match.Resolution == MatchType.PreserveInput )
            {
                bool bFound = false;
                for( int i=0; i< deviceCombo.adapterInformation.displayModeList.Count; i++ )
                {
                    DisplayMode displayMode = (DisplayMode)deviceCombo.adapterInformation.displayModeList[i];
                    if (displayMode.Format != deviceCombo.AdapterFormat )
                        continue; // Skip this display mode if it doesn't match the combo's adapter format

                    if (displayMode.Width == settings.presentParams.BackBufferWidth &&
                        displayMode.Height == settings.presentParams.BackBufferHeight )
                    {
                        bFound = true;
                        break;
                    }
                }

                // If the width and height are not supported by this combo, return false
                if (!bFound )
                    return false;
            }

            //---------------------
            // Back buffer format
            //---------------------
            if (match.BackBufferFormat == MatchType.PreserveInput && 
                deviceCombo.BackBufferFormat != settings.presentParams.BackBufferFormat )
                return false;

            //---------------------
            // Back buffer count
            //---------------------
            // No caps for the back buffer count

            //---------------------
            // Multisample
            //---------------------
            if (match.MultiSample == MatchType.PreserveInput )
            {
                bool bFound = false;
                for( int i=0; i<deviceCombo.multiSampleTypeList.Count; i++ )
                {
                    MultiSampleType msType = (MultiSampleType)deviceCombo.multiSampleTypeList[i];
                    uint msQuality  = (uint)(int)deviceCombo.multiSampleQualityList[i];

                    if (msType == settings.presentParams.MultiSample &&
                        msQuality >= settings.presentParams.MultiSampleQuality )
                    {
                        bFound = true;
                        break;
                    }
                }

                // If multisample type/quality not supported by this combo, then return false
                if (!bFound )
                    return false;
            }
        
            //---------------------
            // Swap effect
            //---------------------
            // No caps for swap effects

            //---------------------
            // Depth stencil 
            //---------------------
            // If keep depth stencil format then check that the depth stencil format is supported by this combo
            if (match.DepthFormat == MatchType.PreserveInput &&
                match.StencilFormat == MatchType.PreserveInput )
            {
                if (settings.presentParams.AutoDepthStencilFormat != (DepthFormat)Format.Unknown &&
                    !deviceCombo.depthStencilFormatList.Contains( settings.presentParams.AutoDepthStencilFormat ) )
                    return false;
            }

            // If keep depth format then check that the depth format is supported by this combo
            if (match.DepthFormat == MatchType.PreserveInput &&
                settings.presentParams.AutoDepthStencilFormat != DepthFormat.Unknown )
            {
                bool bFound = false;
                uint depthBits = ManagedUtility.GetDepthBits( settings.presentParams.AutoDepthStencilFormat );
                for( int i=0; i<deviceCombo.depthStencilFormatList.Count; i++ )
                {
                    DepthFormat depthStencilFmt = (DepthFormat)deviceCombo.depthStencilFormatList[i];
                    uint curDepthBits = ManagedUtility.GetDepthBits(depthStencilFmt);
                    if (curDepthBits - depthBits == 0)
                        bFound = true;
                }

                if (!bFound )
                    return false;
            }

            // If keep depth format then check that the depth format is supported by this combo
            if (match.StencilFormat == MatchType.PreserveInput &&
                settings.presentParams.AutoDepthStencilFormat != DepthFormat.Unknown )
            {
                bool bFound = false;
                uint stencilBits = ManagedUtility.GetStencilBits( settings.presentParams.AutoDepthStencilFormat );
                for( int i=0; i<deviceCombo.depthStencilFormatList.Count; i++ )
                {
                    DepthFormat depthStencilFmt = (DepthFormat)deviceCombo.depthStencilFormatList[i];
                    uint curStencilBits = ManagedUtility.GetStencilBits(depthStencilFmt);
                    if (curStencilBits - stencilBits == 0)
                        bFound = true;
                }

                if (!bFound )
                    return false;
            }

            //---------------------
            // Present flags
            //---------------------
            // No caps for the present flags

            //---------------------
            // Refresh rate
            //---------------------
            // If keep refresh rate then check that the resolution is supported by this combo
            if (match.RefreshRate == MatchType.PreserveInput )
            {
                bool bFound = false;
                for( int i=0; i<deviceCombo.adapterInformation.displayModeList.Count; i++ )
                {
                    DisplayMode displayMode = (DisplayMode)deviceCombo.adapterInformation.displayModeList[i];
                    if (displayMode.Format != deviceCombo.AdapterFormat )
                        continue;
                    if (displayMode.RefreshRate == settings.presentParams.FullScreenRefreshRateInHz )
                    {
                        bFound = true;
                        break;
                    }
                }

                // If refresh rate not supported by this combo, then return false
                if (!bFound )
                    return false;
            }

            //---------------------
            // Present interval
            //---------------------
            // If keep present interval then check that the present interval is supported by this combo
            if (match.PresentInterval == MatchType.PreserveInput &&
                !deviceCombo.presentIntervalList.Contains( settings.presentParams.PresentationInterval ) )
                return false;

            return true;
        }

        /// <summary>
        /// Arbitrarily ranks device combo's
        /// </summary>
        private float RankDeviceCombo(EnumDeviceSettingsCombo deviceCombo, DeviceSettings settings, DisplayMode adapterMode)
        {
            float currentRanking = 0.0f; 

            // Arbitrary weights.  Gives preference to the ordinal, device type, and windowed
            const float adapterOrdinalWeight = 1000.0f;
            const float deviceTypeWeight = 100.0f;
            const float windowWeight = 10.0f;
            const float adapterFormatWeight = 1.0f;
            const float vertexProcessingWeight = 1.0f;
            const float resolutionWeight = 1.0f;
            const float backBufferFormatWeight = 1.0f;
            const float multiSampleWeight = 1.0f;
            const float depthStencilWeight = 1.0f;
            const float refreshRateWeight = 1.0f;
            const float presentIntervalWeight = 1.0f;


            //---------------------
            // Adapter ordinal
            //---------------------
            if (deviceCombo.AdapterOrdinal == settings.AdapterOrdinal )
                currentRanking += adapterOrdinalWeight;

            //---------------------
            // Device type
            //---------------------
            if (deviceCombo.DeviceType == settings.DeviceType )
                currentRanking += deviceTypeWeight;
            // Slightly prefer HAL 
            if (deviceCombo.DeviceType == DeviceType.Hardware )
                currentRanking += 0.1f; 

            //---------------------
            // Windowed
            //---------------------
            if (deviceCombo.IsWindowed == settings.presentParams.Windowed )
                currentRanking += windowWeight;

            //---------------------
            // Adapter format
            //---------------------
            if (deviceCombo.AdapterFormat == settings.AdapterFormat )
            {
                currentRanking += adapterFormatWeight;
            }
            else
            {
                int bitDepthDelta = Math.Abs((int)ManagedUtility.GetColorChannelBits(deviceCombo.AdapterFormat) -
                    (int)ManagedUtility.GetColorChannelBits(settings.AdapterFormat) );
                float scale = Math.Max(0.9f - (float)bitDepthDelta*0.2f, 0);
                currentRanking += scale * adapterFormatWeight;
            }

            if (!deviceCombo.IsWindowed )
            {
                // Slightly prefer when it matches the desktop format or is Format.X8R8G8B8
                bool isAdapterOptimalMatch;
                if (ManagedUtility.GetColorChannelBits(adapterMode.Format) >= 8 )
                    isAdapterOptimalMatch = (deviceCombo.AdapterFormat == adapterMode.Format);
                else
                    isAdapterOptimalMatch = (deviceCombo.AdapterFormat == Format.X8R8G8B8);

                if (isAdapterOptimalMatch )
                    currentRanking += 0.1f;
            }

            //---------------------
            // Vertex processing
            //---------------------
            if ((settings.BehaviorFlags & CreateFlags.HardwareVertexProcessing) != 0 || 
                (settings.BehaviorFlags & CreateFlags.MixedVertexProcessing) != 0 )
            {
                if(deviceCombo.deviceInformation.Caps.DeviceCaps.SupportsHardwareTransformAndLight)
                    currentRanking += vertexProcessingWeight;
            }
            // Slightly prefer HW T&L
            if(deviceCombo.deviceInformation.Caps.DeviceCaps.SupportsHardwareTransformAndLight)
                currentRanking += 0.1f;

            //---------------------
            // Resolution
            //---------------------
            bool bResolutionFound = false;
            for( int idm = 0; idm < deviceCombo.adapterInformation.displayModeList.Count; idm++ )
            {
                DisplayMode displayMode = (DisplayMode)deviceCombo.adapterInformation.displayModeList[idm];
                if (displayMode.Format != deviceCombo.AdapterFormat )
                    continue;
                if (displayMode.Width == settings.presentParams.BackBufferWidth &&
                    displayMode.Height == settings.presentParams.BackBufferHeight )
                    bResolutionFound = true;
            }
            if (bResolutionFound )
                currentRanking += resolutionWeight;

            //---------------------
            // Back buffer format
            //---------------------
            if (deviceCombo.BackBufferFormat == settings.presentParams.BackBufferFormat )
            {
                currentRanking += backBufferFormatWeight;
            }
            else
            {
                int bitDepthDelta = Math.Abs((int)ManagedUtility.GetColorChannelBits(deviceCombo.BackBufferFormat) -
                    (int)ManagedUtility.GetColorChannelBits(settings.presentParams.BackBufferFormat) );
                float scale = Math.Max(0.9f - (float)bitDepthDelta*0.2f, 0);
                currentRanking += scale * backBufferFormatWeight;
            }

            // Check if this back buffer format is the same as 
            // the adapter format since this is preferred.
            bool bAdapterMatchesBB = (deviceCombo.BackBufferFormat == deviceCombo.AdapterFormat);
            if (bAdapterMatchesBB )
                currentRanking += 0.1f;

            //---------------------
            // Back buffer count
            //---------------------
            // No caps for the back buffer count

            //---------------------
            // Multisample
            //---------------------
            bool bMultiSampleFound = false;
            for( int i=0; i<deviceCombo.multiSampleTypeList.Count; i++ )
            {
                MultiSampleType msType = (MultiSampleType)deviceCombo.multiSampleTypeList[i];
                uint msQuality  = (uint)(int)deviceCombo.multiSampleQualityList[i];

                if (msType == settings.presentParams.MultiSample &&
                    msQuality >= settings.presentParams.MultiSampleQuality )
                {
                    bMultiSampleFound = true;
                    break;
                }
            }
            if (bMultiSampleFound )
                currentRanking += multiSampleWeight;
        
            //---------------------
            // Swap effect
            //---------------------
            // No caps for swap effects

            //---------------------
            // Depth stencil 
            //---------------------
            if (deviceCombo.depthStencilFormatList.Contains( settings.presentParams.AutoDepthStencilFormat ) )
                currentRanking += depthStencilWeight;

            //---------------------
            // Present flags
            //---------------------
            // No caps for the present flags

            //---------------------
            // Refresh rate
            //---------------------
            bool bRefreshFound = false;
            for( int idm = 0; idm < deviceCombo.adapterInformation.displayModeList.Count; idm++ )
            {
                DisplayMode displayMode = (DisplayMode)deviceCombo.adapterInformation.displayModeList[idm];
                if (displayMode.Format != deviceCombo.AdapterFormat )
                    continue;
                if (displayMode.RefreshRate == settings.presentParams.FullScreenRefreshRateInHz )
                    bRefreshFound = true;
            }
            if (bRefreshFound )
                currentRanking += refreshRateWeight;

            //---------------------
            // Present interval
            //---------------------
            // If keep present interval then check that the present interval is supported by this combo
            if (deviceCombo.presentIntervalList.Contains( settings.presentParams.PresentationInterval ) )
                currentRanking += presentIntervalWeight;

            return currentRanking;
        }

        /// <summary>
        /// public helper function to find the closest allowed display mode to the optimal 
        /// </summary>
        private DisplayMode FindValidResolution(EnumDeviceSettingsCombo deviceCombo, DisplayMode displayMode)
        {
            DisplayMode bestDisplayMode = displayMode;

            if (deviceCombo.IsWindowed)
            {
                // Get the desktop resolution of the current monitor to use to keep the window
                // in a reasonable size in the desktop's 
                // This isn't the same as the current resolution from GetAdapterDisplayMode
                // since the device might be fullscreen
                EnumAdapterInformation adapterInfo = Enumeration.GetAdapterInformation(deviceCombo.AdapterOrdinal);
                // Find the right screen
                System.Windows.Forms.Screen displayScreen = null;
                string adapterName = adapterInfo.AdapterInformation.DeviceName.ToLower();
                foreach(System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
                {
                    string deviceName = s.DeviceName.ToLower();
                    // For some reason the device name has null characters in it.  Remove them
                    if (deviceName.IndexOf("\0") > 0)
                    {
                        deviceName = deviceName.Substring(0, deviceName.IndexOf("\0"));
                    }
                    if (deviceName == adapterName)
                    {
                        // Found the correct screen
                        displayScreen = s;
                        break;
                    }
                }
                System.Diagnostics.Debug.Assert(displayScreen != null, "We should have found the screen by now.");

                // For windowed mode, just keep it something reasonable within the size 
                // of the working area of the desktop
                if (bestDisplayMode.Width > displayScreen.WorkingArea.Width )
                    bestDisplayMode.Width = displayScreen.WorkingArea.Width;
                if (bestDisplayMode.Height > displayScreen.WorkingArea.Height )
                    bestDisplayMode.Height = displayScreen.WorkingArea.Height;

            }
            else
            {
                int bestRanking = 100000;
                int currentRanking;
                for( int iDisplayMode=0; iDisplayMode<deviceCombo.adapterInformation.displayModeList.Count; iDisplayMode++ )
                {
                    DisplayMode storedMode = (DisplayMode)deviceCombo.adapterInformation.displayModeList[iDisplayMode];

                    // Skip display modes that don't match the combo's adapter format
                    if (storedMode.Format != deviceCombo.AdapterFormat )
                        continue;

                    // Find the delta between the current width/height and the optimal width/height
                    currentRanking = Math.Abs((int)storedMode.Width - (int)displayMode.Width) + 
                        Math.Abs((int)storedMode.Height- (int)displayMode.Height);

                    if (currentRanking < bestRanking )
                    {
                        bestDisplayMode = displayMode;
                        bestRanking = currentRanking;

                        // Stop if perfect match found
                        if (bestRanking == 0 )
                            break;
                    }
                }

                // Were any found?
                if (bestDisplayMode.Width == 0 )
                {
                    throw new NoCompatibleDevicesException();
                }
            }

            return bestDisplayMode;
        }

        /// <summary>
        /// Change to a Direct3D device created from the device settings or passed in.
        /// The framework will only reset if the device is similar to the previous device 
        /// otherwise it will cleanup the previous device (if there is one) and recreate the 
        /// scene using the app's device callbacks.
        /// </summary>
        private void ChangeDevice(DeviceSettings newDeviceSettings, Device deviceFromApp, bool forceRecreate)
        {
            // Nothing to do if the framework is already disposed
            if (isDisposed)
                return;

            // First store the current device settings
            DeviceSettings oldSettings = null;
            if (State.CurrentDeviceSettings != null)
                oldSettings = State.CurrentDeviceSettings.Clone();

            // Pause the application
            Pause(true, true);

            // When a size message is received, it calls HandlePossibleSizeChange().
            // A size message might be sent when adjusting the window, so tell 
            // HandlePossibleSizeChange() to ignore size changes temporarily
            State.AreSizeChangesIgnored = true;

            // Only apply the cmd line overrides if this is the first device created
            // and SetDevice() isn't used
            if ( (deviceFromApp == null) && (oldSettings == null) )
            {
                // Updates the device settings struct based on the cmd line args.  
                // Warning: if the device doesn't support these new settings then CreateDevice() will fail.
                UpdateDeviceSettingsWithOverrides(ref newDeviceSettings);
            }

            // If windowed, then update the window client rect and window bounds rect
            // with the new pp.BackBufferWidth & pp.BackBufferHeight
            // The window will be resized after the device is reset/creeted
            if (newDeviceSettings.presentParams.Windowed)
            {
                // Don't allow smaller than what's used in minimum window size
                // otherwise the window size will be different than the backbuffer size
                if (newDeviceSettings.presentParams.BackBufferWidth < MinimumWindowSizeX)
                    newDeviceSettings.presentParams.BackBufferWidth = MinimumWindowSizeX;
                if (newDeviceSettings.presentParams.BackBufferHeight < MinimumWindowSizeY)
                    newDeviceSettings.presentParams.BackBufferHeight = MinimumWindowSizeY;

                if (State.WindowFocus != null)
                {
                    // Create a new client rectangle of the correct size
                    System.Drawing.Rectangle windowClient = State.ClientRectangle;
                    windowClient.Size = new System.Drawing.Size(newDeviceSettings.presentParams.BackBufferWidth, newDeviceSettings.presentParams.BackBufferHeight);

                    State.ClientRectangle = windowClient; // Store this for resizing later
                    State.WindowBoundsRectangle = WindowFocus.Bounds;;
                }
            }

            // Set the new device settings
            State.CurrentDeviceSettings = newDeviceSettings;


            // If AdapterOrdinal and DeviceType are the same, we can just do a Reset().
            // If they've changed, we need to do a complete device tear down/rebuild.
            // Also only allow a reset if deviceFromApp is the same as the current device 
            if( !forceRecreate && 
                (deviceFromApp == null || deviceFromApp == State.Device) && 
                oldSettings != null &&
                oldSettings.AdapterOrdinal == newDeviceSettings.AdapterOrdinal &&
                oldSettings.DeviceType == newDeviceSettings.DeviceType &&
                oldSettings.BehaviorFlags == newDeviceSettings.BehaviorFlags )
            {
                try
                {
                    // Reset the Direct3D device 
                    Reset3DEnvironment();
                }
                catch
                {
                    // The reset has failed, the device is lost
                    Pause (false, false);
                    State.IsDeviceLost = true;
                }
            }
            else
            {
                // Recreate the device
                if (oldSettings != null)
                {
                    // The adapter and device type don't match so 
                    // cleanup and create the 3D device again
                    Cleanup3DEnvironment(false);
                }
                
                Device device = deviceFromApp;
                // Only create a Direct3D device if one hasn't been supplied by the app
                if (deviceFromApp == null)
                {
                    try
                    {
                        device = new Device((int)newDeviceSettings.AdapterOrdinal, newDeviceSettings.DeviceType,
                            State.WindowFocus, newDeviceSettings.BehaviorFlags,
                            newDeviceSettings.presentParams);
                    }
                    catch (Exception e)
                    {
                        // There was an error creating the device
                        Pause(false, false);
                        CreatingDeviceException cde = new CreatingDeviceException(e);
                        DisplayErrorMessage(cde);
                        throw;
                    }
                }

                // Hook the device lost/reset events
                device.DeviceLost += new EventHandler(OnDeviceLost);
                device.DeviceReset += new EventHandler(OnDeviceReset);
                device.Disposing += new EventHandler(OnDeviceDisposing);

                // Store the device
                State.Device = device;

                // Now that the device is created, update the window and misc settings and
                // call the app's DeviceCreated and DeviceReset callbacks.
                try
                {
                    Initialize3DEnvironment();
                }
                catch
                {
                    Pause(false, false);
                    throw;
                }
                // Update the device stats text
                Enumeration.Enumerate(null);
                EnumAdapterInformation adapterInfo = Enumeration.GetAdapterInformation(newDeviceSettings.AdapterOrdinal);
                UpdateDeviceStats(newDeviceSettings.DeviceType, newDeviceSettings.BehaviorFlags, adapterInfo.AdapterInformation);
            }

            // Get the adapter monitor
            State.AdapterMonitor = Manager.GetAdapterMonitor((int)newDeviceSettings.AdapterOrdinal);

            // When moving from full screen to windowed mode, it is important to
            // adjust the window size after resetting the device rather than
            // beforehand to ensure that you get the window size you want.  For
            // example, when switching from 640x480 full screen to windowed with
            // a 1000x600 window on a 1024x768 desktop, it is impossible to set
            // the window size to 1000x600 until after the display mode has
            // changed to 1024x768, because windows cannot be larger than the
            // desktop.
            if (newDeviceSettings.presentParams.Windowed)
            {
                if (State.WindowFocus == null)
                {
                    System.Drawing.Rectangle rect = State.WindowBoundsRectangle;

                    // Resize the window
                    System.Drawing.Point pt = rect.Location;
                    pt = Window.PointToScreen(pt);

                    if (!State.IsMaximized)
                    {
                        Window.SetBounds(pt.X, pt.Y, rect.Width, rect.Height, System.Windows.Forms.BoundsSpecified.All);
                    }

                        

                    // Check to see if the monitor has changed windows
                    NativeMethods.MonitorInformation infoAdapter = new NativeMethods.MonitorInformation();
                    // Set size
                    infoAdapter.Size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.MonitorInformation));
                    NativeMethods.GetMonitorInfo(State.AdapterMonitor, ref infoAdapter);
                    // Calculate width/height, this needs to happen because the unmanaged RECT structure is different than the managed Rectangle
                    int monitorWidth = infoAdapter.WorkRectangle.Right - infoAdapter.WorkRectangle.Left;
                    int monitorHeight = infoAdapter.WorkRectangle.Bottom - infoAdapter.WorkRectangle.Top;

                    // Get the monitor this app is on
                    IntPtr monitorHandle = NativeMethods.MonitorFromWindow(Window.Handle, 1); // Default to primary
                    NativeMethods.MonitorInformation infoWindow = new NativeMethods.MonitorInformation();
                    // Set size
                    infoWindow.Size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.MonitorInformation));
                    NativeMethods.GetMonitorInfo(monitorHandle, ref infoWindow);

                    rect = State.WindowBoundsRectangle;
                    int windowOffsetX = rect.Left - infoWindow.MonitorRectangle.Left;
                    int windowOffsetY = rect.Top - infoWindow.MonitorRectangle.Top;
                    int windowWidth = rect.Right - rect.Left;
                    int windowHeight = rect.Bottom - rect.Top;

                    if (State.IsWindowCreatedWithDefaultPositions)
                    {
                        // Since the window was created with a default window position
                        // center it in the work area if its outside the monitor's work area

                        // Only do this the first time.
                        State.IsWindowCreatedWithDefaultPositions = false;

                        // Center window if the bottom or right of the window is outside the monitor's work area
                        if (infoAdapter.WorkRectangle.Left + windowOffsetX + windowWidth > infoAdapter.WorkRectangle.Right)
                            windowOffsetX = (monitorWidth - windowWidth) / 2;
                        if (infoAdapter.WorkRectangle.Top + windowOffsetY + windowHeight > infoAdapter.WorkRectangle.Bottom)
                            windowOffsetY = (monitorHeight - windowHeight) / 2;
                    }

                    // Move & show the window 
                    pt.X = infoAdapter.MonitorRectangle.Left + windowOffsetX;
                    pt.Y = infoAdapter.MonitorRectangle.Top + windowOffsetY;
                }

            }
            else
            {
                // Just store the full screen client rectangle
                State.FullScreenClientRectangle = new System.Drawing.Rectangle(0,0, newDeviceSettings.presentParams.BackBufferWidth,
                    newDeviceSettings.presentParams.BackBufferHeight);
            }

            // Done creating the device
            State.AreSizeChangesIgnored = false;
            Pause(false, false);
            State.WasDeviceCreated = true;
        }

        /// <summary>
        /// Updates the string which describes the device 
        /// </summary>
        private void UpdateDeviceStats(DeviceType deviceType, CreateFlags behaviorFlags, AdapterDetails info)
        {
            // Get the behavior flags
            BehaviorFlags flags = new BehaviorFlags(behaviorFlags);
            // Store device information
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            // Append device type
            builder.Append(deviceType.ToString());
            // Append other types based on flags
            if (flags.HardwareVertexProcessing && flags.PureDevice)
            {
                if (deviceType == DeviceType.Hardware)
                    builder.Append(" (pure hw vp)");
                else
                    builder.Append(" (simulated pure hw vp)");
            }
            else if (flags.HardwareVertexProcessing)
            {
                if (deviceType == DeviceType.Hardware)
                    builder.Append(" (hw vp)");
                else
                    builder.Append(" (simulated hw vp)");
            }
            else if (flags.MixedVertexProcessing)
            {
                if (deviceType == DeviceType.Hardware)
                    builder.Append(" (mixed vp)");
                else
                    builder.Append(" (simulated mixed vp)");
            }
            else if (flags.SoftwareVertexProcessing)
            {
                builder.Append(" (sw vp)");
            }

            // now add the description
            builder.Append(": ");
            builder.Append(info.Description);

            // Store the device stats string
            State.DeviceStats = builder.ToString();
        }

        /// <summary>
        /// Updates the device settings struct based on the cmd line args.  
        /// </summary>
        private void UpdateDeviceSettingsWithOverrides(ref DeviceSettings settings)
        {
            if (State.OverrideAdapterOrdinal != -1)
                settings.AdapterOrdinal = (uint)State.OverrideAdapterOrdinal;

            if (State.IsOverridingFullScreen)
                settings.presentParams.Windowed = false;
            if (State.IsOverridingWindowed)
                settings.presentParams.Windowed = true;

            if (State.IsOverridingForceReference)
                settings.DeviceType = DeviceType.Reference;
            else if (State.IsOverridingForceHardware)
                settings.DeviceType = DeviceType.Hardware;

            if (State.OverrideWidth != 0)
                settings.presentParams.BackBufferWidth = State.OverrideWidth;
            if (State.OverrideHeight != 0)
                settings.presentParams.BackBufferHeight = State.OverrideHeight;

            if (State.IsOverridingForcePureHardwareVertexProcessing)
            {
                settings.BehaviorFlags &= ~CreateFlags.SoftwareVertexProcessing;
                settings.BehaviorFlags |= CreateFlags.HardwareVertexProcessing;
                settings.BehaviorFlags |= CreateFlags.PureDevice;
            }
            else if (State.IsOverridingForceHardwareVertexProcessing)
            {
                settings.BehaviorFlags &= ~CreateFlags.SoftwareVertexProcessing;
                settings.BehaviorFlags &= ~CreateFlags.PureDevice;
                settings.BehaviorFlags |= CreateFlags.HardwareVertexProcessing;
            }
            else if (State.IsOverridingForceSoftwareVertexProcessing)
            {
                settings.BehaviorFlags &= ~CreateFlags.HardwareVertexProcessing;
                settings.BehaviorFlags &= ~CreateFlags.PureDevice;
                settings.BehaviorFlags |= CreateFlags.SoftwareVertexProcessing;
            }
        }


        /// <summary>
        /// Resets the 3D environment by:
        /// - Calls the device lost callback 
        /// - Resets the device
        /// - Stores the back buffer description
        /// - Sets up the full screen Direct3D cursor if requested
        /// - Calls the device reset callback 
        /// </summary>
        public void Reset3DEnvironment()
        {
            Device device = State.Device;
            System.Diagnostics.Debug.Assert(device != null, "The device should not be null here.");
            if (device == null)
                throw new InvalidOperationException("The device should not be null.");

            // Fire the event if required
            if (DeviceLost != null)
                DeviceLost(this, EventArgs.Empty);

            // Prepare the window for a possible change between windowed mode 
            // and full screen mode by adjusting the window style and its menu.
            AdjustWindowStyle(Window, IsWindowed);

            try
            {
                // If the settings dialog exists call its OnResetDevice() 
                // Reset the device
                device.Reset(State.CurrentDeviceSettings.presentParams);
            }
            catch  (Exception e)
            {
                if (e is MediaNotFoundException)
                    DisplayErrorMessage(e as MediaNotFoundException);
                else
                {
                    // Reset could legitimately fail if the device is lost
                    if (!(e is DeviceLostException))
                    {
                        DisplayErrorMessage(new ResettingDeviceException(e));
                    }
                }

                // We failed, call the lost callback once more
                OnDeviceLost(null, EventArgs.Empty);
                if (!(e is DeviceLostException))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Render the 3D environment by:
        /// - Checking if the device is lost and trying to reset it if it is
        /// - Get the elapsed time since the last frame
        /// - Calling the app's framemove and render callback
        /// - Calling Present()
        /// If you're not using MainLoop, consider using Render3DEnvironment
        /// </summary>
        public void Render3DEnvironment()
        {
            Device device = State.Device;
            if (device == null)
                return; // Nothing to do

            if (State.IsDeviceLost || State.IsRenderingPaused)
            {
                // Window is minimized or paused so yield 
                // CPU time to other processes
                System.Threading.Thread.Sleep(100); 
            }

            if (!State.IsActive)
            {
                // Window is not in focus so yield some CPU time to other processes
                System.Threading.Thread.Sleep(20); 
            }

            if (State.IsDeviceLost && !State.IsRenderingPaused)
            {
                int result;
                // Check the cooperative level to see if it's ok to render
                if (!device.CheckCooperativeLevel(out result))
                {
                    if (result == (int)ResultCode.DeviceLost)
                    {
                        // The device has been lost but cannot be reset at this time.  
                        // So wait until it can be reset.
                        System.Threading.Thread.Sleep(50); 
                        return;
                    }

                    // If we are windowed, read the desktop format and 
                    // ensure that the Direct3D device is using the same format 
                    // since the user could have changed the desktop bitdepth 
                    if (IsWindowed)
                    {
                        DisplayMode adapterDesktopMode = Manager.Adapters[(int)State.CurrentDeviceSettings.AdapterOrdinal].CurrentDisplayMode;
                        if (State.CurrentDeviceSettings.AdapterFormat != adapterDesktopMode.Format)
                        {
                            // Set up the match options
                            MatchOptions match = new MatchOptions();
                            match.AdapterOrdinal = MatchType.PreserveInput;
                            match.DeviceType = MatchType.PreserveInput;
                            match.Windowed = MatchType.PreserveInput;
                            match.AdapterFormat = MatchType.PreserveInput;
                            match.VertexProcessing = MatchType.ClosestToInput;
                            match.Resolution = MatchType.ClosestToInput;
                            match.BackBufferFormat = MatchType.ClosestToInput;
                            match.BackBufferCount = MatchType.ClosestToInput;
                            match.MultiSample = MatchType.ClosestToInput;
                            match.SwapEffect = MatchType.ClosestToInput;
                            match.DepthFormat = MatchType.ClosestToInput;
                            match.StencilFormat = MatchType.ClosestToInput;
                            match.PresentFlags = MatchType.ClosestToInput;
                            match.RefreshRate = MatchType.ClosestToInput;
                            match.PresentInterval = MatchType.ClosestToInput;

                            DeviceSettings settings = State.CurrentDeviceSettings;
                            settings.AdapterFormat = adapterDesktopMode.Format;
                            try
                            {
                                settings = FindValidDeviceSettings(settings, match);
                                // Change to a Direct3D device created from the new device settings.  
                                // If there is an existing device, then either reset or recreate the scene
                                ChangeDevice(settings, null, false);
                            }
                            catch
                            {
                                DisplayErrorMessage(new NoCompatibleDevicesException());
                                Dispose();
                            }
                            return;
                        }
                    }

                    // Try to reset the device
                    try
                    {
                        Reset3DEnvironment();
                    }
                    catch (DeviceLostException)
                    {
                        // The device was lost again, so continue waiting until it can be reset.
                        System.Threading.Thread.Sleep(50); 
                        return;
                    }
                    catch
                    {
                        // Reset failed, but the device wasn't lost so something bad happened, 
                        // so recreate the device to try to recover
                        DeviceSettings deviceSettings = State.CurrentDeviceSettings;
                        try
                        {
                            ChangeDevice(deviceSettings, null, false);
                        }
                        catch (Exception e)
                        {
                            ResettingDeviceException rde = new ResettingDeviceException(e);
                            DisplayErrorMessage(rde);
                            Dispose();
                            throw;
                        }
                    }
                }
                State.IsDeviceLost = false;
            }

            // Get the app's time, in seconds. Skip rendering if no time elapsed
            double time = FrameworkTimer.GetTime();
            float elapsedTime = (float)FrameworkTimer.GetElapsedTime();

            // Store the time for the app
            if (State.IsUsingConstantFrameTime)
            {        
                elapsedTime = State.TimePerFrame;
                time = State.CurrentTime + elapsedTime;
            }

            State.CurrentTime = time;
            State.ElapsedTime = elapsedTime;

            // Update the FPS stats
            UpdateFrameStats();

            // If the settings dialog exists and is being shown, then 
            // render it instead of rendering the app's scene
            if (State.Settings != null && State.IsD3DSettingsDialogShowing)
            {
                if (!State.IsRenderingPaused)
                {
                    // Clear the render target and the zbuffer 
                    device.Clear(ClearFlags.Target, 0x00003F3F, 1.0f, 0);

                    // Render the scene
                    device.BeginScene();
                    State.Settings.OnRender(elapsedTime);
                    device.EndScene();
                }
            }
            else
            {
                HandleTimers();

                //Animate the scene by calling the app's frame move callback
                if (State.CallbackInterface != null)
                {
                    State.CallbackInterface.OnFrameMove(device, time, elapsedTime);
                    device = State.Device;
                    if (device == null)
                        return;
                }

                if (!State.IsRenderingPaused)
                {
                    // Render the scene by calling the app's render callback
                    if (State.CallbackInterface != null)
                    {
                        State.CallbackInterface.OnFrameRender(device, time, elapsedTime);
                        device = State.Device;
                        if (device == null)
                            return;
                    }
                }
            }

            if (!State.IsRenderingPaused)
            {
                // Show the frame on the primary surface
                try
                {
                    device.Present();
                }
                catch (DeviceLostException)
                {
                    // Whoops, device is lost now
                    State.IsDeviceLost = true;
                }
                catch (DriverInternalErrorException)
                {
                    // When DriverInternalErrorException is thrown from Present(),
                    // the application can do one of the following:
                    // 
                    // - End, with the pop-up window saying that the application cannot continue 
                    //   because of problems in the display adapter and that the user should 
                    //   contact the adapter manufacturer.
                    //
                    // - Attempt to restart by calling Device.Reset, which is essentially the same 
                    //   path as recovering from a lost device. If Device.Reset throws the 
                    //   DriverInternalErrorException, the application should end immediately with the 
                    //   message that the user should contact the adapter manufacturer.
                    // 
                    // The framework attempts the path of resetting the device
                    // 
                    State.IsDeviceLost = true;
                }
            }

            // Update the current frame number
            State.CurrentFrameNumber++;

            if (State.OverrideQuitAfterFrame != 0)
            {
                if (State.CurrentFrameNumber > State.OverrideQuitAfterFrame)
                    Dispose();
            }
        }

        /// <summary>
        /// Closes down the window.  When the window closes, it will cleanup everything
        /// </summary>
        private void Shutdown()
        {
            // Cleanup the environment
            Cleanup3DEnvironment(true);
            // Close the window
            if (Window != null)
            {
                if (WindowForm != null)
                {
                    WindowForm.Close();
                }
            }
        }

        /// <summary>
        /// Pauses time or rendering.  Keeps a ref count so pausing can be layered
        /// </summary>
        public void Pause(bool pauseTime, bool pauseRendering)
        {
            // Update counters
            State.PauseTimeCount += pauseTime ? +1 : -1;
            if (State.PauseTimeCount < 0)
                State.PauseTimeCount = 0;

            State.PauseRenderingCount += pauseRendering ? +1 : -1;
            if (State.PauseRenderingCount < 0)
                State.PauseRenderingCount = 0;

            if (State.PauseTimeCount > 0)
            {
                // Stop the scene from animating
                FrameworkTimer.Stop();
            }
            else
            {
                // Restart the timer
                FrameworkTimer.Start();
            }

            State.IsRenderingPaused = (State.PauseRenderingCount > 0);
            State.IsTimePaused = (State.PauseTimeCount > 0);
        }


        /// <summary>
        /// Display an custom error msg box
        /// </summary>
        public void DisplayErrorMessage(Exception e)
        {
            if (e == lastDisplayedMessage) 
                return; //Already displayed this

            lastDisplayedMessage = e;

            State.ApplicationExitCode = 1;
            bool found = true;
            if (e is ApplicationException)
            {
                if (e is NoDirect3DException)
                    State.ApplicationExitCode = 2;
                else if (e is MediaNotFoundException)
                    State.ApplicationExitCode = 4;
                else if (e is CreatingDeviceException)
                    State.ApplicationExitCode = 6;
                else if (e is ResettingDeviceException)
                    State.ApplicationExitCode = 7;
                else if (e is CreatingDeviceObjectsException)
                    State.ApplicationExitCode = 8;
                else if (e is ResettingDeviceObjectsException)
                    State.ApplicationExitCode = 9;
                else if (e is NoCompatibleDevicesException)
                    State.ApplicationExitCode = 3;
                else
                    found = false;
            }
            else
                found = false;

            string errorText = string.Empty;
            if (found && State.IsShowingMsgBoxOnError)
            {
                errorText = e.Message;
                if (e.InnerException != null)
                {
                    errorText += "\r\n\r\n" + e.InnerException.ToString();
                    lastDisplayedMessage = e.InnerException;
                }

                if (State.WindowTitle != null && State.WindowTitle.Length > 0)
                    System.Windows.Forms.MessageBox.Show(errorText, "DirectX Application", 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                else
                    System.Windows.Forms.MessageBox.Show(errorText, State.WindowTitle, 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            else if (!found && State.IsShowingMsgBoxOnError)
            {
                errorText = e.ToString();
                // Unknown error, but they still want to see them
                if (State.WindowTitle != null && State.WindowTitle.Length > 0)
                    System.Windows.Forms.MessageBox.Show(errorText, "DirectX Application", 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                else
                    System.Windows.Forms.MessageBox.Show(errorText, State.WindowTitle, 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        /// <summary>
        ///  Cleans up the 3D environment by:
        ///  - Calls the device lost callback 
        ///  - Calls the device destroyed callback 
        ///  - Disposes the D3D device
        /// </summary>
        private void Cleanup3DEnvironment(bool releaseSettings)
        {
            DialogResourceManager.GetGlobalInstance().OnLostDevice();
            DialogResourceManager.GetGlobalInstance().OnDestroyDevice();
            ResourceCache.GetGlobalInstance().OnLostDevice();
            ResourceCache.GetGlobalInstance().OnDestroyDevice();
            if (State.Settings != null)
            {
                State.Settings.OnLostDevice();
                State.Settings.OnDestroyDevice( this, EventArgs.Empty);
            }

            using(Device device = State.Device)
            {
                if (releaseSettings)
                {
                    // Get rid of settings
                    State.CurrentDeviceSettings = null;
                }

                // Empty out other structures
                State.BackBufferSurfaceDesc = new SurfaceDescription();
                State.Caps = new Caps();
                State.WasDeviceCreated = false;
            }
            // No more device
            State.Device = null;
        }

        /// <summary>
        /// Initializes the 3D environment by:
        /// - Adjusts the window size, style, and menu 
        /// - Stores the back buffer description
        /// - Sets up the full screen Direct3D cursor if requested
        /// - Calls the device created callback
        /// - Calls the device reset callback
        /// - If both callbacks succeed it unpauses the app 
        /// </summary>
        private void Initialize3DEnvironment()
        {
            Device device = State.Device;
            System.Diagnostics.Debug.Assert(device != null, "The device should not be null here.");
            if (device == null)
                throw new InvalidOperationException("The device should not be null.");

            bool success = true;
            try
            {
                State.AreDeviceObjectsCreated = false;
                State.AreDeviceObjectsReset = false;

                // Prepare the window for a possible change between windowed mode 
                // and full screen mode by adjusting the window style and its menu.
                AdjustWindowStyle(Window, IsWindowed);

                // Prepare the device with cursor info and 
                // store backbuffer desc and caps from the device
                PrepareDevice(device);

                try
                {
                    // If the settings dialog exists call its OnCreateDevice() and OnResetDevice()
                    if (State.Settings != null)
                    {
                        State.Settings.OnCreateDevice(device);
                        State.Settings.OnResetDevice();
                    }
                    // Call the dialog resource manager create device method
                    DialogResourceManager.GetGlobalInstance().OnCreateDevice(device);

                    // Call the resource cache created function
                    ResourceCache.GetGlobalInstance().OnCreateDevice(device);

                    // Call the applications device created callback if it's set
                    State.IsInsideDeviceCallback = true;
                    if (DeviceCreated != null)
                    {
                        DeviceCreated(this, new DeviceEventArgs(device, State.BackBufferSurfaceDesc));
                    }
                    State.IsInsideDeviceCallback = false;
                    State.AreDeviceObjectsCreated = true;
                }
                catch (MediaNotFoundException e)
                {
                    DisplayErrorMessage(e);
                    success = false;
                    throw;
                }
                catch (Exception e)
                {
                    DisplayErrorMessage(new CreatingDeviceException(e));
                    success = false;
                    throw;
                }

                try
                {
                    // Call the dialog resource manager reset device method
                    DialogResourceManager.GetGlobalInstance().OnResetDevice(device);

                    // Call the resource cache device reset function
                    ResourceCache.GetGlobalInstance().OnResetDevice(device);

                    // Call app's reset
                    State.IsInsideDeviceCallback = true;
                    if (DeviceReset != null)
                        DeviceReset(this, new DeviceEventArgs(device, State.BackBufferSurfaceDesc));

                    State.IsInsideDeviceCallback = false;
                    State.AreDeviceObjectsReset = true;
                }
                catch (MediaNotFoundException e)
                {
                    DisplayErrorMessage(e);
                    success = false;
                    throw;
                }
                catch (Exception e)
                {
                    DisplayErrorMessage(new CreatingDeviceException(e));
                    success = false;
                    throw;
                }
            }
            finally
            {
                if (!success)
                    Cleanup3DEnvironment(true);
            }

        }

        /// <summary>
        /// Prepares a new or resetting device by with cursor info and 
        /// store backbuffer desc and caps from the device
        /// </summary>
        private void PrepareDevice(Device device)
        {
            // Update the device stats text
            UpdateStaticFrameStats();

            // Store render target surface desc
            using(Surface backBuffer = device.GetBackBuffer(0, 0, BackBufferType.Mono))
            {
                State.BackBufferSurfaceDesc = backBuffer.Description;
            }

            // Update the state's copy of caps
            State.Caps = device.DeviceCaps;

            // Setup the full screen cursor
            if (State.IsShowingCursorWhenFullScreen && !IsWindowed)
            {
                // Get a valid cursor
                System.Windows.Forms.Cursor cursor = System.Windows.Forms.Cursor.Current;
                if (cursor == null)
                    cursor = System.Windows.Forms.Cursors.Default;

                device.SetCursor(cursor, false);
                device.ShowCursor(true);
            }

            // Confine cursor to full screen window
            if (State.IsCursorClippedWhenFullScreen)
            {
                if (!IsWindowed)
                {
                    // Turn on clipping for the full screen window
                    System.Windows.Forms.Cursor.Clip = State.FullScreenClientRectangle;
                }
                else
                {
                    // Turn off clipping
                    System.Windows.Forms.Cursor.Clip = System.Drawing.Rectangle.Empty;
                }
            }
        }

        /// <summary>
        /// Prepare the window for a possible change between windowed mode and full screen mode
        /// </summary>
        private void AdjustWindowStyle(System.Windows.Forms.Control control, bool isWindowed)
        {
            System.Windows.Forms.Form window = control as System.Windows.Forms.Form;
            if (window == null) // nothing to do
                return;

            if (isWindowed)
            {
                // If different device windows are used for windowed mode and fullscreen mode,
                // hide the fullscreen window so that it doesn't obscure the screen.
                if (State.WindowDeviceWindowed != State.WindowDeviceFullScreen)
                {
                    State.WindowDeviceFullScreen.Hide();
                }

                // Set the window style mode
                window.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            }
            else
            {
                // If different device windows are used for windowed mode and fullscreen mode,
                // restore and show the fullscreen device window.
                if (State.WindowDeviceWindowed != State.WindowDeviceFullScreen)
                {
                    System.Windows.Forms.Form fullscreen = State.WindowDeviceFullScreen as System.Windows.Forms.Form;
                    if (fullscreen != null)
                    {
                        if (fullscreen.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                            fullscreen.WindowState = System.Windows.Forms.FormWindowState.Normal;

                        State.WindowDeviceFullScreen.Show();
                    }
                }
                // Set the full screen style mode
                window.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            }
        }

        /// <summary>
        /// Updates the frames/sec stat once per second
        /// </summary>
        private void UpdateFrameStats()
        {
            // Keep track of frame count
            double time = FrameworkTimer.GetAbsoluteTime();
            State.LastStatsUpdateFrames++;

            if (time - State.LastStatsUpdateTime > 1.0)
            {
                float fps = (float)(State.LastStatsUpdateFrames / (time - State.LastStatsUpdateTime));
                State.CurrentFrameRate = fps;
                State.LastStatsUpdateFrames = 0;
                State.LastStatsUpdateTime = time;

                State.FrameStats = string.Format(State.StaticFrameStats, fps.ToString("f2",
                    System.Globalization.CultureInfo.CurrentUICulture));
            }
        }

        /// <summary>
        /// Updates the static part of the frame stats so it doesn't have be generated every frame
        /// </summary>
        private void UpdateStaticFrameStats()
        {
            State.StaticFrameStats = string.Empty;
            DeviceSettings settings = State.CurrentDeviceSettings;
            if (settings.presentParams == null)
                return; // Nothing to do

            EnumDeviceSettingsCombo combo = Enumeration.GetDeviceSettingsCombo(
                settings.AdapterOrdinal, settings.DeviceType, settings.AdapterFormat,
                settings.presentParams.BackBufferFormat, settings.presentParams.Windowed);

            if (combo == null)
                return; // Still nothing to do 

            // Get adapter/backbuffer format string
            string formatString = null;
            if (settings.AdapterFormat == combo.BackBufferFormat)
            {
                // Get the format string
                formatString = "Format." + settings.AdapterFormat.ToString();
            }
            else
            {
                formatString = string.Format("backbuffer Format.{0}, adapter Format.{1}",
                    combo.BackBufferFormat, settings.AdapterFormat);
            }

            // Get depth stencil format string
            string depthFormat = null;
            if (settings.presentParams.EnableAutoDepthStencil)
            {
                depthFormat = " Format." + settings.presentParams.AutoDepthStencilFormat.ToString();
            }

            // Get any multisampling string
            string multiSampleString = null;
            switch(settings.presentParams.MultiSample)
            {
                case MultiSampleType.None:
                    multiSampleString = null; // No display
                    break;
                case MultiSampleType.NonMaskable:
                    multiSampleString = " (Nonmaskable Multisample)";
                    break;
                default:
                    multiSampleString = string.Format(" ({0} Multisample)", settings.presentParams.MultiSample.ToString());
                    break;
            }

            // Update static frame stats now
            State.StaticFrameStats = string.Format("{0} fps ({1}x{2}), {3}{4}{5}",
                "{0}", settings.presentParams.BackBufferWidth, settings.presentParams.BackBufferHeight,
                formatString, depthFormat, multiSampleString);
        }

        /// <summary>
        /// public helper function to return the adapter format from the first device settings 
        /// combo that matches the passed adapter ordinal, device type, backbuffer format, and windowed.  
        /// </summary>
        private Format FindAdapterFormat(uint adapterOrdinal, DeviceType deviceType, Format backbufferFormat,
            bool isWindowed)
        {
            EnumDeviceInformation deviceInfo = Enumeration.GetDeviceInfo(adapterOrdinal, deviceType);
            if (deviceInfo != null)
            {
                for (int i = 0; i < deviceInfo.deviceSettingsList.Count; i++)
                {
                    EnumDeviceSettingsCombo combo = deviceInfo.deviceSettingsList[i] as EnumDeviceSettingsCombo;
                    if ( (combo.BackBufferFormat == backbufferFormat) &&
                        (combo.IsWindowed == isWindowed) )
                    {
                        // return first match
                        return combo.AdapterFormat;
                    }
                }
            }

            // Couldn't find any adapter format's.. Return back buffer format
            return backbufferFormat;
        }


        /// <summary>
        /// Checks if the window client rect has changed and if it has, then reset the device
        /// </summary>
        private void HandlePossibleSizeChange()
        {
            if ( (!State.WasDeviceCreated) || (State.AreSizeChangesIgnored) )
                return; // Nothing to do here

            if (!State.CurrentDeviceSettings.presentParams.Windowed)
                return; // nothing to do here either, can only resize in windowed

            if (Window == null)
                return; // still nothing to do, we don't have a window

            System.Drawing.Rectangle oldClient = State.ClientRectangle;
            // Now store the new client
            State.ClientRectangle = Window.ClientRectangle;

            // Don't forget the bounds rect
            State.WindowBoundsRectangle = Window.Bounds;

            // Check if the old client rect has changed
            if (oldClient.Width != Window.ClientRectangle.Width || oldClient.Height != Window.ClientRectangle.Height)
            {
                // A new window size will require a new backbuffer
                // size, so the 3D structures must be changed accordingly.
                Pause(true, true);

                DeviceSettings settings = State.CurrentDeviceSettings;
                settings.presentParams.BackBufferWidth = Window.ClientRectangle.Width;
                settings.presentParams.BackBufferHeight = Window.ClientRectangle.Height;

                // Reset the 3D environment
                if (State.Device != null)
                {
                    try
                    {
                        Reset3DEnvironment();
                    }
                    catch
                    {
                        State.IsDeviceLost = true;
                    }
                }

                Pause(false, false);
            }

            CheckForWindowMonitorChange();
        }

        /// <summary>
        /// Checks to see if the window changed monitors, and if it did it creates a device 
        /// from the monitor's adapter and recreates the scene.
        /// </summary>
        private void CheckForWindowMonitorChange()
        {
            // Don't do this if the user doesn't want it
            if (!State.CanAutoChangeAdapter)
                return; 

            IntPtr monitorHandle = NativeMethods.MonitorFromWindow(Window.Handle, 1); // Primary monitor
            if (monitorHandle != State.AdapterMonitor)
            {
                // Changing device, pause
                Pause(true, true);
                // Get the new ordinal
                uint newOrdinal = GetAdapterOridinalFromMonitor(monitorHandle);
                
                // Get the current device settings and flip the ordinal then
                // find the closest valid device settings with this change
                DeviceSettings settings = State.CurrentDeviceSettings.Clone();
                settings.AdapterOrdinal = newOrdinal;

                // Set up the match options
                MatchOptions match = new MatchOptions();
                match.AdapterOrdinal = MatchType.PreserveInput;
                match.DeviceType = MatchType.ClosestToInput;
                match.Windowed = MatchType.ClosestToInput;
                match.AdapterFormat = MatchType.ClosestToInput;
                match.VertexProcessing = MatchType.ClosestToInput;
                match.Resolution = MatchType.ClosestToInput;
                match.BackBufferFormat = MatchType.ClosestToInput;
                match.BackBufferCount = MatchType.ClosestToInput;
                match.MultiSample = MatchType.ClosestToInput;
                match.SwapEffect = MatchType.ClosestToInput;
                match.DepthFormat = MatchType.ClosestToInput;
                match.StencilFormat = MatchType.ClosestToInput;
                match.PresentFlags = MatchType.ClosestToInput;
                match.RefreshRate = MatchType.ClosestToInput;
                match.PresentInterval = MatchType.ClosestToInput;

                try
                {
                    // Find some valid settings (if possible)
                    settings = FindValidDeviceSettings(settings, match);
                    try
                    {
                        // Create a Direct3D device using the new device settings.  
                        // If there is an existing device, then it will either reset or recreate the scene.
                        ChangeDevice(settings, null, false);
                    }
                    catch (Exception e)
                    {
                        // This is a much worse error.. Shut down and fail
                        Dispose();
                        Pause(false, false);
                        System.Diagnostics.Debugger.Log(0, string.Empty, e.ToString());
                        return;
                    }
                } 
                catch
                {
                    // No valid device was found for this monitor, that's bad, but not fatal.
                    // Ignore this error
                } 

            }
            Pause(false, false);
        }

        /// <summary>Look for an adapter ordinal that is tied to a monitor handle</summary>
        private uint GetAdapterOridinalFromMonitor(IntPtr monitorHandle)
        {
            uint ordinal = 0;

            foreach(EnumAdapterInformation info in Enumeration.AdapterInformationList)
            {
                if (Manager.GetAdapterMonitor((int)info.AdapterOrdinal) == monitorHandle)
                    ordinal = info.AdapterOrdinal;
            }
            return ordinal;
        }

        /// <summary>Will try to load the first icon from the resources</summary>
        public System.Drawing.Icon LoadFirstIconFromResource()
        {
            // It's no big deal if we can't load the icon, just return null instead
            return null;
        }

        /// <summary>Handles setting up the cursor settings for fullscreen</summary>
        public void SetCursorSettings(bool showCursorWhenFullscreen, bool clipCursorWhenFullscreen)
        {
            State.IsCursorClippedWhenFullScreen = clipCursorWhenFullscreen;
            State.IsShowingCursorWhenFullScreen = showCursorWhenFullscreen;
        }

        /// <summary>Determines if the adapter can automatically change monitors</summary>
        public bool CanAutomaticallyChangeMonitor
        {
            get { return State.CanAutoChangeAdapter; } set { State.CanAutoChangeAdapter = value; }
        }

        /// <summary>Allows to set constant time per frame</summary>
        public void SetConstantFrameTime(bool constantFrameTime, float timePerFrame)
        {
            State.OverrideConstantTimePerFrame = timePerFrame;
            State.IsUsingConstantFrameTime = constantFrameTime;
        }
        /// <summary>Allows to set constant time per frame</summary>
        public void SetConstantFrameTime(bool constantFrameTime)
        {
            State.OverrideConstantTimePerFrame = 0.0333f;
            State.IsUsingConstantFrameTime = constantFrameTime;
        }

        /// <summary>
        /// Exit code from the framework
        /// </summary>
        public int ExitCode
        {
            get { return State.ApplicationExitCode; }
        }

        /// <summary>
        /// Show the settings dialog, and create if needed
        /// </summary>
        public void ShowSettingsDialog(bool shouldShow)
        {
            State.IsD3DSettingsDialogShowing = shouldShow;

            if (shouldShow)
            {
                SettingsDialog dialog = PrepareSettingsDialog();
                dialog.Refresh();
            }
        }

        /// <summary>
        /// public helper function to prepare the settings dialog by creating it if it didn't 
        /// already exist and enumerating if desired.
        /// </summary>
        /// <returns></returns>
        private SettingsDialog PrepareSettingsDialog()
        {
            SettingsDialog dialog = State.Settings;
            if (dialog == null)
            {
                // Create it
                dialog = new SettingsDialog(this);
                State.Settings = dialog; // Store it too

                if (State.AreDeviceObjectsCreated)
                {
                    dialog.OnCreateDevice(State.Device);
                }
                if (State.AreDeviceObjectsReset)
                {
                    dialog.OnResetDevice();
                }
            }

            return dialog;
        }

        #region External state access functions
        /// <summary>
        /// Returns the device current in use for the framework
        /// </summary>
        public Device Device
        {
            get { return State.Device;}
        }

        /// <summary>
        /// Returns the back buffer surface description
        /// </summary>
        public SurfaceDescription BackBufferSurfaceDescription
        {
            get { return State.BackBufferSurfaceDesc; }
        }

        /// <summary>
        /// Returns the device capabilities
        /// </summary>
        public Caps DeviceCaps
        {
            get { return State.Caps; }
        }

        /// <summary>
        /// Get the current window used for rendering
        /// </summary>
        public System.Windows.Forms.Control Window
        {
            get { return IsWindowed ? State.WindowDeviceWindowed : State.WindowDeviceFullScreen; }
        }

        /// <summary>
        /// Get the current window used for rendering
        /// </summary>
        public System.Windows.Forms.Form WindowForm
        {
            get { return Window as System.Windows.Forms.Form; }
        }

        /// <summary>
        /// Get the current focus window 
        /// </summary>
        public System.Windows.Forms.Control WindowFocus
        {
            get { return State.WindowFocus; }
        }

        /// <summary>
        /// Get the current window used for windowed rendering
        /// </summary>
        public System.Windows.Forms.Control WindowDeviceWindowed
        {
            get { return State.WindowDeviceWindowed; }
        }

        /// <summary>
        /// Get the current window used for full screen rendering
        /// </summary>
        public System.Windows.Forms.Control WindowDeviceFullscreen
        {
            get { return State.WindowDeviceFullScreen; }
        }

        /// <summary>
        /// Client rectangle size
        /// </summary>
        public System.Drawing.Rectangle WindowClientRectangle
        {
            get { return State.ClientRectangle; }
        }

        /// <summary>
        /// Client rectangle size
        /// </summary>
        public System.Drawing.Rectangle ClientRectangle
        {
            get { return IsWindowed ? State.ClientRectangle : State.FullScreenClientRectangle; }
        }
        
        /// <summary>
        /// Return if windowed in the current device.  If no device exists yet, return false
        /// </summary>
        public bool IsWindowed
        {
            get 
            {
                DeviceSettings settings = State.CurrentDeviceSettings;
                if ((settings != null) && (settings.presentParams != null))
                    return settings.presentParams.Windowed;
                else
                    return false;
            }
        }


        /// <summary>
        /// Frame statistics
        /// </summary>
        public string FrameStats
        {
            get { return State.FrameStats; }
        }
        /// <summary>
        /// Device statistics
        /// </summary>
        public string DeviceStats
        {
            get { return State.DeviceStats; }
        }

        /// <summary>
        /// Frames per second application is running at
        /// </summary>
        public float FPS
        {
            get { return State.CurrentFrameRate; }
        }

        /// <summary>
        /// Returns whether the settings is showing
        /// </summary>
        public bool IsD3DSettingsDialogShowing
        {
            get { return State.IsD3DSettingsDialogShowing; }
        }

        /// <summary>
        /// Returns whether the framework is showing errors
        /// </summary>
        public bool IsShowingMsgBoxOnError
        {
            get { return State.IsShowingMsgBoxOnError; }
        }

        /// <summary>
        /// Return the device settings of the current device.  If no device exists yet, then
        /// return blank device settings 
        /// </summary>
        public DeviceSettings DeviceSettings
        {
            get 
            { 
                if (State.CurrentDeviceSettings != null)
                {
                    return State.CurrentDeviceSettings;
                }
                else
                {
                    return new DeviceSettings();
                }
            }
        }
        /// <summary>
        /// Return the present params of the current device.  If no device exists yet, then
        /// return blank present params 
        /// </summary>
        public PresentParameters PresentParameters
        {
            get 
            { 
                if (State.CurrentDeviceSettings != null)
                {
                    return State.CurrentDeviceSettings.presentParams;
                }
                else
                {
                    return new PresentParameters();
                }
            }
        }
        /// <summary>Should the framework also notify for mouse moves</summary>
        public bool IsNotifiedOnMouseMove
        {
            get { return State.IsNotifiedOnMouseMove; } set { State.IsNotifiedOnMouseMove = value;}
        }

        /// <summary>Should the framework override and start fullscreen?</summary>
        public bool IsOverridingFullScreen 
        {
            get { return State.IsOverridingFullScreen; } set { State.IsOverridingFullScreen = value;}
        }

        /// <summary>Should the framework ignore size changes?</summary>
        public bool IsIgnoringSizeChanges { get { return State.AreSizeChangesIgnored; } set { State.AreSizeChangesIgnored = value; } }

        /// <summary>Resets the state associated with the sample framework</summary>
        public void ResetState()
        {
            State = new FrameworkData();
        }
        #endregion

        #region Timer Callbacks
        /// <summary>Adds a timer to the list of timers for the application</summary>
        /// <returns>A unqiue id of the timer</returns>
        public int SetTimer(TimerCallback callbackTimer, float timeoutInSeconds)
        {
            if (callbackTimer == null)
                throw new ArgumentNullException("callbackTimer", "You must pass in a valid timer callback .");

            TimerData timer = new TimerData();
            timer.callback = callbackTimer;
            timer.Countdown = timeoutInSeconds;
            timer.TimeoutInSecs = timeoutInSeconds;
            timer.IsEnabled = true;

            State.Timers.Add(timer);
            return State.Timers.Count;
        }

        /// <summary>Kills an already created timer</summary>
        public void KillTimer(int id)
        {
            if (id < State.Timers.Count)
            {
                TimerData timer = (TimerData)State.Timers[id];
                timer.IsEnabled = false;
                State.Timers[id] = timer;
            }
            else
            {
                throw new ArgumentException("An invalid timer ID was passed in", "id");
            }
        }

        /// <summary>public helper function to handle calling the user defined timer callbacks</summary>
        private void HandleTimers()
        {
            float elapsedTime = State.ElapsedTime;

            // Walk through the list of timers
            for (int i = 0; i < State.Timers.Count; i++)
            {
                TimerData ft = (TimerData)State.Timers[i];
                if (ft.IsEnabled)
                {
                    ft.Countdown -= elapsedTime;
                    // Call the callback if the countdown expired
                    if (ft.Countdown < 0)
                    {
                        ft.callback((uint)i);
                        ft.Countdown = ft.TimeoutInSecs;
                    }
                    State.Timers[i] = ft;
                }
            }
        }
        #endregion

        /// <summary>Will close the window</summary>
        public void CloseWindow()
        {
            if (WindowForm != null)
                WindowForm.Close();
        }

        #region Window Message Handling

        /// <summary>
        /// Fired when the window needs to be repainted
        /// </summary>
        private void OnPaint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Handle paint messages when the app is paused
            if ( (State.Device != null) && (State.AreDeviceObjectsCreated) && 
                (State.AreDeviceObjectsReset) && (State.IsRenderingPaused) )
            {
                double time = State.CurrentTime;
                float elapsedTime = State.ElapsedTime;

                if (State.Settings != null && State.IsD3DSettingsDialogShowing)
                {
                    // Clear the render target and the zbuffer 
                    State.Device.Clear(ClearFlags.Target, 0x00003F3F, 1.0f, 0);

                    // Render the scene
                    State.Device.BeginScene();
                    State.Settings.OnRender(elapsedTime);
                    State.Device.EndScene();
                }
                else
                {
                    if (State.CallbackInterface != null)
                    {
                        State.CallbackInterface.OnFrameRender(State.Device, time, elapsedTime);
                    }
                }

                // Now try to present
                try
                {
                    State.Device.Present();
                }
                catch(DeviceLostException)
                {
                    // Device was lost
                    State.IsDeviceLost = true;
                }
                catch(DriverInternalErrorException)
                {
                    // See Render3DEnvironment for information on this exception
                    State.IsDeviceLost = true;
                }
            }
        }

        /// <summary>
        /// Fired when the control is resized
        /// </summary>
        private void OnResize(object sender, EventArgs e)
        {
            System.Windows.Forms.Form form = sender as System.Windows.Forms.Form;
            System.Windows.Forms.Control control = sender as System.Windows.Forms.Control;

            if (form != null)
            {
                if (form.WindowState == System.Windows.Forms.FormWindowState.Minimized)
                {
                    if ((State.IsCursorClippedWhenFullScreen) && !(IsWindowed) )
                        System.Windows.Forms.Cursor.Clip = System.Drawing.Rectangle.Empty;
                    State.IsMinimized = true;
                    State.IsMaximized = false;
                    Pause(true, true); // Pause while minimized
                }
                else
                {
                    if (State.IsMinimized)
                    {
                        Pause(false, false);
                    }
                    State.IsMinimized = false;
                    State.IsMaximized = form.WindowState == System.Windows.Forms.FormWindowState.Maximized;

                    // Resize
                    HandlePossibleSizeChange();
                }
            }

        }

        /// <summary>
        /// Fired when a key is pressed
        /// </summary>
        private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (State.IsHandlingDefaultHotkeys)
            {
                switch(e.KeyCode)
                {
                    case System.Windows.Forms.Keys.Enter:
                        if (e.Alt) // Alt was pressed as well
                        {
                            // Toggle the full screen/window mode
                            Pause(true, true);
                            ToggleFullscreen();
                            Pause(false, false);                        
                            e.Handled = true;
                        }
                        break;
                    case System.Windows.Forms.Keys.F2:
                        ShowSettingsDialog(!State.IsD3DSettingsDialogShowing);
                        break;
                    case System.Windows.Forms.Keys.F3: // Toggle reference
                        Pause(true, true);
                        ToggleReference();
                        Pause(false, false);
                        break;

                    case System.Windows.Forms.Keys.Escape:
                        // Received key to exit app, check for a form to close
                        if (WindowForm != null)
                        {
                            WindowForm.Close();
                        }
                        break;

                }
            }
        }

        /// <summary>
        /// Fired when the cursor changes
        /// </summary>
        private void OnCursorChanged(object sender, EventArgs e)
        {
            // Turn off Windows cursor in full screen mode
            if ( (!State.IsRenderingPaused) && (!IsWindowed) )
            {
                System.Windows.Forms.Cursor.Current = null;
                if ( (State.Device != null) && State.IsShowingCursorWhenFullScreen)
                    State.Device.ShowCursor(true);
            }
            else
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
        }

        /// <summary>
        /// Fired as the mouse moves
        /// </summary>
        private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ( (!State.IsRenderingPaused) && (!IsWindowed) )
            {
                if (State.Device != null)
                {
                    State.Device.SetCursorPosition(e.X, e.Y, false);
                }
            }
        }

        /// <summary>
        /// Fired when a form is closed
        /// </summary>
        private void OnFormClosed(object sender, EventArgs e)
        {
            // No more windows
            State.WindowFocus = null;
            State.WindowDeviceWindowed = null;
            State.WindowDeviceFullScreen = null;
        }

        /// <summary>
        /// Fired when a form is activated
        /// </summary>
        private void OnFormActivated(object sender, EventArgs e)
        {
            State.IsActive = true;
        }

        /// <summary>
        /// Fired when a form is deactivated
        /// </summary>
        private void OnFormDeactivated(object sender, EventArgs e)
        {
            State.IsActive = false;
        }

        /// <summary>
        /// Fired when a menu is started
        /// </summary>
        private void OnMenuStart(object sender, EventArgs e)
        {
            Pause(true, true);
        }

        /// <summary>
        /// Fired when a menu is finished
        /// </summary>
        private void OnMenuEnd(object sender, EventArgs e)
        {
            Pause(false, false);
        }

        /// <summary>
        /// Will hook all events required for a control
        /// </summary>
        private void HookEvents(System.Windows.Forms.Control c)
        {
            c.Paint += new System.Windows.Forms.PaintEventHandler(OnPaint);
            c.Resize += new EventHandler(OnResize);
            c.KeyDown += new System.Windows.Forms.KeyEventHandler(OnKeyDown);
            c.CursorChanged += new EventHandler(OnCursorChanged);
            c.MouseMove += new System.Windows.Forms.MouseEventHandler(OnMouseMove);
            System.Windows.Forms.Form form = c as System.Windows.Forms.Form;
            if (form != null)
            {
                // Hook form specific events
                form.Closed += new EventHandler(OnFormClosed);
                form.Activated += new EventHandler(OnFormActivated);
                form.Deactivate += new EventHandler(OnFormDeactivated);
                form.MenuStart += new EventHandler(OnMenuStart);
                form.MenuComplete += new EventHandler(OnMenuEnd);
            }
        }
        /// <summary>Handles window messages</summary>
        internal void WindowsProcedure(ref System.Windows.Forms.Message m)
        {
            if (State.Settings != null && State.IsD3DSettingsDialogShowing)
            {
                State.Settings.HandleMessages(m.HWnd, (NativeMethods.WindowMessage)m.Msg, m.WParam, m.LParam);
            }
            else
            {
                if (State.WndProcFunction != null)
                {
                    bool doneProcessing = false;
                    // Pass all messages to the app's MsgProc callback, and don't 
                    // process further messages if the apps says not to.
                    IntPtr result = State.WndProcFunction(m.HWnd, (NativeMethods.WindowMessage)m.Msg, m.WParam, m.LParam, ref doneProcessing);
                    if (doneProcessing)
                        m.Result = result;
                }
            }

            switch((NativeMethods.WindowMessage)m.Msg)
            {
                case NativeMethods.WindowMessage.PowerBroadcast:
                    if (m.WParam == IntPtr.Zero) // Query Suspend
                    {
                        // At this point, the app should save any data for open
                        // network connections, files, etc., and prepare to go into
                        // a suspended mode.  The app can use the MsgProc callback
                        // to handle this if desired.
                        m.Result = TrueIntPtr;
                    }
                    if (m.WParam.ToInt32() == 7) // Resume Suspend
                    {
                        // At this point, the app should recover any data, network
                        // connections, files, etc., and resume running from when
                        // the app was suspended. The app can use the MsgProc callback
                        // to handle this if desired.

                        // QPC may lose consistency when suspending, so reset the timer
                        // upon resume.
                        FrameworkTimer.Reset();
                        State.LastStatsUpdateTime = 0;

                        m.Result = TrueIntPtr;
                    }
                    break;
                case NativeMethods.WindowMessage.SystemCommand:
                    // Prevent moving/sizing and power loss in full screen mode
                    const int SystemCommandSize = 0xF000;
                    const int SystemCommandMove = 0xF010;
                    const int SystemCommandMaximize = 0xF030;
                    const int SystemCommandKeyMenu = 0xF100;
                    const int SystemCommandMonitorPower = 0xF170;
                    switch(m.WParam.ToInt32())
                    {
                        case SystemCommandSize:
                        case SystemCommandMove:
                        case SystemCommandMaximize:
                        case SystemCommandKeyMenu:
                        case SystemCommandMonitorPower:
                            if (!IsWindowed)
                                m.Result = TrueIntPtr;
                            break;
                    }
                    break;
            }
        }
        #endregion

        /// <summary>
        /// Handles app's message loop and rendering when idle.  If CreateDevice*() or SetDevice() 
        /// has not already been called, it will call CreateDevice() with the default parameters.  
        /// </summary>
        public void MainLoop()
        {
            // No need to do anything if the framework is already disposed
            if (isDisposed)
                return; 

            // Not allowed to call this from inside the device callbacks or reenter
            if (State.IsInsideDeviceCallback || State.IsInsideMainloop)
            {
                State.ApplicationExitCode = 1;
                throw new InvalidOperationException("You cannot call this method from a callback, or reenter the method.");
            }

            // We're inside the loop now
            State.IsInsideMainloop = true;

            // If CreateDevice*() or SetDevice() has not already been called, 
            // then call CreateDevice() with the default parameters.         
            if (!State.WasDeviceCreated)
            {
                if (State.WasDeviceCreateCalled)
                {
                    // It already called and failed, don't try again
                    State.ApplicationExitCode = 1;
                    throw new InvalidOperationException("The device was never created.");
                }

                try
                {
                    CreateDevice(0, true, DefaultSizeWidth, DefaultSizeHeight, null);
                }
                catch
                {
                    // Well, can't do anything, just set the exit code and rethrow
                    State.ApplicationExitCode = 1;
                    throw;
                }
            }

            // Initialize() must have been called and succeeded for this function to proceed
            // CreateWindow() or SetWindow() must have been called and succeeded for this function to proceed
            // CreateDevice() or CreateDeviceFromSettings() or SetDevice() must have been called and succeeded for this function to proceed
            if ( (!State.IsInited) || (!State.WasWindowCreated) || (!State.WasDeviceCreated) )
            {
                State.ApplicationExitCode = 1;
                throw new InvalidOperationException("The framework was not initialized, cannot continue.");
            }

            using (System.Windows.Forms.Control app = Window)
            {
                // Hook the application's idle event
                System.Windows.Forms.Application.Idle += new EventHandler(OnApplicationIdle);
                // Start the application
                if (app is System.Windows.Forms.Form)
                {
                    //(app as System.Windows.Forms.Form).ShowDialog();
                    System.Windows.Forms.Application.Run(app as System.Windows.Forms.Form);
                }
                else
                {
                    //throw new ApplicationException("Not possible");
                    // Show the window
                    app.Show();
                    System.Windows.Forms.Application.Run();
                }
            }

            State.IsInsideMainloop = false;
        }

        #region App Idle Events
        /// <summary>Fired when the application first gets idle</summary>
        private void OnApplicationIdle(object sender, EventArgs e)
        {
            while (AppStillIdle)
            {
                // Render a frame during idle time (no messages are waiting)
                Render3DEnvironment();
            }
        }
        /// <summary>Checks to see if the application is still idle</summary>
        private bool AppStillIdle
        {
            get
            {
                NativeMethods.Message msg;
                return !NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }
        #endregion

        #region IDisposable Members
        /// <summary>Shut down the sample framework</summary>
        public void Dispose()
        {
            // Disable finalization of this object now
            GC.SuppressFinalize(this);

            if (!isDisposed)
            {
                // Raise the event if need be
                if (Disposing != null)
                    Disposing(this, EventArgs.Empty);

                // Now shut down the system
                if (Window != null)
                {
                    // If we have a window, use BeginInvoke to start shutdown
                    Window.BeginInvoke(new DisposeDelegate(this.Shutdown));
                }
                else
                {
                    // Otherwise, call it directly
                    Shutdown();
                }
            }
            
            // The object has now been disposed
            isDisposed = true;
        }

        /// <summary>In case they forget to dispose</summary>
        ~Framework()
        {
            Dispose();
        }
        #endregion

        #region Device event handlers
        /// <summary>Fired when the device is disposing</summary>
        private void OnDeviceDisposing(object sender, EventArgs e)
        {
            // Mark as being inside a callback
            State.IsInsideDeviceCallback = true;
 
            // Release all vidmem objects
            if (State.AreDeviceObjectsReset)
            {
                DialogResourceManager.GetGlobalInstance().OnDestroyDevice();
                ResourceCache.GetGlobalInstance().OnDestroyDevice();
                if (State.Settings != null)
                {
                    State.Settings.OnDestroyDevice( this, EventArgs.Empty);
                }
                State.AreDeviceObjectsCreated = false;
            }

            // Pass this event along
            if (Disposing != null)
                Disposing(this, e);

            // Now you're not
            State.IsInsideDeviceCallback = false;
        }

        /// <summary>Fired when the device is lost</summary>
        private void OnDeviceLost(object sender, EventArgs e)
        {
            // Mark as being inside a callback
            State.IsInsideDeviceCallback = true;
 
            // Release all vidmem objects
            if (State.AreDeviceObjectsReset)
            {
                DialogResourceManager.GetGlobalInstance().OnLostDevice();
                ResourceCache.GetGlobalInstance().OnLostDevice();
                if (State.Settings != null)
                {
                    State.Settings.OnLostDevice();
                }
                State.AreDeviceObjectsReset = false;
            }

            // Pass this event along
            if (DeviceLost != null)
                DeviceLost(this, e);

            // Now you're not
            State.IsInsideDeviceCallback = false;
        }

        /// <summary>Fired when the device is reset</summary>
        private void OnDeviceReset(object sender, EventArgs e)
        {
            // Get the device
            Device device = sender as Device;
            System.Diagnostics.Debug.Assert(device != null, "Must have a device here.");

            // Mark as being inside a callback
            State.IsInsideDeviceCallback = true;

            // Prepare the device with cursor info and 
            // store backbuffer desc and caps from the device
            PrepareDevice(device);
 
            if (State.Settings != null)
                State.Settings.OnResetDevice();

            // Call the dialog resource manager device reset function
            DialogResourceManager.GetGlobalInstance().OnResetDevice(device);
            // Call the resource cache device reset function
            ResourceCache.GetGlobalInstance().OnResetDevice(device);

            // Call app's reset callback
            if (DeviceReset != null)
                DeviceReset(this, new DeviceEventArgs(device, State.BackBufferSurfaceDesc));

            // Device objects are reset now
            State.AreDeviceObjectsReset = true;
            // Now you're not
            State.IsInsideDeviceCallback = false;
        }
        #endregion

    }
}