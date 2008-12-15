//--------------------------------------------------------------------------------------
// File: DXMUTEnum.cs
//
// Enumerates D3D adapters, devices, modes, etc.
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
    /// Enumerates available Direct3D adapters, devices, modes, etc.  Singleton.
    /// </summary>
    public sealed class Enumeration
    {
        #region Creation (Not allowed)
        private Enumeration() {} // Do Not allow Creation
        /// <summary>
        /// Static constructor to create default lists
        /// </summary>
        static Enumeration()
        {
            // Create the default lists
            ResetPossibleDepthStencilFormats();
            ResetPossibleMultisampleTypeList();
            ResetPossiblePresentIntervalList();
        } 
        #endregion

        // Static Data
        private static bool isPostPixelShaderBlendingRequired = true;
        private static IDeviceCreation deviceCreationInterface = null;

        // Vertex processing
        private static bool isSoftwareVertexProcessing = true;
        private static bool isHardwareVertexProcessing = true;
        private static bool isPureHardwareVertexProcessing = true;
        private static bool isMixedVertexProcessing = false;

        // Limits
        private static uint minimumWidth = 0;
        private static uint maximumWidth = uint.MaxValue;
        private static uint minimumHeight = 0;
        private static uint maximumHeight = uint.MaxValue;
        private static uint minimumRefresh = 0;
        private static uint maximumRefresh = uint.MaxValue;
        private static uint multisampleQualityMax = 0xffff;

        // Lists
        private static ArrayList depthStencilPossibleList = new ArrayList();
        private static ArrayList multiSampleTypeList = new ArrayList();
        private static ArrayList presentIntervalList = new ArrayList();
        private static ArrayList adapterInformationList = new ArrayList();

        // Default arrays
        private static readonly Format[] allowedFormats = new Format[] {
                        Format.X8R8G8B8, 
                        Format.X1R5G5B5,
                        Format.R5G6B5,
                        Format.A2R10G10B10 };
        private static readonly Format[] backbufferFormatsArray = new Format[] {
                        Format.A8R8G8B8,
                        Format.X8R8G8B8, 
                        Format.A1R5G5B5,
                        Format.X1R5G5B5,
                        Format.R5G6B5,
                        Format.A2R10G10B10 };
        private static readonly DeviceType[] deviceTypeArray = new DeviceType[] {
                        DeviceType.Hardware,
                        DeviceType.Software,
                        DeviceType.Reference };


        // Implementation

        /// <summary>
        /// Enumerates available D3D adapters, devices, modes, etc
        /// </summary>
        public static void Enumerate(IDeviceCreation acceptableCallback)
        {
            DisplayModeSorter sorter = new DisplayModeSorter();
            try
            {
                // Store the callback
                deviceCreationInterface = acceptableCallback;

                // Clear the adapter information stored currently
                adapterInformationList.Clear();
                ArrayList adapterFormatList = new ArrayList();

                // Look through every adapter on the system
                foreach(AdapterInformation ai in Manager.Adapters)
                {
                    EnumAdapterInformation adapterInfo = new EnumAdapterInformation();
                    // Store some information
                    adapterInfo.AdapterOrdinal = (uint)ai.Adapter; // Ordinal
                    adapterInfo.AdapterInformation = ai.Information; // Information

                    // Get list of all display modes on this adapter.  
                    // Also build a temporary list of all display adapter formats.
                    adapterFormatList.Clear();

                    // Now check to see which formats are supported
                    for(int i = 0; i < allowedFormats.Length; i++)
                    {
                        // Check each of the supported display modes for this format
                        foreach(DisplayMode dm in ai.SupportedDisplayModes[allowedFormats[i]])
                        {
                            if ( (dm.Width < minimumWidth) ||
                                (dm.Height < minimumHeight) ||
                                (dm.Width > maximumWidth) ||
                                (dm.Height > maximumHeight) ||
                                (dm.RefreshRate < minimumRefresh) ||
                                (dm.RefreshRate > maximumRefresh) )
                            {
                                continue; // This format isn't valid
                            }

                            // Add this to the list
                            adapterInfo.displayModeList.Add(dm);

                            // Add this to the format list if it doesn't already exist
                            if (!adapterFormatList.Contains(dm.Format))
                            {
                                adapterFormatList.Add(dm.Format);
                            }
                        }
                    }

                    // Get the adapter display mode
                    DisplayMode currentAdapterMode = ai.CurrentDisplayMode;
                    // Check to see if this format is in the list
                    if (!adapterFormatList.Contains(currentAdapterMode.Format))
                    {
                        adapterFormatList.Add(currentAdapterMode.Format);
                    }

                    // Sort the display mode list
                    adapterInfo.displayModeList.Sort(sorter);

                    // Get information for each device with this adapter
                    EnumerateDevices(adapterInfo, adapterFormatList);

                    // If there was at least one device on the adapter, and it's compatible
                    // add it to the list
                    if (adapterInfo.deviceInfoList.Count > 0)
                    {
                        adapterInformationList.Add(adapterInfo);
                    }
                }

                // See if all of the descriptions are unique
                bool isUnique = true;
                for(int i = 0; i < adapterInformationList.Count; i++)
                {
                    for (int j = i+1; j < adapterInformationList.Count; j++)
                    {
                        EnumAdapterInformation eai1 = adapterInformationList[i] as EnumAdapterInformation;
                        EnumAdapterInformation eai2 = adapterInformationList[j] as EnumAdapterInformation;

                        if (string.Compare(eai1.AdapterInformation.Description,
                            eai2.AdapterInformation.Description, true) == 0)
                        {
                            isUnique = false;
                            break;
                        }
                    }
                    if (!isUnique)
                        break;
                }

                // Now fill the unique description
                for(int i = 0; i < adapterInformationList.Count; i++)
                {
                    EnumAdapterInformation eai1 = adapterInformationList[i] as EnumAdapterInformation;

                    eai1.UniqueDescription = eai1.AdapterInformation.Description;
                    // If the descriptions aren't unique, append the adapter ordinal
                    if (!isUnique)
                        eai1.UniqueDescription += string.Format(" (#{0})", eai1.AdapterOrdinal);
                }
            }
            catch (TypeLoadException)
            {
                // Couldn't load the manager class, no Direct is available.
                throw new NoDirect3DException();
            }
        }

        /// <summary>
        /// Enumerates D3D devices for a particular adapter.
        /// </summary>
        private static void EnumerateDevices(EnumAdapterInformation adapterInfo, ArrayList adapterFormatList)
        {
            // Ignore any exceptions while looking for these device types
            DirectXException.IgnoreExceptions();
            // Enumerate each Direct3D device type
            for(uint i = 0; i < deviceTypeArray.Length; i++)
            {
                // Create a new device information object
                EnumDeviceInformation deviceInfo = new EnumDeviceInformation();

                // Store the type
                deviceInfo.DeviceType = deviceTypeArray[i];

                // Try to get the capabilities
                deviceInfo.Caps = Manager.GetDeviceCaps((int)adapterInfo.AdapterOrdinal, deviceInfo.DeviceType);

                // Get information about each device combination on this device
                EnumerateDeviceCombos( adapterInfo, deviceInfo, adapterFormatList);

                // Do we have any device combinations?
                if (deviceInfo.deviceSettingsList.Count > 0)
                {
                    // Yes, add it
                    adapterInfo.deviceInfoList.Add(deviceInfo);
                }
            }
            // Turn exception handling back on
            DirectXException.EnableExceptions();
        }

        /// <summary>
        /// Enumerates device combinations for a particular device.
        /// </summary>
        private static void EnumerateDeviceCombos(EnumAdapterInformation adapterInfo, EnumDeviceInformation deviceInfo, 
            ArrayList adapterFormatList)
        {
            // Find out which adapter formats are supported by this device
            foreach(Format adapterFormat in adapterFormatList)
            {
                for(int i = 0; i < backbufferFormatsArray.Length; i++)
                {
                    // Go through each windowed mode
                    for (int windowedIndex = 0; windowedIndex < 2; windowedIndex++)
                    {
                        bool isWindowedIndex = (windowedIndex == 1);
                        if ((!isWindowedIndex) && (adapterInfo.displayModeList.Count == 0))
                            continue; // Nothing here

                        if (!Manager.CheckDeviceType((int)adapterInfo.AdapterOrdinal, deviceInfo.DeviceType,
                            adapterFormat, backbufferFormatsArray[i], isWindowedIndex))
                            continue; // Unsupported

                        // Do we require post pixel shader blending?
                        if (isPostPixelShaderBlendingRequired)
                        {
                            // If the backbuffer format doesn't support Usage.QueryPostPixelShaderBlending
                            // then alpha test, pixel fog, render-target blending, color write enable, and dithering
                            // are not supported.
                            if (!Manager.CheckDeviceFormat((int)adapterInfo.AdapterOrdinal, deviceInfo.DeviceType,
                                    adapterFormat, Usage.QueryPostPixelShaderBlending, 
                                    ResourceType.Textures, backbufferFormatsArray[i]))
                                continue; // Unsupported
                        }

                        // If an application callback function has been provided, make sure this device
                        // is acceptable to the app.
                        if (deviceCreationInterface != null)
                        {
                            if (!deviceCreationInterface.IsDeviceAcceptable(deviceInfo.Caps, 
                                adapterFormat, backbufferFormatsArray[i],isWindowedIndex))
                                continue; // Application doesn't like this device
                        }

                        // At this point, we have an adapter/device/adapterformat/backbufferformat/iswindowed
                        // DeviceCombo that is supported by the system and acceptable to the app. We still 
                        // need to find one or more suitable depth/stencil buffer format,
                        // multisample type, and present interval.

                        EnumDeviceSettingsCombo deviceCombo = new EnumDeviceSettingsCombo();

                        // Store the information
                        deviceCombo.AdapterOrdinal = adapterInfo.AdapterOrdinal;
                        deviceCombo.DeviceType = deviceInfo.DeviceType;
                        deviceCombo.AdapterFormat = adapterFormat;
                        deviceCombo.BackBufferFormat = backbufferFormatsArray[i];
                        deviceCombo.IsWindowed = isWindowedIndex;

                        // Build the depth stencil format and multisample type list
                        BuildDepthStencilFormatList(deviceCombo);
                        BuildMultiSampleTypeList(deviceCombo);
                        if (deviceCombo.multiSampleTypeList.Count == 0)
                        {
                            // Nothing to do
                            continue;
                        }
                        // Build the conflict and present lists
                        BuildConflictList(deviceCombo);
                        BuildPresentIntervalList(deviceInfo, deviceCombo);

                        deviceCombo.adapterInformation = adapterInfo;
                        deviceCombo.deviceInformation = deviceInfo;

                        // Add the combo to the list of devices
                        deviceInfo.deviceSettingsList.Add(deviceCombo);
                    }
                }
            }
        }

        /// <summary>
        /// Adds all depth/stencil formats that are compatible with the device 
        /// and application to the given device combo
        /// </summary>
        private static void BuildDepthStencilFormatList(EnumDeviceSettingsCombo deviceCombo)
        {
            foreach(DepthFormat depthStencil in depthStencilPossibleList)
            {
                if (Manager.CheckDeviceFormat((int)deviceCombo.AdapterOrdinal,
                    deviceCombo.DeviceType, deviceCombo.AdapterFormat,
                    Usage.DepthStencil, ResourceType.Surface, depthStencil))
                {
                    // This can be used as a depth stencil, make sure it matches
                    if (Manager.CheckDepthStencilMatch((int)deviceCombo.AdapterOrdinal,
                        deviceCombo.DeviceType, deviceCombo.AdapterFormat,
                        deviceCombo.BackBufferFormat, depthStencil))
                    {
                        // Yup, add it
                        deviceCombo.depthStencilFormatList.Add(depthStencil);
                    }
                }
            }
        }

        /// <summary>
        /// Adds all multisample types that are compatible with the device and app to
        /// the given device combo
        /// </summary>
        private static void BuildMultiSampleTypeList(EnumDeviceSettingsCombo deviceCombo)
        {
            foreach(MultiSampleType msType in multiSampleTypeList)
            {
                int result, quality;
                // Check this
                if (Manager.CheckDeviceMultiSampleType((int)deviceCombo.AdapterOrdinal,
                    deviceCombo.DeviceType, deviceCombo.BackBufferFormat,
                    deviceCombo.IsWindowed, msType, out result, out quality))
                {
                    deviceCombo.multiSampleTypeList.Add(msType);
                    if (quality > multisampleQualityMax + 1)
                        quality = (int)(multisampleQualityMax + 1);

                    deviceCombo.multiSampleQualityList.Add(quality);
                }
            }
        }


        /// <summary>
        /// Find any conflicts between the available depth/stencil formats and
        /// multisample types.
        /// </summary>
        private static void BuildConflictList(EnumDeviceSettingsCombo deviceCombo)
        {
            foreach(DepthFormat depthFormat in deviceCombo.depthStencilFormatList)
            {
                foreach(MultiSampleType msType in deviceCombo.multiSampleTypeList)
                {
                    // Check this for conflict
                    if (!Manager.CheckDeviceMultiSampleType((int)deviceCombo.AdapterOrdinal,
                        deviceCombo.DeviceType, (Format)depthFormat,
                        deviceCombo.IsWindowed, msType))
                    {
                        // Add it to the list
                        EnumDepthStencilMultisampleConflict conflict = new EnumDepthStencilMultisampleConflict();
                        conflict.DepthStencilFormat = depthFormat;
                        conflict.MultisampleType = msType;
                        deviceCombo.depthStencilConflictList.Add(conflict);
                    }
                }
            }
        }


        /// <summary>
        /// Adds all present intervals that are compatible with the device and app 
        /// to the given device combo
        /// </summary>
        private static void BuildPresentIntervalList(EnumDeviceInformation deviceInfo, EnumDeviceSettingsCombo deviceCombo)
        {
            for (int i = 0; i < presentIntervalList.Count; i++)
            {
                PresentInterval pi = (PresentInterval)presentIntervalList[i];
                if (deviceCombo.IsWindowed)
                {
                    if ( (pi == PresentInterval.Two) ||
                        (pi == PresentInterval.Three) ||
                        (pi == PresentInterval.Four) )
                    {
                        // These intervals are never supported in windowed mode
                        continue;
                    }
                }

                // Not that PresentInterval.Default is zero so you can't do a bitwise
                // check for it, it's always available
                if ( (pi == PresentInterval.Default) ||
                    ((deviceInfo.Caps.PresentationIntervals & pi) != 0))
                {
                    deviceCombo.presentIntervalList.Add(pi);
                }
            }
        }

        /// <summary>
        /// Resets the list of possible depth stencil formats
        /// </summary>
        public static void ResetPossibleDepthStencilFormats()
        {
            depthStencilPossibleList.Clear();
            depthStencilPossibleList.AddRange(new DepthFormat[] {
                                                             DepthFormat.D16,
                                                             DepthFormat.D15S1,
                                                             DepthFormat.D24X8,
                                                             DepthFormat.D24S8,
                                                             DepthFormat.D24X4S4,
                                                             DepthFormat.D32 });
        }

        /// <summary>
        /// Resets the possible multisample type list
        /// </summary>
        public static void ResetPossibleMultisampleTypeList()
        {
            multiSampleTypeList.Clear();
            multiSampleTypeList.AddRange(new MultiSampleType[] {
                                                            MultiSampleType.None,
                                                            MultiSampleType.NonMaskable,
                                                            MultiSampleType.TwoSamples,
                                                            MultiSampleType.ThreeSamples,
                                                            MultiSampleType.FourSamples,
                                                            MultiSampleType.FiveSamples,
                                                            MultiSampleType.SixSamples,
                                                            MultiSampleType.SevenSamples,
                                                            MultiSampleType.EightSamples,
                                                            MultiSampleType.NineSamples,
                                                            MultiSampleType.TenSamples,
                                                            MultiSampleType.ElevenSamples,
                                                            MultiSampleType.TwelveSamples,
                                                            MultiSampleType.ThirteenSamples,
                                                            MultiSampleType.FourteenSamples,
                                                            MultiSampleType.FifteenSamples,
                                                            MultiSampleType.SixteenSamples });
        }

        /// <summary>
        /// Resets the possible present interval list
        /// </summary>
        public static void ResetPossiblePresentIntervalList()
        {
            presentIntervalList.Clear();
            presentIntervalList.AddRange(new PresentInterval[] {
                                                            PresentInterval.Immediate,
                                                            PresentInterval.Default,
                                                            PresentInterval.One,
                                                            PresentInterval.Two,
                                                            PresentInterval.Three,
                                                            PresentInterval.Four });
        }

        /// <summary>
        /// Set the minimum and maximum resolution items
        /// </summary>
        public static void SetResolutionMinMax(uint minWidth, uint minHeight, uint maxWidth, uint maxHeight)
        {
            minimumWidth = minWidth;
            minimumHeight = minHeight;
            maximumWidth = maxHeight;
            maximumWidth = maxWidth;
        }

        /// <summary>
        /// Sets the minimum and maximum refresh
        /// </summary>
        public static void SetRefreshMinMax(uint minRefresh, uint maxRefresh)
        {
            minimumRefresh = minRefresh;
            maximumRefresh = maxRefresh;
        }

        /// <summary>
        /// Property for MultisampleQualityMax
        /// </summary>
        public static uint MultisampleQualityMax
        {
            get { return multisampleQualityMax; }
            set 
            {
                if (value > 0xffff)
                    multisampleQualityMax = 0xffff;
                else
                    multisampleQualityMax = value;
            }
        }

        /// <summary>
        /// Allows the user to set if post pixel shader blending is required
        /// </summary>
        public static bool IsPostPixelShaderBlendingRequred
        {
            get { return isPostPixelShaderBlendingRequired; }
            set { isPostPixelShaderBlendingRequired = value; }
        }

        /// <summary>
        /// Allows the user to set if software vertex processing is available
        /// </summary>
        public static bool IsSoftwareVertexProcessingPossible
        {
            get { return isSoftwareVertexProcessing; }
            set { isSoftwareVertexProcessing = value; }
        }

        /// <summary>
        /// Allows the user to set if hardware vertex processing is available
        /// </summary>
        public static bool IsHardwareVertexProcessingPossible
        {
            get { return isHardwareVertexProcessing; }
            set { isHardwareVertexProcessing = value; }
        }

        /// <summary>
        /// Allows the user to set if pure hardware vertex processing is available
        /// </summary>
        public static bool IsPureHardwareVertexProcessingPossible
        {
            get { return isPureHardwareVertexProcessing; }
            set { isPureHardwareVertexProcessing = value; }
        }

        /// <summary>
        /// Allows the user to set if mixed vertex processing is available
        /// </summary>
        public static bool IsMixedVertexProcessingPossible
        {
            get { return isMixedVertexProcessing; }
            set { isMixedVertexProcessing = value; }
        }

        /// <summary>
        /// Allows the user to get the possible depth stencil formats
        /// </summary>
        public static ArrayList PossibleDepthStencilFormatList
        {
            get { return depthStencilPossibleList; }
        }

        /// <summary>
        /// Allows the user to get the possible multisample types
        /// </summary>
        public static ArrayList PossibleMultisampleTypeList
        {
            get { return multiSampleTypeList; }
        }

        /// <summary>
        /// Allows the user to get the possible present intervals
        /// </summary>
        public static ArrayList PossiblePresentIntervalsList
        {
            get { return presentIntervalList; }
        }

        /// <summary>
        /// Use this after Enumerate to get the list of adapter information
        /// </summary>
        public static ArrayList AdapterInformationList
        {
            get { return adapterInformationList; }
        }

        /// <summary>
        /// Get the adapter information for a specific adapter
        /// </summary>
        public static EnumAdapterInformation GetAdapterInformation(uint ordinal)
        {
            foreach(EnumAdapterInformation eai in adapterInformationList)
            {
                if (eai.AdapterOrdinal == ordinal)
                {
                    return eai;
                }
            }

            // Never found it
            return null;
        }

        /// <summary>
        /// Get a specific device information for a device and type
        /// </summary>
        public static EnumDeviceInformation GetDeviceInfo(uint ordinal, DeviceType deviceType)
        {
            EnumAdapterInformation info = GetAdapterInformation(ordinal);
            if (info != null)
            {
                foreach(EnumDeviceInformation edi in info.deviceInfoList)
                {
                    // Is this the right device type?
                    if (edi.DeviceType == deviceType)
                    {
                        return edi;
                    }
                }
            }

            // Never found it
            return null;
        }

        /// <summary>
        /// Returns a specific device combination
        /// </summary>
        public static EnumDeviceSettingsCombo GetDeviceSettingsCombo(uint ordinal, DeviceType deviceType,
            Format adapterFormat, Format backBufferFormat, bool isWindowed)
        {
            EnumDeviceInformation info = GetDeviceInfo(ordinal, deviceType);
            if (info != null)
            {
                foreach(EnumDeviceSettingsCombo edsc in info.deviceSettingsList)
                {
                    // Is this the right settings combo?
                    if ( (edsc.AdapterFormat == adapterFormat) && 
                        (edsc.BackBufferFormat == backBufferFormat) &&
                        (edsc.IsWindowed == isWindowed) )
                    {
                        return edsc;
                    }
                }
            }

            // Never found it
            return null;
        }

        /// <summary>
        /// Returns a specific device combination from a device settings object
        /// </summary>
        public static EnumDeviceSettingsCombo GetDeviceSettingsCombo(DeviceSettings settings)
        {
            return GetDeviceSettingsCombo(settings.AdapterOrdinal, settings.DeviceType, settings.AdapterFormat,
                settings.presentParams.BackBufferFormat, settings.presentParams.Windowed);
        }
    }

    /// <summary>
    /// Class describing an adapter which contains a unique adapter ordinal that
    /// is installed on the system.
    /// </summary>
    public class EnumAdapterInformation
    {
        public uint AdapterOrdinal; // Ordinal for this adapter
        public AdapterDetails AdapterInformation; // Information about this adapter
        public ArrayList displayModeList = new ArrayList(); // Array of display modes
        public ArrayList deviceInfoList = new ArrayList(); // Array of device information
        public string UniqueDescription; // Unique description of this device
    }

    /// <summary>
    /// Class describing a Direct3D device that contains a 
    /// unique supported device type
    /// </summary>
    public class EnumDeviceInformation
    {
        public uint AdapterOrdinal; // Ordinal for this adapter
        public DeviceType DeviceType; // Type of the device
        public Caps Caps; // Capabilities of the device
        public ArrayList deviceSettingsList = new ArrayList(); // Array with unique set of adapter format, back buffer format, and windowed
    }

    /// <summary>
    /// Class describing device settings that contain a unique combination
    /// of adapter format, back buffer format, and windowed that is compatible
    /// with a particular Direct3D device and the application
    /// </summary>
    public class EnumDeviceSettingsCombo
    {
        public uint AdapterOrdinal;
        public DeviceType DeviceType;
        public Format AdapterFormat;
        public Format BackBufferFormat;
        public bool IsWindowed;

        // Array lists
        public ArrayList depthStencilFormatList = new ArrayList();
        public ArrayList multiSampleTypeList = new ArrayList();
        public ArrayList multiSampleQualityList = new ArrayList();
        public ArrayList presentIntervalList = new ArrayList();
        public ArrayList depthStencilConflictList = new ArrayList();

        public EnumAdapterInformation adapterInformation = null;
        public EnumDeviceInformation deviceInformation = null;
    }

    /// <summary>
    /// A depth/stencil buffer format that is incompatible
    /// with a multisample type
    /// </summary>
    public struct EnumDepthStencilMultisampleConflict
    {
        public DepthFormat DepthStencilFormat;
        public MultiSampleType MultisampleType;
    }

    /// <summary>
    /// Used to sort display modes
    /// </summary>
    public class DisplayModeSorter : IComparer
    {
        #region IComparer Members

        /// <summary>
        /// Compare two display modes
        /// </summary>
        public int Compare(object x, object y)
        {
            DisplayMode d1 = (DisplayMode)x;
            DisplayMode d2 = (DisplayMode)y;

            if (d1.Width > d2.Width)
                return +1;
            if (d1.Width < d2.Width)
                return -1;
            if (d1.Height > d2.Height)
                return +1;
            if (d1.Height < d2.Height)
                return -1;
            if (d1.Format > d2.Format)
                return +1;
            if (d1.Format < d2.Format)
                return -1;
            if (d1.RefreshRate > d2.RefreshRate)
                return +1;
            if (d1.RefreshRate < d2.RefreshRate)
                return -1;

            // They must be the same, return 0
            return 0;
        }

        #endregion
    }

    #region Helper Utility Class
    /// <summary>
    /// Helper methods
    /// </summary>
    class ManagedUtility
    {
        private ManagedUtility() { } // No creation

        /// <summary>
        /// Gets the number of ColorChanelBits from a format
        /// </summary>
        public static uint GetColorChannelBits(Format format)
        {
            switch (format)
            {
                case Format.R8G8B8:
                case Format.A8R8G8B8:
                case Format.X8R8G8B8:
                    return 8;
                case Format.R5G6B5:
                case Format.X1R5G5B5:
                case Format.A1R5G5B5:
                    return 5;
                case Format.A4R4G4B4:
                case Format.X4R4G4B4:
                    return 4;
                case Format.R3G3B2:
                case Format.A8R3G3B2:
                    return 2;
                case Format.A2B10G10R10:
                case Format.A2R10G10B10:
                    return 10;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the number of alpha channel bits 
        /// </summary>
        public static uint GetAlphaChannelBits(Format format)
        {
            switch (format)
            {
                case Format.X8R8G8B8:
                case Format.R8G8B8:
                case Format.R5G6B5:
                case Format.X1R5G5B5:
                case Format.R3G3B2:
                case Format.X4R4G4B4:
                    return 0;
                case Format.A8R3G3B2:
                case Format.A8R8G8B8:
                    return 8;
                case Format.A1R5G5B5:
                    return 1;
                case Format.A4R4G4B4:
                    return 4;
                case Format.A2B10G10R10:
                case Format.A2R10G10B10:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the number of depth bits
        /// </summary>
        public static uint GetDepthBits(DepthFormat format)
        {
            switch (format)
            {
                case DepthFormat.D16:
                case DepthFormat.D16Lockable:
                    return 16;

                case DepthFormat.D15S1:
                    return 15;

                case DepthFormat.D24X8:
                case DepthFormat.D24S8:
                case DepthFormat.D24X4S4:
                case DepthFormat.D24SingleS8:
                    return 24;

                case DepthFormat.D32:
                case DepthFormat.D32SingleLockable:
                    return 32;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the number of stencil bits
        /// </summary>
        public static uint GetStencilBits(DepthFormat format)
        {
            switch (format)
            {
                case DepthFormat.D16:
                case DepthFormat.D16Lockable:
                case DepthFormat.D24X8:
                case DepthFormat.D32:
                case DepthFormat.D32SingleLockable:
                    return 0;

                case DepthFormat.D15S1:
                    return 1;

                case DepthFormat.D24X4S4:
                    return 4;

                case DepthFormat.D24SingleS8:
                case DepthFormat.D24S8:
                    return 8;

                default:
                    return 0;
            }
        }
    }
    #endregion
}