using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Transactions;
using ChinhDo.Transactions;
using Fomm.PackageManager;

namespace Fomm.GraphicsSettings
{
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
					return Value as string;
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
			/// <param name="other"><lang cref="true"/> if this item's name is equal to the given
			/// ComboBoxItem's name; <lang cref="false"/> otherwise.</param></returns>
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
			/// <param name="other"><lang cref="true"/> if this item's name is equal to the given
			/// string; <lang cref="false"/> otherwise.</param></returns>
			public bool Equals(string other)
			{
				return Name.Equals(other);
			}

			#endregion

			/// <summary>
			/// 
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals(object obj)
			{
				if (obj is string)
					return Equals((string)obj);
				if (obj is ComboBoxItem)
					return Equals((ComboBoxItem)obj);
				return base.Equals(obj);
			}
		}

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
			cbxAdapter.Items.Add(NativeMethods.GetPrivateProfileString("Display", "sD3DDevice", null, Program.FOPrefsIniPath));
			cbxAdapter.SelectedIndex = 0;

			//aspect ratios
			Int32 intCurrentAspect = NativeMethods.GetPrivateProfileIntA("Launcher", "uLastAspectRatio", 0, Program.FOPrefsIniPath);
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
			Int32 intScreenWidth = NativeMethods.GetPrivateProfileIntA("Display", "iSize W", 0, Program.FOPrefsIniPath);
			Int32 intScreenHeight = NativeMethods.GetPrivateProfileIntA("Display", "iSize H", 0, Program.FOPrefsIniPath);
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
			Int32 intAliasingSamples = NativeMethods.GetPrivateProfileIntA("Display", "iMultiSample", 0, Program.FOPrefsIniPath);
			cbxAntialiasing.Items.Add(cbiAliasing);
			for (Int32 i = 2; i < Math.Max(4, intAliasingSamples) + 1; i += 2)
			{
				cbiAliasing = new ComboBoxItem(i + " Samples", i);
				cbxAntialiasing.Items.Add(cbiAliasing);
			}
			cbxAntialiasing.SelectedIndex = intAliasingSamples / 2;

			//ansio
			ComboBoxItem cbiAniso = new ComboBoxItem("Off (best performance)", 0);
			Int32 intAnisoSamples = NativeMethods.GetPrivateProfileIntA("Display", "iMaxAnisotropy", 0, Program.FOPrefsIniPath);
			cbxAnisotropic.Items.Add(cbiAniso);
			for (Int32 i = 2; i < Math.Max(15, intAnisoSamples) + 1; i++)
			{
				cbiAniso = new ComboBoxItem(i + " Samples", i);
				cbxAnisotropic.Items.Add(cbiAniso);
			}
			cbxAnisotropic.SelectedIndex = (intAnisoSamples > 0) ? intAnisoSamples - 1 : 0;

			//windowed
			ckbWindowed.Checked = (NativeMethods.GetPrivateProfileIntA("Display", "bFull Screen", 0, Program.FOPrefsIniPath) == 0);

			//vsync
			ckbVSync.Checked = (NativeMethods.GetPrivateProfileIntA("Display", "iPresentInterval", 0, Program.FOPrefsIniPath) == 1);

			//screen effects
			radNone.Checked = true;
			radHDR.Checked = (NativeMethods.GetPrivateProfileIntA("BlurShaderHDR", "bDoHighDynamicRange", 0, Program.FOPrefsIniPath) == 1);
			radBloom.Checked = (NativeMethods.GetPrivateProfileIntA("BlurShader", "bUseBlurShader", 0, Program.FOPrefsIniPath) == 1);
		}

		/// <summary>
		/// Loads the details settings.
		/// </summary>
		private void LoadDetailValues()
		{
			//texture quality
			Int32 intTextureQuality = NativeMethods.GetPrivateProfileIntA("Display", "iTexMipMapSkip", 0, Program.FOPrefsIniPath);
			for (Int32 i = 0; i < m_cbiTextureQulities.Length; i++)
			{
				cbxTextureQuality.Items.Add(m_cbiTextureQulities[i]);
				if (intTextureQuality.Equals(m_cbiTextureQulities[i].Value))
					cbxTextureQuality.SelectedIndex = i;
			}

			//radial blur quality
			Int32 intRadialBlurQuality = NativeMethods.GetPrivateProfileIntA("Imagespace", "iRadialBlurLevel", 0, Program.FOPrefsIniPath);
			for (Int32 i = 0; i < m_cbiRadialBlurQualities.Length; i++)
			{
				cbxRadialBlurQuality.Items.Add(m_cbiRadialBlurQualities[i]);
				if (intRadialBlurQuality.Equals(m_cbiRadialBlurQualities[i].Value))
					cbxRadialBlurQuality.SelectedIndex = i;
			}

			//depth of field
			ckbDepthOfField.Checked = (NativeMethods.GetPrivateProfileIntA("Imagespace", "bDoDepthOfField", 0, Program.FOPrefsIniPath) == 1);

			//transparency multisampling
			ckbTransparencyMultisampling.Checked = (NativeMethods.GetPrivateProfileIntA("Display", "bTransparencyMultisampling", 0, Program.FOPrefsIniPath) == 1);

			//decal cap
			oslDecalCap.Value = NativeMethods.GetPrivateProfileIntA("Display", "iMaxDecalsPerFrame", 0, Program.FOPrefsIniPath);
		}

		/// <summary>
		/// Loads the water settings.
		/// </summary>
		private void LoadWaterValues()
		{
			//refractions
			ckbWaterRefractions.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bUseWaterRefractions", 0, Program.FOPrefsIniPath) == 1);

			//reflections
			ckbWaterReflections.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bUseWaterReflections", 0, Program.FOPrefsIniPath) == 1);

			//reflection quality
			// iWaterReflectWidth
			// iWaterReflectHeight
			// the above values should always be the same
			Int32 intReflectionSize = NativeMethods.GetPrivateProfileIntA("Water", "iWaterReflectWidth", 0, Program.FOPrefsIniPath);
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
			ckbSoftReflections.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bUseWaterReflectionBlur", 0, Program.FOPrefsIniPath) == 1);

			//full scene
			ckbFullSceneReflections.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bForceHighDetailReflections", 0, Program.FOPrefsIniPath) == 1);

			//full detail
			ckbFullDetailReflections.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bAutoWaterSilhouetteReflections", 0, Program.FOPrefsIniPath) != 1);

			//water displacements
			ckbWaterDisplacement.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bUseWaterDisplacements", 0, Program.FOPrefsIniPath) == 1);

			//depth fog
			ckbDepthFog.Checked = (NativeMethods.GetPrivateProfileIntA("Water", "bUseWaterDepth", 0, Program.FOPrefsIniPath) == 1);

			//water multisamples
			Int32 intSamples = NativeMethods.GetPrivateProfileIntA("Display", "iWaterMultisamples", 0, Program.FOPrefsIniPath);
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
			ckbEnableShadows.Checked = (NativeMethods.GetPrivateProfileIntA("Display", "bDrawShadows", 0, Program.FOPrefsIniPath) == 1);

			//shadow quality
			Int32 intShadowsRes = NativeMethods.GetPrivateProfileIntA("Display", "iShadowMapResolution", 0, Program.FOPrefsIniPath);
			for (Int32 i = 0; i < m_cbiShadowQualities.Length; i++)
			{
				cbxShadowQuality.Items.Add(m_cbiShadowQualities[i]);
				if (intShadowsRes.Equals(m_cbiShadowQualities[i].Value))
					cbxShadowQuality.SelectedIndex = i;
			}

			//shadow filtering
			Int32 intShadowFilter = NativeMethods.GetPrivateProfileIntA("Display", "iShadowFilter", 0, Program.FOPrefsIniPath);
			for (Int32 i = 0; i < m_cbiShadowFilters.Length; i++)
			{
				cbxShadowFiltering.Items.Add(m_cbiShadowFilters[i]);
				if (intShadowFilter.Equals(m_cbiShadowFilters[i].Value))
					cbxShadowFiltering.SelectedIndex = i;
			}

			//interior shadows
			oslMaxInteriorShadows.Value = NativeMethods.GetPrivateProfileIntA("Display", "iActorShadowCountInt", 0, Program.FOPrefsIniPath);

			//exterior shadows
			oslMaxExteriorShadows.Value = NativeMethods.GetPrivateProfileIntA("Display", "iActorShadowCountExt", 0, Program.FOPrefsIniPath);
		}

		/// <summary>
		/// Loads the view distance settings.
		/// </summary>
		private void LoadViewDistanceValues()
		{
			//object fade
			oslObjectFade.Value = NativeMethods.GetPrivateProfileIntA("LOD", "fLODFadeOutMultObjects", 0, Program.FOPrefsIniPath);

			//actor fade
			oslActorFade.Value = NativeMethods.GetPrivateProfileIntA("LOD", "fLODFadeOutMultActors", 0, Program.FOPrefsIniPath);

			//grass fade
			oslGrassFade.Value = NativeMethods.GetPrivateProfileIntA("Grass", "fGrassStartFadeDistance", 0, Program.FOPrefsIniPath) / 1000;

			//specularity fade
			oslSpecularityFade.Value = NativeMethods.GetPrivateProfileIntA("Display", "fSpecularLODStartFade", 0, Program.FOPrefsIniPath) / 100;

			//light fade
			oslLightFade.Value = NativeMethods.GetPrivateProfileIntA("Display", "fLightLODStartFade", 0, Program.FOPrefsIniPath) / 100;

			//item fade
			decimal dcmValue = 0;
			Decimal.TryParse(NativeMethods.GetPrivateProfileString("LOD", "fLODFadeOutMultItems", "0", Program.FOPrefsIniPath), out dcmValue);
			oslItemFade.Value = dcmValue;

			//shadow fade
			oslLightFade.Value = NativeMethods.GetPrivateProfileIntA("Display", "fShadowLODStartFade", 0, Program.FOPrefsIniPath) / 100;
		}

		/// <summary>
		/// Loads the view distant lod settings.
		/// </summary>
		private void LoadDistantLODValues()
		{
			//tree lod fade
			oslTreeLODFade.Value = NativeMethods.GetPrivateProfileIntA("TerrainManager", "fTreeLoadDistance", 0, Program.FOPrefsIniPath) / 1000;

			//object lod fade fBlockLoadDistanceLow
			oslObjectLODFade.Value = NativeMethods.GetPrivateProfileIntA("TerrainManager", "fBlockLoadDistanceLow", 0, Program.FOPrefsIniPath) / 1000;

			//land quality
			decimal dcmValue = 0;
			Decimal.TryParse(NativeMethods.GetPrivateProfileString("TerrainManager", "fSplitDistanceMult", "0", Program.FOPrefsIniPath), out dcmValue);
			oslLandQuality.Value = dcmValue * 100;
		}

		#endregion

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

		private void cbxAspectRatio_SelectedIndexChanged(object sender, EventArgs e)
		{
			Int32 intScreenWidth = NativeMethods.GetPrivateProfileIntA("Display", "iSize W", 0, Program.FOPrefsIniPath);
			Int32 intScreenHeight = NativeMethods.GetPrivateProfileIntA("Display", "iSize H", 0, Program.FOPrefsIniPath);
			string strCurrentRes = String.Format("{0}x{1}", intScreenWidth, intScreenHeight);
			LoadResolutions((double)((ComboBoxItem)cbxAspectRatio.SelectedItem).Tag, strCurrentRes);
			if ((cbxResolution.SelectedItem == null) && (cbxResolution.Items.Count > 0))
				cbxResolution.SelectedIndex = 0;
		}



		private void ckbWaterReflections_CheckedChanged(object sender, EventArgs e)
		{
			foreach (Control ctlControl in gbxWaterReflections.Controls)
				ctlControl.Enabled = ckbWaterReflections.Checked;
		}

		private void ckbEnableShadows_CheckedChanged(object sender, EventArgs e)
		{
			foreach (Control ctlControl in gbxShadows.Controls)
				ctlControl.Enabled = ckbEnableShadows.Checked;
		}

		private void butOK_Click(object sender, EventArgs e)
		{

			SettingsInstaller sinSaver = new SettingsInstaller();
			sinSaver.SaveSettings(this);
			DialogResult = DialogResult.OK;
		}

		private class SettingsInstaller : ModInstallScript
		{
			#region Constructors

			/// <summary>
			/// A simple constructor that initializes the object.
			/// </summary>
			internal SettingsInstaller()
				: base(null)
			{
			}

			#endregion

			#region Value Saving

			/// <summary>
			/// Saves the general display settings.
			/// </summary>
			private void SaveGeneralValues(GraphicsSettings p_gstSettings)
			{
				//aspect ratios
				EditPrefsINI("Launcher", "uLastAspectRatio", ((ComboBoxItem)p_gstSettings.cbxAspectRatio.SelectedItem).ValueAsString, true);

				//screen resolutions
				Int32[] intResolution = (Int32[])((ComboBoxItem)(p_gstSettings.cbxResolution.SelectedItem)).Value;
				EditPrefsINI("Display", "iSize W", intResolution[0].ToString(), true);
				EditPrefsINI("Display", "iSize H", intResolution[1].ToString(), true);

				//antialiasing
				EditPrefsINI("Display", "iMultiSample", ((ComboBoxItem)(p_gstSettings.cbxAntialiasing.SelectedItem)).ValueAsString, true);

				//ansio
				EditPrefsINI("Display", "iMaxAnisotropy", ((ComboBoxItem)(p_gstSettings.cbxAnisotropic.SelectedItem)).ValueAsString, true);

				//windowed
				EditPrefsINI("Display", "bFull Screen", (p_gstSettings.ckbWindowed.Checked ? "0" : "1"), true);

				//vsync
				EditPrefsINI("Display", "iPresentInterval", (p_gstSettings.ckbVSync.Checked ? "1" : "0"), true);

				//screen effects
				if (p_gstSettings.radNone.Checked)
				{
					EditPrefsINI("BlurShaderHDR", "bDoHighDynamicRange", "0", true);
					EditPrefsINI("BlurShader", "bUseBlurShader", "0", true);
				}
				else if (p_gstSettings.radBloom.Checked)
				{
					EditPrefsINI("BlurShaderHDR", "bDoHighDynamicRange", "0", true);
					EditPrefsINI("BlurShader", "bUseBlurShader", "1", true);
				}
				else
				{
					EditPrefsINI("BlurShaderHDR", "bDoHighDynamicRange", "1", true);
					EditPrefsINI("BlurShader", "bUseBlurShader", "0", true);
				}
			}

			/// <summary>
			/// Saves the details settings.
			/// </summary>
			private void SaveDetailValues()
			{
				//texture quality
				Int32 intTextureQuality = NativeMethods.GetPrivateProfileIntA("Display", "iTexMipMapSkip", 0, Program.FOPrefsIniPath);
				for (Int32 i = 0; i < m_cbiTextureQulities.Length; i++)
				{
					cbxTextureQuality.Items.Add(m_cbiTextureQulities[i]);
					if (intTextureQuality.Equals(m_cbiTextureQulities[i].Value))
						cbxTextureQuality.SelectedIndex = i;
				}

				//radial blur quality
				Int32 intRadialBlurQuality = NativeMethods.GetPrivateProfileIntA("Imagespace", "iRadialBlurLevel", 0, Program.FOPrefsIniPath);
				for (Int32 i = 0; i < m_cbiRadialBlurQualities.Length; i++)
				{
					cbxRadialBlurQuality.Items.Add(m_cbiRadialBlurQualities[i]);
					if (intRadialBlurQuality.Equals(m_cbiRadialBlurQualities[i].Value))
						cbxRadialBlurQuality.SelectedIndex = i;
				}

				//depth of field
				ckbDepthOfField.Checked = (NativeMethods.GetPrivateProfileIntA("Imagespace", "bDoDepthOfField", 0, Program.FOPrefsIniPath) == 1);

				//transparency multisampling
				ckbTransparencyMultisampling.Checked = (NativeMethods.GetPrivateProfileIntA("Display", "bTransparencyMultisampling", 0, Program.FOPrefsIniPath) == 1);

				//decal cap
				oslDecalCap.Value = NativeMethods.GetPrivateProfileIntA("Display", "iMaxDecalsPerFrame", 0, Program.FOPrefsIniPath);
			}

			#endregion

			public void SaveSettings(GraphicsSettings p_gstSettings)
			{
				try
				{
					lock (ModInstallScript.objInstallLock)
					{
						using (TransactionScope tsTransaction = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0)))
						{
							InitTransactionalFileManager();
							TransactionalFileManager.Snapshot(Program.FOPrefsIniPath);
							OverwriteAllIni = true;
							MergeModule = new InstallLogMergeModule();

							SaveGeneralValues(p_gstSettings);
							//LoadDetailValues();
							//LoadWaterValues();
							//LoadShadowValues();
							//LoadViewDistanceValues();
							//LoadDistantLODValues();

							InstallLog.Current.Merge(InstallLog.FOMM, MergeModule);
							tsTransaction.Complete();
						}
					}
				}
				catch (Exception e)
				{
					string strMessage = "A problem occurred while saving settings: " + Environment.NewLine + e.Message;
					if (e.InnerException != null)
						strMessage += Environment.NewLine + e.InnerException.Message;
					strMessage += Environment.NewLine + "The settings were not changed.";
					System.Windows.Forms.MessageBox.Show(strMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				finally
				{
					ReleaseTransactionalFileManager();
				}
			}
		}
	}
}
