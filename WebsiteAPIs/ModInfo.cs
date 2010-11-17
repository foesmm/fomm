using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteAPIs
{
	/// <summary>
	/// Encapsulates information about a mod.
	/// </summary>
	public class ModInfo
	{
		#region Properties

		/// <summary>
		/// Gets the author of the mod.
		/// </summary>
		/// <value>The author of the mod.</value>
		public string Author { get; private set; }

		/// <summary>
		/// Gets the version of the mod.
		/// </summary>
		/// <value>The version of the mod.</value>
		public string Version { get; private set; }

		/// <summary>
		/// Gets the webpage of the mod.
		/// </summary>
		/// <value>The webpage of the mod.</value>
		public Uri URL { get; private set; }

		/// <summary>
		/// Gets the url to the mod's screenshot.
		/// </summary>
		/// <value>The url to the mod's screenshot.</value>
		public Uri ScreenshotURL { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_strAuthor">The author of the mod.</param>
		/// <param name="p_strVersion">The version of the mod.</param>
		/// <param name="p_uriURL">The webpage of the mod.</param>
		/// <param name="p_uriScreenshotURL">The url to the mod's screenshot.</param>
		public ModInfo(string p_strAuthor, string p_strVersion, Uri p_uriURL, Uri p_uriScreenshotURL)
		{
			Author = p_strAuthor;
			Version = p_strVersion;
			URL = p_uriURL;
			ScreenshotURL = p_uriScreenshotURL;
		}

		#endregion
	}
}
