using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Fomm.PackageManager;

namespace Fomm.Games.Fallout3.Script
{
	/// <summary>
	/// This class encapsulates the management of textures.
	/// </summary>
	public class TextureManager : IDisposable
	{
		private bool m_booDdsParserInited = false;
		private List<IntPtr> m_lstTextures = null;

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		internal TextureManager()
		{
			m_lstTextures = new List<IntPtr>();
		}

		#endregion

		/// <summary>
		/// Loads the given texture.
		/// </summary>
		/// <param name="p_bteTexture">The texture to load.</param>
		/// <returns>A pointer to the loaded texture.</returns>
		public IntPtr LoadTexture(byte[] p_bteTexture)
		{
			PermissionsManager.CurrentPermissions.Assert();
			if (!m_booDdsParserInited)
			{
				NativeMethods.ddsInit(Application.OpenForms[0].Handle);
				m_booDdsParserInited = true;
			}
			IntPtr ptr = NativeMethods.ddsLoad(p_bteTexture, p_bteTexture.Length);
			if (ptr != IntPtr.Zero) m_lstTextures.Add(ptr);
			return ptr;
		}

		/// <summary>
		/// Creates a texture with the given dimensions.
		/// </summary>
		/// <param name="p_intWidth">The width of the texture.</param>
		/// <param name="p_intHeight">The height of the texture.</param>
		/// <returns>A pointer to the new texture.</returns>
		public IntPtr CreateTexture(int p_intWidth, int p_intHeight)
		{
			PermissionsManager.CurrentPermissions.Assert();
			if (!m_booDdsParserInited)
			{
				NativeMethods.ddsInit(Application.OpenForms[0].Handle);
				m_booDdsParserInited = true;
			}
			IntPtr ptr = NativeMethods.ddsCreate(p_intWidth, p_intHeight);
			if (ptr != IntPtr.Zero) m_lstTextures.Add(ptr);
			return ptr;
		}

		/// <summary>
		/// Saves the specified texture.
		/// </summary>
		/// <param name="p_ptrTexture">The pointer to the texture to save.</param>
		/// <param name="p_intFormat">The format in which to save the texture.</param>
		/// <param name="p_booMipmaps">Whether or not to create mipmaps (or maybe whether or
		/// not the given texture contains mipmaps?).</param>
		/// <returns>The saved texture.</returns>
		public byte[] SaveTexture(IntPtr p_ptrTexture, int p_intFormat, bool p_booMipmaps)
		{
			if (!m_booDdsParserInited || !m_lstTextures.Contains(p_ptrTexture)) return null;
			PermissionsManager.CurrentPermissions.Assert();
			int length;
			IntPtr data = NativeMethods.ddsSave(p_ptrTexture, p_intFormat, p_booMipmaps ? 1 : 0, out length);
			if (data == IntPtr.Zero) return null;
			byte[] result = new byte[length];
			System.Runtime.InteropServices.Marshal.Copy(data, result, 0, length);
			return result;
		}

		/// <summary>
		/// Copies part of one texture to another.
		/// </summary>
		/// <param name="p_ptrSource">A pointer to the texture from which to make the copy.</param>
		/// <param name="p_rctSourceRect">The area of the source texture from which to make the copy.</param>
		/// <param name="p_ptrDestination">A pointer to the texture to which to make the copy.</param>
		/// <param name="p_rctDestinationRect">The area of the destination texture to which to make the copy.</param>
		public void CopyTexture(IntPtr p_ptrSource, Rectangle p_rctSourceRect, IntPtr p_ptrDestination, Rectangle p_rctDestinationRect)
		{
			if (!m_booDdsParserInited || !m_lstTextures.Contains(p_ptrSource) || !m_lstTextures.Contains(p_ptrDestination)) return;
			PermissionsManager.CurrentPermissions.Assert();
			NativeMethods.ddsBlt(p_ptrSource, p_rctSourceRect.Left, p_rctSourceRect.Top, p_rctSourceRect.Width, p_rctSourceRect.Height, p_ptrDestination, p_rctDestinationRect.Left,
				p_rctDestinationRect.Top, p_rctDestinationRect.Width, p_rctDestinationRect.Height);
		}

		/// <summary>
		/// Gets the dimensions of the specified texture.
		/// </summary>
		/// <param name="p_ptrTexture">A pointer to the texture whose dimensions are to be determined.</param>
		/// <param name="p_intWidth">The out parameter that will contain the width of the texture.</param>
		/// <param name="p_intHeight">The out parameter that will contain the height of the texture.</param>
		public void GetTextureSize(IntPtr p_ptrTexture, out int p_intWidth, out int p_intHeight)
		{
			p_intWidth = 0;
			p_intHeight = 0;
			if (!m_booDdsParserInited || !m_lstTextures.Contains(p_ptrTexture)) return;
			PermissionsManager.CurrentPermissions.Assert();
			NativeMethods.ddsGetSize(p_ptrTexture, out p_intWidth, out p_intHeight);
		}

		/// <summary>
		/// Retrieves the texture data for the specified texture.
		/// </summary>
		/// <param name="p_ptrTexture">A pointer to the texture whose data is to be retrieved.</param>
		/// <param name="p_intPitch">The out parameter that will contain the texture's pitch.</param>
		/// <returns>The texture data.</returns>
		public byte[] GetTextureData(IntPtr p_ptrTexture, out int p_intPitch)
		{
			p_intPitch = 0;
			if (!m_booDdsParserInited || !m_lstTextures.Contains(p_ptrTexture)) return null;
			PermissionsManager.CurrentPermissions.Assert();
			int length;
			IntPtr ptr = NativeMethods.ddsLock(p_ptrTexture, out length, out p_intPitch);
			if (ptr == IntPtr.Zero) return null;
			byte[] result = new byte[length];
			System.Runtime.InteropServices.Marshal.Copy(ptr, result, 0, length);
			NativeMethods.ddsUnlock(p_ptrTexture);
			return result;
		}

		/// <summary>
		/// Sets the data for the specified texture.
		/// </summary>
		/// <param name="p_ptrTexture">A pointer to the texture whose data is to be set.</param>
		/// <param name="p_bteData">The data to which to set the texture.</param>
		public void SetTextureData(IntPtr p_ptrTexture, byte[] p_bteData)
		{
			if (!m_booDdsParserInited || !m_lstTextures.Contains(p_ptrTexture)) return;
			PermissionsManager.CurrentPermissions.Assert();
			NativeMethods.ddsSetData(p_ptrTexture, p_bteData, p_bteData.Length);
		}

		/// <summary>
		/// Releases the specified texture.
		/// </summary>
		/// <param name="p_ptrTexture">A pointer to the texture to release.</param>
		public void ReleaseTexture(IntPtr p_ptrTexture)
		{
			if (!m_booDdsParserInited || !m_lstTextures.Contains(p_ptrTexture)) return;
			PermissionsManager.CurrentPermissions.Assert();
			NativeMethods.ddsRelease(p_ptrTexture);
			m_lstTextures.Remove(p_ptrTexture);
		}

		#region IDisposable Members

		/// <summary>
		/// Disposes the texture manager.
		/// </summary>
		/// <remarks>
		/// This method ensures that all textures have been released.
		/// </remarks>
		public void Dispose()
		{
			if (m_booDdsParserInited)
			{
				for (int i = 0; i < m_lstTextures.Count; i++)
					NativeMethods.ddsRelease(m_lstTextures[i]);
				m_lstTextures = null;
				NativeMethods.ddsClose();
				m_booDdsParserInited = false;
			}
		}

		#endregion
	}
}
