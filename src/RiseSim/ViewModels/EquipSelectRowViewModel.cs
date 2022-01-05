﻿/*    RiseSim : MHRise skill simurator for Windows
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels
{
    internal class EquipSelectRowViewModel : BindableBase
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

        // 装備一覧
        private List<Equipment> equips;
        public List<Equipment> Equips
        {
            get { return this.equips; }
            set
            {
                this.SetProperty(ref this.equips, value);
            }
        }

        private Equipment selectedEquip;

        public Equipment SelectedEquip
        {
            get { return this.selectedEquip; }
            set
            {
                this.SetProperty(ref this.selectedEquip, value);
            }
        }

        public EquipSelectRowViewModel(string dispKind, List<Equipment> equips)
        {
            DispKind = dispKind;
            Equips = equips;
        }

        internal void Exclude()
        {
            if (SelectedEquip != null)
            {
                MainViewModel.Instance.AddExclude(SelectedEquip.Name, SelectedEquip.DispName);
            }
        }
        internal void Include()
        {
            if (SelectedEquip != null)
            {
                MainViewModel.Instance.AddInclude(SelectedEquip.Name, SelectedEquip.DispName);
            }
        }
    }
}
