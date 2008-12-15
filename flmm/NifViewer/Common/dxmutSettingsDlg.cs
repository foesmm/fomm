//--------------------------------------------------------------------------------------
// File: DXMUTSettingsDlg.cs
//
// Dialog for selection of device settings 
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//--------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Microsoft.Samples.DirectX.UtilityToolkit
{
    #region Control Ids
    public enum SettingsDialogControlIds
    {
        Static = -1,
        None,
        OK,
        Cancel,
        Adapter,
        DeviceType,
        Windowed,
        Fullscreen,
        AdapterFormat,
        AdapterFormatLabel,
        Resolution,
        ResolutionLabel,
        RefreshRate,
        RefreshRateLabel,
        BackBufferFormat,
        DepthStencil,
        MultisampleType,
        MultisampleQuality,
        VertexProcessing,
        PresentInterval,
        DeviceClip,
        RadioButtonGroup = 0x100,
    }
    #endregion

    /// <summary>
    /// Dialog for selection of device settings 
    /// </summary>
    public class SettingsDialog
    {
        #region Creation
        /// <summary>Creates a new settings dialog</summary>
        public SettingsDialog(Framework sample) 
        {
            parent = sample;
            windowWidth = Framework.DefaultSizeWidth; windowHeight = Framework.DefaultSizeHeight;
            CreateControls();
        } 
        #endregion

        #region Class Data
        private Framework parent; // Parent framework for this dialog
        private Dialog dialog; // Dialog that will be rendered
        private uint windowWidth; // Width of window
        private uint windowHeight; // Height of window
        private DeviceSettings globalSettings; // Device settings
        private StateBlock state; // state block for device
        #endregion

        #region Control variables
        // Combo boxes
        private ComboBox resolution;
        private ComboBox adapterCombo;
        private ComboBox deviceCombo;
        private ComboBox adapterFormatCombo;
        private ComboBox refreshCombo;
        private ComboBox backBufferCombo;
        private ComboBox depthStencilCombo;
        private ComboBox multiSampleTypeCombo;
        private ComboBox multiSampleQualityCombo;
        private ComboBox vertexCombo;
        private ComboBox presentCombo;
        // Check boxes
        private Checkbox clipBox;
        // Radio buttons
        private RadioButton windowedButton;
        private RadioButton fullscreenButton;
        // Static controls that are cared about
        private StaticText adapterFormatStatic;
        private StaticText resolutionStatic;
        private StaticText refreshStatic;
        #endregion

        /// <summary>
        /// Creates the controls for use in the dialog
        /// </summary>
        private void CreateControls()
        {
            dialog = new Dialog(parent);
            dialog.IsUsingKeyboardInput = true;
            dialog.SetFont(0, "Arial", 15, FontWeight.Normal);
            dialog.SetFont(1, "Arial", 28, FontWeight.Bold);
            
            // Right justify static controls
            Element e = dialog.GetDefaultElement(ControlType.StaticText, 0);
            e.textFormat = DrawTextFormat.VerticalCenter | DrawTextFormat.Right;

            // Title
            StaticText title = dialog.AddStatic((int)SettingsDialogControlIds.Static, "Direct3D Settings", 10, 5, 400, 50);
            e = title[0];
            e.FontIndex = 1;
            e.textFormat = DrawTextFormat.Top | DrawTextFormat.Left;

            // Adapter
            dialog.AddStatic((int)SettingsDialogControlIds.Static, "Display Adapter", 10, 50, 180, 23);
            adapterCombo = dialog.AddComboBox((int)SettingsDialogControlIds.Adapter, 200, 50, 300, 23);

            // Device Type
            dialog.AddStatic((int)SettingsDialogControlIds.Static, "Render Device", 10, 75, 180, 23);
            deviceCombo = dialog.AddComboBox((int)SettingsDialogControlIds.DeviceType, 200, 75, 300, 23);

            // Windowed / Fullscreen
            windowedButton = dialog.AddRadioButton((int)SettingsDialogControlIds.Windowed, (int)SettingsDialogControlIds.RadioButtonGroup, 
                "Windowed", 240, 105, 300, 16, false);
            clipBox = dialog.AddCheckBox((int)SettingsDialogControlIds.DeviceClip, "Clip to device when window spans across multiple monitors", 
                250, 126, 400, 16, false);
            fullscreenButton = dialog.AddRadioButton((int)SettingsDialogControlIds.Fullscreen, (int)SettingsDialogControlIds.RadioButtonGroup, "Full Screen", 
                240, 147, 300, 16, false);

            // Adapter Format
            adapterFormatStatic = dialog.AddStatic((int)SettingsDialogControlIds.AdapterFormatLabel, "Adapter Format", 
                10, 180, 180, 23);
            adapterFormatCombo = dialog.AddComboBox((int)SettingsDialogControlIds.AdapterFormat, 200, 180, 300, 23);

            // Resolution
            resolutionStatic = dialog.AddStatic((int)SettingsDialogControlIds.ResolutionLabel, "Resolution", 10, 205, 180, 23);
            resolution = dialog.AddComboBox((int)SettingsDialogControlIds.Resolution, 200, 205, 300, 23);
            resolution.SetDropHeight(106);

            // Refresh Rate
            refreshStatic = dialog.AddStatic((int)SettingsDialogControlIds.RefreshRateLabel, "Refresh Rate", 10, 230, 180, 23);
            refreshCombo = dialog.AddComboBox((int)SettingsDialogControlIds.RefreshRate, 200, 230, 300, 23);

            // BackBuffer Format
            dialog.AddStatic((int)SettingsDialogControlIds.Static, "Back Buffer Format", 10, 265, 180, 23 );
            backBufferCombo = dialog.AddComboBox((int)SettingsDialogControlIds.BackBufferFormat, 200, 265, 300, 23 );

            // Depth Stencil
            dialog.AddStatic((int)SettingsDialogControlIds.Static, "Depth/Stencil Format", 10, 290, 180, 23 );
            depthStencilCombo = dialog.AddComboBox((int)SettingsDialogControlIds.DepthStencil, 200, 290, 300, 23 );

            // Multisample Type
            dialog.AddStatic((int)SettingsDialogControlIds.Static, "Multisample Type", 10, 315, 180, 23 );
            multiSampleTypeCombo = dialog.AddComboBox((int)SettingsDialogControlIds.MultisampleType, 200, 315, 300, 23 );

            // Multisample Quality
            dialog.AddStatic((int)SettingsDialogControlIds.Static, "Multisample Quality", 10, 340, 180, 23 );
            multiSampleQualityCombo = dialog.AddComboBox((int)SettingsDialogControlIds.MultisampleQuality, 200, 340, 300, 23 );

            // Vertex Processing
            dialog.AddStatic((int)SettingsDialogControlIds.Static, "Vertex Processing", 10, 365, 180, 23 );
            vertexCombo = dialog.AddComboBox((int)SettingsDialogControlIds.VertexProcessing, 200, 365, 300, 23 );

            // Present Interval
            dialog.AddStatic((int)SettingsDialogControlIds.Static, "Present Interval", 10, 390, 180, 23 );
            presentCombo = dialog.AddComboBox((int)SettingsDialogControlIds.PresentInterval, 200, 390, 300, 23 );

            // Add the ok/cancel buttons
            Button okButton = dialog.AddButton((int)SettingsDialogControlIds.OK, "OK", 230,435,73,31);
            Button cancelButton = dialog.AddButton((int)SettingsDialogControlIds.Cancel, "Cancel", 315,435,73,31,0, true);
            okButton.Click += new EventHandler(OnOkClicked);
            cancelButton.Click += new EventHandler(OnCancelClicked);
        }

        /// <summary>Changes the UI defaults to the current device settings</summary>
        public void Refresh()
        {
            // Get some information
            globalSettings = parent.DeviceSettings.Clone();
            System.Drawing.Rectangle client = parent.WindowClientRectangle;
            windowWidth = (uint)client.Width;
            windowHeight = (uint)client.Height;

            // Fill the UI with the current settings
            if (!deviceCombo.ContainsItem(globalSettings.DeviceType.ToString()))
                deviceCombo.AddItem(globalSettings.DeviceType.ToString(), globalSettings.DeviceType.ToString());

            SetWindowed(globalSettings.presentParams.Windowed);
            clipBox.IsChecked = ((globalSettings.presentParams.PresentFlag & PresentFlag.DeviceClip) != 0);

            if (!adapterFormatCombo.ContainsItem(globalSettings.AdapterFormat.ToString()))
                adapterFormatCombo.AddItem(globalSettings.AdapterFormat.ToString(), globalSettings.AdapterFormat);

            AddResolution((short)globalSettings.presentParams.BackBufferWidth, (short)globalSettings.presentParams.BackBufferHeight);
            AddRefreshRate(globalSettings.presentParams.FullScreenRefreshRateInHz);

            if (!backBufferCombo.ContainsItem(globalSettings.presentParams.BackBufferFormat.ToString()))
                backBufferCombo.AddItem(globalSettings.presentParams.BackBufferFormat.ToString(), globalSettings.presentParams.BackBufferFormat);

            if (!depthStencilCombo.ContainsItem(globalSettings.presentParams.AutoDepthStencilFormat.ToString()))
                depthStencilCombo.AddItem(globalSettings.presentParams.AutoDepthStencilFormat.ToString(), globalSettings.presentParams.AutoDepthStencilFormat);

            if (!multiSampleTypeCombo.ContainsItem(globalSettings.presentParams.MultiSample.ToString()))
                multiSampleTypeCombo.AddItem(globalSettings.presentParams.MultiSample.ToString(), globalSettings.presentParams.MultiSample);

            if (!multiSampleQualityCombo.ContainsItem(globalSettings.presentParams.MultiSampleQuality.ToString()))
                multiSampleQualityCombo.AddItem(globalSettings.presentParams.MultiSampleQuality.ToString(), globalSettings.presentParams.MultiSampleQuality);

            if (!presentCombo.ContainsItem(globalSettings.presentParams.PresentationInterval.ToString()))
                presentCombo.AddItem(globalSettings.presentParams.PresentationInterval.ToString(), globalSettings.presentParams.PresentationInterval);

            BehaviorFlags flags = new BehaviorFlags(globalSettings.BehaviorFlags);
            if (flags.PureDevice)
                AddVertexProcessing(CreateFlags.PureDevice);
            else if (flags.HardwareVertexProcessing)
                AddVertexProcessing(CreateFlags.HardwareVertexProcessing);
            else if (flags.SoftwareVertexProcessing)
                AddVertexProcessing(CreateFlags.SoftwareVertexProcessing);
            else if (flags.MixedVertexProcessing)
                AddVertexProcessing(CreateFlags.MixedVertexProcessing);

            // Get the adapters list from Enumeration object
            ArrayList adapterInfoList = Enumeration.AdapterInformationList;

            if (adapterInfoList.Count == 0)
                throw new NoCompatibleDevicesException();

            adapterCombo.Clear();
            
            // Add all of the adapters
            for (int iAdapter = 0; iAdapter < adapterInfoList.Count; iAdapter++)
            {
                EnumAdapterInformation adapterInfo = adapterInfoList[iAdapter] as EnumAdapterInformation;
                if (!adapterCombo.ContainsItem(adapterInfo.UniqueDescription))
                    adapterCombo.AddItem(adapterInfo.UniqueDescription, iAdapter);
            }
            adapterCombo.SetSelectedByData(globalSettings.AdapterOrdinal);

            // The adapter changed, call the handler
            OnAdapterChanged(adapterCombo, EventArgs.Empty);

            Dialog.SetRefreshTime((float)FrameworkTimer.GetTime());
        }

        /// <summary>Render the dialog</summary>
        public void OnRender(float elapsedTime)
        {
            state.Capture();
            parent.Device.RenderState.FillMode = FillMode.Solid;
            dialog.OnRender(elapsedTime);
            state.Apply();
        }

        /// <summary>Hand messages off to dialog</summary>
        public void HandleMessages(IntPtr hWnd, NativeMethods.WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            dialog.MessageProc(hWnd, msg, wParam, lParam);
        }
        #region Device event callbacks
        /// <summary>
        /// Called when the device is created
        /// </summary>
        public void OnCreateDevice(Device d)  
        { 
            // Hook all the events we care about
            resolution.Changed += new EventHandler(OnResolutionChanged);
            adapterCombo.Changed += new EventHandler(OnAdapterChanged);
            deviceCombo.Changed += new EventHandler(OnDeviceChanged);
            adapterFormatCombo.Changed += new EventHandler(OnAdapterFormatChange);
            refreshCombo.Changed += new EventHandler(OnRefreshRateChanged);
            backBufferCombo.Changed += new EventHandler(OnBackBufferChanged);
            depthStencilCombo.Changed += new EventHandler(OnDepthStencilChanged);
            multiSampleTypeCombo.Changed += new EventHandler(OnMultisampleTypeChanged);
            multiSampleQualityCombo.Changed += new EventHandler(OnMultisampleQualityChanged);
            vertexCombo.Changed += new EventHandler(OnVertexProcessingChanged);
            presentCombo.Changed += new EventHandler(OnPresentIntervalChanged);
            clipBox.Changed += new EventHandler(OnClipWindowChanged);
            windowedButton.Changed += new EventHandler(OnWindowedFullscreenChanged);
            fullscreenButton.Changed += new EventHandler(OnWindowedFullscreenChanged);
        } 

        /// <summary>
        /// Called when the device is reset
        /// </summary>
        public void OnResetDevice()
        {
            SurfaceDescription desc = parent.BackBufferSurfaceDescription;

            // Set up the dialog
            dialog.SetLocation(0, 0);
            dialog.SetSize(desc.Width, desc.Height);
            dialog.SetBackgroundColors(new ColorValue((float)98/255, (float)138/255, (float)206/255),
                new ColorValue((float)54/255, (float)105/255, (float)192/255),
                new ColorValue((float)54/255, (float)105/255, (float)192/255),
                new ColorValue((float)10/255, (float)73/255, (float)179/255) );

            Device device = parent.Device;
            device.BeginStateBlock();
            device.RenderState.FillMode = FillMode.Solid;
            state = device.EndStateBlock();

        }

        /// <summary>
        /// Called when the device is lost
        /// </summary>
        public void OnLostDevice()
        {
            if (state != null)
                state.Dispose();

            state = null;
        }

        /// <summary>Destroy any resources</summary>
        public void OnDestroyDevice(object sender, EventArgs e)
        {
            // Clear the focus
            Dialog.ClearFocus();
            // Unhook all the events we care about
            resolution.Changed -= new EventHandler(OnResolutionChanged);
            adapterCombo.Changed -= new EventHandler(OnAdapterChanged);
            deviceCombo.Changed -= new EventHandler(OnDeviceChanged);
            adapterFormatCombo.Changed -= new EventHandler(OnAdapterFormatChange);
            refreshCombo.Changed -= new EventHandler(OnRefreshRateChanged);
            backBufferCombo.Changed -= new EventHandler(OnBackBufferChanged);
            depthStencilCombo.Changed -= new EventHandler(OnDepthStencilChanged);
            multiSampleTypeCombo.Changed -= new EventHandler(OnMultisampleTypeChanged);
            multiSampleQualityCombo.Changed -= new EventHandler(OnMultisampleQualityChanged);
            vertexCombo.Changed -= new EventHandler(OnVertexProcessingChanged);
            presentCombo.Changed -= new EventHandler(OnPresentIntervalChanged);
            clipBox.Changed -= new EventHandler(OnClipWindowChanged);
            windowedButton.Changed -= new EventHandler(OnWindowedFullscreenChanged);
            fullscreenButton.Changed -= new EventHandler(OnWindowedFullscreenChanged);
        }

        #endregion

        /// <summary>Returns the current device information</summary>
        private EnumDeviceInformation GetCurrentDeviceInfo()
        {
            return Enumeration.GetDeviceInfo(globalSettings.AdapterOrdinal, globalSettings.DeviceType);
        }
        /// <summary>Returns the current adapter information</summary>
        private EnumAdapterInformation GetCurrentAdapterInfo()
        {
            return Enumeration.GetAdapterInformation(globalSettings.AdapterOrdinal);
        }
        /// <summary>Returns the current adapter information</summary>
        private EnumDeviceSettingsCombo GetCurrentDeviceSettingsCombo()
        {
            return Enumeration.GetDeviceSettingsCombo(globalSettings.AdapterOrdinal,
                globalSettings.DeviceType, globalSettings.AdapterFormat, 
                globalSettings.presentParams.BackBufferFormat, globalSettings.presentParams.Windowed);
        }

        // TODO: SetDeviceSettingsFromUI
        #region Update UI Methods
        /// <summary>Sets whether this is running in windowed or fullscreen mode</summary>
        private void SetWindowed(bool windowed)
        {
            windowedButton.IsChecked = windowed;
            fullscreenButton.IsChecked = !windowed;
        }
        /// <summary>Adds a resolution to the combo box</summary>
        private void AddResolution(short width, short height)
        {
            string itemText = string.Format("{0} by {1}", width, height);
            // Store the resolution in a single variable
            uint resolutionData = NativeMethods.MakeUInt32(width, height);

            // Add this item
            if (!resolution.ContainsItem(itemText))
                resolution.AddItem(itemText, resolutionData);
        }
        /// <summary>Adds a refresh rate to the combo box</summary>
        private void AddRefreshRate(int rate)
        {
            string itemText = (rate == 0) ? "Default Rate" : string.Format("{0} Hz", rate);
            // Add this item
            if (!refreshCombo.ContainsItem(itemText))
                refreshCombo.AddItem(itemText, rate);
        }
        /// <summary>Adds a vertex processing type to the combo box</summary>
        private void AddVertexProcessing(CreateFlags flags)
        {
            string itemText = "Unknown vertex processing type";
            switch(flags)
            {
                case CreateFlags.PureDevice:
                    itemText = "Pure hardware vertex processing"; break;
                case CreateFlags.HardwareVertexProcessing:
                    itemText = "Hardware vertex processing"; break;
                case CreateFlags.SoftwareVertexProcessing:
                    itemText = "Software vertex processing"; break;
                case CreateFlags.MixedVertexProcessing:
                    itemText = "Mixed vertex processing"; break;
            }
            // Add this item
            if (!vertexCombo.ContainsItem(itemText))
                vertexCombo.AddItem(itemText, flags);
        }
        #endregion

        #region Event handlers for the controls
        /// <summary>Called when the resolution changes</summary>
        private void OnResolutionChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            // Set the resolution
            uint data = (uint)cb.GetSelectedData();
            int width = NativeMethods.LoWord(data);
            int height = NativeMethods.HiWord(data);
            globalSettings.presentParams.BackBufferWidth = width;
            globalSettings.presentParams.BackBufferHeight = height;

            int refreshRate = globalSettings.presentParams.FullScreenRefreshRateInHz;

            // Update the refresh rate list
            refreshCombo.Clear();

            EnumAdapterInformation adapterInfo = GetCurrentAdapterInfo();
            Format adapterFormat = globalSettings.AdapterFormat;
            foreach(DisplayMode dm in adapterInfo.displayModeList)
            {
                if (dm.Format == adapterFormat &&
                    dm.Width == width &&
                    dm.Height == height)
                {
                    AddRefreshRate(dm.RefreshRate);
                }
            }

            // select and update
            refreshCombo.SetSelectedByData(refreshRate);
            OnRefreshRateChanged(refreshCombo, e);
        }

        /// <summary>Called when the adapter changes</summary>
        private void OnAdapterChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            // Store the adapter index
            globalSettings.AdapterOrdinal = (uint)(int)cb.GetSelectedData();

            // Remove all the items from the device type
            deviceCombo.Clear();

            // Get the adapter information
            EnumAdapterInformation adapterInfo = GetCurrentAdapterInfo();

            // Add each device type to the combo list
            foreach(EnumDeviceInformation edi in adapterInfo.deviceInfoList)
            {
                if (!deviceCombo.ContainsItem(edi.DeviceType.ToString()))
                    deviceCombo.AddItem(edi.DeviceType.ToString(), edi.DeviceType);
            }
            deviceCombo.SetSelectedByData(globalSettings.DeviceType);

            // Device type was changed update
            OnDeviceChanged(deviceCombo, e);
        }
        /// <summary>Called when the device type changes</summary>
        private void OnDeviceChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            globalSettings.DeviceType = (DeviceType)cb.GetSelectedData();

            // Update windowed/full screen radio buttons
            bool hasWindowCombo = false;
            bool hasFullscreen = false;

            EnumDeviceInformation edi = GetCurrentDeviceInfo();

            // See if there are any windowed/fullscreen combos
            foreach(EnumDeviceSettingsCombo edsc in edi.deviceSettingsList)
            {
                if (edsc.IsWindowed)
                    hasWindowCombo = true;
                else
                    hasFullscreen = true;
            }

            // Set the controls enable/disable property based on whether they are available or not
            dialog.SetControlEnable((int)SettingsDialogControlIds.Windowed, hasWindowCombo);
            dialog.SetControlEnable((int)SettingsDialogControlIds.Fullscreen, hasFullscreen);

            SetWindowed(globalSettings.presentParams.Windowed && hasWindowCombo);

            OnWindowedFullscreenChanged(null, e);
        }

        /// <summary>Called when the adapter format changes</summary>
        private void OnAdapterFormatChange(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            Format adapterFormat = (Format)cb.GetSelectedData();

            // Resolutions
            resolution.Clear();

            EnumAdapterInformation adapterInfo = GetCurrentAdapterInfo();
            foreach(DisplayMode dm in adapterInfo.displayModeList)
            {
                if (dm.Format == adapterFormat)
                    AddResolution((short)dm.Width, (short)dm.Height);
            }

            uint currentResolution = NativeMethods.MakeUInt32(
                (short)globalSettings.presentParams.BackBufferWidth, (short)globalSettings.presentParams.BackBufferHeight);

            resolution.SetSelectedByData(currentResolution);
            // Resolution changed
            OnResolutionChanged(resolution, e);

            // Back buffer formats
            backBufferCombo.Clear();

            EnumDeviceInformation edi = GetCurrentDeviceInfo();
            bool hasWindowedBackBuffer = false;
            bool isWindowed = windowedButton.IsChecked;

            foreach(EnumDeviceSettingsCombo edsc in edi.deviceSettingsList)
            {
                if (edsc.IsWindowed == isWindowed &&
                    edsc.AdapterFormat == globalSettings.AdapterFormat)
                {
                    hasWindowedBackBuffer = true;
                    if (!backBufferCombo.ContainsItem(edsc.BackBufferFormat.ToString()))
                        backBufferCombo.AddItem(edsc.BackBufferFormat.ToString(), edsc.BackBufferFormat);
                }
            }
            // Update back buffer
            backBufferCombo.SetSelectedByData(globalSettings.presentParams.BackBufferFormat);
            OnBackBufferChanged(backBufferCombo, e);

            if (!hasWindowedBackBuffer)
            {
                dialog.SetControlEnable((int)SettingsDialogControlIds.Windowed, false);
                if (globalSettings.presentParams.Windowed)
                {
                    SetWindowed(false);
                    OnWindowedFullscreenChanged(null, e);
                }
            }
        }

        /// <summary>Called when the refresh rate changes</summary>
        private void OnRefreshRateChanged(object sender, EventArgs e)
        {
            ComboBox c = sender as ComboBox;
            globalSettings.presentParams.FullScreenRefreshRateInHz = (int)c.GetSelectedData();
        }
        
        /// <summary>Called when the back buffer format changes</summary>
        private void OnBackBufferChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            globalSettings.presentParams.BackBufferFormat = (Format)backBufferCombo.GetSelectedData();

            // Store formats
            Format adapterFormat = globalSettings.AdapterFormat;
            Format backFormat = globalSettings.presentParams.BackBufferFormat;

            EnumDeviceInformation edi = GetCurrentDeviceInfo();

            // Get possible vertex processing
            bool isAllowSoftware = Enumeration.IsSoftwareVertexProcessingPossible;
            bool isAllowHardware = Enumeration.IsHardwareVertexProcessingPossible;
            bool isAllowPure = Enumeration.IsPureHardwareVertexProcessingPossible;
            bool isAllowMixed = Enumeration.IsMixedVertexProcessingPossible;

            foreach(EnumDeviceSettingsCombo edsc in edi.deviceSettingsList)
            {
                if (edsc.IsWindowed == globalSettings.presentParams.Windowed &&
                    edsc.AdapterFormat == adapterFormat &&
                    edsc.BackBufferFormat == backFormat)
                {
                    // Clear the depth stencil buffer
                    depthStencilCombo.Clear();
                    depthStencilCombo.IsEnabled = (globalSettings.presentParams.EnableAutoDepthStencil);
                    if (globalSettings.presentParams.EnableAutoDepthStencil)
                    {
                        foreach(Format f in edsc.depthStencilFormatList)
                        {
                            if (!depthStencilCombo.ContainsItem(f.ToString()))
                                depthStencilCombo.AddItem(f.ToString(), f);
                        }

                        depthStencilCombo.SetSelectedByData(globalSettings.presentParams.AutoDepthStencilFormat);
                    }
                    else
                    {
                        if (!depthStencilCombo.ContainsItem("(not used)") )
                            depthStencilCombo.AddItem("(not used)", null);
                    }
                    OnDepthStencilChanged(depthStencilCombo, e);

                    // Now remove all the vertex processing information
                    vertexCombo.Clear();
                    if (isAllowPure)
                        AddVertexProcessing(CreateFlags.PureDevice);
                    if (isAllowHardware)
                        AddVertexProcessing(CreateFlags.HardwareVertexProcessing);
                    if (isAllowSoftware)
                        AddVertexProcessing(CreateFlags.SoftwareVertexProcessing);
                    if (isAllowMixed)
                        AddVertexProcessing(CreateFlags.MixedVertexProcessing);

                    // Select the right one
                    BehaviorFlags flags = new BehaviorFlags(globalSettings.BehaviorFlags);
                    if (flags.PureDevice)
                        vertexCombo.SetSelectedByData(CreateFlags.PureDevice);
                    else if (flags.HardwareVertexProcessing)
                        vertexCombo.SetSelectedByData(CreateFlags.HardwareVertexProcessing);
                    else if (flags.SoftwareVertexProcessing)
                        vertexCombo.SetSelectedByData(CreateFlags.SoftwareVertexProcessing);
                    else if (flags.MixedVertexProcessing)
                        vertexCombo.SetSelectedByData(CreateFlags.MixedVertexProcessing);

                    OnVertexProcessingChanged(vertexCombo, e);

                    // Now present intervals
                    presentCombo.Clear();
                    foreach(PresentInterval pf in edsc.presentIntervalList)
                    {
                        if (!presentCombo.ContainsItem(pf.ToString()))
                            presentCombo.AddItem(pf.ToString(), pf);
                    }

                    presentCombo.SetSelectedByData(globalSettings.presentParams.PresentationInterval);
                    OnPresentIntervalChanged(presentCombo, e);
                }
            }

        }
        
        /// <summary>Called when the depth stencil changes</summary>
        private void OnDepthStencilChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            DepthFormat stencilFormat = (DepthFormat)cb.GetSelectedData();

            if (globalSettings.presentParams.EnableAutoDepthStencil)
                globalSettings.presentParams.AutoDepthStencilFormat = stencilFormat;

            EnumDeviceSettingsCombo combo = GetCurrentDeviceSettingsCombo();

            // Remove all of the multisample items and add the new ones
            multiSampleTypeCombo.Clear();
            foreach(MultiSampleType mst in combo.multiSampleTypeList)
            {
                bool conflictFound = false;
                foreach(EnumDepthStencilMultisampleConflict c in combo.depthStencilConflictList)
                {
                    if (c.DepthStencilFormat == stencilFormat &&
                        c.MultisampleType == mst)
                    {
                        conflictFound = true;
                        break;
                    }
                }

                if (!conflictFound)
                {
                    if (!multiSampleTypeCombo.ContainsItem(mst.ToString()))
                        multiSampleTypeCombo.AddItem(mst.ToString(), mst);
                }
            }
            // Select the correct multisampling type
            multiSampleTypeCombo.SetSelectedByData(globalSettings.presentParams.MultiSample);
            OnMultisampleTypeChanged(multiSampleTypeCombo, e);
        }
        
        /// <summary>Called when the multisample type changes</summary>
        private void OnMultisampleTypeChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            MultiSampleType mst = (MultiSampleType)cb.GetSelectedData();
            globalSettings.presentParams.MultiSample = mst;

            EnumDeviceSettingsCombo combo = GetCurrentDeviceSettingsCombo();

            int maxQuality = 0;
            for (int i = 0; i < combo.multiSampleTypeList.Count; i++)
            {
                MultiSampleType msType = (MultiSampleType)combo.multiSampleTypeList[i];
                if (msType == mst)
                {
                    maxQuality = (int)combo.multiSampleQualityList[i];
                }
            }

            // We have the max quality now, add to our list
            multiSampleQualityCombo.Clear();
            for(int i = 0; i < maxQuality; i++)
            {
                if (!multiSampleQualityCombo.ContainsItem(i.ToString()))
                    multiSampleQualityCombo.AddItem(i.ToString(), i);
            }
            multiSampleQualityCombo.SetSelectedByData(globalSettings.presentParams.MultiSampleQuality);
            OnMultisampleQualityChanged(multiSampleQualityCombo, e);
        }
        /// <summary>Called when the multisample quality changes</summary>
        private void OnMultisampleQualityChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            globalSettings.presentParams.MultiSampleQuality = (int)cb.GetSelectedData();
        }
        /// <summary>Called when the vertex processing changes</summary>
        private void OnVertexProcessingChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            CreateFlags behavior = globalSettings.BehaviorFlags;

            // Clear flags
            behavior &= ~CreateFlags.HardwareVertexProcessing;
            behavior &= ~CreateFlags.SoftwareVertexProcessing;
            behavior &= ~CreateFlags.PureDevice;
            behavior &= ~CreateFlags.MixedVertexProcessing;

            // Determine new flags
            CreateFlags newFlags = (CreateFlags)vertexCombo.GetSelectedData();
            if ((newFlags & CreateFlags.PureDevice) != 0)
                newFlags |= CreateFlags.HardwareVertexProcessing;

            // Make changes
            globalSettings.BehaviorFlags = behavior | newFlags;
        }
        /// <summary>Called when the presentation interval changes</summary>
        private void OnPresentIntervalChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            globalSettings.presentParams.PresentationInterval = (PresentInterval)cb.GetSelectedData();
        }
        /// <summary>Called when the clip to window state changes</summary>
        private void OnClipWindowChanged(object sender, EventArgs e)
        {
            Checkbox cb = sender as Checkbox;
            if (cb.IsChecked)
                globalSettings.presentParams.PresentFlag |= PresentFlag.DeviceClip;
            else
                globalSettings.presentParams.PresentFlag &= ~PresentFlag.DeviceClip;
        }
        /// <summary>Called when the fullscreen or windowed item changes</summary>
        private void OnWindowedFullscreenChanged(object sender, EventArgs e)
        {
            bool isWindowed = windowedButton.IsChecked;
            globalSettings.presentParams.Windowed = isWindowed;

            // Set the control enabled or disabled properties
            dialog.SetControlEnable((int)SettingsDialogControlIds.AdapterFormatLabel, !isWindowed);
            dialog.SetControlEnable((int)SettingsDialogControlIds.ResolutionLabel, !isWindowed);
            dialog.SetControlEnable((int)SettingsDialogControlIds.RefreshRateLabel, !isWindowed);

            dialog.SetControlEnable((int)SettingsDialogControlIds.AdapterFormat, !isWindowed);
            dialog.SetControlEnable((int)SettingsDialogControlIds.Resolution, !isWindowed);
            dialog.SetControlEnable((int)SettingsDialogControlIds.RefreshRate, !isWindowed);
            dialog.SetControlEnable((int)SettingsDialogControlIds.DeviceClip, isWindowed);

            bool deviceClip = ((globalSettings.presentParams.PresentFlag & PresentFlag.DeviceClip) != 0);

            // If windowed, get the appropriate adapter format from Direct3D
            if (globalSettings.presentParams.Windowed)
            {
                DisplayMode mode = Manager.Adapters[(int)globalSettings.AdapterOrdinal].CurrentDisplayMode;
                globalSettings.AdapterFormat = mode.Format;
                globalSettings.presentParams.BackBufferWidth = mode.Width;
                globalSettings.presentParams.BackBufferHeight = mode.Height;
                globalSettings.presentParams.FullScreenRefreshRateInHz = mode.RefreshRate;
            }

            // Update the clip check box
            clipBox.IsChecked = deviceClip;

            // Update the adapter format list
            adapterFormatCombo.Clear();

            EnumDeviceInformation edi = GetCurrentDeviceInfo();

            if (isWindowed)
            {
                if (!adapterFormatCombo.ContainsItem(globalSettings.AdapterFormat.ToString()))
                    adapterFormatCombo.AddItem(globalSettings.AdapterFormat.ToString(), globalSettings.AdapterFormat);
            }
            else
            {
                // Add all the supported formats
                foreach(EnumDeviceSettingsCombo edsc in edi.deviceSettingsList)
                {
                    if (!adapterFormatCombo.ContainsItem(edsc.AdapterFormat.ToString()))
                        adapterFormatCombo.AddItem(edsc.AdapterFormat.ToString(), edsc.AdapterFormat);
                }
            }
            adapterFormatCombo.SetSelectedByData(globalSettings.AdapterFormat);
            // Adapter format changed, update there
            OnAdapterFormatChange(adapterFormatCombo, EventArgs.Empty);

            // Update resolution
            if (isWindowed)
            {
                resolution.Clear();
                AddResolution((short)globalSettings.presentParams.BackBufferWidth, (short)globalSettings.presentParams.BackBufferHeight);
            }
            resolution.SetSelectedByData(NativeMethods.MakeUInt32(
                (short)globalSettings.presentParams.BackBufferWidth, (short)globalSettings.presentParams.BackBufferHeight));

            // Resolution changed
            OnResolutionChanged(resolution, EventArgs.Empty);

            // Update refresh
            if (isWindowed)
            {
                refreshCombo.Clear();
                AddRefreshRate(globalSettings.presentParams.FullScreenRefreshRateInHz);
            }

            // Select the correct refresh rate
            refreshCombo.SetSelectedByData(globalSettings.presentParams.FullScreenRefreshRateInHz);

            // refresh rate changed
            OnRefreshRateChanged(refreshCombo, EventArgs.Empty);
        }
        /// <summary>Called when the cancel button is clicked</summary>
        private void OnCancelClicked(object sender, EventArgs e)
        {
            // Nothing left to do, quit showing the screen
            parent.ShowSettingsDialog(false);
        }
        /// <summary>Called when the ok button is clicked</summary>
        private void OnOkClicked(object sender, EventArgs e)
        {
            // The device needs to be updated
            if (globalSettings.presentParams.Windowed)
            {
                globalSettings.presentParams.FullScreenRefreshRateInHz = 0;
                globalSettings.presentParams.BackBufferWidth = (int)windowWidth;
                globalSettings.presentParams.BackBufferHeight = (int)windowHeight;
            }

            if (globalSettings.presentParams.MultiSample != MultiSampleType.None)
            {
                globalSettings.presentParams.PresentFlag &= ~PresentFlag.LockableBackBuffer;
            }

            // Create a device
            parent.CreateDeviceFromSettings(globalSettings);

            // Stop showing the dialog now
            parent.ShowSettingsDialog(false);
        }
        #endregion
    }
}