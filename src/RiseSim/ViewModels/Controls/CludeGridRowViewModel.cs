using Reactive.Bindings;
using SimModel.Model;
using System.Linq;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// 除外固定一覧表用の各行のVM
    /// </summary>
    internal class CludeGridRowViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 頭装備
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> Head { get; } = new();

        /// <summary>
        /// 胴装備
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> Body { get; } = new();

        /// <summary>
        /// 腕装備
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> Arm { get; } = new();

        /// <summary>
        /// 腰装備
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> Waist { get; } = new();

        /// <summary>
        /// 足装備
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> Leg { get; } = new();

        /// <summary>
        /// 護石
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> Charm { get; } = new();

        /// <summary>
        /// 装飾品
        /// </summary>
        public ReactivePropertySlim<CludeGridCellViewModel> Deco { get; } = new();

        /// <summary>
        /// 指定した名称の装備があればVMを返す
        /// </summary>
        /// <param name="name">装備名</param>
        /// <returns>指定した名称の装備があればVM、なければnull</returns>
        public CludeGridCellViewModel? FindByName(string name)
        {
            var equips = new ReactivePropertySlim<CludeGridCellViewModel>[]
            {
                Head, Body, Arm, Waist, Leg, Charm, Deco
            };
            return equips.Where(p => p.Value.BaseEquip?.Name == name).FirstOrDefault()?.Value;
        }

        /// <summary>
        /// 指定した部位のVMを返す
        /// </summary>
        /// <param name="kaind">部位</param>
        /// <returns>指定した部位のVM、なければnull</returns>
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
