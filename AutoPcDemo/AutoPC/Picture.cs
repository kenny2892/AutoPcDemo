using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace AutoPC
{
	public class Picture
	{
		public byte[] Pixels { get; }
		public int Width { get; }
		public int Height { get; }
		public PixelFormat PixelFormat { get; }
		private int ColorDepth { get; }
		private int RowSize { get; }

		public Picture(string filePath) : this(new Bitmap(filePath))
		{

		}

		public Picture(Bitmap photo)
		{
			if(photo is null)
			{
				throw new ArgumentNullException("The passed in aurgument \"" + nameof(photo) + "\" was null.");
			}

			using(photo)
			{
				Width = photo.Width;
				Height = photo.Height;
				PixelFormat = photo.PixelFormat;

				var pixelData = photo.LockBits(new Rectangle(0, 0, photo.Width, photo.Height), ImageLockMode.ReadWrite, photo.PixelFormat);
				ColorDepth = Bitmap.GetPixelFormatSize(photo.PixelFormat);
				RowSize = Math.Abs(pixelData.Stride);

				if(ColorDepth != 8 && ColorDepth != 24 && ColorDepth != 32)
				{
					throw new Exception("Invalid Image Format. Must be 8, 24, or 32.");
				}

				Pixels = new byte[Height * RowSize];

				for(int y = 0; y < Height; y++)
				{
					Marshal.Copy(IntPtr.Add(pixelData.Scan0, y * pixelData.Stride), Pixels, y * RowSize, RowSize);
				}

				photo.UnlockBits(pixelData);
			}
		}

		public Color GetColor(int x, int y)
		{
			int colorComponentsCount = ColorDepth / 8;
			int coordOffset = y * RowSize + x * colorComponentsCount;

			if(coordOffset > Pixels.Length - colorComponentsCount)
			{
				throw new Exception("Image Get Color Index Error");
			}

			if(ColorDepth == 32)
			{
				var a = Pixels[coordOffset + 3];
				var r = Pixels[coordOffset + 2];
				var g = Pixels[coordOffset + 1];
				var b = Pixels[coordOffset + 0];

				return Color.FromArgb(a, r, g, b);
			}

			else if(ColorDepth == 24)
			{
				var r = Pixels[coordOffset + 2];
				var g = Pixels[coordOffset + 1];
				var b = Pixels[coordOffset + 0];

				return Color.FromArgb(r, g, b);
			}

			else if(ColorDepth == 8) // r, g, b are all the same
			{
				var value = Pixels[coordOffset];

				return Color.FromArgb(value, value, value);
			}

			else
			{
				throw new Exception("Invalid Getting Image Color Depth");
			}
		}

		public void SetColor(int x, int y, Color color)
		{
			int colorComponentsCount = ColorDepth / 8;
			int coordOffset = y * RowSize + x * colorComponentsCount;

			if(coordOffset > Pixels.Length - colorComponentsCount)
			{
				throw new Exception("Image Set Color Index Error");
			}

			if(ColorDepth == 32)
			{
				Pixels[coordOffset + 3] = color.A;
				Pixels[coordOffset + 2] = color.R;
				Pixels[coordOffset + 1] = color.G;
				Pixels[coordOffset + 0] = color.B;
			}

			else if(ColorDepth == 24)
			{
				Pixels[coordOffset + 2] = color.R;
				Pixels[coordOffset + 1] = color.G;
				Pixels[coordOffset + 0] = color.B;
			}

			else if(ColorDepth == 8) // r, g, b are all the same
			{
				Pixels[coordOffset + 0] = color.B;
			}

			else
			{
				throw new Exception("Invalid Setting Image Color Depth");
			}
		}

		public bool ContainsExactPicture(Picture toSearchFor, int colorSearchBuffer = 2, bool skipTransparentPixels = false)
		{
			return PictureEditor.ContainsExactPic(this, toSearchFor, colorSearchBuffer, skipTransparentPixels);
		}

		public bool ContainsFuzzyPicture(Picture toSearchFor, int colorSearchBuffer = 2, bool skipTransparentPixels = false, int matchPercentage = 90)
		{
			return PictureEditor.ContainsFuzzyPic(this, toSearchFor, colorSearchBuffer, skipTransparentPixels, matchPercentage);
		}

		public Bitmap AsBitmap()
		{
			Bitmap blankMap = new Bitmap(Width, Height, PixelFormat);
			var photoData = blankMap.LockBits(new Rectangle(0, 0, blankMap.Width, blankMap.Height), ImageLockMode.ReadWrite, blankMap.PixelFormat);

			Marshal.Copy(Pixels, 0, photoData.Scan0, Pixels.Length);
			blankMap.UnlockBits(photoData);

			return blankMap;
		}

		public Picture SubImage(int x, int y, int width, int height)
		{
			return new Picture(AsBitmap().Clone(new Rectangle(x, y, width, height), PixelFormat));
		}

		public void SaveToFile(string filePath)
		{
			var pic = AsBitmap();
			pic.Save(filePath, ImageFormat.Png);
			pic.Dispose();
		}

		public void SaveToFile(string filePath, ImageFormat format)
		{
			var pic = AsBitmap();
			pic.Save(filePath, format);
			pic.Dispose();
		}

		public bool CompareTo(Picture other)
		{
			if(Pixels.Length != other.Pixels.Length)
			{
				return false;
			}

			for(int x = 0; x < Width; x++)
			{
				for(int y = 0; y < Height; y++)
				{
					if(GetColor(x, y) != other.GetColor(x, y))
					{
						return false;
					}
				}
			}

			return true;
		}

		public Picture Clone()
		{
			return new Picture(AsBitmap());
		}
	}
}
