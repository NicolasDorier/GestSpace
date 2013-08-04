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
					getValue: Observable.FromEvent<AudioVolumeNotificationData>(
											o => defaultDevice.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(o),
										    o => defaultDevice.AudioEndpointVolume.OnVolumeNotification -= new AudioEndpointVolumeNotificationDelegate(o))
										.Select(n=>n.MasterVolume)
										 .Merge(Observable.Return(defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar))
										 .Select(v=>(double)(v * 100.0)));
		}

	}
}
