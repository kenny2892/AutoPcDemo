using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPC
{
	public struct Coordinate
	{
		public int X { get; set; }
		public int Y { get; set; }
		public bool IsEmpty { get; set; }

		public Coordinate() : this(0, 0)
		{
			IsEmpty = true;
		}

		public Coordinate(int x, int y)
		{
			X = x;
			Y = y;
			IsEmpty = false;
		}
	}
}
