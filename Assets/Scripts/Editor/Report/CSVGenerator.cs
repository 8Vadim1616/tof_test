using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using UnityEditor;

namespace Assets.Scripts.UI.WindowsSystem.Editor.Reports
{
	public class CSVGenerator
	{
		public const string Separator = ";";//"\t";
		private const string SPACE_SYMBOL = "__sp_";
		
		public Dictionary<string, Dictionary<string, object>> Grid = new Dictionary<string, Dictionary<string, object>>();
		
		public void AddCell(string column, string line, bool data) => AddCell(column, line, data ? "1" : string.Empty);
            
		public void AddCell(string column, string line, string data)
		{
			if(!Grid.ContainsKey(line))
				Grid[line] = new Dictionary<string, object>();
			Grid[line][column] = data;
		}

		private int _spaceCount = 0;

		public void AddSpace()
		{
			Grid[$"{SPACE_SYMBOL}{_spaceCount++}"] = new Dictionary<string, object>();
		}

		public static CSVGenerator FromString(string table, char separator = ';')
		{
			var result = new CSVGenerator();
			
			var lines = table.Split('\n');
			bool isFirstLine = true;

			string[] columnsNames = null;
			
			foreach (var line in lines)
			{
				var columns = line.Split(separator);
				if (columns.Length == 1)
					columns = line.Split('\t');

				if (isFirstLine)
				{
					isFirstLine = false;
					columnsNames = columns;
				}
				else
				{
					for (var i = 1; i < columnsNames.Length && i < columns.Length; i++)
						if(!columns[i].IsNullOrEmpty())
							result.AddCell(columnsNames[i], columns[0], columns[i]);
				}
			}

			return result;
		}

		public override string ToString()
		{
			var result = new StringBuilder();
			var lines = Grid.Keys;
			var columns = new List<string>();
			Grid.Values.SelectMany(c => c.Keys).Each(l => columns.AddOnce(l));

			Append("");
			foreach (var column in columns)
				Append(column);
            
			foreach (var line in lines)
			{
				NextLine();
				if (line.Contains(SPACE_SYMBOL))
					continue;

				Append(line);
				foreach (var column in columns)
				{
					if(Grid.ContainsKey(line) && Grid[line].ContainsKey(column))
						Append(Grid[line][column].ToString());
					else
						Append(string.Empty);
				}
			}
            
			return result.ToString();

			void Append(string cell)
			{
				result.Append(cell);
				result.Append(Separator);
			}
            
			void NextLine() => result.Append("\n");
		}

		public void SaveAs(string name = "report")
		{
			var path = EditorUtility.SaveFilePanel("Save report...", "", name, "csv");
            
			if (path.Length != 0)
			{
				File.WriteAllText(path, ToString(), Encoding.UTF8);
			}
		}

		public string GetSaveFolder() => EditorUtility.SaveFolderPanel("Save report...", "", "");

		public void AddToFile(string line, string path)
		{
			var result = new StringBuilder();
			var columns = new List<string>();
			Grid.Values.SelectMany(c => c.Keys).Each(l => columns.AddOnce(l));
			
			if (!File.Exists(path))
			{
				Append("");
				foreach (var column in columns)
					Append(column);
			}
			
			NextLine();
			Append(line);
			foreach (var column in columns)
			{
				if(Grid.ContainsKey(line) && Grid[line].ContainsKey(column))
					Append(Grid[line][column].ToString());
				else
					Append(string.Empty);
			}
			
			File.AppendAllText(path, result.ToString());
			

			void Append(string cell)
			{
				result.Append(cell);
				result.Append(Separator);
			}
            
			void NextLine() => result.Append("\n");
		}

		public void Save(string path)
		{
			File.WriteAllText(path, ToString(), Encoding.UTF8);
		}

		public object GetCell(string line, string column)
		{
			if (Grid.ContainsKey(line) && Grid[line].ContainsKey(column))
				return Grid[line][column];
			return null;
		}
	}
}