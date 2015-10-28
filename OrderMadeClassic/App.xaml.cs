using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Aldentea.Wpf;

namespace GrandMutus.OrderMadeClassic
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Aldentea.Wpf.Application.Application
	{
		// 1. まずアプリケーションと抽象プロパティを実装する．

		Properties.Settings MySettings
		{
			get
			{
				return OrderMadeClassic.Properties.Settings.Default;
			}
		}
		#region ファイル履歴関連
		public override StringCollection FileHistory
		{
			get
			{
				return MySettings.FileHistory;
			}
			set
			{
				MySettings.FileHistory = value;
			}
		}

		public override byte FileHistoryCount
		{
			get
			{
				return MySettings.FileHistoryCount;
			}
			// setterがあっても問題ないよね？
		}

		public override byte FileHistoryDisplayCount
		{
			get
			{
				return MySettings.FileHistoryDisplayCount;
			}
			// setterがあっても問題ないよね？
		}
		#endregion

		public new static App Current
		{
			get
			{
				return System.Windows.Application.Current as App;
			}
		}

		#region  2. お決まりの設定．(コピペでいいかも．)
		protected App()
			: base()
		{
			this.Document = new GrandMutus.Data.MutusDocument();
		}

		void App_Exit(object sender, ExitEventArgs e)
		{
			MySettings.Save();
		}


		// (0.0.5)
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			if (MySettings.RequireUpgrade)
			{
				MySettings.Upgrade();
				MySettings.RequireUpgrade = false;
			}
		}
		#endregion

	}
}
