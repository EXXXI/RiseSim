using Reactive.Bindings;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.Controls
{
    internal class CludeGridRowViewModel : ChildViewModelBase
    {
        public ReactivePropertySlim<CludeGridCellViewModel> Head { get; } = new();
        public ReactivePropertySlim<CludeGridCellViewModel> Body { get; } = new();
        public ReactivePropertySlim<CludeGridCellViewModel> Arm { get; } = new();
        public ReactivePropertySlim<CludeGridCellViewModel> Waist { get; } = new();
        public ReactivePropertySlim<CludeGridCellViewModel> Leg { get; } = new();
        public ReactivePropertySlim<CludeGridCellViewModel> Charm { get; } = new();
        public ReactivePropertySlim<CludeGridCellViewModel> Deco { get; } = new();

        public CludeGridCellViewModel? FindByName(string name)
        {
            var equips = new ReactivePropertySlim<CludeGridCellViewModel>[]
            {
                Head, Body, Arm, Waist, Leg, Charm, Deco
            };
            return equips.Where(p => p.Value.BaseEquip?.Name == name).FirstOrDefault()?.Value;
        }

        public CludeGridCellViewModel? FindByKind(EquipKind kind)
        {
            var equips = new ReactivePropertySlim<CludeGridCellViewModel>[]
            {
                Head, Body, Arm, Waist, Leg, Charm, Deco
            };
            return equips.Where(p => p.Value.BaseEquip?.Kind == kind).FirstOrDefault()?.Value;
        }
    }
}
