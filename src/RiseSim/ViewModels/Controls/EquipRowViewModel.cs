using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RiseSim.ViewModels.BindableWrapper;
using SimModel.Model;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// 装備表示部品
    /// </summary>
    internal class EquipRowViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 表示用装備種類
        /// </summary>
        public ReactivePropertySlim<string> DispKind { get; } = new();

        /// <summary>
        /// 表示用装備名
        /// </summary>
        public ReactivePropertySlim<string> DispName { get; } = new();

        /// <summary>
        /// 装備説明
        /// </summary>
        public ReactivePropertySlim<string> Description { get; } = new();

        /// <summary>
        /// 固定可能フラグ
        /// </summary>
        public ReactivePropertySlim<bool> CanInclude { get; } = new(true);

        /// <summary>
        /// 除外可能フラグ
        /// </summary>
        public ReactivePropertySlim<bool> CanExclude { get; } = new(true);

        /// <summary>
        /// 管理用装備種類
        /// </summary>
        public EquipKind TrueKind { get; set; }

        /// <summary>
        /// 管理用装備名
        /// </summary>
        public string TrueName { get; set; }

        /// <summary>
        /// 除外コマンド
        /// </summary>
        public ReactiveCommand ExcludeCommand { get; private set; }

        /// <summary>
        /// 固定コマンド
        /// </summary>
        public ReactiveCommand IncludeCommand { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equip">装備</param>
        public EquipRowViewModel(BindableEquipment equip)
        {
            DispName.Value = equip.DispName;
            TrueKind = equip.Kind;
            if (TrueKind.Equals(EquipKind.deco))
            {
                CanInclude.Value = false;
            }
            if (TrueKind.Equals(EquipKind.gskill))
            {
                CanInclude.Value = false;
                CanExclude.Value = false;
            }
            TrueName = equip.Name;
            Description.Value = equip.Description;
            DispKind.Value = TrueKind.StrWithColon();

            // コマンドを設定
            ExcludeCommand = CanExclude.ToReactiveCommand().WithSubscribe(() => Exclude()).AddTo(Disposable);
            IncludeCommand = CanInclude.ToReactiveCommand().WithSubscribe(() => Include()).AddTo(Disposable);
        }

        /// <summary>
        /// 装備セットを丸ごとVMのリストにして返却
        /// </summary>
        /// <param name="set">装備セット</param>
        /// <returns>装備表示部品VMのリスト</returns>
        static public ObservableCollection<EquipRowViewModel> SetToEquipRows(BindableEquipSet set)
        {
            ObservableCollection<EquipRowViewModel> list = new();
            if (set != null)
            {
                list.Add(new EquipRowViewModel(set.Head));
                list.Add(new EquipRowViewModel(set.Body));
                list.Add(new EquipRowViewModel(set.Arm));
                list.Add(new EquipRowViewModel(set.Waist));
                list.Add(new EquipRowViewModel(set.Leg));
                list.Add(new EquipRowViewModel(set.Charm));
                foreach (var deco in set.Decos)
                {
                    list.Add(new EquipRowViewModel(deco));
                }
                foreach (var gskill in set.GenericSkills)
                {
                    list.Add(new EquipRowViewModel(gskill));
                }
            }
            return list;
        }

        /// <summary>
        /// 装備を除外
        /// </summary>
        private void Exclude()
        {
            CludeTabVM.AddExclude(TrueName, DispName.Value);

        }

        /// <summary>
        /// 装備を固定
        /// </summary>
        private void Include()
        {
            CludeTabVM.AddInclude(TrueName, DispName.Value);
        }
    }
}
