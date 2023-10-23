using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoPC
{
	public class PictureEditor
	{
		public static bool CompareColors(Color toMatch, Color toCompare, int buffer = 2)
		{
			bool red = toMatch.R - buffer <= toCompare.R && toCompare.R <= toMatch.R + buffer;
			bool green = toMatch.G - buffer <= toCompare.G && toCompare.G <= toMatch.G + buffer;
			bool blue = toMatch.B - buffer <= toCompare.B && toCompare.B <= toMatch.B + buffer;

			return red && green && blue;
		}

		public static bool ContainsExactPic(Picture toSearchIn, Picture toSearchFor, int colorSearchBuffer = 2, bool skipTransparentPixels = false)
		{
			return FindExactInnerPic(toSearchIn, toSearchFor, colorSearchBuffer, skipTransparentPixels).X > -1;
		}

		public static bool ContainsFuzzyPic(Picture toSearchIn, Picture toSearchFor, int colorSearchBuffer = 2, bool skipTransparentPixels = false, int matchPercentage = 90)
		{
			return FindFuzzyInnerPic(toSearchIn, toSearchFor, colorSearchBuffer, skipTransparentPixels, matchPercentage).X > -1;
		}

		public static Coordinate FindExactInnerPic(Picture toSearchIn, Picture toSearchFor, int colorSearchBuffer = 2, bool skipTransparentPixels = false)
		{
			var results = FindInnerImagesViaLeastCommonColor(toSearchIn, toSearchFor, (toSearchIn, toSearchFor, colorToFindCoords, threadCount, coordsPerThread) =>
			{
				List<Coordinate> searchResults = new List<Coordinate>();
				List<Thread> threadsToWatch = new List<Thread>();
				object locker = new object();
				CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

				for(int i = 0; i < threadCount && i < coordsPerThread.Count; i++)
				{
					List<Coordinate> coords = coordsPerThread[i];

					Thread thread = new Thread(() =>
					{
						foreach(var coordToCheck in coords)
						{
							// Calculate the Upper Left Coords
							int x = coordToCheck.X - colorToFindCoords.X;
							int y = coordToCheck.Y - colorToFindCoords.Y;

							if(x < 0 || y < 0)
							{
								continue;
							}

							var searchResult = ScanAreaAndCancel(toSearchIn, toSearchFor, new Coordinate() { X = x, Y = y, IsEmpty = false }, cancelTokenSource.Token, cancelTokenSource, colorSearchBuffer, skipTransparentPixels);
							if(searchResult.X > -1)
							{
								lock(locker)
								{
									searchResults.Add(searchResult);
								}
							}
						}
					});

					thread.Name = i.ToString();
					threadsToWatch.Add(thread);
					thread.Start();
				}

				foreach(var thread in threadsToWatch)
				{
					thread.Join();
				}

				cancelTokenSource.Dispose();

				return searchResults;
			});

			return results[0];
		}

		public static Coordinate FindFuzzyInnerPic(Picture toSearchIn, Picture toSearchFor, int colorSearchBuffer = 2, bool skipTransparentPixels = false, int matchPercentage = 90)
		{
			return FindFuzzyInnerPics(toSearchIn, toSearchFor, colorSearchBuffer, skipTransparentPixels, matchPercentage)[0];
		}

		public static List<Coordinate> FindExactInnerPics(Picture toSearchIn, Picture toSearchFor, int colorSearchBuffer = 2, bool skipTransparentPixels = false)
		{
			var results = FindInnerImagesViaLeastCommonColor(toSearchIn, toSearchFor, (toSearchIn, toSearchFor, colorToFindCoords, threadCount, coordsPerThread) =>
			{
				List<Coordinate> searchResults = new List<Coordinate>();
				List<Thread> threadsToWatch = new List<Thread>();
				object locker = new object();

				for(int i = 0; i < threadCount && i < coordsPerThread.Count; i++)
				{
					List<Coordinate> coords = coordsPerThread[i];

					Thread thread = new Thread(() =>
					{
						foreach(var coordToCheck in coords)
						{
							// Calculate the Upper Left Coords
							int x = coordToCheck.X - colorToFindCoords.X;
							int y = coordToCheck.Y - colorToFindCoords.Y;

							if(x < 0 || y < 0)
							{
								continue;
							}

							var searchResult = ScanArea(toSearchIn, toSearchFor, new Coordinate() { X = x, Y = y, IsEmpty = false }, colorSearchBuffer, skipTransparentPixels);
							if(searchResult.X > -1)
							{
								lock(locker)
								{
									searchResults.Add(searchResult);
								}
							}
						}
					});

					thread.Name = i.ToString();
					threadsToWatch.Add(thread);
					thread.Start();
				}

				foreach(var thread in threadsToWatch)
				{
					thread.Join();
				}

				return searchResults;
			});

			return results;
		}

		public static List<Coordinate> FindFuzzyInnerPics(Picture toSearchIn, Picture toSearchFor, int colorSearchBuffer = 2, bool skipTransparentPixels = false, int matchPercentage = 90)
		{
			int threadCount = 15;
			List<(Coordinate Coords, int IncorrectPixelCount)> searchResults = new List<(Coordinate Coords, int IncorrectPixelCount)>();
			List<Thread> threadsToWatch = new List<Thread>();
			int threadWidthRange = toSearchIn.Width / threadCount;
			object locker = new object();

			for(int i = 0; i < threadCount; i++)
			{
				int xStartInclusive = threadWidthRange * i;
				int xStopExclusive = threadWidthRange * (i + 1) < toSearchIn.Width ? threadWidthRange * (i + 1) : toSearchIn.Width;

				Thread thread = new Thread(() =>
				{
					var matches = ScanAreaFuzzy(toSearchIn, toSearchFor, xStartInclusive, xStopExclusive, colorSearchBuffer, skipTransparentPixels, matchPercentage);
					lock(locker)
					{
						searchResults.AddRange(matches);
					}
				});

				thread.Name = i.ToString();
				threadsToWatch.Add(thread);
				thread.Start();
			}

			foreach(var thread in threadsToWatch)
			{
				thread.Join();
			}

			return searchResults.Count == 0 ? new List<Coordinate>() { new Coordinate() { X = -1, Y = -1, IsEmpty = true } } :
				searchResults.OrderBy(pair => pair.IncorrectPixelCount).Select(pair => pair.Coords).ToList();
		}

		private static List<Coordinate> FindInnerImagesViaLeastCommonColor(Picture toSearchIn, Picture toSearchFor, Func<Picture, Picture, Coordinate, int, Dictionary<int, List<Coordinate>>, List<Coordinate>> searchFunction)
		{
			var colorToFind = FindLeastCommonColor(toSearchFor);

			// Find the coords that match the least common color
			List<Coordinate> coordsToCheck = new List<Coordinate>();
			for(int x = 0; x < toSearchIn.Width; x++)
			{
				for(int y = 0; y < toSearchIn.Height; y++)
				{
					if(PictureEditor.CompareColors(colorToFind.color, toSearchIn.GetColor(x, y)))
					{
						coordsToCheck.Add(new Coordinate() { X = x, Y = y, IsEmpty = false });
					}
				}
			}

			// Multithread search
			int threadCount = 15;
			Dictionary<int, List<Coordinate>> coordsPerThread = new Dictionary<int, List<Coordinate>>();

			// Divy up the coords to amongst the threads
			for(int i = 0; i < coordsToCheck.Count; i++)
			{
				if(!coordsPerThread.ContainsKey(i % threadCount))
				{
					List<Coordinate> coords = new List<Coordinate>();
					coords.Add(coordsToCheck[i]);

					coordsPerThread.Add(i, coords);
				}

				else
				{
					List<Coordinate> coords = coordsPerThread[i % threadCount];
					coords.Add(coordsToCheck[i]);
				}
			}

			List<Coordinate> results = searchFunction(toSearchIn, toSearchFor, colorToFind.coords, threadCount, coordsPerThread);
			return results.Any(result => result.X > -1) ? results.Where(result => result.X > -1).OrderBy(result => result.X).ToList() : new List<Coordinate>() { new Coordinate() { X = -1, Y = -1, IsEmpty = true } };
		}

		private static (Coordinate coords, Color color) FindLeastCommonColor(Picture pic)
		{
			// Pair Coords with Color
			List<(Coordinate Coords, Color Color)> coordColors = new List<(Coordinate Coords, Color Color)>();
			for(int x = 0; x < pic.Width; x++)
			{
				for(int y = 0; y < pic.Height; y++)
				{
					Color color = pic.GetColor(x, y);

					if(color.A > 0)
					{
						coordColors.Add((new Coordinate() { X = x, Y = y, IsEmpty = false }, color));
					}
				}
			}

			// Find the least common
			int smallestAmount = coordColors.GroupBy(pair => pair.Color).Min(group => group.Count());
			return coordColors.GroupBy(pair => pair.Color).Where(group => group.Count() == smallestAmount).Select(group => group.First()).First();
		}

		private static Coordinate ScanAreaAndCancel(Picture toSearchIn, Picture toSearchFor, Coordinate upperLeftCoords, CancellationToken cancelToken, CancellationTokenSource cancelTokenSource, int colorSearchBuffer, bool skipTransparentPixels)
		{
			// Check each of those pixel locations for a match
			for(int x = 0; x < toSearchFor.Width && upperLeftCoords.X + x < toSearchIn.Width; x++)
			{
				for(int y = 0; y < toSearchFor.Height && upperLeftCoords.Y + y < toSearchIn.Height; y++)
				{
					var toSearchInColor = toSearchIn.GetColor(upperLeftCoords.X + x, upperLeftCoords.Y + y);
					var toSearchForColor = toSearchFor.GetColor(x, y);

					if(skipTransparentPixels && (toSearchInColor.A == 0 || toSearchForColor.A == 0))
					{
						continue;
					}

					else if(cancelToken.IsCancellationRequested || !CompareColors(toSearchInColor, toSearchForColor, colorSearchBuffer))
					{
						return new Coordinate() { X = -1, Y = -1, IsEmpty = true };
					}
				}
			}

			cancelTokenSource.Cancel();
			return upperLeftCoords;
		}

		private static Coordinate ScanArea(Picture toSearchIn, Picture toSearchFor, Coordinate upperLeftCoords, int colorSearchBuffer, bool skipTransparentPixels)
		{
			// Check each of those pixel locations for a match
			for(int x = 0; x < toSearchFor.Width; x++)
			{
				for(int y = 0; y < toSearchFor.Height; y++)
				{
					var toSearchInColor = toSearchIn.GetColor(upperLeftCoords.X + x, upperLeftCoords.Y + y);
					var toSearchForColor = toSearchFor.GetColor(x, y);

					if(skipTransparentPixels && (toSearchInColor.A == 0 || toSearchForColor.A == 0))
					{
						continue;
					}

					else if(!CompareColors(toSearchInColor, toSearchForColor, colorSearchBuffer))
					{
						return new Coordinate() { X = -1, Y = -1, IsEmpty = true };
					}
				}
			}

			return upperLeftCoords;
		}

		private static List<(Coordinate Coords, int IncorrectPixelCount)> ScanAreaFuzzy(Picture toSearchIn, Picture toSearchFor, int xStartInclusive, int xStopExclusive, int colorSearchBuffer, bool skipTransparentPixels, int matchPercentage)
		{
			List<(Coordinate Coords, int IncorrectPixelCount)> matches = new List<(Coordinate Coords, int IncorrectPixelCount)>();
			int totalPixelCount = toSearchFor.Width * toSearchFor.Height;
			int minCorrectPixelCount = (int) ((double) totalPixelCount * ((double) matchPercentage / 100.0));
			int maxIncorrectPixelCount = totalPixelCount - minCorrectPixelCount;

			for(int searchInX = xStartInclusive; searchInX < xStopExclusive; searchInX++)
			{
				for(int searchInY = 0; searchInY < toSearchIn.Height; searchInY++)
				{
					int incorrectPixelCount = 0;
					bool moveToNextPixel = false;

					// Check if the Inner Image is a match from here
					for(int searchForX = 0; searchForX < toSearchFor.Width && !moveToNextPixel; searchForX++)
					{
						for(int searchForY = 0; searchForY < toSearchFor.Height && !moveToNextPixel; searchForY++)
						{
							if(searchInX + searchForX >= toSearchIn.Width || searchInY + searchForY >= toSearchIn.Height)
							{
								moveToNextPixel = true;
								break;
							}

							var toSearchInColor = toSearchIn.GetColor(searchInX + searchForX, searchInY + searchForY);
							var toSearchForColor = toSearchFor.GetColor(searchForX, searchForY);

							if(skipTransparentPixels && (toSearchInColor.A == 0 || toSearchForColor.A == 0))
							{
								continue;
							}

							else if(!CompareColors(toSearchForColor, toSearchInColor, colorSearchBuffer))
							{
								incorrectPixelCount++;
								moveToNextPixel = incorrectPixelCount > maxIncorrectPixelCount;
							}
						}
					}

					// Made it to the end without going over the maxIncorrectPixelCount
					if(!moveToNextPixel)
					{
						matches.Add((new Coordinate() { X = searchInX, Y = searchInY, IsEmpty = false }, incorrectPixelCount));
					}
				}
			}

			return matches;
		}

		public static Picture ConvertToBlackAndWhite(Picture pic, int threshold = 150)
		{
			Picture blackAndWhitePic = pic.Clone();

			for(int y = 0; y < blackAndWhitePic.Height; y++)
			{
				for(int x = 0; x < blackAndWhitePic.Width; x++)
				{
					Color pixelColor = blackAndWhitePic.GetColor(x, y);
					int pixelValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;

					if(pixelValue >= threshold)
					{
						blackAndWhitePic.SetColor(x, y, Color.White);
					}

					else
					{
						blackAndWhitePic.SetColor(x, y, Color.Black);
					}
				}
			}

			return blackAndWhitePic;
		}
	}
}
