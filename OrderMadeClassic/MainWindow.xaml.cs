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
using GrandMutus.Data;

namespace GrandMutus.OrderMadeClassic
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : BasicWindow
	{

		Properties.Settings MySettings
		{
			get
			{
				return Properties.Settings.Default;
			}
		}

		// (0.0.4)MediaOpenedイベントのハンドラを追加。
		#region *コンストラクタ(MainWindow)
		public MainWindow()
		{
			InitializeComponent();
			
			this.FileHistoryShortcutParent = this.MenuItemFileHistoryParent;

			this._songPlayer.MediaOpened += SongPlayer_MediaOpened;
		}
		#endregion

		// (0.0.5)
		private void MainWindow_Initialized(object sender, EventArgs e)
		{
			// Initializedより
			SongPlayer.Volume = MySettings.SongPlayerVolume;
		}

		// (0.0.5)
		private void MainWindow_Closed(object sender, EventArgs e)
		{
			MySettings.SongPlayerVolume = SongPlayer.Volume;
		}

		// (0.0.5)
		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
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

		// (0.0.4)CurrentGameModeの設定を追加。

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
					CurrentGameMode = GameMode.Talking;
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
			e.CanExecute = CurrentMode == Mode.Ready && this.listBoxQuestions.Items.Count > 0;
		}


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
		#endregion

		// (0.0.6)
		#region ClearList

		private void ClearList_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			listBoxQuestions.Items.Clear();
		}

		#endregion

		/// <summary>
		/// 出題した問題のカウントです。
		/// StandbyQuestionしたときに1つ増えます。
		/// </summary>
		int _questionCount = 0;


		// (0.0.4)
		#region NextQuestion
		private void NextQuestion_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (listBoxQuestions.Items.Count > _questionCount)
			{
				var question = (IntroQuestion)listBoxQuestions.Items.GetItemAt(_questionCount);
				StandbyQuestion(question);
				_questionCount += 1;
			}
			else
			{
				// 終了勧告。
				var message = "全ての問題を出題しました。準備モードに戻りますか？";
				if (MessageBox.Show(message, "セット終了", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					SongPlayer.Close();
					CurrentQuestion = null;
					CurrentMode = Mode.Ready;
					// 問題をクリアする仕様にしてみる。
					listBoxQuestions.Items.Clear();
				}
			}
		}

		private void NextQuestion_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CurrentMode == Mode.Game && CurrentGameMode == GameMode.Talking;
		}

		// (0.0.4)
		protected void StandbyQuestion(IntroQuestion question)
		{
			listBoxQuestions.SelectedItem = question;

			// 現在の曲を設定する。
			SongPlayer.Open(question.Song.FileName);
			// シークはStartQuestionコマンドで行う。
			CurrentQuestion = question;
			CurrentGameMode = GameMode.Standby;
			
		}
		#endregion

		#region GameMode列挙体
		public enum GameMode
		{
			/// <summary>
			/// 出題準備が完了した状態です。
			/// </summary>
			Standby,
			/// <summary>
			/// 出題中の状態です。
			/// </summary>
			Playing,
			/// <summary>
			/// 解答権を得て考え中の状態です。
			/// </summary>
			Thinking,
			/// <summary>
			/// 正誤判定を終了して、正解のフォローなどをしている状態です。
			/// </summary>
			Talking
		}
		#endregion

		// (0.0.4)
		#region *[dependency]CurrentGameModeプロパティ

		/// <summary>
		/// 現在のモードを取得／設定します．
		/// </summary>
		GameMode CurrentGameMode
		{
			get
			{ return (GameMode)GetValue(CurrentGameModeProperty); }
			set
			{ SetValue(CurrentGameModeProperty, value); }
		}
		public static readonly DependencyProperty CurrentGameModeProperty
			= DependencyProperty.Register(
				"CurrentGameMode", typeof(GameMode), typeof(MainWindow),
				new PropertyMetadata(GameMode.Talking));

		// 依存関係プロパティはデータバインディングとの相性がよくて便利なんだけど，
		// プロパティの変更時にもうちょっと複雑なことをしようとすると，
		// コールバックがstaticなので使いにくいんだよなぁ．
		// こうやってinternalメソッドを駆使するのは普通の使い方なのかしら？

		/// <summary>
		/// CurrentModeに応じてレイアウトを変更します．
		/// CurrentModeプロパティの変更時に呼び出されることを想定しています．
		/// </summary>
/*		internal void ChangeGameLayout()
		{
			switch (CurrentGameMode)
			{
			}
			//this.UpdateLayout();	// これが必要らしい．
		}
		*/
		#endregion



		#region 問題再生関連

		// (0.0.4)
		#region *SongPlayerプロパティ
		public HyperMutus.SongPlayer SongPlayer
		{
			get
			{
				return _songPlayer;
			}
		}
		HyperMutus.SongPlayer _songPlayer = new HyperMutus.SongPlayer();
		#endregion

		// (0.0.4)
		#region *曲ファイルオープン時(SongPlayer_MediaOpened)
		void SongPlayer_MediaOpened(object sender, EventArgs e)
		{
			if (_songPlayer.Duration.HasValue)
			{
				this.labelDuration.Content = _songPlayer.Duration.Value;
				this.sliderSeekSong.Maximum = _songPlayer.Duration.Value.TotalSeconds;
			}
		}
		#endregion

		// (0.0.7)フィールドからプロパティに昇格(？)
		#region *[dependency]CurrentQuestionプロパティ
		IntroQuestion CurrentQuestion
		{
			get
			{ return (IntroQuestion)GetValue(CurrentQuestionProperty); }
			set
			{ SetValue(CurrentQuestionProperty, value); }
		}
		public static readonly DependencyProperty CurrentQuestionProperty
			= DependencyProperty.Register(
				"CurrentQuestion", typeof(IntroQuestion), typeof(MainWindow),
				new PropertyMetadata(null));
		#endregion

		// (0.0.4)
		#region StartQuestion
		private void StartQuestion_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SongPlayer.CurrentPosition = CurrentQuestion.PlayPos;
			SongPlayer.Play();
			CurrentGameMode = GameMode.Playing;
		}

		private void StartQuestion_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CurrentGameMode == GameMode.Standby;
		}
		#endregion

		// (0.0.4)
		#region StopQuestion
		private void StopQuestion_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SongPlayer.Pause();
			CurrentGameMode = GameMode.Thinking;
		}

		private void StopQuestion_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			// とりあえず。
			e.CanExecute = CurrentMode == Mode.Game && CurrentGameMode == GameMode.Playing;
		}
		#endregion

		// (0.0.4)
		#region EndQuestion
		private void EndQuestion_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SongPlayer.Pause();
			CurrentGameMode = GameMode.Talking;
		}
		#endregion

		// (0.0.4)
		private void SwitchPlayPause_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (CurrentGameMode == GameMode.Thinking)
			{
				CurrentGameMode = GameMode.Talking;
			}
			SongPlayer.TogglePlayPause();
		}

		// (0.0.4)
		private void SeekSabi_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SongPlayer.CurrentPosition = CurrentQuestion.Song.SabiPos;
		}

		// (0.0.4)
		private void SongPlayer_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = CurrentMode == Mode.Game &&
				(CurrentGameMode == GameMode.Thinking || CurrentGameMode == GameMode.Talking) &&
				SongPlayer.CurrentState != HyperMutus.SongPlayer.State.Inactive;
		}


		#endregion


	}
}
