using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace GestSpace
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			//RegisterDataTemplates();
			base.OnStartup(e);
		}
		class DataTemplateType
		{
			public Type Type
			{
				get;
				set;
			}
			public Match Match
			{
				get;
				set;
			}
		}
		private void RegisterDataTemplates()
		{
			var views = typeof(App).Assembly
				.GetTypes()
				.Select(t => new DataTemplateType()
				{
					Match = Regex.Match(t.Name, "^(.*)View$"),
					Type = t
				})
				.Where(o => o.Match.Success)
				.ToDictionary(o => o.Match.Groups[1].Value);

			var viewModels = typeof(App).Assembly
				.GetTypes()
				.Select(t => new DataTemplateType()
				{
					Match = Regex.Match(t.Name, "^(.*)ViewModel$"),
					Type = t
				})
				.Where(o => o.Match.Success)
				.ToDictionary(o => o.Match.Groups[1].Value);

			foreach(var vm in viewModels)
			{
				DataTemplateType view = null;
				if(views.TryGetValue(vm.Key, out view))
				{
					AddDataTemplate(view.Type, vm.Value.Type);
				}
			}
		}

		private void AddDataTemplate(Type viewType, Type viewModelType)
		{
			var dataTemplate = new DataTemplate(viewModelType);
			dataTemplate.VisualTree = new FrameworkElementFactory(viewType);
			dataTemplate.Seal();
			App.Current.Resources.Add(new DataTemplateKey(viewModelType), dataTemplate);
		}
	}
}
