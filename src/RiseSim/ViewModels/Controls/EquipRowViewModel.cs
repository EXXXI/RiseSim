/*    RiseSim : MHRise skill simurator for Windows
 *    Copyright (C) 2022  EXXXI
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using Prism.Mvvm;
using Reactive.Bindings;
using RiseSim.ViewModels.BindableWrapper;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.Controls
{
    internal class EquipRowViewModel : BindableBase
    {
        // 表示用装備種類
        public ReactivePropertySlim<string> DispKind { get; } = new();

        // 表示用装備名
        public ReactivePropertySlim<string> DispName { get; } = new();

        // 装備説明
        public ReactivePropertySlim<string> Description { get; } = new();

        // 固定可能フラグ
        public ReactivePropertySlim<bool> CanInclude { get; } = new(true);

        // 除外可能フラグ
        public ReactivePropertySlim<bool> CanExclude { get; } = new(true);

        // 管理用装備種類
        public EquipKind TrueKind { get; set; }

        // 管理用装備名
        public string TrueName { get; set; }

        // 除外コマンド
        public ReactiveCommand ExcludeCommand { get; private set; }

        // 固定コマンド
        public ReactiveCommand IncludeCommand { get; private set; }

        // コマンドを設定
        private void SetCommand()
        {
            ExcludeCommand = CanExclude.ToReactiveCommand().WithSubscribe(() => Exclude());
            IncludeCommand = CanInclude.ToReactiveCommand().WithSubscribe(() => Include());
        }

        // コンストラクタ
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

            SetCommand();
        }

        // 装備セットを丸ごとVMのリストにして返却
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

        // 装備を除外
        internal void Exclude()
        {
            MainViewModel.Instance.AddExclude(TrueName, DispName.Value);

        }

        // 装備を固定
        internal void Include()
        {
            MainViewModel.Instance.AddInclude(TrueName, DispName.Value);
        }
    }
}
