using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ChinhDo.Transactions;
using Fomm.PackageManager;
using fomm.Transactions;
using Fomm.PackageManager.ModInstallLog;
using Fomm.Games.Fallout3.Script;

namespace Fomm.Games.Fallout3.Tools.GraphicsSettings
{
	/// <summary>
	/// The form used to change graphics settings.
	/// </summary>
	public partial class GraphicsSettings : Form
	{
		#region InterOp

		[DllImport("user32.dll")]
		[System.Security.SuppressUnmanagedCodeSecurity()]
		public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

		[StructLayout(LayoutKind.Sequential)]
		public struct DEVMODE
		{
			private const int CCHDEVICENAME = 0x20;
			private const int CCHFORMNAME = 0x20;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
			public string dmDeviceName;
			public short dmSpecVersion;
			public short dmDriverVersion;
			public short dmSize;
			public short dmDriverExtra;
			public int dmFields;
			public int dmPositionX;
			public int dmPositionY;
			public ScreenOrientation dmDisplayOrientation;
			public int dmDisplayFixedOutput;
			public short dmColor;
			public short dmDuplex;
			public short dmYResolution;
			public short dmTTOption;
			public short dmCollate;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
			public string dmFormName;
			public short dmLogPixels;
			public int dmBitsPerPel;
			public int dmPelsWidth;
			public int dmPelsHeight;
			public int dmDisplayFlags;
			public int dmDisplayFrequency;
			public int dmICMMethod;
			public int dmICMIntent;
			public int dmMediaType;
			public int dmDitherType;
			public int dmReserved1;
			public int dmReserved2;
			public int dmPanningWidth;
			public int dmPanningHeight;
		}

		#endregion

		/// <summary>
		/// A container class to simplify working with comboboxes.
		/// </summary>
		private class ComboBoxItem : IComparable<ComboBoxItem>, IComparable<string>, IEquatable<ComboBoxItem>, IEquatable<string>
		{
			#region Properties

			/// <summary>
			/// Gets or set the display name of the item.
			/// </summary>
			/// <value>The display name of the item.</value>
			public string Name { get; set; }

			/// <summary>
			/// Gets or sets the value of the item.
			/// </summary>
			/// <value>The value of the item.</value>
			public object Value { get; set; }

			/// <summary>
			/// Gets or sets the tag of the item.
			/// </summary>
			/// <value>The tag of the item.</value>
			public object Tag { get; set; }

			/// <summary>
			/// Gets the value of the item as a string.
			/// </summary>
			/// <value>The value of the item as a string.</value>
			public string ValueAsString
			{
				get
				{
					return Value.ToString();
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor the sets the itme's name and value to the given value.
			/// </summary>
			/// <param name="p_objValue">The value of ths item.</param>
			public ComboBoxItem(object p_objValue)
			{
				Name = p_objValue.ToString();
				Value = p_objValue;
			}

			/// <summary>
			/// A simple constructor the sets the itme's name and value.
			/// </summary>
			/// <param name="p_strName">The name of the item.</param>
			/// <param name="p_objValue">The value of ths item.</param>
			public ComboBoxItem(string p_strName, object p_objValue)
			{
				Name = p_strName;
				Value = p_objValue;
			}

			/// <summary>
			/// A simple constructor the sets the itme's name and value.
			/// </summary>
			/// <param name="p_strName">The name of the item.</param>
			/// <param name="p_objValue">The value of ths item.</param>
			/// <param name="p_objTag">The value of the tag.</param>
			public ComboBoxItem(string p_strName, object p_objValue, object p_objTag)
			{
				Name = p_strName;
				Value = p_objValue;
				Tag = p_objTag;
			}

			#endregion

			/// <summary>
			/// Returns the name of the item as the string representation of the object.
			/// </summary>
			/// <returns>The name of the item as the string representation of the object.</returns>
			public override string ToString()
			{
				return Name;
			}

			#region IComparable<ComboBoxItem> Members

			/// <summary>
			/// Compares this item to the given item.
			/// </summary>
			/// <remarks>Items' equality is determined by their names.</remarks>
			/// <param name="other">The item to which to compare this item.</param>
			/// <returns>A value less than zero if this instance is less than the given instance,
			/// or a value of zero  if this instance is equal to the given instance,
			/// or a value greater than zero if this instance is greater than the given
			/// instance.</returns>
			public int CompareTo(ComboBoxItem other)
			{
				return Name.CompareTo(other.Name);
			}

			#endregion

			#region IComparable<string> Members

			/// <summary>
			/// Compares this item to the given string.
			/// </summary>
			/// <remarks>Items' equality is determined by their names.</remarks>
			/// <param name="other">The item to which to compare this item.</param>
			/// <returns>A value less than zero if this instance is less than the given instance,
			/// or a value of zero  if this instance is equal to the given instance,
			/// or a value greater than zero if this instance is greater than the given
			/// instance.</returns>
			public int CompareTo(string other)
			{
				return Name.CompareTo(other);
			}

			#endregion

			#region IEquatable<ComboBoxItem> Members

			/// <summary>
			/// Compares this item to the given ComboBoxItem.
			/// </summary>
			/// <remarks>Items' equality is determined by their names.</remarks>
			/// <param name="other">The item to which to compare this item.</param>
			/// <returns><lang cref="true"/> if this item's name is equal to the given
			/// ComboBoxItem's name; <lang cref="false"/> otherwise.</returns>
			public bool Equals(ComboBoxItem other)
			{
				return Name.Equals(other.Name);
			}

			#endregion

			#region IEquatable<string> Members

			/// <summary>
			/// Compares this item to the given string.
			/// </summary>
			/// <remarks>Items' equality is determined by their names.</remarks>
			/// <param name="other">The item to which to compare this item.</param>
			/// <returns><lang cref="true"/> if this item's name is equal to the given
			/// ComboBoxItem's name; <lang cref="false"/> otherwise.</returns>
			public bool Equals(string other)
			{
				return Name.Equals(other);
			}

			#endregion

			/// <summary>
			/// Handles equation checking against objects.
			/// </summary>
			/// <remarks>
			/// If the given object is of a recognized type this method delegates to one
			/// of the types Equals methods.
			/// </remarks>
			/// <param name="obj">The item to which to compare this item.</param>
			/// <returns><lang cref="true"/> if this item's name is equal to the given
			/// ComboBoxItem's name; <lang cref="false"/> otherwise.</returns>
			public override bool Equals(object obj)
			{
				if (obj is string)
					return Equals((string)obj);
				if (obj is ComboBoxItem)
					return Equals((ComboBoxItem)obj);
				return base.Equals(obj);
			}
		}

		#region DropDown Items

		private static ComboBoxItem[] m_cbiAspectRatios = new ComboBoxItem[] {
													new ComboBoxItem("Standard (4:3 or 5:4)", 1, 4.0/3.0),
													new ComboBoxItem("16:9 Widescreen", 3, 16.0/9.0),
													new ComboBoxItem("16:10 Widescreen", 4, 1.6) };
		private static ComboBoxItem[] m_cbiTextureQulities = new ComboBoxItem[] {
													new ComboBoxItem("Low", 2),
													new ComboBoxItem("Medium", 1),
													new ComboBoxItem("High", 0) };
		private static ComboBoxItem[] m_cbiRadialBlurQualities = new ComboBoxItem[] {
													new ComboBoxItem("Low", 0),
													new ComboBoxItem("Medium", 1),
													new ComboBoxItem("High", 2) };
		private static ComboBoxItem[] m_cbiReflectionQualities = new ComboBoxItem[] {
													new ComboBoxItem("Low", 256),
													new ComboBoxItem("Medium", 512),
													new ComboBoxItem("High", 1024) };
		private static ComboBoxItem[] m_cbiWaterMultisamples = new ComboBoxItem[] {
													new ComboBoxItem("Low", 1),
													new ComboBoxItem("Medium", 2),
													new ComboBoxItem("High", 4) };
		private static ComboBoxItem[] m_cbiShadowQualities = new ComboBoxItem[] {
													new ComboBoxItem("Low", 256),
													new ComboBoxItem("Medium", 512),
													new ComboBoxItem("High", 1024) };
		private static ComboBoxItem[] m_cbiShadowFilters = new ComboBoxItem[] {
													new ComboBoxItem("Low", 0),
													new ComboBoxItem("Medium", 1),
													new ComboBoxItem("High", 2) };

		#endregion

		public GraphicsSettings()
		{
			InitializeComponent();

			LoadGeneralValues();
			LoadDetailValues();
			LoadWaterValues();
			LoadShadowValues();
			LoadViewDistanceValues();
			LoadDistantLODValues();
		}

		#region Value Loading

		/// <summary>
		/// Loads the general display settings.
		/// </summary>
		private void LoadGeneralValues()
		{
			//adapters
			cbxAdapter.Items.Add(NativeMethods.GetPrivateProfileString("Display", "sD3DDevice", null, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath));
			cbxAdapter.SelectedIndex = 0;

			//aspect ratios
			Int32 intCurrentAspect = NativeMethods.GetPrivateProfileIntA("Launcher", "uLastAspectRatio", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			double dblCurrentRatio = 0;
			for (Int32 i = 0; i < m_cbiAspectRatios.Length; i++)
			{
				cbxAspectRatio.Items.Add(m_cbiAspectRatios[i]);
				if (intCurrentAspect.Equals(m_cbiAspectRatios[i].Value))
				{
					cbxAspectRatio.SelectedIndex = i;
					dblCurrentRatio = (double)m_cbiAspectRatios[i].Tag;
				}
			}

			//screen resolutions
			Int32 intScreenWidth = NativeMethods.GetPrivateProfileIntA("Display", "iSize W", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			Int32 intScreenHeight = NativeMethods.GetPrivateProfileIntA("Display", "iSize H", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			string strCurrentRes = String.Format("{0}x{1}", intScreenWidth, intScreenHeight);
			LoadResolutions(dblCurrentRatio, strCurrentRes);
			if (!cbxResolution.Items.Contains(strCurrentRes))
			{
				ComboBoxItem cbiResolution = new ComboBoxItem(strCurrentRes);
				cbiResolution.Value = new Int32[] { intScreenWidth, intScreenHeight };
				cbxResolution.SelectedIndex = cbxResolution.Items.Add(cbiResolution);
			}

			//antialiasing
			ComboBoxItem cbiAliasing = new ComboBoxItem("Off (best performance)", 0);
			Int32 intAliasingSamples = NativeMethods.GetPrivateProfileIntA("Display", "iMultiSample", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			cbxAntialiasing.Items.Add(cbiAliasing);
			for (Int32 i = 2; i < Math.Max(4, intAliasingSamples) + 1; i += 2)
			{
				cbiAliasing = new ComboBoxItem(i + " Samples", i);
				cbxAntialiasing.Items.Add(cbiAliasing);
			}
			cbxAntialiasing.SelectedIndex = intAliasingSamples / 2;

			//ansio
			ComboBoxItem cbiAniso = new ComboBoxItem("Off (best performance)", 0);
			Int32 intAnisoSamples = NativeMethods.GetPrivateProfileIntA("Display", "iMaxAnisotropy", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			cbxAnisotropic.Items.Add(cbiAniso);
			for (Int32 i = 2; i < Math.Max(15, intAnisoSamples) + 1; i++)
			{
				cbiAniso = new ComboBoxItem(i + " Samples", i);
				cbxAnisotropic.Items.Add(cbiAniso);
			}
			cbxAnisotropic.SelectedIndex = (intAnisoSamples > 0) ? intAnisoSamples - 1 : 0;

			//windowed
			ckbWindowed.Checked = (NativeMethods.GetPrivateProfileIntA("Display", "bFull Screen", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 0);

			//vsync
			ckbVSync.Checked = (NativeMethods.GetPrivateProfileIntA("Display", "iPresentInterval", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);

			//screen effects
			radNone.Checked = true;
			radHDR.Checked = (NativeMethods.GetPrivateProfileIntA("BlurShaderHDR", "bDoHighDynamicRange", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);
			radBloom.Checked = (NativeMethods.GetPrivateProfileIntA("BlurShader", "bUseBlurShader", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);
		}

		/// <summary>
		/// Loads the details settings.
		/// </summary>
		private void LoadDetailValues()
		{
			//texture quality
			Int32 intTextureQuality = NativeMethods.GetPrivateProfileIntA("Display", "iTexMipMapSkip", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			for (Int32 i = 0; i < m_cbiTextureQulities.Length; i++)
			{
				cbxTextureQuality.Items.Add(m_cbiTextureQulities[i]);
				if (intTextureQuality.Equals(m_cbiTextureQulities[i].Value))
					cbxTextureQuality.SelectedIndex = i;
			}

			//radial blur quality
			Int32 intRadialBlurQuality = NativeMethods.GetPrivateProfileIntA("Imagespace", "iRadialBlurLevel", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			for (Int32 i = 0; i < m_cbiRadialBlurQualities.Length; i++)
			{
				cbxRadialBlurQuality.Items.Add(m_cbiRadialBlurQualities[i]);
				if (intRadialBlurQuality.Equals(m_cbiRadialBlurQualities[i].Value))
					cbxRadialBlurQuality.SelectedIndex = i;
			}

			//depth of field
			ckbDepthOfField.Checked = (NativeMethods.GetPrivateProfileIntA("Imagespace", "bDoDepthOfField", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);

			//transparency multisampling
			ckbTransparencyMultisampling.Checked = (NativeMethods.GetPrivateProfileIntA("Display", "bTransparencyMultisampling", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);

			//decal cap
			oslDecalCap.Value = NativeMethods.GetPrivateProfileIntA("Display", "iMaxDecalsPerFrame", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
		}

		/// <summary>
		/// Loads the water settings.
		/// </summary>
		private void LoadWaterValues()
		{
			//refractions
			ckbWaterRefractions.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bUseWaterRefractions", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);

			//reflections
			ckbWaterReflections.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bUseWaterReflections", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);

			//reflection quality
			// iWaterReflectWidth
			// iWaterReflectHeight
			// the above values should always be the same
			Int32 intReflectionSize = NativeMethods.GetPrivateProfileIntA("Water", "iWaterReflectWidth", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			for (Int32 i = 0; i < m_cbiReflectionQualities.Length; i++)
			{
				cbxReflectionQuality.Items.Add(m_cbiReflectionQualities[i]);
				if (intReflectionSize.Equals(m_cbiReflectionQualities[i].Value))
					cbxReflectionQuality.SelectedIndex = i;
			}

			//soft reflections
			// iWaterBlurAmount=2
			// sometimes the above is 1, but it should always be 2
			// as it is ignored if bUseWaterReflectionBlur is 0
			ckbSoftReflections.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bUseWaterReflectionBlur", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);

			//full scene
			ckbFullSceneReflections.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bForceHighDetailReflections", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);

			//full detail
			ckbFullDetailReflections.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bAutoWaterSilhouetteReflections", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) != 1);

			//water displacements
			ckbWaterDisplacement.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bUseWaterDisplacements", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);

			//depth fog
			ckbDepthFog.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bUseWaterDepth", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);

			//water multisamples
			Int32 intSamples = NativeMethods.GetPrivateProfileIntA("Display", "iWaterMultisamples", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			for (Int32 i = 0; i < m_cbiWaterMultisamples.Length; i++)
			{
				cbxWaterMultisampling.Items.Add(m_cbiWaterMultisamples[i]);
				if (intSamples.Equals(m_cbiWaterMultisamples[i].Value))
					cbxWaterMultisampling.SelectedIndex = i;
			}
		}

		/// <summary>
		/// Loads the shadow settings.
		/// </summary>
		private void LoadShadowValues()
		{
			//enable shadows
			ckbEnableShadows.Checked = (NativeMethods.GetPrivateProfileIntA("Display", "bDrawShadows", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) == 1);

			//shadow quality
			Int32 intShadowsRes = NativeMethods.GetPrivateProfileIntA("Display", "iShadowMapResolution", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			for (Int32 i = 0; i < m_cbiShadowQualities.Length; i++)
			{
				cbxShadowQuality.Items.Add(m_cbiShadowQualities[i]);
				if (intShadowsRes.Equals(m_cbiShadowQualities[i].Value))
					cbxShadowQuality.SelectedIndex = i;
			}

			//shadow filtering
			Int32 intShadowFilter = NativeMethods.GetPrivateProfileIntA("Display", "iShadowFilter", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			for (Int32 i = 0; i < m_cbiShadowFilters.Length; i++)
			{
				cbxShadowFiltering.Items.Add(m_cbiShadowFilters[i]);
				if (intShadowFilter.Equals(m_cbiShadowFilters[i].Value))
					cbxShadowFiltering.SelectedIndex = i;
			}

			//interior shadows
			oslMaxInteriorShadows.Value = NativeMethods.GetPrivateProfileIntA("Display", "iActorShadowCountInt", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);

			//exterior shadows
			oslMaxExteriorShadows.Value = NativeMethods.GetPrivateProfileIntA("Display", "iActorShadowCountExt", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
		}

		/// <summary>
		/// Loads the view distance settings.
		/// </summary>
		private void LoadViewDistanceValues()
		{
			//object fade
			oslObjectFade.Value = NativeMethods.GetPrivateProfileIntA("LOD", "fLODFadeOutMultObjects", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);

			//actor fade
			oslActorFade.Value = NativeMethods.GetPrivateProfileIntA("LOD", "fLODFadeOutMultActors", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);

			//grass fade
			oslGrassFade.Value = NativeMethods.GetPrivateProfileIntA("Grass", "fGrassStartFadeDistance", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) / 1000;

			//specularity fade
			oslSpecularityFade.Value = NativeMethods.GetPrivateProfileIntA("Display", "fSpecularLODStartFade", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) / 100;

			//light fade
			oslLightFade.Value = NativeMethods.GetPrivateProfileIntA("Display", "fLightLODStartFade", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) / 100;

			//item fade
			decimal dcmValue = 0;
			Decimal.TryParse(NativeMethods.GetPrivateProfileString("LOD", "fLODFadeOutMultItems", "0", ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath), out dcmValue);
			oslItemFade.Value = dcmValue;

			//shadow fade
			oslShadowFade.Value = NativeMethods.GetPrivateProfileIntA("Display", "fShadowLODStartFade", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) / 100;
		}

		/// <summary>
		/// Loads the view distant lod settings.
		/// </summary>
		private void LoadDistantLODValues()
		{
			//tree lod fade
			oslTreeLODFade.Value = NativeMethods.GetPrivateProfileIntA("TerrainManager", "fTreeLoadDistance", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) / 1000;

			//object lod fade fBlockLoadDistanceLow
			oslObjectLODFade.Value = NativeMethods.GetPrivateProfileIntA("TerrainManager", "fBlockLoadDistanceLow", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath) / 1000;

			//land quality
			decimal dcmValue = 0;
			Decimal.TryParse(NativeMethods.GetPrivateProfileString("TerrainManager", "fSplitDistanceMult", "0", ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath), out dcmValue);
			oslLandQuality.Value = dcmValue * 100;
		}

		#endregion

		/// <summary>
		/// Loads the available resolutions.
		/// </summary>
		/// <remarks>
		/// This queries the system for supported resolutions at the given aspect ratio, and populates
		/// the resolution dropdown with the results.
		/// </remarks>
		/// <param name="p_dblSelectedRatio">The aspect ratio to which to limit the available resolutions.</param>
		/// <param name="p_strCurrentRes">The current resolution.</param>
		protected void LoadResolutions(double p_dblSelectedRatio, string p_strCurrentRes)
		{
			cbxResolution.Items.Clear();
			DEVMODE vDevMode = new DEVMODE();
			Int32 intEnumCounter = 0;
			double dblRatio = 0;
			while (EnumDisplaySettings(null, intEnumCounter, ref vDevMode))
			{
				dblRatio = (double)vDevMode.dmPelsWidth / (double)vDevMode.dmPelsHeight;
				ComboBoxItem cbiResolution = new ComboBoxItem(String.Format("{0}x{1}", vDevMode.dmPelsWidth, vDevMode.dmPelsHeight));
				cbiResolution.Value = new Int32[] { vDevMode.dmPelsWidth, vDevMode.dmPelsHeight };
				if ((!cbxResolution.Items.Contains(cbiResolution)) && (Math.Abs(dblRatio - p_dblSelectedRatio) < 0.001))
				{
					Int32 intIndex = cbxResolution.Items.Add(cbiResolution);
					if (cbiResolution.Equals(p_strCurrentRes))
						cbxResolution.SelectedIndex = intIndex;
				}
				intEnumCounter++;
			}
		}

		#region Event Handling

		/// <summary>
		/// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of the aspect ratio
		/// dropdown box.
		/// </summary>
		/// <remarks>
		/// This causes the resolution dropdown to refresh.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event's arguments.</param>
		private void cbxAspectRatio_SelectedIndexChanged(object sender, EventArgs e)
		{
			Int32 intScreenWidth = NativeMethods.GetPrivateProfileIntA("Display", "iSize W", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			Int32 intScreenHeight = NativeMethods.GetPrivateProfileIntA("Display", "iSize H", 0, ((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
			string strCurrentRes = String.Format("{0}x{1}", intScreenWidth, intScreenHeight);
			LoadResolutions((double)((ComboBoxItem)cbxAspectRatio.SelectedItem).Tag, strCurrentRes);
			if ((cbxResolution.SelectedItem == null) && (cbxResolution.Items.Count > 0))
				cbxResolution.SelectedIndex = 0;
		}

		/// <summary>
		/// Handles the <see cref="CheckBox.CheckedChanged"/> event of the water reflections
		/// checkbox.
		/// </summary>
		/// <remarks>
		/// This enables/disables the controls having to to with water reflections, as appropriate.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event's arguments.</param>
		private void ckbWaterReflections_CheckedChanged(object sender, EventArgs e)
		{
			foreach (Control ctlControl in gbxWaterReflections.Controls)
				ctlControl.Enabled = ckbWaterReflections.Checked;
		}

		/// <summary>
		/// Handles the <see cref="CheckBox.CheckedChanged"/> event of the enable shadows
		/// checkbox.
		/// </summary>
		/// <remarks>
		/// This enables/disables the controls having to to with shadows, as appropriate.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event's arguments.</param>
		private void ckbEnableShadows_CheckedChanged(object sender, EventArgs e)
		{
			foreach (Control ctlControl in gbxShadows.Controls)
				ctlControl.Enabled = ckbEnableShadows.Checked;
		}

		/// <summary>
		/// Handles the <see cref="Button.Click"/> event of the OK button.
		/// </summary>
		/// <remarks>
		/// This saves any setting that have changed.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event's arguments.</param>
		private void butOK_Click(object sender, EventArgs e)
		{
			SettingsInstaller sinSaver = new SettingsInstaller();
			sinSaver.SaveSettings(this);
			DialogResult = DialogResult.OK;
		}

		#endregion

		/// <summary>
		/// The installer used to change the settings.
		/// </summary>
		/// <remarks>
		/// This installer treats FOMM as a mod. The advantage of this is that changes that are
		/// made to the settings are recorded in the install log.
		/// </remarks>
		private class SettingsInstaller : ModInstallerBase
		{
			private bool m_booChanged = false;
			private GraphicsSettings m_gstSettings = null;

			#region Properties

			protected Fallout3ModInstallScript Fallout3Script
			{
				get
				{
					return (Fallout3ModInstallScript)Script;
				}
			}

			/// <seealso cref="ModInstallScript.ExceptionMessage"/>
			protected override string ExceptionMessage
			{
				get
				{
					return "A problem occurred while saving settings: " + Environment.NewLine + "{0}" + Environment.NewLine + "The settings were not changed.";
				}
			}

			/// <seealso cref="ModInstallScript.SuccessMessage"/>
			protected override string SuccessMessage
			{
				get
				{
					return null;
				}
			}

			/// <seealso cref="ModInstallScript.FailMessage"/>
			protected override string FailMessage
			{
				get
				{
					return null;
				}
			}

			#endregion

			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the object.
			/// </summary>
			internal SettingsInstaller()
				: base(null)
			{
			}

			#endregion

			/// <summary>
			/// Determines if the settings have already been saved.
			/// </summary>
			/// <remarks>
			/// This always returns <lang cref="false"/>.
			/// </remarks>
			/// <returns><lang cref="false"/></returns>
			/// <seealso cref="ModInstallScript.CheckAlreadyDone()"/>
			protected override bool CheckAlreadyDone()
			{
				return false;
			}

			#region Value Saving

			/// <summary>
			/// Sets the specified Ini key to the given value if it differs
			/// from the current value.
			/// </summary>
			/// <param name="p_strSection">The section of the file containing the key to change.</param>
			/// <param name="p_strKey">The key to change.</param>
			/// <param name="p_strValue">The value to which to set the key.</param>
			/// <returns><lang cref="true"/> if the value differed and so was changed;
			/// <lang cref="false"/> otherwise.</returns>
			private bool SaveValue(string p_strSection, string p_strKey, string p_strValue)
			{
				if (!Fallout3Script.GetPrefsIniString(p_strSection, p_strKey).Equals(p_strValue))
				{
					Fallout3Script.EditPrefsINI(p_strSection, p_strKey, p_strValue, true);
					m_booChanged = true;
					return true;
				}
				return false;
			}

			/// <summary>
			/// Saves the general display settings.
			/// </summary>
			/// <param name="p_gstSettings">The form used to gather the new settings.</param>
			private void SaveGeneralValues(GraphicsSettings p_gstSettings)
			{
				//aspect ratios
				if (p_gstSettings.cbxAspectRatio.SelectedItem != null)
					SaveValue("Launcher", "uLastAspectRatio", ((ComboBoxItem)p_gstSettings.cbxAspectRatio.SelectedItem).ValueAsString);

				//screen resolutions
				if (p_gstSettings.cbxResolution.SelectedItem != null)
				{
					Int32[] intResolution = (Int32[])((ComboBoxItem)(p_gstSettings.cbxResolution.SelectedItem)).Value;
					if (intResolution != null)
					{
						SaveValue("Display", "iSize W", intResolution[0].ToString());
						SaveValue("Display", "iSize H", intResolution[1].ToString());
					}
				}

				//antialiasing
				if (p_gstSettings.cbxAntialiasing.SelectedItem != null)
					SaveValue("Display", "iMultiSample", ((ComboBoxItem)(p_gstSettings.cbxAntialiasing.SelectedItem)).ValueAsString);

				//ansio
				if (p_gstSettings.cbxAnisotropic.SelectedItem != null)
					SaveValue("Display", "iMaxAnisotropy", ((ComboBoxItem)(p_gstSettings.cbxAnisotropic.SelectedItem)).ValueAsString);

				//windowed
				SaveValue("Display", "bFull Screen", (p_gstSettings.ckbWindowed.Checked ? "0" : "1"));

				//vsync
				SaveValue("Display", "iPresentInterval", (p_gstSettings.ckbVSync.Checked ? "1" : "0"));

				//screen effects
				if (p_gstSettings.radNone.Checked)
				{
					SaveValue("BlurShaderHDR", "bDoHighDynamicRange", "0");
					SaveValue("BlurShader", "bUseBlurShader", "0");
				}
				else if (p_gstSettings.radBloom.Checked)
				{
					SaveValue("BlurShaderHDR", "bDoHighDynamicRange", "0");
					SaveValue("BlurShader", "bUseBlurShader", "1");
				}
				else
				{
					SaveValue("BlurShaderHDR", "bDoHighDynamicRange", "1");
					SaveValue("BlurShader", "bUseBlurShader", "0");
				}
			}

			/// <summary>
			/// Saves the details settings.
			/// </summary>
			/// <param name="p_gstSettings">The form used to gather the new settings.</param>
			private void SaveDetailValues(GraphicsSettings p_gstSettings)
			{
				//texture quality
				SaveValue("Display", "iTexMipMapSkip", ((ComboBoxItem)p_gstSettings.cbxTextureQuality.SelectedItem).ValueAsString);

				//radial blur quality
				SaveValue("Imagespace", "iRadialBlurLevel", ((ComboBoxItem)p_gstSettings.cbxRadialBlurQuality.SelectedItem).ValueAsString);

				//depth of field
				SaveValue("Imagespace", "bDoDepthOfField", (p_gstSettings.ckbDepthOfField.Checked ? "1" : "0"));

				//transparency multisampling
				SaveValue("Display", "bTransparencyMultisampling", (p_gstSettings.ckbTransparencyMultisampling.Checked ? "1" : "0"));

				//decal cap
				SaveValue("Display", "iMaxDecalsPerFrame", p_gstSettings.oslDecalCap.Value.ToString("f0"));
			}

			/// <summary>
			/// Saves the water settings.
			/// </summary>
			/// <param name="p_gstSettings">The form used to gather the new settings.</param>
			private void SaveWaterValues(GraphicsSettings p_gstSettings)
			{
				//refractions
				SaveValue("Water", "bUseWaterRefractions", (p_gstSettings.ckbWaterRefractions.Checked ? "1" : "0"));

				//reflections
				SaveValue("Water", "bUseWaterReflections", (p_gstSettings.ckbWaterReflections.Checked ? "1" : "0"));

				//reflection quality
				// iWaterReflectWidth
				// iWaterReflectHeight
				// the above values should always be the same
				if (SaveValue("Water", "iWaterReflectWidth", ((ComboBoxItem)p_gstSettings.cbxReflectionQuality.SelectedItem).ValueAsString))
					Fallout3Script.EditPrefsINI("Water", "iWaterReflectHeight", ((ComboBoxItem)p_gstSettings.cbxReflectionQuality.SelectedItem).ValueAsString, true);

				//soft reflections
				// iWaterBlurAmount=2
				// sometimes the above is 1, but it should always be 2
				// as it is ignored if bUseWaterReflectionBlur is 0
				if (SaveValue("Water", "bUseWaterReflectionBlur", (p_gstSettings.ckbSoftReflections.Checked ? "1" : "0")))
					Fallout3Script.EditPrefsINI("Water", "iWaterBlurAmount", "2", true);

				//full scene
				SaveValue("Water", "bForceHighDetailReflections", (p_gstSettings.ckbFullSceneReflections.Checked ? "1" : "0"));

				//full detail
				SaveValue("Water", "bAutoWaterSilhouetteReflections", (p_gstSettings.ckbFullDetailReflections.Checked ? "0" : "1"));

				//water displacements
				SaveValue("Water", "bUseWaterDisplacements", (p_gstSettings.ckbWaterDisplacement.Checked ? "1" : "0"));

				//depth fog
				SaveValue("Water", "bUseWaterDepth", (p_gstSettings.ckbDepthFog.Checked ? "1" : "0"));

				//water multisamples
				SaveValue("Display", "iWaterMultisamples", ((ComboBoxItem)p_gstSettings.cbxWaterMultisampling.SelectedItem).ValueAsString);
			}

			/// <summary>
			/// Saves the shadow settings.
			/// </summary>
			/// <param name="p_gstSettings">The form used to gather the new settings.</param>
			private void SaveShadowValues(GraphicsSettings p_gstSettings)
			{
				//enable shadows
				SaveValue("Display", "bDrawShadows", (p_gstSettings.ckbEnableShadows.Checked ? "1" : "0"));

				//shadow quality
				SaveValue("Display", "iShadowMapResolution", ((ComboBoxItem)p_gstSettings.cbxShadowQuality.SelectedItem).ValueAsString);

				//shadow filtering
				SaveValue("Display", "iShadowFilter", ((ComboBoxItem)p_gstSettings.cbxShadowFiltering.SelectedItem).ValueAsString);

				//interior shadows
				SaveValue("Display", "iActorShadowCountInt", p_gstSettings.oslMaxInteriorShadows.Value.ToString("f0"));

				//exterior shadows
				SaveValue("Display", "iActorShadowCountExt", p_gstSettings.oslMaxExteriorShadows.Value.ToString("f0"));
			}

			/// <summary>
			/// Saves the view distance settings.
			/// </summary>
			/// <param name="p_gstSettings">The form used to gather the new settings.</param>
			private void SaveViewDistanceValues(GraphicsSettings p_gstSettings)
			{
				//object fade
				SaveValue("LOD", "fLODFadeOutMultObjects", p_gstSettings.oslObjectFade.Value.ToString("f0"));

				//actor fade
				SaveValue("LOD", "fLODFadeOutMultActors", p_gstSettings.oslActorFade.Value.ToString("f0"));

				//grass fade
				SaveValue("Grass", "fGrassStartFadeDistance", (p_gstSettings.oslGrassFade.Value * 1000).ToString("f0"));

				//specularity fade
				SaveValue("Display", "fSpecularLODStartFade", (p_gstSettings.oslSpecularityFade.Value * 100).ToString("f0"));

				//light fade
				SaveValue("Display", "fLightLODStartFade", (p_gstSettings.oslLightFade.Value * 100).ToString("f0"));

				//item fade
				SaveValue("LOD", "fLODFadeOutMultItems", (p_gstSettings.oslItemFade.Value).ToString("#0.#"));

				//shadow fade
				SaveValue("Display", "fShadowLODStartFade", (p_gstSettings.oslShadowFade.Value * 100).ToString("f0"));
			}

			/// <summary>
			/// Saves the view distant lod settings.
			/// </summary>
			/// <param name="p_gstSettings">The form used to gather the new settings.</param>
			private void SaveDistantLODValues(GraphicsSettings p_gstSettings)
			{
				//tree lod fade
				SaveValue("TerrainManager", "fTreeLoadDistance", (p_gstSettings.oslTreeLODFade.Value * 1000).ToString("f0"));

				//object lod fade
				SaveValue("TerrainManager", "fBlockLoadDistanceLow", (p_gstSettings.oslObjectLODFade.Value * 1000).ToString("f0"));

				//land quality
				SaveValue("TerrainManager", "fSplitDistanceMult", (p_gstSettings.oslLandQuality.Value / 100m).ToString("#0.##"));
			}

			#endregion

			/// <summary>
			/// Saves the changed settings.
			/// </summary>
			/// <remarks>
			/// This uses transactions to support rollback in case of a problem. This also
			/// modifies teh install log to track any changes that were made.
			/// </remarks>
			/// <param name="p_gstSettings">The form used to gather the new settings.</param>
			public void SaveSettings(GraphicsSettings p_gstSettings)
			{
				m_gstSettings = p_gstSettings;
				Run();
			}

			/// <summary>
			/// This does the actual changing of settings.
			/// </summary>
			/// <returns><lang cref="true"/> if the script work was completed successfully and needs to
			/// be committed; <lang cref="false"/> otherwise.</returns>
			/// <exception cref="InvalidOperationException">Thrown if m_gstSettings is
			/// <lang cref="null"/>.</exception>
			/// <seealso cref="ModInstallScript.DoScript"/>
			protected override bool DoScript()
			{
				if (m_gstSettings == null)
					throw new InvalidOperationException("The SettingsForm property must be set before calling Run(); or Run(GraphicsSettings) can be used instead.");

				TransactionalFileManager.Snapshot(((Fallout3GameMode.SettingsFilesSet)Program.GameMode.SettingsFiles).FOPrefsIniPath);
				Fallout3Script.OverwriteAllIni = true;

				m_booChanged = false;
				MergeModule = new InstallLogMergeModule();
				SaveGeneralValues(m_gstSettings);
				SaveDetailValues(m_gstSettings);
				SaveWaterValues(m_gstSettings);
				SaveShadowValues(m_gstSettings);
				SaveViewDistanceValues(m_gstSettings);
				SaveDistantLODValues(m_gstSettings);

				if (m_booChanged)
				{
					InstallLog.Current.UnversionedFomodMerge(InstallLog.FOMM, MergeModule);
					return true;
				}
				return false;
			}
		}
	}
}
