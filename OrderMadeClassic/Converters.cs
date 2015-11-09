using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GrandMutus.OrderMadeClassic
{

	#region ModeIsGameConverterクラス
	public class ModeIsGameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is MainWindow.Mode)
			{
				// boolを返す．
				return (MainWindow.Mode)value == MainWindow.Mode.Game;
			}
			else
			{
				throw new ArgumentException("コンバートできるのはMainWindow.Modeの値だけです．");
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
			{
				return (bool)value ? MainWindow.Mode.Game : MainWindow.Mode.Ready;
			}
			else
			{
				throw new ArgumentException("bool型の値を与えて下さい．");
			}
		}
	}
	#endregion

	// (0.0.7)nullに対応。
	// (0.0.4)
	#region CategoryQuestionConverterクラス
	public class CategoryQuestionConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is GrandMutus.Data.IntroQuestion)
			{
				var question = (GrandMutus.Data.IntroQuestion)value;
				return string.Format("[{0}] {1} / {2}", question.Category, question.Song.Title, question.Song.Artist);
			}
			else if (value == null)
			{
				return string.Empty;
			}
			else
			{
				throw new ArgumentException("IntroQuestionにのみ対応しています。");
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	#endregion


}
