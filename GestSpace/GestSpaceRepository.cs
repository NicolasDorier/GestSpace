using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GestSpace
{
	public class GestSpaceRepository
	{
		[DataContract]
		public class EventData
		{
			[DataMember]
			public string Name
			{
				get;
				set;
			}

			[DataMember]
			public string Command
			{
				get;
				set;
			}
		}
		[DataContract]
		public class TileData
		{
			public TileData()
			{
				Events = new List<EventData>();
			}
			[DataMember]
			public int X
			{
				get;
				set;
			}
			[DataMember]
			public int Y
			{
				get;
				set;
			}

			[DataMember]
			public string ForcedName
			{
				get;
				set;
			}

			[DataMember]
			public string GestureTemplate
			{
				get;
				set;
			}

			[DataMember]
			public string PresenterTemplate
			{
				get;
				set;
			}
			[DataMember]
			public List<EventData> Events
			{
				get;
				set;
			}
		}
		[DataContract]
		public class GestSpaceData
		{
			public GestSpaceData()
			{
				Tiles = new List<TileData>();
			}
			[DataMember]
			public List<TileData> Tiles
			{
				get;
				set;
			}
			[DataMember]
			public int LastX
			{
				get;
				set;
			}
			[DataMember]
			public int LastY
			{
				get;
				set;
			}
		}

		public void Load(MainViewModel viewModel)
		{
			viewModel.CurrentTile = null;
			viewModel.Tiles.Clear();
			viewModel.Tiles.Add(new TileViewModel()
			{
				Presenter = PresenterViewModel.Unused
			});
			var spaceData = GetSpaceData();
			foreach(var tile in spaceData.Tiles)
			{
				var tileVm = new TileViewModel();
				tileVm.Position = new System.Windows.Point(tile.X, tile.Y);
				viewModel.Tiles.Add(tileVm);
				if(tile.ForcedName != null)
				{
					tileVm.Description = tile.ForcedName;
				}
				tileVm.SelectedGestureTemplate = viewModel.GestureTemplates
													.FirstOrDefault(g => g.Name.Equals(tile.GestureTemplate, StringComparison.InvariantCultureIgnoreCase));
				tileVm.SelectedPresenterTemplate = viewModel.PresenterTemplates
													.FirstOrDefault(g => g.Description.Equals(tile.PresenterTemplate, StringComparison.InvariantCultureIgnoreCase));
				foreach(var evt in tile.Events)
				{
					var evtVm =
						tileVm.Events
						  .FirstOrDefault(e => e.Name.Equals(evt.Name, StringComparison.InvariantCultureIgnoreCase));
					if(evtVm != null)
						evtVm.Command.Script = evt.Command;
				}
			}
			viewModel.SelectTile(new System.Windows.Point(spaceData.LastX, spaceData.LastY));
		}

		

		public void Save(MainViewModel viewModel)
		{
			GestSpaceData space = new GestSpaceData();
			foreach(var tileVm in viewModel.Tiles.Where(t => !t.IsUnused))
			{
				var tile = new TileData();
				space.Tiles.Add(tile);
				if(!tileVm.TakeSuggestedName)
				{
					tile.ForcedName = tileVm.Description;
				}
				tile.X = (int)tileVm.Position.X;
				tile.Y = (int)tileVm.Position.Y;
				if(tileVm.SelectedPresenterTemplate != null)
					tile.PresenterTemplate = tileVm.SelectedPresenterTemplate.Description;
				if(tileVm.SelectedGestureTemplate != null)
					tile.GestureTemplate = tileVm.SelectedGestureTemplate.Name;
				foreach(var evtVm in tileVm.Events)
				{
					var evt = new EventData();
					tile.Events.Add(evt);
					evt.Name = evtVm.Name;
					if(evtVm.Command != null)
						evt.Command = evtVm.Command.Script;
				}
			}
			if(viewModel.CurrentTile != null)
			{
				space.LastX = (int)viewModel.CurrentTile.Position.X;
				space.LastY = (int)viewModel.CurrentTile.Position.Y;
			}
			Save(space);
		}

		private void Save(GestSpaceData space)
		{
			var store = GetStore();
			using(var fs = store.OpenFile("spacedata", System.IO.FileMode.Create))
			{
				DataContractSerializer seria = new DataContractSerializer(typeof(GestSpaceData));
				seria.WriteObject(fs,space);
			}
		}

		private GestSpaceData GetSpaceData()
		{
			var store = GetStore();
			if(!store.FileExists("spacedata"))
				return new GestSpaceData();
			using(var stream = store.OpenFile("spacedata", System.IO.FileMode.Open))
			{
				DataContractSerializer seria = new DataContractSerializer(typeof(GestSpaceData));
				return (GestSpaceData)seria.ReadObject(stream);
			}
		}
		IsolatedStorageFile GetStore()
		{
			return IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, null, null);
		}
	}
}
