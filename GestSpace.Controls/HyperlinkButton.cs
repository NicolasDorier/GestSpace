using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace GestSpace.Controls
{
	public class HyperlinkButton : Control
	{
		class HyperlinkCommand : ICommand, IDisposable
		{
			ICommand _Inner;
			HyperlinkButton _Button;
			public HyperlinkCommand(HyperlinkButton button, ICommand inner)
			{
				_Inner = inner;
				_Button = button;
				_Inner.CanExecuteChanged += _Inner_CanExecuteChanged;
			}

			void _Inner_CanExecuteChanged(object sender, EventArgs e)
			{
				OnCanExecuteChanged();
			}

			public void OnCanExecuteChanged()
			{
				if(CanExecuteChanged != null)
					CanExecuteChanged(this, EventArgs.Empty);
			}
			#region ICommand Members

			public bool CanExecute(object parameter)
			{
				return _Inner.CanExecute(parameter);
			}

			public event EventHandler CanExecuteChanged;

			public void Execute(object parameter)
			{
				if(_Button.AskConfirmation)
				{
					var result = MessageBox.Show("Are you sure to continue ?", "Confirmation", MessageBoxButton.OKCancel);
					if(result == MessageBoxResult.OK)
						_Inner.Execute(parameter);
				}
				else
					_Inner.Execute(parameter);
			}



			public void Dispose()
			{
				_Inner.CanExecuteChanged -= _Inner_CanExecuteChanged;
			}

			#endregion
		}
		static HyperlinkButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(HyperlinkButton), new FrameworkPropertyMetadata(typeof(HyperlinkButton)));
		}

		public HyperlinkButton()
		{
			this.KeyDown += new KeyEventHandler(HyperlinkButton_KeyDown);
			this.Unloaded += HyperlinkButton_Unloaded;
			this.IsTabStop = false;
		}

		void HyperlinkButton_Unloaded(object sender, RoutedEventArgs e)
		{
			DisposeCommand();
		}

		void HyperlinkButton_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Enter)
			{
				link_Click(null, null);
				e.Handled = true;
			}
		}



		public bool AskConfirmation
		{
			get
			{
				return (bool)GetValue(AskConfirmationProperty);
			}
			set
			{
				SetValue(AskConfirmationProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for AskConfirmation.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AskConfirmationProperty =
			DependencyProperty.Register("AskConfirmation", typeof(bool), typeof(HyperlinkButton), new PropertyMetadata(false));



		public string GoTo
		{
			get
			{
				return (string)GetValue(GoToProperty);
			}
			set
			{
				SetValue(GoToProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for GoTo.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty GoToProperty =
			DependencyProperty.Register("GoTo", typeof(string), typeof(HyperlinkButton), new UIPropertyMetadata(null));



		public ICommand Command
		{
			get
			{
				return (ICommand)GetValue(CommandProperty);
			}
			set
			{
				SetValue(CommandProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.Register("Command", typeof(ICommand), typeof(HyperlinkButton), new UIPropertyMetadata(OnPropertyChanged));


		public string Text
		{
			get
			{
				return (string)GetValue(TextProperty);
			}
			set
			{
				SetValue(TextProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(HyperlinkButton), new UIPropertyMetadata(OnPropertyChanged));

		static void OnPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			HyperlinkButton sender = (HyperlinkButton)source;
			sender.RefreshLink();
		}

		public event RoutedEventHandler Click;

		Hyperlink link;
		Run run;
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			link = (Hyperlink)this.GetTemplateChild("link");
			run = (Run)this.GetTemplateChild("run");
			RefreshLink();
		}

		private void RefreshLink()
		{
			if(run != null)
			{
				run.Text = Text;
				DisposeCommand();
				if(Command != null)
					link.Command = new HyperlinkCommand(this, Command);
				link.Click -= link_Click;
				link.Click += link_Click;
			}
		}

		private void DisposeCommand()
		{
			if(link != null)
			{
				var command = link.Command as HyperlinkCommand;
				if(command != null)
				{
					command.Dispose();
					link.Command = null;
				}
			}
		}


		void link_Click(object sender, RoutedEventArgs e)
		{
			if(link != null)
				if(Click != null)
					Click(this, e);
				else if(GoTo != null)
					Process.Start(GoTo);

		}
	}
}
