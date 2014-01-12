using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Fomm.Games.Fallout3.Tools.AutoSorter
{
	public class LoadOrderSorter
	{
		private struct ModInfo
		{
			public readonly string name;
			public double id;
			public readonly bool hadEntry;

			public ModInfo(string s, double id, bool hadEntry)
			{
				name = s;
				this.id = id;
				this.hadEntry = hadEntry;
			}
		}
		private struct RecordInfo
		{
			public readonly int id;
			public string[] requires;
			public string[] conflicts;
			public string[] comments;

			public RecordInfo(int id)
			{
				this.id = id;
				requires = null;
				conflicts = null;
				comments = null;
			}
		}

		private static readonly string m_strLoadOrderTemplatePath = Path.Combine(Program.GameMode.InstallInfoDirectory, "lotemplate.txt");
		private Dictionary<string, RecordInfo> m_dicMasterList;
		private int duplicateCount;
		private int fileVersion;

		public bool HasMasterList
		{
			get
			{
				return File.Exists(m_strLoadOrderTemplatePath);
			}
		}

		public static string LoadOrderTemplatePath
		{
			get
			{
				return m_strLoadOrderTemplatePath;
			}
		}

		private Dictionary<string, RecordInfo> MasterListOrder
		{
			get
			{
				return m_dicMasterList;
			}
		}

		public LoadOrderSorter()
		{
			LoadList();
		}

		/// <summary>
		/// Loads the master list.
		/// </summary>
		public void LoadList()
		{
			m_dicMasterList = new Dictionary<string, RecordInfo>();
			if (!File.Exists(LoadOrderTemplatePath))
				return;
			string[] fileLines = File.ReadAllLines(LoadOrderTemplatePath);
			
			if (!int.TryParse(fileLines[0], out fileVersion))
			{
				fileVersion = 0;
			}
			int upto = 0;
			List<string> requires = new List<string>();
			List<string> conflicts = new List<string>();
			List<string> comments = new List<string>();
			for (int i = 0; i < fileLines.Length; i++)
			{
				int comment = fileLines[i].IndexOf('\\');
				if (comment != -1) fileLines[i] = fileLines[i].Remove(comment);
				fileLines[i] = fileLines[i].Trim();
				if (fileLines[i] != string.Empty)
				{
					RecordInfo ri = new RecordInfo(upto++);
					int skiplines = 0;
					for (int j = i + 1; j < fileLines.Length; j++)
					{
						fileLines[j] = fileLines[j].Trim();
						if (fileLines[j].Length > 0)
						{
							switch (fileLines[j][0])
							{
								case ':':
									requires.Add(fileLines[j].Substring(1).ToLowerInvariant().Trim());
									skiplines++;
									continue;
								case '"':
									conflicts.Add(fileLines[j].Substring(1).ToLowerInvariant().Trim());
									skiplines++;
									continue;
								case '*':
								case '?':
									comments.Add(fileLines[j].Substring(1).Trim());
									skiplines++;
									continue;
							}
							break;
						}
						skiplines++;
					}
					if (requires.Count > 0)
					{
						ri.requires = requires.ToArray();
						requires.Clear();
					}
					if (conflicts.Count > 0)
					{
						ri.conflicts = conflicts.ToArray();
						conflicts.Clear();
					}
					if (comments.Count > 0)
					{
						ri.comments = comments.ToArray();
						comments.Clear();
					}
					fileLines[i] = fileLines[i].ToLowerInvariant();
					if (m_dicMasterList.ContainsKey(fileLines[i])) duplicateCount++;
					m_dicMasterList[fileLines[i]] = ri;
					i += skiplines;
				}
			}
		}

		private ModInfo[] BuildModInfo(string[] plugins)
		{
			ModInfo[] mi = new ModInfo[plugins.Length];
			int addcount = 1;
			int lastPos = -1;
			int maxPos = 0;
			for (int i = 0; i < mi.Length; i++)
			{
				string lplugin = plugins[i].ToLowerInvariant();
				if (m_dicMasterList.ContainsKey(lplugin))
				{
					lastPos = m_dicMasterList[lplugin].id;
					if (lastPos > maxPos) maxPos = lastPos;
					mi[i] = new ModInfo(plugins[i], lastPos, true);
					addcount = 1;
				}
				else
				{
					mi[i] = new ModInfo(plugins[i], lastPos + addcount * 0.001, false);
					addcount++;
				}
			}
			addcount = 1;
			maxPos++;
			for (int i = mi.Length - 1; i >= 0; i--)
			{
				if (mi[i].hadEntry) break;
				mi[i].id = maxPos - addcount * 0.001;
				addcount++;
			}
			return mi;
		}

		public string GenerateReport(string[] plugins, bool[] active, bool[] corrupt, string[][] masters)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(plugins.Length * 32);
			string[] lplugins = new string[plugins.Length];
			for (int i = 0; i < plugins.Length; i++) lplugins[i] = plugins[i].ToLowerInvariant();
			double latestPosition = 0;
			sb.AppendLine("Mod load order report");
			if (duplicateCount > 0) sb.AppendLine("! Warning: current load order template contains " + duplicateCount + " duplicate entries. This warning can be ignored.");
			sb.AppendLine();
			bool LoadOrderWrong = false;
			for (int i = 0; i < plugins.Length; i++)
			{
				sb.AppendLine(plugins[i] + (active[i] ? string.Empty : " (Inactive)"));
				plugins[i] = plugins[i].ToLowerInvariant();
				if (corrupt[i])
				{
					sb.AppendLine("! This plugin is unreadable, and probably corrupt");
				}
				if (active[i] && masters[i] != null)
				{
					for (int k = 0; k < masters[i].Length; k++)
					{
						bool found = false;
						for (int j = 0; j < i; j++)
						{
							if (active[j] && lplugins[j] == masters[i][k])
							{
								found = true;
								break;
							}
						}
						if (!found)
						{
							for (int j = i + 1; j < plugins.Length; j++)
							{
								if (active[j] && lplugins[j] == masters[i][k])
								{
									sb.AppendLine("! This plugin depends on master '" + masters[i][k] + "', which is loading after it in the load order");
									found = true;
									break;
								}
							}
							if (!found)
							{
								sb.AppendLine("! This plugin depends on master '" + masters[i][k] + "', which is not loading");
							}
						}
					}
				}
				if (m_dicMasterList.ContainsKey(plugins[i]))
				{
					RecordInfo ri = m_dicMasterList[plugins[i]];
					if (ri.id < latestPosition)
					{
						sb.AppendLine("* The current load order of this mod does not match the current template");
						LoadOrderWrong = true;
					}
					else latestPosition = ri.id;
					if (active[i] && ri.requires != null)
					{
						for (int k = 0; k < ri.requires.Length; k++)
						{
							bool found = false;
							for (int j = 0; j < lplugins.Length; j++)
							{
								if (lplugins[j] == ri.requires[k])
								{
									if (active[j]) found = true;
									break;
								}
							}
							if (!found)
							{
								sb.AppendLine("! This plugin requires '" + ri.requires[k] + "', which was not found");
							}
						}
					}
					if (active[i] && ri.conflicts != null)
					{
						for (int k = 0; k < ri.conflicts.Length; k++)
						{
							for (int j = 0; j < lplugins.Length; j++)
							{
								if (lplugins[j] == ri.conflicts[k])
								{
									if (active[j]) sb.AppendLine("! This plugin conflicts with '" + ri.conflicts[k] + "'");
									break;
								}
							}
						}
					}
					if (ri.comments != null)
					{
						for (int k = 0; k < ri.comments.Length; k++)
						{
							sb.AppendLine("  " + ri.comments[k]);
						}
					}
				}
				else
				{
					sb.AppendLine("* The auto-sorter doesn't recognize this mod. It is probably safe to put it anywhere, depending on how you want the various plugins to override one another.");
				}
				sb.AppendLine();
			}
			if (LoadOrderWrong)
			{
				string[] dup = (string[])plugins.Clone();
				SortList(dup);
				sb.AppendLine("The order that the current template suggests is as follows:");
				for (int i = 0; i < dup.Length; i++) sb.AppendLine(dup[i]);
			}
			return sb.ToString();
		}

		public void SortList(string[] plugins)
		{
			ModInfo[] mi = BuildModInfo(plugins);
			Array.Sort<ModInfo>(mi, delegate(ModInfo a, ModInfo b) { return a.id.CompareTo(b.id); });
			for (int i = 0; i < mi.Length; i++) plugins[i] = mi[i].name;
		}

		/// <summary>
		/// Determins if the given list of plugins has been auto-sorted.
		/// </summary>
		/// <param name="plugins">The plugins whose order is to be verified.</param>
		/// <returns><lang cref="true"/> if the plugins have been auto-sorted;
		/// <lang cref="false"/> otherwise.</returns>
		public bool CheckList(string[] plugins)
		{
			if (!HasMasterList)
				return false;
			ModInfo[] mi = BuildModInfo(plugins);
			double upto = 0;
			for (int i = 0; i < mi.Length; i++)
			{
				if (mi[i].id < upto) return false;
				else upto = mi[i].id;
			}
			return true;
		}

		public int GetInsertionPos(string[] plugins, string plugin)
		{
			plugin = plugin.ToLowerInvariant();
			if (!m_dicMasterList.ContainsKey(plugin)) return plugins.Length;
			ModInfo[] mi = BuildModInfo(plugins);
			int target = m_dicMasterList[plugin].id;
			for (int i = 0; i < mi.Length; i++)
			{
				if (mi[i].id >= target) return i;
			}
			return plugins.Length;
		}

		public int GetFileVersion()
		{
			return fileVersion;
		}
	}
}
