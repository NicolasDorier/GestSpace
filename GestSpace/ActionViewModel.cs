using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class ActionViewModel : NotifyPropertyChangedBase
	{
		public ActionViewModel()
		{
			//MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
			//MMDevice defaultDevice =
			//  devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
			//defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 0;

			//var a = GestSpace.Interop.user32.CountVisibleWindows();
			//InputSimulator.SimulateKeyDown(VirtualKeyCode.LWIN);
			//InputSimulator.SimulateKeyDown(VirtualKeyCode.TAB);



		}
		private PresenterViewModel _Presenter;
		public PresenterViewModel Presenter
		{
			get
			{
				return _Presenter;
			}
			set
			{
				if(value != _Presenter)
				{
					_Presenter = value;
					OnPropertyChanged(() => this.Presenter);
				}
			}
		}
	}
}
