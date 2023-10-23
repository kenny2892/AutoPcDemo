using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoPC
{
	public class Monitor
	{
		public Screen Display { get; private set; }
		public int ScreenIndex { get; private set; }
		public int Width { get { return Display.Bounds.Width; } }
		public int Height { get { return Display.Bounds.Height; } }

		public Monitor(int screenIndex)
		{
			Display = Screen.AllScreens[screenIndex];
			ScreenIndex = screenIndex;
		}

		public Coordinate ConvertToGlobalCoords(Coordinate coords)
		{
			return ConvertToGlobalCoords(coords.X, coords.Y);
		}

		public Coordinate ConvertToGlobalCoords(int x, int y)
		{
			return new Coordinate(x + Display.Bounds.X, y + Display.Bounds.Y);
		}

		public Coordinate ConvertToLocalCoords(Coordinate coords)
		{
			return ConvertToLocalCoords(coords.X, coords.Y);
		}

		public Coordinate ConvertToLocalCoords(int x, int y)
		{
			return new Coordinate(x - Display.Bounds.X, y - Display.Bounds.Y);
		}

		public Picture GetScreenshotPicture()
		{
			return new Picture(GetScreenshotBitmap());
		}

		public Bitmap GetScreenshotBitmap()
		{
			Rectangle screenBounds = Display.Bounds;
			Bitmap screenshot = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(screenshot);

			graphics.CopyFromScreen(new Point(Display.Bounds.X, Display.Bounds.Y), new Point(0, 0), screenBounds.Size, CopyPixelOperation.SourceCopy);
			return screenshot;
		}

		public Color GetColor(int x, int y)
		{
			var screenshot = GetScreenshotPicture();
			return screenshot.GetColor(x, y);
		}

		public bool IsColorOnScreen(Color color, bool leftToRight = true)
		{
			return !FindColor(color, leftToRight).IsEmpty;
		}

		public Coordinate FindColor(Color color, bool leftToRight = true)
		{
			return FindColor(color.R, color.R, color.G, color.G, color.B, color.B, leftToRight = true);
		}

		public Coordinate FindColor(int r, int g, int b, bool leftToRight = true)
		{
			return FindColor(r, r, g, g, b, b, leftToRight);
		}

		public Coordinate FindColor(int rMin, int rMax, int gMin, int gMax, int bMin, int bMax, bool leftToRight = true)
		{
			Coordinate coords = new Coordinate();
			var screenshot = GetScreenshotPicture();

			for(int x = leftToRight ? 0 : screenshot.Width; leftToRight && x < screenshot.Width - 1 || !leftToRight && x > 1; x = leftToRight ? x + 1 : x - 1)
			{
				for(int y = 0; y < screenshot.Height - 1; y++)
				{
					var color = screenshot.GetColor(x, y);

					if(color.R >= rMin && color.R <= rMax && color.G >= gMin && color.G <= gMax && color.B >= bMin && color.B <= bMax)
					{
						coords = ConvertToGlobalCoords(x, y);
						break;
					}
				}

				if(coords.X != 0)
				{
					break;
				}
			}

			return coords;
		}

		public List<Coordinate> FindPictures(bool isResource, bool useExactSearch, string picturePath, Assembly assemblyToSearch = null, int colorSearchBuffer = 2, bool skipTransparentPixels = false, int matchPercentage = 90)
		{
			var screenshot = GetScreenshotPicture();
			var toSearchFor = isResource ? GetPictureFromResource(picturePath, assemblyToSearch) : new Picture(picturePath);

			var results = useExactSearch ? PictureEditor.FindExactInnerPics(screenshot, toSearchFor, colorSearchBuffer, skipTransparentPixels) :
				PictureEditor.FindFuzzyInnerPics(screenshot, toSearchFor, colorSearchBuffer, skipTransparentPixels, matchPercentage);

			for(int i = 0; i < results.Count; i++)
			{
				if(!results[i].IsEmpty)
				{
					results[i] = ConvertToGlobalCoords(results[i]);
				}
			}

			return results;
		}

		public Coordinate FindPicture(bool isResource, bool useExactSearch, string picturePath, Assembly assemblyToSearch = null, int colorSearchBuffer = 2, bool skipTransparentPixels = false, int matchPercentage = 90)
		{
			var screenshot = GetScreenshotPicture();
			var toSearchFor = isResource ? GetPictureFromResource(picturePath, assemblyToSearch) : new Picture(picturePath);

			var result = useExactSearch ? PictureEditor.FindExactInnerPic(screenshot, toSearchFor, colorSearchBuffer, skipTransparentPixels) :
				PictureEditor.FindFuzzyInnerPic(screenshot, toSearchFor, colorSearchBuffer, skipTransparentPixels, matchPercentage);

			if(!result.IsEmpty)
			{
				result = ConvertToGlobalCoords(result);
			}

			return result;
		}

		public bool IsPictureOnScreen(bool isResource, bool useExactSearch, string picturePath, Assembly assemblyToSearch = null, int colorSearchBuffer = 2, bool skipTransparentPixels = false, int matchPercentage = 90)
		{
			var screenshot = GetScreenshotPicture();
			var toSearchFor = isResource ? GetPictureFromResource(picturePath, assemblyToSearch) : new Picture(picturePath);
			var result = useExactSearch ? PictureEditor.FindExactInnerPic(screenshot, toSearchFor, colorSearchBuffer, skipTransparentPixels) :
				PictureEditor.FindFuzzyInnerPic(screenshot, toSearchFor, colorSearchBuffer, skipTransparentPixels, matchPercentage);

			return !result.IsEmpty;
		}

		private Picture GetPictureFromResource(string resourceName, Assembly assemblyToSearch)
		{
			var assembly = Assembly.GetEntryAssembly();

			if(assemblyToSearch != null)
			{
				assembly = assemblyToSearch;
			}

			string imagePath = assembly.GetManifestResourceNames().Single(fileName => fileName.EndsWith(resourceName));
			using var imageStream = assembly.GetManifestResourceStream(imagePath);

			return new Picture(new Bitmap(imageStream));
		}
	}
}
