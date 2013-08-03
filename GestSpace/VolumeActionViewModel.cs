using CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class VolumeActionViewModel : ActionViewModel
	{
		public VolumeActionViewModel()
		{
			MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
			MMDevice defaultDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

			Presenter =
				new ValuePresenterViewModel(
					minValue: 0,
					maxValue: 100,
					setValue: (v) => defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float)(v / 100.0),
					getValue: () => (double)defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0);
		}
	}
}
