using System;
using Fomm.PackageManager.Upgrade;

namespace Fomm.PackageManager
{
	/// <summary>
	/// Performs a reactivation of a <see cref="fomod"/>.
	/// </summary>
	/// <remarks>
	/// A reactivation is almost the same as an in-place upgrade. The only differences are
	/// that a reactivation is never already done (<see cref="CheckAlreadyDone()"/> always
	/// returns false), and failure doesn't imply the mod is deactivated.
	/// </remarks>
	/// <seealso cref="ModUpgrader"/>
	public class ModReactivator : ModUpgrader
	{
		#region Properties

		/// <seealso cref="ModInstallScript.ExceptionMessage"/>
		protected override string ExceptionMessage
		{
			get
			{
				return "A problem occurred during reactivation: " + Environment.NewLine + "{0}" + Environment.NewLine + "The mod was not reactivated.";
			}
		}

		/// <seealso cref="ModInstallScript.SuccessMessage"/>
		protected override string SuccessMessage
		{
			get
			{
				return "The mod was successfully reactivated.";
			}
		}

		/// <seealso cref="ModInstallScript.FailMessage"/>
		protected override string FailMessage
		{
			get
			{
				return "The mod was not reactivated.";
			}
		}

		/// <seealso cref="ModUpgrader.ProgressMessage"/>
		protected virtual string ProgressMessage
		{
			get
			{
				return "Reactivating Fomod";
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object.
		/// </summary>
		/// <param name="p_fomodMod">The <see cref="fomod"/> to be reactivated.</param>
		internal ModReactivator(fomod p_fomodMod)
			: base(p_fomodMod)
		{
		}

		#endregion

		/// <summary>
		/// Indicates that this script's work has not already been completed.
		/// </summary>
		/// <returns><lang cref="false"/>.</returns>
		/// <seealso cref="ModInstallScript.CheckAlreadyDone()"/>
		protected override bool CheckAlreadyDone()
		{
			return false;
		}

		/// <summary>
		/// Determines whether or not the fomod should be activated, based on whether
		/// or not the script was successful.
		/// </summary>
		/// <param name="p_booSucceeded">Whether or not the script was successful.</param>
		/// <returns><lang cref="true"/> if the fomod is already active;
		/// <lang cref="false"/> otherwise.</returns>
		protected override bool DetermineFomodActiveStatus(bool p_booSucceeded)
		{
			return Fomod.IsActive;
		}
	}
}
