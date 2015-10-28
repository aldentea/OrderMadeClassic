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

}
