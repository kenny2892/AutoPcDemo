using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoPC
{
	public class Selection
	{
		public static string SelectFile(string title = "", string filter = "")
		{
			string filePath = "";
			var fileSelector = new OpenFileDialog();

			if(!String.IsNullOrEmpty(title))
			{
				fileSelector.Title = title;
			}

			if(!String.IsNullOrEmpty(filter))
			{
				fileSelector.Filter = filter;
			}

			if(fileSelector.ShowDialog() == DialogResult.OK)
			{
				filePath = fileSelector.FileName;

				if(!File.Exists(filePath))
				{
					return "";
				}
			}

			return filePath;
		}

		public static List<string> SelectFiles(string title = "", string filter = "")
		{
			List<string> filePaths = new List<string>();
			var fileSelector = new OpenFileDialog();
			fileSelector.Multiselect = true;

			if(!String.IsNullOrEmpty(title))
			{
				fileSelector.Title = title;
			}

			if(!String.IsNullOrEmpty(filter))
			{
				fileSelector.Filter = filter;
			}

			if(fileSelector.ShowDialog() == DialogResult.OK)
			{
				filePaths.AddRange(fileSelector.FileNames.Where(filePath => File.Exists(filePath)));
			}

			Thread.Sleep(500);
			return filePaths;
		}

		public static string FolderSelect()
		{
			string folderPath = "";
			var folderSelctor = new FolderBrowserDialog();

			if(folderSelctor.ShowDialog() == DialogResult.OK)
			{
				folderPath = folderSelctor.SelectedPath;

				if(!Directory.Exists(folderPath))
				{
					folderPath = "";
				}
			}

			return folderPath;
		}

		public static string SelectCsv(string title = "Open CSV file")
		{
			return SelectFile(title, "CSV files (*.csv)|*.csv");
		}

		public static string SelectTxt(string title = "Open Txt file")
		{
			return SelectFile(title, "Text files (*.txt)|*.txt");
		}

		public static string SelectSaveFile(string fileName, string title = "Save file", string additionalFilter = "")
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();

			saveFileDialog.Filter = (String.IsNullOrEmpty(additionalFilter) ? "" : additionalFilter + "|") + "All files (*.*)|*.*";
			saveFileDialog.RestoreDirectory = true;
			saveFileDialog.FileName = fileName;
			saveFileDialog.Title = title;

			if(saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				var filePath = Path.GetFullPath(saveFileDialog.FileName);
				return filePath;
			}

			return "";
		}

		public static bool IsFileOpen(string filePath)
		{
			if(!File.Exists(filePath))
			{
				return false;
			}

			FileInfo fileInfo = new FileInfo(filePath);
			bool isOpen = false;

			try
			{
				using(FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
				{
					stream.Close();
				}
			}

			catch(IOException)
			{
				isOpen = true;
			}

			return isOpen;
		}
	}
}
