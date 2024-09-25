using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;

namespace RiseSim.Util
{
    /// <summary>
    /// ReactiveProperty＜ObservableCollection＜IDisposable＞＞用の拡張メソッドクラス
    /// </summary>
    internal static class CollectionExt
    {
        /// <summary>
        /// 今までのCollectionをDisposeして新しいCollectionを使う
        /// </summary>
        /// <param name="prop">ReactiveProperty</param>
        /// <param name="value">新しいCollection</param>
        public static void ChangeCollection<T>(this ReactivePropertySlim<ObservableCollection<T>> prop, ObservableCollection<T> value)
            where T : IDisposable
        {
            ObservableCollection<T> oldCollection = prop.Value;
            if (oldCollection != null)
            {
                foreach (var item in oldCollection)
                {
                    item.Dispose();
                }
            }

            prop.Value = value;
        }
    }
}
