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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels
{
    internal class CludeRowViewModel : BindableBase
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

        // 除外・固定状況
        private string status;
        public string Status
        {
            get { return this.status; }
            set
            {
                this.SetProperty(ref this.status, value);
            }
        }


        public CludeRowViewModel(Clude clude)
        {
            Equipment? equip = Masters.GetEquipByName(clude.Name);
            if (equip == null)
            {
                throw new ArgumentException(clude.Name + "is not found.");
            }
            DispName = equip.DispName;
            TrueName = equip.Name;
            TrueKind = equip.Kind;
            switch (TrueKind)
            {
                case EquipKind.head:
                    DispKind = "頭：";
                    break;
                case EquipKind.body:
                    DispKind = "胴：";
                    break;
                case EquipKind.arm:
                    DispKind = "腕：";
                    break;
                case EquipKind.waist:
                    DispKind = "腰：";
                    break;
                case EquipKind.leg:
                    DispKind = "足：";
                    break;
                case EquipKind.deco:
                    DispKind = "装飾品：";
                    break;
                case EquipKind.charm:
                    DispKind = "護石：";
                    break;
                default:
                    break;
            }
            switch (clude.Kind)
            {
                case CludeKind.exclude:
                    Status = "除外中";
                    break;
                case CludeKind.include:
                    Status = "固定中";
                    break;
                default:
                    break;
            }
        }

        internal void DeleteClude()
        {
            MainViewModel.Instance.DeleteClude(TrueName);
        }
    }
}
