using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Aldentea.Wpf.Application;

namespace GrandMutus.OrderMadeClassic
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : BasicWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			
			this.FileHistoryShortcutParent = this.MenuItemFileHistoryParent;
		}

		#region *MyDocumentプロパティ
		public GrandMutus.Data.MutusDocument MyDocument
		{
			get { return (Data.MutusDocument)App.Current.Document; }
		}
		#endregion


		#region モード関連

		#region Mode列挙体
		public enum Mode
		{
			/// <summary>
			/// 試合前の準備中．
			/// </summary>
			Ready,
			/// <summary>
			/// 試合中．
			/// </summary>
			Game
		}
		#endregion


		#region *[dependency]CurrentModeプロパティ

		/// <summary>
		/// 現在のモードを取得／設定します．
		/// </summary>
		Mode CurrentMode
		{
			get
			{ return (Mode)GetValue(CurrentModeProperty); }
			set
			{ SetValue(CurrentModeProperty, value); }
		}
		public static readonly DependencyProperty CurrentModeProperty
			= DependencyProperty.Register(
				"CurrentMode", typeof(Mode), typeof(MainWindow),
				new PropertyMetadata(Mode.Ready, (d, e) => { ((MainWindow)d).ChangeLayout(); }));

		// 依存関係プロパティはデータバインディングとの相性がよくて便利なんだけど，
		// プロパティの変更時にもうちょっと複雑なことをしようとすると，
		// コールバックがstaticなので使いにくいんだよなぁ．
		// こうやってinternalメソッドを駆使するのは普通の使い方なのかしら？

		/// <summary>
		/// CurrentModeに応じてレイアウトを変更します．
		/// CurrentModeプロパティの変更時に呼び出されることを想定しています．
		/// </summary>
		internal void ChangeLayout()
		{
			switch (CurrentMode)
			{
				case Mode.Ready:
					RowQuestionPlayer.Height = new GridLength(0);
					ColumnCategories.Width = new GridLength(1, GridUnitType.Star);  // XAMLで指定する"1*"に対応．
					buttonEnter.Visibility = Visibility.Visible;
					break;
				case Mode.Game:
					RowQuestionPlayer.Height = new GridLength(200);
					ColumnCategories.Width = new GridLength(0);
					buttonEnter.Visibility = Visibility.Collapsed;
					break;
			}
			this.UpdateLayout();	// これが必要らしい．
		}

		#endregion

		#endregion


		#region EnterCategory

		private void EnterCategory_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string)
			{
				var category = (string)e.Parameter;
				foreach (var question in MyDocument.Questions.Where(q => q.Category == category))
				{
					listBoxQuestions.Items.Add(question);
				}
			}

		}

		private void EnterCategory_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Parameter is string)
			{
				e.CanExecute =  MyDocument.Questions.Categories.Contains((string)e.Parameter);
			}
			else
			{
				e.CanExecute = false;
			}
		}

		#endregion

		#region Shuffle
		private void Shuffle_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ShuffleQuestions();
		}

		private void Shuffle_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.listBoxQuestions.Items.Count > 0;
		}

		#endregion

		/// <summary>
		/// 問題リストをシャッフルします．
		/// </summary>
		private void ShuffleQuestions()
		{
			Random random = new Random();
//			try
//			{
					// あれ，ListBoxの描画の抑止ってできないんだっけ？
				for (int i = listBoxQuestions.Items.Count; i > 1; i--)
				{
					int n = random.Next(i);
					if (n < i - 1)
					{
						var item = listBoxQuestions.Items.GetItemAt(n);
						listBoxQuestions.Items.RemoveAt(n);
						listBoxQuestions.Items.Insert(i - 1, item);
					}
				}
//			}
//			finally
//			{

//			}

		}

	}
}
