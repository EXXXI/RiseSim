using RiseSim.ViewModels;
using RiseSim.Views;
using System;
using System.Windows;
using NLog;

namespace RiseSim
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// ロガー
        /// </summary>
        static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 開始時処理
        /// MainViewModelをバインドしたMainViewを開く
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
       {
            base.OnStartup(e);

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyUnhandledExceptionHandler);

            var w = new MainView();
            var vm = new MainViewModel();

            w.DataContext = vm;
            w.Show();
        }

        /// <summary>
        /// 予期せぬエラー時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        static void MyUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            logger.Error(e, "エラーが発生しました。");
        }
    }
}
