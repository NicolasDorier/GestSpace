using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput;

namespace GestSpace
{
	public class KeyboardActionViewModel
	{
		public static ActionViewModel CreateSwitchWindow()
		{
			var action = new ActionViewModel()
			{
				Presenter = new MovePresenterViewModel
				(
				onEnter: () =>
				{
					InputSimulator.SimulateKeyDown(VirtualKeyCode.LWIN);
				},
				onMoveUp: () =>
				{
					InputSimulator.SimulateKeyPress(VirtualKeyCode.TAB);
				},
				onMoveDown: () =>
				{
					InputSimulator.SimulateKeyDown(VirtualKeyCode.SHIFT);
					InputSimulator.SimulateKeyPress(VirtualKeyCode.TAB);
					InputSimulator.SimulateKeyUp(VirtualKeyCode.SHIFT);
				},
				onRelease: () =>
				{
					InputSimulator.SimulateKeyUp(VirtualKeyCode.LWIN);
				})
			};
			return action;
		}

		//public int MinIncrement
		//{
		//	get;
		//	set;
		//}
	}
}
