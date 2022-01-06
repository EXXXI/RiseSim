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
using System.Windows.Controls;

namespace RiseSim.ViewModels
{
    internal class MainViewModel : BindableBase
    {
        // MainViewインスタンス：子VMからの参照用
        static public MainViewModel Instance { get; set; }

        // シミュ本体
        public Simulator Simulator { get; set; }

        // ログ用StringBuilderインスタンス
        private StringBuilder LogSb { get; } = new StringBuilder();

        // TODO: 外部ファイル化するべきか？
        // 定数
        // スキル選択部品の個数
        private const int SkillSelectorCount = 15;
        // 護石のスキル個数
        private const int CharmMaxSkillCount = 2;
        // スロットの最大の大きさ
        private const int MaxSlotSize = 3;
        // デフォルトの頑張り度
        private const string DefaultLimit = "100";
        // TODO: 他のファイルでも利用しているので一元化したい
        // スキル未選択時の表示
        private const string NoSkillName = "スキル選択";


        // シミュ画面のスキル選択部品のVM
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> SkillSelectorVMs { get; } = new();

        // シミュ画面の検索結果一覧
        public ReactivePropertySlim<ObservableCollection<EquipSet>> SearchResult { get; } = new();

        // シミュ画面の検索結果の選択行
        public ReactivePropertySlim<EquipSet> DetailSet { get; } = new();

        // シミュ画面の装備詳細の各行のVM
        public ReactivePropertySlim<ObservableCollection<EquipRowViewModel>> EquipRowVMs { get; } = new();

        // シミュ画面の武器スロ指定
        public ReactivePropertySlim<string> WeaponSlots { get; } = new();

        // シミュ画面の頑張り度(検索件数)
        public ReactivePropertySlim<string> Limit { get; } = new();

        // ビジー判定
        public ReactivePropertySlim<bool> IsFree { get; } = new(true);

        // 除外・固定画面の登録部品のVM
        public ReactivePropertySlim<ObservableCollection<EquipSelectRowViewModel>> EquipSelectRowVMs { get; } = new();

        // 除外・固定画面の一覧表示の各行のVM
        public ReactivePropertySlim<ObservableCollection<CludeRowViewModel>> CludeRowVMs { get; } = new();

        // 護石画面のスキル選択部品のVM
        public ReactivePropertySlim<ObservableCollection<SkillSelectorViewModel>> CharmSkillSelectorVMs { get; } = new();

        // 護石画面の一覧用部品のVM
        public ReactivePropertySlim<ObservableCollection<CharmRowViewModel>> CharmRowVMs { get; } = new();

        // 護石画面のスロット指定
        public ReactivePropertySlim<string> CharmWeaponSlots { get; } = new();

        // マイセット一覧
        public ReactivePropertySlim<ObservableCollection<EquipSet>> MySetList { get; } = new();

        // マイセットの選択行データ
        public ReactivePropertySlim<EquipSet> MyDetailSet { get; } = new();

        // マイセット画面の装備詳細の各行のVM
        public ReactivePropertySlim<ObservableCollection<EquipRowViewModel>> MyEquipRowVMs { get; } = new();

        // ライセンス画面の内容
        public ReactivePropertySlim<string> License { get; } = new();

        // ライセンス画面の雑な要約
        public ReactivePropertySlim<string> WhatIsLicense { get; } = new();

        // スロット選択の選択肢
        public ReactivePropertySlim<ObservableCollection<string>> SlotMaster { get; } = new();

        // ログ表示
        public ReactivePropertySlim<string> LogBoxText { get; } = new();


        // コマンド
        // 検索
        public AsyncReactiveCommand SearchCommand { get; private set; } 
        public AsyncReactiveCommand SearchMoreCommand { get; private set; } 
        public AsyncReactiveCommand SearchExtraSkillCommand { get; private set; } 
        public ReactiveCommand AddCharmCommand { get; } = new ReactiveCommand();
        public ReactiveCommand AddMySetCommand { get; } = new ReactiveCommand();
        public ReactiveCommand DeleteMySetCommand { get; } = new ReactiveCommand();
        public ReactiveCommand InputMySetConditionCommand { get; } = new ReactiveCommand();

        // コマンドを設定
        private void SetCommand()
        {
            SearchCommand = IsFree.ToAsyncReactiveCommand().WithSubscribe(async () => await Search());
            SearchMoreCommand = IsFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchMore());
            SearchExtraSkillCommand = IsFree.ToAsyncReactiveCommand().WithSubscribe(async () => await SearchExtraSkill());
            AddCharmCommand.Subscribe(_ => AddCharm());
            AddMySetCommand.Subscribe(_ => AddMySet());
            DeleteMySetCommand.Subscribe(_ => DeleteMySet());
            InputMySetConditionCommand.Subscribe(_ => InputMySetCondition());
        }



        // コンストラクタ：起動時処理
        public MainViewModel()
        {
            // 子VMからの参照用にstaticにインスタンスを登録
            Instance = this;

            // シミュ本体のインスタンス化
            Simulator = new Simulator();
            Simulator.LoadData();

            // シミュ画面のスキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> selectorVMs = new();
            for (int i = 0; i < SkillSelectorCount; i++)
            {
                selectorVMs.Add(new SkillSelectorViewModel());
            }
            SkillSelectorVMs.Value = selectorVMs;

            // 護石画面のスキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> charmSelectorVMs = new();
            for (int i = 0; i < CharmMaxSkillCount; i++)
            {
                charmSelectorVMs.Add(new SkillSelectorViewModel());
            }
            CharmSkillSelectorVMs.Value = charmSelectorVMs;

            // シミュ画面の検索結果と装備詳細を紐づけ
            DetailSet.Subscribe(set => EquipRowVMs.Value = EquipRowViewModel.SetToEquipRows(set));

            // マイセット画面の一覧と装備詳細を紐づけ
            MyDetailSet.Subscribe(set => MyEquipRowVMs.Value = EquipRowViewModel.SetToEquipRows(set));

            // スロットの選択肢を生成し、シミュ画面と護石画面に反映
            ObservableCollection<string> slots = new();
            for (int i = 0; i <= MaxSlotSize; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k <= j; k++)
                    {
                        slots.Add(string.Join('-', i, j, k));
                    }
                }
            }
            SlotMaster.Value = slots;
            WeaponSlots.Value = "0-0-0";
            CharmWeaponSlots.Value = "0-0-0";

            // 頑張り度を設定
            Limit.Value = DefaultLimit;

            // TODO:外部ファイル化するべきか？
            // ライセンス表示
            StringBuilder sb = new();
            sb.Append("■このシミュのライセンス\n");
            sb.Append("GNU General Public License v3.0\n");
            sb.Append('\n');
            sb.Append("■使わせていただいたOSS(+必要であればライセンス)\n");
            sb.Append("・GLPK for C#/CLI\n");
            sb.Append("プロジェクト：http://glpk-cli.sourceforge.net/\n");
            sb.Append('\n');
            sb.Append("・GlpkWrapperCS(一部改変)\n");
            sb.Append("プロジェクト：https://github.com/YSRKEN/GlpkWrapperCS\n");
            sb.Append('\n');
            sb.Append("・CSV\n");
            sb.Append("プロジェクト：https://github.com/stevehansen/csv/\n");
            sb.Append("ライセンス：https://raw.githubusercontent.com/stevehansen/csv/master/LICENSE\n");
            sb.Append('\n');
            sb.Append("・Prism.Wpf\n");
            sb.Append("プロジェクト：https://github.com/PrismLibrary/Prism\n");
            sb.Append("ライセンス：https://www.nuget.org/packages/Prism.Wpf/8.1.97/license\n");
            sb.Append('\n');
            sb.Append("■スペシャルサンクス\n");
            sb.Append("・5chモンハン板シミュスレの方々\n");
            sb.Append("特にVer.13の >> 480様の以下論文を大いに参考にしました\n");
            sb.Append("https://github.com/13-480/lp-doc\n");
            sb.Append('\n');
            sb.Append("・先人のシミュ作成者様\n");
            sb.Append("特に頑シミュ様のUIに大きく影響を受けています\n");
            License.Value = sb.ToString();

            // TODO:外部ファイル化するべきか？
            // ライセンスの雑な説明を表示
            StringBuilder sbw = new();
            sbw.Append("■←ライセンスって何？\n");
            sbw.Append("普通にスキルシミュとして使う分には気にしないでOK\n");
            sbw.Append("        \n");
            sbw.Append("■いや、怖いからちゃんと説明して？\n");
            sbw.Append("こういう使い方までならOKだよ、ってのを定める取り決め\n");
            sbw.Append("今回のは大体こんな感じ\n");
            sbw.Append("・シミュとして使う分には好きに使ってOK\n");
            sbw.Append("・このシミュのせいで何か起きても開発者は責任取らんよ\n");
            sbw.Append("・改変や再配布するときはよく調べてルールに従ってね\n");
            WhatIsLicense.Value = sbw.ToString();

            // 各コマンドを設定
            SetCommand();

            // マスタファイル読み込み
            LoadMasters();
        }

        // 検索
        async internal Task Search()
        {
            // スキル条件を整理
            List<Skill> skills = new();
            foreach (var selectorVM in SkillSelectorVMs.Value)
            {
                if(selectorVM.SkillName.Value != NoSkillName)
                {
                    skills.Add(new Skill(selectorVM.SkillName.Value, selectorVM.SkillLevel.Value));
                }
            }

            // 武器スロ条件を整理
            string[] splited = WeaponSlots.Value.Split('-');
            int weaponSlot1 = int.Parse(splited[0]);
            int weaponSlot2 = int.Parse(splited[1]);
            int weaponSlot3 = int.Parse(splited[2]);

            // 頑張り度を整理
            int searchLimit;
            try
            {
                searchLimit = int.Parse(Limit.Value);
            }
            catch (Exception)
            {
                // 数値以外が入力されていたら初期値を利用
                searchLimit = int.Parse(DefaultLimit);
            }

            // 開始ログ表示
            LogSb.Clear();
            LogSb.Append("■検索開始：\n");
            LogSb.Append("武器スロ");
            LogSb.Append(weaponSlot1);
            LogSb.Append('-');
            LogSb.Append(weaponSlot2);
            LogSb.Append('-');
            LogSb.Append(weaponSlot3);
            LogSb.Append('\n');
            foreach (Skill skill in skills)
            {
                LogSb.Append(skill.Name);
                LogSb.Append(" Lv");
                LogSb.Append(skill.Level);
                LogSb.Append('\n');
            }
            LogBoxText.Value = LogSb.ToString();

            // ビジーフラグ
            IsFree.Value = false;

            // 検索
            List<EquipSet> result = await Task.Run(() => Simulator.Search(skills, weaponSlot1, weaponSlot2, weaponSlot3, searchLimit));
            SearchResult.Value = new ObservableCollection<EquipSet>(result);

            // ビジーフラグ解除
            IsFree.Value = true;

            // 完了ログ表示
            LogSb.Append("■検索完了：");
            LogSb.Append(SearchResult.Value.Count);
            LogSb.Append("件\n");
            LogBoxText.Value = LogSb.ToString();
        }

        // もっと検索
        async internal Task SearchMore()
        {
            // 頑張り度を整理
            int searchLimit;
            try
            {
                searchLimit = int.Parse(Limit.Value);
            }
            catch (Exception)
            {
                // 数値以外が入力されていたら初期値を利用
                searchLimit = int.Parse(DefaultLimit);
            }

            // 開始ログ表示
            LogSb.Clear();
            LogSb.Append("■もっと検索開始：\n");
            LogBoxText.Value = LogSb.ToString();

            // ビジーフラグ
            IsFree.Value = false;

            // もっと検索
            List<EquipSet> result = await Task.Run(() => Simulator.SearchMore(searchLimit));
            SearchResult.Value = new ObservableCollection<EquipSet>(result);

            // ビジーフラグ解除
            IsFree.Value = true;

            // 完了ログ表示
            LogSb.Append("■もっと検索完了：");
            LogSb.Append(SearchResult.Value.Count);
            LogSb.Append("件\n");
            LogBoxText.Value = LogSb.ToString();
        }

        // 追加スキル検索
        async internal Task SearchExtraSkill()
        {
            // 開始ログ表示
            LogSb.Clear();
            LogSb.Append("■追加スキル検索開始：\n");
            LogBoxText.Value = LogSb.ToString();

            // ビジーフラグ
            IsFree.Value = false;

            // 追加スキル検索
            List<Skill> result = await Task.Run(() => Simulator.SearchExtraSkill());

            // ビジーフラグ解除
            IsFree.Value = true;

            // TODO: もっといい方法はないか？インジケータとかも欲しい
            // ログ表示
            LogSb.Append("■追加スキル検索完了\n");
            foreach (Skill skill in result)
            {
                LogSb.Append(skill.Name);
                LogSb.Append(" Lv");
                LogSb.Append(skill.Level);
                LogSb.Append('\n');
            }
            LogBoxText.Value = LogSb.ToString();
        }

        // 除外装備設定
        internal void AddExclude(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 除外
            Simulator.AddExclude(trueName);

            // 除外固定のマスタをリロード
            LoadCludes();

            // TODO: もっといい方法はないか？
            // ログ表示
            LogSb.Append("■除外\n");
            LogSb.Append(dispName);
            LogSb.Append('\n');
            LogBoxText.Value = LogSb.ToString();
        }

        // 固定装備設定
        internal void AddInclude(string trueName, string dispName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 固定
            Simulator.AddInclude(trueName);

            // 除外固定のマスタをリロード
            LoadCludes();

            // TODO: もっといい方法はないか？
            // ログ表示
            LogSb.Append("■固定\n");
            LogSb.Append(dispName);
            LogSb.Append('\n');
            LogBoxText.Value = LogSb.ToString();
        }

        // 除外・固定の解除
        internal void DeleteClude(string trueName)
        {
            if (string.IsNullOrEmpty(trueName))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 解除
            Simulator.DeleteClude(trueName);

            // 除外固定のマスタをリロード
            LoadCludes();
        }

        // 護石追加
        internal void AddCharm()
        {
            // スキルを整理
            List<Skill> skills = new();
            foreach (var vm in CharmSkillSelectorVMs.Value)
            {
                if (!vm.SkillName.Value.Equals(NoSkillName))
                {
                    skills.Add(new Skill(vm.SkillName.Value, vm.SkillLevel.Value));
                }
            }

            // スロットを整理
            string[] splited = CharmWeaponSlots.Value.Split('-');
            int weaponSlot1 = int.Parse(splited[0]);
            int weaponSlot2 = int.Parse(splited[1]);
            int weaponSlot3 = int.Parse(splited[2]);

            // 護石追加
            Simulator.AddCharm(skills, weaponSlot1, weaponSlot2, weaponSlot3);

            // 装備マスタをリロード
            LoadEquips();
        }

        // 護石削除
        internal void DeleteCharm(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 除外・固定設定があったら削除
            Simulator.DeleteClude(name);

            // この護石を使っているマイセットがあったら削除
            DeleteMySetUsingCharm(name);

            // 護石削除
            Simulator.DeleteCharm(name);

            // マスタをリロード
            LoadCludes();
            LoadMySets();
            LoadEquips();
        }

        // 指定した護石を使っているマイセットがあったら削除
        private void DeleteMySetUsingCharm(string name)
        {
            List<EquipSet> delMySets = new();
            foreach (var set in Masters.MySets)
            {
                if (set.CharmName != null && set.CharmName.Equals(name))
                {
                    delMySets.Add(set);
                }
            }
            foreach (var set in delMySets)
            {
                Simulator.DeleteMySet(set);
            }
        }

        // マイセットを追加
        internal void AddMySet()
        {
            EquipSet set = DetailSet.Value;
            if (set == null)
            {
                // 詳細画面が空の状態で実行したなら何もせず終了
                return;
            }

            // 追加
            Simulator.AddMySet(set);

            // マイセットマスタのリロード
            LoadMySets();

            // TODO: もっといい方法はないか？
            // ログ表示
            LogSb.Append("■マイセット登録\n");
            LogSb.Append(set.SimpleSetName);
            LogSb.Append('\n');
            LogBoxText.Value = LogSb.ToString();
        }

        // マイセットを削除
        internal void DeleteMySet()
        {
            EquipSet set = MyDetailSet.Value;
            if (set == null)
            {
                // 詳細画面が空の状態で実行したなら何もせず終了
                return;
            }

            // 削除
            Simulator.DeleteMySet(set);

            // マイセットマスタのリロード
            LoadMySets();
        }

        // マイセットのスキルをシミュ画面の検索条件に反映
        internal void InputMySetCondition()
        {
            if (MyDetailSet == null)
            {
                // マイセットの詳細画面が空の場合何もせず終了
                return;
            }

            // スキル入力部品の数以上には実行しない(できない)
            int count = Math.Min(SkillSelectorCount, MyDetailSet.Value.Skills.Count);
            for (int i = 0; i < count; i++)
            {
                // スキル情報反映
                SkillSelectorVMs.Value[i].SkillName.Value = MyDetailSet.Value.Skills[i].Name;
                SkillSelectorVMs.Value[i].SkillLevel.Value = MyDetailSet.Value.Skills[i].Level;
            }
            for (int i = count; i < SkillSelectorCount; i++)
            {
                // 使わない行は選択状態をリセット
                SkillSelectorVMs.Value[i].SetDefault();
            }

            // スロット情報反映
            WeaponSlots.Value = MyDetailSet.Value.WeaponSlotDisp;
        }

        // マスタ情報を全てVMにロード
        private void LoadMasters()
        {
            LoadEquips();
            LoadCludes();
            LoadMySets();
        }

        // 装備関係のマスタ情報をVMにロード
        private void LoadEquips()
        {
            // 除外固定画面用のVMの設定
            ObservableCollection<EquipSelectRowViewModel> equipList = new();
            equipList.Add(new EquipSelectRowViewModel("頭：", Masters.Heads));
            equipList.Add(new EquipSelectRowViewModel("胴：", Masters.Bodys));
            equipList.Add(new EquipSelectRowViewModel("腕：", Masters.Arms));
            equipList.Add(new EquipSelectRowViewModel("腰：", Masters.Waists));
            equipList.Add(new EquipSelectRowViewModel("足：", Masters.Legs));
            equipList.Add(new EquipSelectRowViewModel("護石：", Masters.Charms));
            equipList.Add(new EquipSelectRowViewModel("装飾品：", Masters.Decos));
            EquipSelectRowVMs.Value = equipList;

            // 護石画面用のVMの設定
            ObservableCollection<CharmRowViewModel> charmList = new();
            foreach (var charm in Masters.Charms)
            {
                charmList.Add(new CharmRowViewModel(charm));
            }
            CharmRowVMs.Value = charmList;
        }

        // 除外固定のマスタ情報をVMにロード
        private void LoadCludes()
        {
            // 除外固定画面用のVMの設定
            ObservableCollection<CludeRowViewModel> cludeList = new();
            foreach (var clude in Masters.Cludes)
            {
                cludeList.Add(new CludeRowViewModel(clude));
            }
            CludeRowVMs.Value = cludeList;
        }

        // マイセットのマスタ情報をVMにロード
        private void LoadMySets()
        {
            // マイセット画面用のVMの設定
            MySetList.Value = new ObservableCollection<EquipSet>(Masters.MySets); ;
        }
    }
}
