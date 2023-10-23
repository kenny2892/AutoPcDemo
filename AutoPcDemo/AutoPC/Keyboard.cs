using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoPC
{
	public class Keyboard
	{
		public static void Type(string text, bool ctrl = false, bool alt = false, bool shift = false, bool specialKey = false, int sleepTime = 500)
		{
			string toUse = text.Replace("+", "{+}").Replace("^", "{^}").Replace("%", "{%}").Replace("~", "{~}");

			if(specialKey)
			{
				toUse = "{" + toUse.ToUpper() + "}";
			}

			if(ctrl)
			{
				toUse = $"^({toUse.ToLower()})";
			}

			if(alt)
			{
				toUse = $"%({toUse.ToLower()})";
			}

			if(shift)
			{
				toUse = $"+({toUse.ToLower()})";
			}

			SendKeys.SendWait(toUse);
			Thread.Sleep(sleepTime);
		}

		public static void Type(SpecialKeys specialKey, bool ctrl = false, bool alt = false, bool shift = false, int sleepTime = 500)
		{
			Type(specialKey.ToString(), ctrl, alt, shift, true, sleepTime);
		}
	}
}
