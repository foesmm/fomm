using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace GeMod.Interface
{
	/// <summary>
	/// Description of ModVersion.
	/// </summary>
	[Serializable]
	public class ModVersion : IComparable, IComparable<ModVersion>, IEquatable<ModVersion>
	{
		private string _Prefix = "";
		public string Prefix
		{
			get
			{
				return _Prefix;
			}
		}
		private int _Major = 0;
		public int Major
		{
			get
			{
				return _Major;
			}
		}
		private int _Minor = 1;
		public int Minor
		{
			get
			{
				return _Minor;
			}
		}
		private int _Patch = -1;
		public int Patch
		{
			get
			{
				return _Patch;
			}
		}
		private string _Suffix = "";
		public string Suffix
		{
			get
			{
				return _Suffix;
			}
		}
		
		public ModVersion()
		{
		}
		
		public ModVersion(string version)
		{
			ModVersion ver = ModVersion.Parse(version);
			this._Prefix = ver.Prefix;
			this._Major = ver.Major;
			this._Minor = ver.Minor;
			this._Patch = ver.Patch;
			this._Suffix = ver.Suffix;
		}
		
		private static string SanitizeVersionString(string input)
		{
			string result;
			if (input.StartsWith("v")) {
				result = input.Substring(1);
			} else {
				result = input;
			}
			
			Match rxPreMatch = Regex.Match(result, @"^(alpha|beta|hotfix|rc)(\d+)?$");
			if (rxPreMatch.Success) {
				result = string.Format("1.0-{0}.{1}", rxPreMatch.Groups[1].Value, rxPreMatch.Groups[2].Value);
			} else if (result.StartsWith("alpha")) {
				result = string.Format("{0}{1}", result.Substring(5, result.Length - 5), "a");
			} else if (result.StartsWith("beta")) {
				result = string.Format("{0}{1}", result.Substring(4, result.Length - 4), "b");
			} else if (result.StartsWith("rc")) {
				result = string.Format("{0}{1}", result.Substring(2, result.Length - 2), "rc");
			} else {
				result = result.Replace("point", ".");
			}
			return result;
		}
		
		private static string ExtractPrefix(string input, out string prefix)
		{
			string result = input;
			prefix = "";

			Match rxMatch = Regex.Match(input, @"^([a-z]+)");
			if (rxMatch.Success) {
				prefix = rxMatch.Groups[0].Value;
				result = input.Substring(prefix.Length, input.Length - prefix.Length);
			}
			
			return result;
		}
		
		private static string ExtractSuffix(string input, out string suffix)
		{
			string result = input;
			suffix = "";
			if (input.Contains("-")) {
				string[] parts = input.Split('-');
				suffix = parts[1];
				result = parts[0];
			} else if (input.EndsWith("alpha")) {
			    suffix = "alpha";
			    result = input.Substring(0, input.Length - 5);
			} else if (input.EndsWith("beta")) {
	            suffix = "beta";
	            result = input.Substring(0, input.Length - 4);
			} else if (input.EndsWith("rc")) {
				suffix = "rc";
				result = input.Substring(0, input.Length - 2);
			} else if (input.EndsWith("a")) {
				suffix = "alpha";
				result = input.Substring(0, input.Length - 1);
			} else if (input.EndsWith("b")) {
				suffix = "beta";
				result = input.Substring(0, input.Length - 1);
			} else {
				Match rxMatch = Regex.Match(input, @"([a-z]+)$");
				if (rxMatch.Success) {
					suffix = rxMatch.Groups[0].Value;
					result = input.Substring(0, input.Length - suffix.Length);
				}
			}
			
			return result;
		}
		
		public static ModVersion Parse(string input)
		{
			ModVersion version = new ModVersion();
			
			string sanitizedVersion;
			sanitizedVersion = SanitizeVersionString(input.ToLower());
			sanitizedVersion = ExtractPrefix(sanitizedVersion, out version._Prefix);
			sanitizedVersion = ExtractSuffix(sanitizedVersion, out version._Suffix);
			
			string[] parts = sanitizedVersion.Split('.');
			
			if (!int.TryParse(parts[0], out version._Major)) {
				if (parts.Length > 1) {
					version._Major = 0;
				} else {
					version._Major = 1;
				}
			}
			
			if (parts.Length >= 2 && !int.TryParse(parts[1], out version._Minor)) {
				version._Minor = 0;
			} else if (parts.Length < 2) {
				version._Minor = -1;
			}
			
			if (parts.Length >= 3 && !int.TryParse(parts[2], out version._Patch)) {
				version._Patch = -1;
			}
			
			return version;
		}
		
		public int CompareTo(ModVersion value)
		{
			if (object.ReferenceEquals(value, null)) {
				return 1;
			}
			
			if (this._Major != value._Major) {
				return (this._Major > value._Major)?1:-1;
			} else {
				if (this._Minor != value._Minor) {
					return (this._Minor > value._Minor)?1:-1;
				} else {
					if (this._Patch != value._Patch) {
						return (this._Patch > value._Patch)?1:-1;
					} else {
						if (!value._Suffix.Contains("."))
							value._Suffix += ".1";
						if (!this._Suffix.Contains("."))
							value._Suffix += ".1";
						
						string[] thisSuffixParts = this._Suffix.Split('.');
						string[] valueSuffixParts = value._Suffix.Split('.');
						
						if (thisSuffixParts[0] != valueSuffixParts[0]) {
							Dictionary<string, int> dict = new Dictionary<string, int>{
								{"alpha", 1},
								{"beta", 2},
								{"rc", 3}
							};
							
							if (dict.ContainsKey(thisSuffixParts[0]) && dict.ContainsKey(valueSuffixParts[0]))
								return (dict[thisSuffixParts[0]] > dict[valueSuffixParts[0]])?1:-1;
							if (dict.ContainsKey(thisSuffixParts[0]) && !dict.ContainsKey(valueSuffixParts[0]))
								return -1;
							if (!dict.ContainsKey(thisSuffixParts[0]) && dict.ContainsKey(valueSuffixParts[0]))
								return 1;
							return string.CompareOrdinal(thisSuffixParts[0], valueSuffixParts[0]);
						} else {
							return (int.Parse(thisSuffixParts[1]) > int.Parse(valueSuffixParts[1]))?1:-1;
						}
					}
				}
			}
			return 0;
		}
		
		public int CompareTo(object version)
		{
			ModVersion version2 = version as ModVersion;
			if (version2 == null) {
				throw new ArgumentException("Argument must be ModVersion type");
			}
				
			return this.CompareTo(version2);
		}
		
		#region Equals and GetHashCode implementation
		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				if (_Prefix != null)
					hashCode += 1000000007 * _Prefix.GetHashCode();
				hashCode += 1000000009 * _Major.GetHashCode();
				hashCode += 1000000021 * _Minor.GetHashCode();
				hashCode += 1000000033 * _Patch.GetHashCode();
				if (_Suffix != null)
					hashCode += 1000000087 * _Suffix.GetHashCode();
			}
			return hashCode;
		}
		
		public override bool Equals(object obj)
		{
			ModVersion other = obj as ModVersion;
			if (other == null)
				return false;
			return this._Prefix == other._Prefix && this._Major == other._Major && this._Minor == other._Minor && this._Patch == other._Patch && this._Suffix == other._Suffix;
		}
		
		#endregion

		
		public bool Equals(ModVersion value)
		{
			return (this._Major == value._Major && this._Minor == value._Minor &&
			        this._Patch == value._Patch && this._Suffix == value._Suffix &&
			        this._Prefix == value._Prefix);
		}
		
		public static bool operator ==(ModVersion v1, ModVersion v2)
		{
			if (object.ReferenceEquals(v1, null))
		    {
				return object.ReferenceEquals(v2, null);
		    }
			
			return v1.Equals(v2);
		}
		
		public static bool operator !=(ModVersion v1, ModVersion v2)
		{
			return !(v1 == v2);
		}
		
		public static bool operator <(ModVersion v1, ModVersion v2)
		{
			if (object.ReferenceEquals(v1, null))
			{
				throw new ArgumentNullException("v1");
			}
			return v1.CompareTo(v2) < 0;
		}
		
		public static bool operator <=(ModVersion v1, ModVersion v2)
		{
			if (object.ReferenceEquals(v1, null))
			{
				throw new ArgumentNullException("v1");
			}
			return v1.CompareTo(v2) <= 0;
		}
		
		public static bool operator >(ModVersion v1, ModVersion v2)
		{
			return v2 < v1;
		}
		
		public static bool operator >=(ModVersion v1, ModVersion v2)
		{
			return v2 <= v1;
		}
				
		public override string ToString()
		{
			if (_Prefix != "" && _Major == 1 && _Minor == -1)
				return _Prefix;
			
			string version = string.Format("{0}{1}", _Prefix, _Major);
			if (_Minor >= 0) {
				version = string.Format("{0}.{1}", version, _Minor);
				
				if (_Patch >= 0) {
					version = string.Format("{0}.{1}", version, _Patch);
				}
			}
			
			if (_Suffix != "") {
				version = string.Format("{0}-{1}", version, _Suffix);
			}
			
			return version;
		}

        public static implicit operator String(ModVersion version)
        {
            return version.ToString();
        }
	}
}
