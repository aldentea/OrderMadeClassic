using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GrandMutus.OrderMadeClassic
{
	public static class Commands
	{
		public static RoutedCommand EnterCategoryCommand = new RoutedCommand();

		public static RoutedCommand ShuffleCommand = new RoutedCommand();

		// (0.0.4)
		public static RoutedCommand NextQuestionCommand = new RoutedCommand();

		// (0.0.4)どこかにあったはずだが、とりあえずここで実装しておく。
		public static RoutedCommand StartQuestionCommand = new RoutedCommand();

		// (0.0.4)どこかにあったはずだが、とりあえずここで実装しておく。
		public static RoutedCommand StopQuestionCommand = new RoutedCommand();
	
		// (0.0.4)どこかにあったはずだが、とりあえずここで実装しておく。
		public static RoutedCommand EndQuestionCommand = new RoutedCommand();

		// (0.0.6)もう少し一般的なところに移動するつもり。
		public static RoutedCommand ClearListCommand = new RoutedCommand();

	}
}
