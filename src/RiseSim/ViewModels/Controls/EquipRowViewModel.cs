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
using SimModel.model;
using SimModel.service;
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

        // 管理用装備種類
        public EquipKind TrueKind { get; set; }

        // 管理用装備名
        public string TrueName { get; set; }

        // 除外コマンド
        public ReactiveCommand ExcludeCommand { get; } = new ReactiveCommand();

        // 固定コマンド
        public ReactiveCommand IncludeCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            ExcludeCommand.Subscribe(_ => Exclude());
            IncludeCommand.Subscribe(_ => Include());
        }

        public EquipRowViewModel(string dispKind, string dispName, EquipKind trueKind, string trueName)
        {
            DispKind.Value = dispKind;
            DispName.Value = dispName;
            TrueKind = trueKind;
            TrueName = trueName;

            SetCommand();
        }

        // 装備セットを丸ごとVMのリストにして返却
        static public ObservableCollection<EquipRowViewModel> SetToEquipRows(EquipSet set)
        {
            ObservableCollection<EquipRowViewModel> list = new();
            if (set != null)
            {
                list.Add(new EquipRowViewModel("頭：", set.HeadName, EquipKind.head, set.HeadName));
                list.Add(new EquipRowViewModel("胴：", set.BodyName, EquipKind.body, set.BodyName));
                list.Add(new EquipRowViewModel("腕：", set.ArmName, EquipKind.arm, set.ArmName));
                list.Add(new EquipRowViewModel("腰：", set.WaistName, EquipKind.waist, set.WaistName));
                list.Add(new EquipRowViewModel("足：", set.LegName, EquipKind.leg, set.LegName));
                list.Add(new EquipRowViewModel("護石：", set.CharmNameDisp, EquipKind.charm, set.CharmName));
                foreach (var deco in set.DecoNames)
                {
                    list.Add(new EquipRowViewModel("装飾品：", deco, EquipKind.deco, deco));
                }
            }
            return list;
        }

        internal void Exclude()
        {
            MainViewModel.Instance.AddExclude(TrueName, DispName.Value);

        }
        internal void Include()
        {
            MainViewModel.Instance.AddInclude(TrueName, DispName.Value);
        }
    }
}
