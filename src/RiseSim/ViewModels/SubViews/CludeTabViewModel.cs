using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RiseSim.Util;
using RiseSim.ViewModels.BindableWrapper;
using RiseSim.ViewModels.Controls;
using SimModel.Model;
using SimModel.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Claims;
using System.Windows.Documents;

namespace RiseSim.ViewModels.SubViews
{
    /// <summary>
    /// 除外固定設定タブのVM
    /// </summary>
    class CludeTabViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 除外・固定画面の登録部品のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<EquipSelectRowViewModel>> EquipSelectRowVMs { get; } = new();

        /// <summary>
        /// 除外・固定画面の一覧表示の各行のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<CludeRowViewModel>> CludeRowVMs { get; } = new();

        /// <summary>
        /// 除外・固定画面の表表示の各行のVM
        /// </summary>
        public ReactivePropertySlim<ObservableCollection<CludeGridRowViewModel>> ShowingEquips { get; } = new();

        /// <summary>
        /// フィルタ用名前入力欄
        /// </summary>
        public ReactivePropertySlim<string> FilterName { get; } = new();

        /// <summary>
        /// フィルタ用設定済絞り込みフラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsCludeOnlyFilter { get; } = new(false);



        /// <summary>
        /// 除外固定をすべて解除するコマンド
        /// </summary>
        public ReactiveCommand DeleteAllCludeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// 防具を除外するコマンド
        /// </summary>
        public ReactiveCommand ExcludeAllAugmentationCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// レア9以下防具を除外するコマンド
        /// </summary>
        public ReactiveCommand ExcludeRare9Command { get; } = new ReactiveCommand();

        /// <summary>
        /// フィルタをクリアするコマンド
        /// </summary>
        public ReactiveCommand ClearFilterCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// フィルタを適用するコマンド
        /// </summary>
        public ReactiveCommand ApplyFilterCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CludeTabViewModel()
        {
            // コマンドを設定
            DeleteAllCludeCommand.Subscribe(_ => DeleteAllClude());
            ExcludeAllAugmentationCommand.Subscribe(_ => ExcludeAllAugmentation());
            ExcludeRare9Command.Subscribe(_ => ExcludeRare9());
            ClearFilterCommand.Subscribe(_ => ClearFilter());
            ApplyFilterCommand.Subscribe(_ => ApplyFilter());
        }

        private void ApplyFilter()
        {
            LoadGridData(FilterName.Value, IsCludeOnlyFilter.Value);
        }

        private void ClearFilter()
        {
            LoadGridData();
        }

        /// <summary>
        /// 除外装備設定
        /// </summary>
        /// <param name="equip">装備</param>
        internal void AddExclude(Equipment equip)
        {
            AddExclude(equip.Name, equip.DispName);
        }

        /// <summary>
        /// 除外装備設定
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void AddExclude(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 除外
            Simulator.AddExclude(trueName);

            //TODO: 削除
            // 除外固定のマスタをリロード
            // LoadCludes();

            // 表の該当部分の更新
            CludeGridCellViewModel? target = ShowingEquips.Value
                .Select(vm => vm.FindByName(trueName))
                .Where(vm => vm != null)
                .FirstOrDefault();
            target?.ChangeIncludeSilent(false);
            target?.ChangeExcludeSilent(true);

            // ログ表示
            SetStatusBar("除外登録完了：" + dispName);
        }

        /// <summary>
        /// 固定装備設定
        /// </summary>
        /// <param name="equip">装備</param>
        internal void AddInclude(Equipment equip)
        {
            AddInclude(equip.Name, equip.DispName);
        }

        /// <summary>
        /// 固定装備設定
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void AddInclude(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 固定
            Simulator.AddInclude(trueName);

            //TODO: 削除
            // 除外固定のマスタをリロード
            // LoadCludes();

            // 表の該当部分の更新
            CludeGridCellViewModel? target = ShowingEquips.Value
                .Select(vm => vm.FindByName(trueName))
                .Where(vm => vm != null)
                .FirstOrDefault();
            target?.ChangeExcludeSilent(false);
            target?.ChangeIncludeSilent(true);

            // 同じ部位の別の固定表示を解除
            EquipKind kind = target?.BaseEquip?.Kind ?? EquipKind.error;
            CludeGridCellViewModel? otherInclude = ShowingEquips.Value
                .Select(vm => vm.FindByKind(kind))
                .Where(vm => vm != null && vm.IsInclude.Value == true && vm.BaseEquip?.Name != target?.BaseEquip?.Name)
                .FirstOrDefault();
            otherInclude?.ChangeIncludeSilent(false);
            otherInclude?.ChangeExcludeSilent(false);

            // ログ表示
            SetStatusBar("固定登録完了：" + dispName);
        }

        /// <summary>
        /// 除外・固定の解除
        /// </summary>
        /// <param name="equip">装備</param>
        internal void DeleteClude(Equipment equip)
        {
            DeleteClude(equip.Name, equip.DispName);
        }

        /// <summary>
        /// 除外・固定の解除
        /// </summary>
        /// <param name="trueName">物理名</param>
        /// <param name="dispName">表示名</param>
        internal void DeleteClude(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 解除
            Simulator.DeleteClude(trueName);

            //TODO: 削除
            // 除外固定のマスタをリロード
            // LoadCludes();

            // 表の該当部分の更新
            CludeGridCellViewModel? target = ShowingEquips.Value
                .Select(vm => vm.FindByName(trueName))
                .Where(vm => vm != null)
                .FirstOrDefault();
            target?.ChangeIncludeSilent(false);
            target?.ChangeExcludeSilent(false);

            // ログ表示
            SetStatusBar("除外・固定解除完了：" + dispName);
        }

        /// <summary>
        /// 除外・固定の全解除
        /// </summary>
        private void DeleteAllClude()
        {
            // 解除
            Simulator.DeleteAllClude();

            // 除外固定のマスタをリロード
            LoadGridData();

            // ログ表示
            SetStatusBar("固定・除外の全解除完了");
        }

        /// <summary>
        /// 錬成防具を全て除外
        /// </summary>
        private void ExcludeAllAugmentation()
        {
            // 除外
            Simulator.ExcludeAllAugmentation();

            // 除外固定のマスタをリロード
            LoadGridData();

            // ログ表示
            SetStatusBar("錬成防具の全除外完了");
        }

        /// <summary>
        /// レア9以下を全て除外
        /// </summary>
        private void ExcludeRare9()
        {
            // 除外
            Simulator.ExcludeByRare(9);

            // 除外固定のマスタをリロード
            LoadGridData();

            // ログ表示
            SetStatusBar("レア9以下防具の全除外完了");
        }

        /// <summary>
        /// 装備のマスタ情報をVMにロード
        /// </summary>
        internal void LoadEquipsForClude()
        {
            // 除外固定画面用のVMの設定
            ObservableCollection<EquipSelectRowViewModel> equipList = new();
            equipList.Add(new EquipSelectRowViewModel(EquipKind.head.StrWithColon(), Masters.Heads));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.body.StrWithColon(), Masters.Bodys));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.arm.StrWithColon(), Masters.Arms));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.waist.StrWithColon(), Masters.Waists));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.leg.StrWithColon(), Masters.Legs));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.charm.StrWithColon(), Masters.Charms));
            equipList.Add(new EquipSelectRowViewModel(EquipKind.deco.StrWithColon(), Masters.Decos));
            EquipSelectRowVMs.ChangeCollection(equipList);

            LoadGridData();
        }

        /// <summary>
        /// 除外固定のマスタ情報をVMにロード
        /// </summary>
        private void LoadCludes()
        {
            LoadGridData();

            // TODO: 最終的に消える手筈
            // 除外固定画面用のVMの設定
            ObservableCollection<CludeRowViewModel> cludeList = new();
            foreach (var clude in Masters.Cludes)
            {
                cludeList.Add(new CludeRowViewModel(clude));
            }
            CludeRowVMs.ChangeCollection(cludeList);
        }

        private void LoadGridData(string filterName = "", bool cludeOnly = false)
        {
            // 表示対象
            var heads = Masters.Heads;
            var bodys = Masters.Bodys;
            var arms = Masters.Arms;
            var waists = Masters.Waists;
            var legs = Masters.Legs;
            var charms = Masters.Charms;
            var decos = Masters.Decos;

            // 名称フィルタ
            if (!string.IsNullOrWhiteSpace(filterName))
            {
                heads = heads.Where(x => x.DispName.Contains(filterName)).ToList();
                bodys = bodys.Where(x => x.DispName.Contains(filterName)).ToList();
                arms = arms.Where(x => x.DispName.Contains(filterName)).ToList();
                waists = waists.Where(x => x.DispName.Contains(filterName)).ToList();
                legs = legs.Where(x => x.DispName.Contains(filterName)).ToList();
                charms = charms.Where(x => x.DispName.Contains(filterName)).ToList();
                decos = decos.Where(x => x.DispName.Contains(filterName)).ToList();
            }

            // 設定フィルタ
            if (cludeOnly)
            {
                heads = heads.Where(x => Masters.Cludes.Where(c => c.Name == x.Name).Any()).ToList();
                bodys = bodys.Where(x => Masters.Cludes.Where(c => c.Name == x.Name).Any()).ToList();
                arms = arms.Where(x => Masters.Cludes.Where(c => c.Name == x.Name).Any()).ToList();
                waists = waists.Where(x => Masters.Cludes.Where(c => c.Name == x.Name).Any()).ToList();
                legs = legs.Where(x => Masters.Cludes.Where(c => c.Name == x.Name).Any()).ToList();
                charms = charms.Where(x => Masters.Cludes.Where(c => c.Name == x.Name).Any()).ToList();
                decos = decos.Where(x => Masters.Cludes.Where(c => c.Name == x.Name).Any()).ToList();

            }

            // 一覧用のデータ整理
            List<int> RowNos = new List<int>()
                .Union(heads.Select(equip => equip.RowNo))
                .Union(bodys.Select(equip => equip.RowNo))
                .Union(arms.Select(equip => equip.RowNo))
                .Union(waists.Select(equip => equip.RowNo))
                .Union(legs.Select(equip => equip.RowNo))
                .ToList();
            RowNos.Sort();

            ObservableCollection<CludeGridRowViewModel> rows = new();
            int charmCount = charms.Count;
            int decoCount = decos.Count;

            foreach (var rowNo in RowNos)
            {
                var head = heads.Where(equip => equip.RowNo == rowNo);
                var body = bodys.Where(equip => equip.RowNo == rowNo);
                var arm = arms.Where(equip => equip.RowNo == rowNo);
                var waist = waists.Where(equip => equip.RowNo == rowNo);
                var leg = legs.Where(equip => equip.RowNo == rowNo);
                int max = new int[] { head.Count(), body.Count(), arm.Count(), waist.Count(), leg.Count() }.Max();

                for (int i = 0; i < max; i++)
                {
                    CludeGridRowViewModel row = new();
                    row.Head.Value = new CludeGridCellViewModel(head.ElementAtOrDefault(i));
                    row.Body.Value = new CludeGridCellViewModel(body.ElementAtOrDefault(i));
                    row.Arm.Value = new CludeGridCellViewModel(arm.ElementAtOrDefault(i));
                    row.Waist.Value = new CludeGridCellViewModel(waist.ElementAtOrDefault(i));
                    row.Leg.Value = new CludeGridCellViewModel(leg.ElementAtOrDefault(i));
                    row.Charm.Value = new CludeGridCellViewModel(charms.ElementAtOrDefault(rows.Count));
                    row.Deco.Value = new CludeGridCellViewModel(decos.ElementAtOrDefault(rows.Count));
                    rows.Add(row);
                }
            }

            while (rows.Count < Math.Max(charms.Count, decos.Count))
            {
                CludeGridRowViewModel row = new();
                row.Head.Value = new CludeGridCellViewModel(null);
                row.Body.Value = new CludeGridCellViewModel(null);
                row.Arm.Value = new CludeGridCellViewModel(null);
                row.Waist.Value = new CludeGridCellViewModel(null);
                row.Leg.Value = new CludeGridCellViewModel(null);
                row.Charm.Value = new CludeGridCellViewModel(charms.ElementAtOrDefault(rows.Count));
                row.Deco.Value = new CludeGridCellViewModel(decos.ElementAtOrDefault(rows.Count));
                rows.Add(row);
            }

            ShowingEquips.ChangeCollection(rows);
        }
    }
}
