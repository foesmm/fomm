
using System;
using NUnit.Framework;
using GeMod.Interface;

namespace Fomm.Tests
{
	[TestFixture]
	public class ModVersionTest
	{
		[Test]
		public void TestModVersionParse()
		{
			string[] versions = {
				"beta2",
				"beta.38",
				"beta.65",
				"Mklv5.1HDR",
				"final",
				"H.007.2",
				"1.0BETA",
				"H",
				"beta3",
				"1.0ARP",
				"v6D",
				"1.0",
				"0.97.3",
				"v.1",
				"",
				"12",
				"v10",
				"V1.0",
				"1.04",
				"v1.1",
				"ONLY",
				"v1.00",
				"v1.05",
				"1.05.2",
				"3.3b",
				"1point1",
				"1h",
				"1a",
				"0.4b",
				"1.6a",
				"v12A",
				"Alpha2",
				"V2",
				"02a",
				".6",
				"v003",
				"v1.1.0",
				"Any",
				"2.xx",
				"1b",
				"v0.8b",
				"V0.2",
				"1.30c",
				"0.02a",
				"hotfix1",
				"RC2"
			};
			
			foreach (string version in versions) {
				ModVersion.Parse(version);
			}
			
		}
		
		[Test]
		public void TestModVersionCompare()
		{
			Assert.True(IsNewer("beta2", "beta.38"));
		}
		
		private bool IsNewer(string verA, string verB) {
			ModVersion a = ModVersion.Parse(verA);
			ModVersion b = ModVersion.Parse(verB);
			return (a > b);
		}
	}
}
