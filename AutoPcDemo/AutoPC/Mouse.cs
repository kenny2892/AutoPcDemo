using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoPC
{
	public class Mouse
	{
		// Mouse Codes: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-mouse_event?redirectedfrom=MSDN
		private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
		private const int MOUSEEVENTF_LEFTUP = 0x0004;
		private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
		private const int MOUSEEVENTF_RIGHTUP = 0x0010;
		private const int MOUSEEVENTF_WHEEL = 0x0800;

		private static bool MonitoringMovement { get; set; } = false;
		private static Thread MonitoringThread { get; set; }
		public static Coordinate LastKnownCoords { get; private set; } = new Coordinate();
		private static object Locker { get; set; } = new object();
		public static int ScrollWheelAmount { get; set; } = 120;

		public static Coordinate GetCoords()
		{
			Coordinate coordinate = new Coordinate();
			WindowsFunctions.GetCursorPos(out coordinate);
			coordinate.IsEmpty = false;

			return coordinate;
		}

		public static void LeftDown(Coordinate coords)
		{
			LeftDown(coords.X, coords.Y);
		}

		public static void LeftDown(int x, int y)
		{
			WindowsFunctions.mouse_event(MOUSEEVENTF_LEFTDOWN, (uint) x, (uint) y, 0, 0);
			Thread.Sleep(200);
		}

		public static void LeftUp(Coordinate coords)
		{
			LeftUp(coords.X, coords.Y);
		}

		public static void LeftUp(int x, int y)
		{
			WindowsFunctions.mouse_event(MOUSEEVENTF_LEFTUP, (uint) x, (uint) y, 0, 0);
			Thread.Sleep(200);
		}

		public static void LeftClick()
		{
			var currCoords = GetCoords();
			LeftClick(currCoords.X, currCoords.Y);
		}

		public static void LeftClick(Coordinate coords)
		{
			LeftClick(coords.X, coords.Y);
		}

		public static void LeftClick(int x, int y)
		{
			WindowsFunctions.mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint) x, (uint) y, 0, 0);
			Thread.Sleep(200);
		}

		public static void RightDown(Coordinate coords)
		{
			RightDown(coords.X, coords.Y);
		}

		public static void RightDown(int x, int y)
		{
			WindowsFunctions.mouse_event(MOUSEEVENTF_RIGHTDOWN, (uint) x, (uint) y, 0, 0);
			Thread.Sleep(200);
		}

		public static void RightUp(Coordinate coords)
		{
			RightUp(coords.X, coords.Y);
		}

		public static void RightUp(int x, int y)
		{
			WindowsFunctions.mouse_event(MOUSEEVENTF_RIGHTUP, (uint) x, (uint) y, 0, 0);
			Thread.Sleep(200);
		}

		public static void RightClick()
		{
			var currCoords = GetCoords();
			RightClick(currCoords.X, currCoords.Y);
		}

		public static void RightClick(Coordinate coords)
		{
			RightClick(coords.X, coords.Y);
		}

		public static void RightClick(int x, int y)
		{
			WindowsFunctions.mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint) x, (uint) y, 0, 0);
			Thread.Sleep(200);
		}

		public static void ScrollUp(int amount)
		{
			WindowsFunctions.mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint) amount, 0);
			Thread.Sleep(200);
		}

		public static void ScrollDown(int amount)
		{
			if(amount > 0)
			{
				amount *= -1;
			}

			unchecked // C# doesn't want you to implicitly convert negative numbers into Uints because technically Uints can only be positive. So when you make a negative into a Uint, it loops around to the max positive Uint and subtracts from that.
			{
				WindowsFunctions.mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint) amount, 0);
			}

			Thread.Sleep(200);
		}

		public static void ScrollUpSmall()
		{
			ScrollUp(ScrollWheelAmount * 7);
		}

		public static void ScrollUpBig()
		{
			ScrollUp(ScrollWheelAmount * 23);
		}

		public static void ScrollDownSmall()
		{
			ScrollDown(ScrollWheelAmount * 7);
		}

		public static void ScrollDownBig()
		{
			ScrollDown(ScrollWheelAmount * 23);
		}

		public static void Move(Coordinate coords)
		{
			Move(coords.X, coords.Y);
		}

		public static void Move(int x, int y)
		{
			WindowsFunctions.SetCursorPos(x, y);

			lock(Locker)
			{
				LastKnownCoords = GetCoords();
			}

			Thread.Sleep(200);
		}

		public static void MoveAndLeftClick(Coordinate coords)
		{
			MoveAndLeftClick(coords.X, coords.Y);
		}

		public static void MoveAndLeftClick(int x, int y)
		{
			Move(x, y);
			LeftClick(x, y);
		}

		public static void MoveAndRightClick(Coordinate coords)
		{
			MoveAndRightClick(coords.X, coords.Y);
		}

		public static void MoveAndRightClick(int x, int y)
		{
			Move(x, y);
			RightClick(x, y);
		}

		public static void StartMonitorMovement(Action methodToCallOnMovement, int waitInMillisecond = 1000)
		{
			lock(Locker)
			{
				LastKnownCoords = GetCoords();
			}

			MonitoringMovement = true;
			MonitoringThread = new Thread(() =>
			{
				while(MonitoringMovement)
				{
					lock(Locker)
					{
						var currCoords = GetCoords();
						if(LastKnownCoords.X != currCoords.X || LastKnownCoords.Y != currCoords.Y)
						{
							methodToCallOnMovement();
							LastKnownCoords = currCoords;
						}
					}

					Thread.Sleep(waitInMillisecond);
				}
			});

			MonitoringThread.Name = "Mouse Monitoring Thread";
			MonitoringThread.Start();
		}

		public static void StopMonitorMovement()
		{
			MonitoringMovement = false;

			if(MonitoringThread != null)
			{
				MonitoringThread.Join();
			}
		}

		public static void UpdateLastKnownCoords()
		{
			lock(Locker)
			{
				LastKnownCoords = GetCoords();
			}
		}
	}
}
