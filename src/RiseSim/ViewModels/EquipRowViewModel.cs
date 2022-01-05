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
using SimModel.model;
using SimModel.service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels
{
    internal class EquipRowViewModel : BindableBase
    {
        // 表示用装備種類
        private string dispKind;
        public string DispKind
        {
            get { return this.dispKind; }
            set
            {
                this.SetProperty(ref this.dispKind, value);
            }
        }

        // 表示用装備名
        private string dispName;
        public string DispName
        {
            get { return this.dispName; }
            set
            {
                this.SetProperty(ref this.dispName, value);
            }
        }

        // 管理用装備種類
        private EquipKind trueKind;
        public EquipKind TrueKind
        {
            get { return this.trueKind; }
            set
            {
                this.SetProperty(ref this.trueKind, value);
            }
        }

        // 管理用装備名
        private string trueName;
        public string TrueName
        {
            get { return this.trueName; }
            set
            {
                this.SetProperty(ref this.trueName, value);
            }
        }

        public EquipRowViewModel(string dispKind, string dispName, EquipKind trueKind, string trueName)
        {
            DispKind = dispKind;
            DispName = dispName;
            TrueKind = trueKind;
            TrueName = trueName;
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
            MainViewModel.Instance.AddExclude(TrueName, DispName);

        }
        internal void Include()
        {
            MainViewModel.Instance.AddInclude(TrueName, DispName);
        }
    }
}
