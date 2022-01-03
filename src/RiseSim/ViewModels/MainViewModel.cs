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
using System.Windows.Controls;

namespace RiseSim.ViewModels
{
    internal class MainViewModel : BindableBase
    {
        // MainViewインスタンス：子VMからの参照用
        static public MainViewModel Instance { get; set; }

        // シミュ本体
        public Simulator simulator { get; set; }

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
        private ObservableCollection<SkillSelectorViewModel> skillSelectorVMs;
        public ObservableCollection<SkillSelectorViewModel> SkillSelectorVMs
        {
            get { return this.skillSelectorVMs; }
            set
            {
                this.SetProperty(ref this.skillSelectorVMs, value);
            }
        }

        // シミュ画面の検索結果一覧
        private ObservableCollection<EquipSet> searchResult;
        public ObservableCollection<EquipSet> SearchResult
        {
            get { return this.searchResult; }
            set
            {
                this.SetProperty(ref this.searchResult, value);
            }
        }

        // シミュ画面の検索結果の選択行
        private EquipSet detailSet;
        public EquipSet DetailSet
        {
            get { return this.detailSet; }
            set
            {
                this.SetProperty(ref this.detailSet, value);
            }
        }

        // シミュ画面の装備詳細の各行のVM
        private ObservableCollection<EquipRowViewModel> equipRowVMs;
        public ObservableCollection<EquipRowViewModel> EquipRowVMs
        {
            get { return this.equipRowVMs; }
            set
            {
                this.SetProperty(ref this.equipRowVMs, value);
            }
        }

        // シミュ画面の武器スロ指定
        private string weaponSlots;
        public string WeaponSlots
        {
            get { return this.weaponSlots; }
            set
            {
                this.SetProperty(ref this.weaponSlots, value);
            }
        }

        // シミュ画面の頑張り度(検索件数)
        private string limit;
        public string Limit
        {
            get { return this.limit; }
            set
            {
                this.SetProperty(ref this.limit, value);
            }
        }

        // 除外・固定画面の登録部品のVM
        private ObservableCollection<EquipSelectRowViewModel> equipSelectRowVMs;
        public ObservableCollection<EquipSelectRowViewModel> EquipSelectRowVMs
        {
            get { return this.equipSelectRowVMs; }
            set
            {
                this.SetProperty(ref this.equipSelectRowVMs, value);
            }
        }

        // 除外・固定画面の一覧表示の各行のVM
        private ObservableCollection<CludeRowViewModel> cludeRowVMs;
        public ObservableCollection<CludeRowViewModel> CludeRowVMs
        {
            get { return this.cludeRowVMs; }
            set
            {
                this.SetProperty(ref this.cludeRowVMs, value);
            }
        }

        // 護石画面のスキル選択部品のVM
        private ObservableCollection<SkillSelectorViewModel> charmSkillSelectorVMs;
        public ObservableCollection<SkillSelectorViewModel> CharmSkillSelectorVMs
        {
            get { return this.charmSkillSelectorVMs; }
            set
            {
                this.SetProperty(ref this.charmSkillSelectorVMs, value);
            }
        }

        // 護石画面の一覧用部品のVM
        private ObservableCollection<CharmRowViewModel> charmRowVMs;
        public ObservableCollection<CharmRowViewModel> CharmRowVMs
        {
            get { return this.charmRowVMs; }
            set
            {
                this.SetProperty(ref this.charmRowVMs, value);
            }
        }

        // 護石画面のスロット指定
        private string charmWeaponSlots;
        public string CharmWeaponSlots
        {
            get { return this.charmWeaponSlots; }
            set
            {
                this.SetProperty(ref this.charmWeaponSlots, value);
            }
        }

        // マイセット一覧
        private ObservableCollection<EquipSet> mySetList;
        public ObservableCollection<EquipSet> MySetList
        {
            get { return this.mySetList; }
            set
            {
                this.SetProperty(ref this.mySetList, value);
            }
        }

        // マイセットの選択行データ
        private EquipSet myDetailSet;
        public EquipSet MyDetailSet
        {
            get { return this.myDetailSet; }
            set
            {
                this.SetProperty(ref this.myDetailSet, value);
            }
        }

        // マイセット画面の装備詳細の各行のVM
        private ObservableCollection<EquipRowViewModel> myEquipRowVMs;
        public ObservableCollection<EquipRowViewModel> MyEquipRowVMs
        {
            get { return this.myEquipRowVMs; }
            set
            {
                this.SetProperty(ref this.myEquipRowVMs, value);
            }
        }

        // ライセンス画面の内容
        private string license;
        public string License
        {
            get { return this.license; }
            set
            {
                this.SetProperty(ref this.license, value);
            }
        }

        // ライセンス画面の雑な要約
        private string whatIsLicense;
        public string WhatIsLicense
        {
            get { return this.whatIsLicense; }
            set
            {
                this.SetProperty(ref this.whatIsLicense, value);
            }
        }

        // スロット選択の選択肢
        private ObservableCollection<string> slotMaster;
        public ObservableCollection<string> SlotMaster
        {
            get { return this.slotMaster; }
            set
            {
                this.SetProperty(ref this.slotMaster, value);
            }
        }

        // ログ表示
        private StringBuilder LogSb { get; } = new StringBuilder();
        private string logBoxText;
        public string LogBoxText
        {
            get { return this.logBoxText; }
            set
            {
                this.SetProperty(ref this.logBoxText, value);
            }
        }

        // コンストラクタ：起動時処理
        public MainViewModel()
        {
            // 子VMからの参照用にstaticにインスタンスを登録
            Instance = this;

            // シミュ本体のインスタンス化
            simulator = new Simulator();
            simulator.LoadData();

            // シミュ画面のスキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> selectorVMs = new ObservableCollection<SkillSelectorViewModel>();
            for (int i = 0; i < SkillSelectorCount; i++)
            {
                selectorVMs.Add(new SkillSelectorViewModel());
            }
            SkillSelectorVMs = selectorVMs;

            // 護石画面のスキル選択部品準備
            ObservableCollection<SkillSelectorViewModel> charmSelectorVMs = new ObservableCollection<SkillSelectorViewModel>();
            for (int i = 0; i < CharmMaxSkillCount; i++)
            {
                charmSelectorVMs.Add(new SkillSelectorViewModel());
            }
            CharmSkillSelectorVMs = charmSelectorVMs;

            // シミュ画面装備詳細に空の装備を表示
            EquipRowVMs = EquipRowViewModel.SetToEquipRows(new EquipSet());

            // スロットの選択肢を生成し、シミュ画面と護石画面に反映
            ObservableCollection<string> slots = new ObservableCollection<string>();
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
            SlotMaster = slots;
            WeaponSlots = "0-0-0";
            CharmWeaponSlots = "0-0-0";

            // 頑張り度を設定
            Limit = DefaultLimit;

            // TODO:外部ファイル化するべきか？
            // ライセンス表示
            StringBuilder sb = new StringBuilder();
            sb.Append("■このシミュのライセンス\n");
            sb.Append("GNU General Public License v3.0\n");
            sb.Append("\n");
            sb.Append("■使わせていただいたOSS(+必要であればライセンス)\n");
            sb.Append("・GLPK for C#/CLI\n");
            sb.Append("プロジェクト：http://glpk-cli.sourceforge.net/\n");
            sb.Append("\n");
            sb.Append("・GlpkWrapperCS(一部改変)\n");
            sb.Append("プロジェクト：https://github.com/YSRKEN/GlpkWrapperCS\n");
            sb.Append("\n");
            sb.Append("・CSV\n");
            sb.Append("プロジェクト：https://github.com/stevehansen/csv/\n");
            sb.Append("ライセンス：https://raw.githubusercontent.com/stevehansen/csv/master/LICENSE\n");
            sb.Append("\n");
            sb.Append("・Prism.Wpf\n");
            sb.Append("プロジェクト：https://github.com/PrismLibrary/Prism\n");
            sb.Append("ライセンス：https://www.nuget.org/packages/Prism.Wpf/8.1.97/license\n");
            sb.Append("\n");
            sb.Append("■スペシャルサンクス\n");
            sb.Append("・5chモンハン板シミュスレの方々\n");
            sb.Append("特にVer.13の >> 480様の以下論文を大いに参考にしました\n");
            sb.Append("https://github.com/13-480/lp-doc\n");
            sb.Append("\n");
            sb.Append("・先人のシミュ作成者様\n");
            sb.Append("特に頑シミュ様のUIに大きく影響を受けています\n");
            License = sb.ToString();

            // TODO:外部ファイル化するべきか？
            // ライセンスの雑な説明を表示
            StringBuilder sbw = new StringBuilder();
            sbw.Append("■←ライセンスって何？\n");
            sbw.Append("普通にスキルシミュとして使う分には気にしないでOK\n");
            sbw.Append("        \n");
            sbw.Append("■いや、怖いからちゃんと説明して？\n");
            sbw.Append("こういう使い方までならOKだよ、ってのを定める取り決め\n");
            sbw.Append("今回のは大体こんな感じ\n");
            sbw.Append("・シミュとして使う分には好きに使ってOK\n");
            sbw.Append("・このシミュのせいで何か起きても開発者は責任取らんよ\n");
            sbw.Append("・改変や再配布するときはよく調べてルールに従ってね\n");
            WhatIsLicense = sbw.ToString();

            // マスタファイル読み込み
            LoadMasters();
        }

        // 検索
        internal void Search()
        {
            // スキル条件を整理
            List<Skill> skills = new List<Skill>();
            foreach (var selectorVM in SkillSelectorVMs)
            {
                if(selectorVM.SkillName != NoSkillName)
                {
                    skills.Add(new Skill(selectorVM.SkillName, selectorVM.SkillLevel));
                }
            }

            // 武器スロ条件を整理
            string[] splited = WeaponSlots.Split('-');
            int weaponSlot1 = int.Parse(splited[0]);
            int weaponSlot2 = int.Parse(splited[1]);
            int weaponSlot3 = int.Parse(splited[2]);

            // 頑張り度を整理
            int searchLimit;
            try
            {
                searchLimit = int.Parse(Limit);
            }
            catch (Exception)
            {
                // 数値以外が入力されていたら初期値を利用
                searchLimit = int.Parse(DefaultLimit);
            }

            // 検索
            List<EquipSet> result = simulator.Search(skills, weaponSlot1, weaponSlot2, weaponSlot3, searchLimit);
            SearchResult = new ObservableCollection<EquipSet>(result);

            // TODO: もっといい方法はないか？インジケータとかも欲しい
            // ログ表示
            LogSb.Clear();
            LogSb.Append("■検索完了：");
            LogSb.Append(SearchResult.Count);
            LogSb.Append("件\n");
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
            LogBoxText = LogSb.ToString();
        }

        // もっと検索
        internal void SearchMore()
        {
            // 頑張り度を整理
            int searchLimit;
            try
            {
                searchLimit = int.Parse(Limit);
            }
            catch (Exception)
            {
                // 数値以外が入力されていたら初期値を利用
                searchLimit = int.Parse(DefaultLimit);
            }

            // もっと検索
            List<EquipSet> result = simulator.SearchMore(searchLimit);
            SearchResult = new ObservableCollection<EquipSet>(result);

            // TODO: もっといい方法はないか？インジケータとかも欲しい
            // ログ表示
            LogSb.Clear();
            LogSb.Append("■もっと検索完了：");
            LogSb.Append(SearchResult.Count);
            LogSb.Append("件\n");
            LogBoxText = LogSb.ToString();
        }

        // 追加スキル検索
        internal void SearchExtraSkill()
        {
            // 追加スキル検索
            List<Skill> result = simulator.SearchExtraSkill();

            // TODO: もっといい方法はないか？インジケータとかも欲しい
            // ログ表示
            LogSb.Clear();
            LogSb.Append("■追加スキル検索結果\n");
            foreach (Skill skill in result)
            {
                LogSb.Append(skill.Name);
                LogSb.Append(" Lv");
                LogSb.Append(skill.Level);
                LogSb.Append('\n');
            }
            LogBoxText = LogSb.ToString();
        }

        // シミュ画面に装備詳細を表示する
        internal void ViewSetDetail(EquipSet set)
        {
            EquipRowVMs = EquipRowViewModel.SetToEquipRows(set);
            DetailSet = set;
        }

        // 除外装備設定
        internal void AddExclude(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 除外
            simulator.AddExclude(name);

            // 除外固定のマスタをリロード
            LoadCludes();

            // TODO: もっといい方法はないか？インジケータとかも欲しい
            // ログ表示
            LogSb.Append("■除外\n");
            LogSb.Append(name);
            LogSb.Append("\n");
            LogBoxText = LogSb.ToString();
        }

        // 固定装備設定
        internal void AddInclude(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 固定
            simulator.AddInclude(name);

            // 除外固定のマスタをリロード
            LoadCludes();

            // TODO: もっといい方法はないか？インジケータとかも欲しい
            // ログ表示
            LogSb.Append("■固定\n");
            LogSb.Append(name);
            LogSb.Append("\n");
            LogBoxText = LogSb.ToString();
        }

        // 除外・固定の解除
        internal void DeleteClude(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                // 装備無しなら何もせず終了
                return;
            }

            // 解除
            simulator.DeleteClude(name);

            // 除外固定のマスタをリロード
            LoadCludes();

            // TODO: もっといい方法はないか？インジケータとかも欲しい
            // ログ表示
            LogSb.Append("■除外固定解除\n");
            LogSb.Append(name);
            LogSb.Append("\n");
            LogBoxText = LogSb.ToString();
        }

        // 護石追加
        internal void AddCharm()
        {
            // スキルを整理
            List<Skill> skills = new List<Skill>();
            foreach (var vm in CharmSkillSelectorVMs)
            {
                if (!vm.SkillName.Equals(NoSkillName))
                {
                    skills.Add(new Skill(vm.SkillName, vm.SkillLevel));
                }
            }

            // スロットを整理
            string[] splited = CharmWeaponSlots.Split('-');
            int weaponSlot1 = int.Parse(splited[0]);
            int weaponSlot2 = int.Parse(splited[1]);
            int weaponSlot3 = int.Parse(splited[2]);

            // 護石追加
            simulator.AddCharm(skills, weaponSlot1, weaponSlot2, weaponSlot3);

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
            simulator.DeleteClude(name);

            // この護石を使っているマイセットがあったら削除
            DeleteMySetUsingCharm(name);

            // 護石削除
            simulator.DeleteCharm(name);

            // マスタをリロード
            LoadCludes();
            LoadMySets();
            LoadEquips();
        }

        // 指定した護石を使っているマイセットがあったら削除
        private void DeleteMySetUsingCharm(string name)
        {
            List<EquipSet> delMySets = new List<EquipSet>();
            foreach (var set in Masters.MySets)
            {
                if (set.CharmName != null && set.CharmName.Equals(name))
                {
                    delMySets.Add(set);
                }
            }
            foreach (var set in delMySets)
            {
                simulator.DeleteMySet(set);
            }
        }

        // マイセット画面に装備詳細を表示する
        internal void ViewMySetDetail(EquipSet set)
        {
            MyEquipRowVMs = EquipRowViewModel.SetToEquipRows(set);
            MyDetailSet = set;
        }

        // マイセットを追加
        internal void AddMySet()
        {
            EquipSet set = DetailSet;
            if (set == null)
            {
                // 詳細画面が空の状態で実行したなら何もせず終了
                return;
            }

            // 追加
            simulator.AddMySet(set);

            // マイセットマスタのリロード
            LoadMySets();

            // TODO: もっといい方法はないか？インジケータとかも欲しい
            // ログ表示
            LogSb.Append("■マイセット登録\n");
            LogSb.Append(set.SimpleSetName);
            LogSb.Append("\n");
            LogBoxText = LogSb.ToString();
        }

        // マイセットを削除
        internal void DeleteMySet()
        {
            EquipSet set = MyDetailSet;
            if (set == null)
            {
                // 詳細画面が空の状態で実行したなら何もせず終了
                return;
            }

            // 削除
            simulator.DeleteMySet(set);

            // マイセットマスタのリロード
            LoadMySets();

            // TODO: もっといい方法はないか？インジケータとかも欲しい
            // ログ表示
            LogSb.Append("■マイセット削除\n");
            LogSb.Append(set.SimpleSetName);
            LogSb.Append("\n");
            LogBoxText = LogSb.ToString();
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
            int count = Math.Min(SkillSelectorCount, MyDetailSet.Skills.Count);
            for (int i = 0; i < count; i++)
            {
                // スキル情報反映
                SkillSelectorVMs[i].SkillName = MyDetailSet.Skills[i].Name;
                SkillSelectorVMs[i].SkillLevel = MyDetailSet.Skills[i].Level;
            }
            for (int i = count; i < SkillSelectorCount; i++)
            {
                // 使わない行は選択状態をリセット
                SkillSelectorVMs[i].SetDefault();
            }

            // スロット情報反映
            WeaponSlots = MyDetailSet.WeaponSlotDisp;
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
            ObservableCollection<EquipSelectRowViewModel> equipList = new ObservableCollection<EquipSelectRowViewModel>();
            equipList.Add(new EquipSelectRowViewModel("頭：", Masters.Heads));
            equipList.Add(new EquipSelectRowViewModel("胴：", Masters.Bodys));
            equipList.Add(new EquipSelectRowViewModel("腕：", Masters.Arms));
            equipList.Add(new EquipSelectRowViewModel("腰：", Masters.Waists));
            equipList.Add(new EquipSelectRowViewModel("足：", Masters.Legs));
            equipList.Add(new EquipSelectRowViewModel("護石：", Masters.Charms));
            equipList.Add(new EquipSelectRowViewModel("装飾品：", Masters.Decos));
            EquipSelectRowVMs = equipList;

            // 護石画面用のVMの設定
            ObservableCollection<CharmRowViewModel> charmList = new ObservableCollection<CharmRowViewModel>();
            foreach (var charm in Masters.Charms)
            {
                charmList.Add(new CharmRowViewModel(charm));
            }
            CharmRowVMs = charmList;
        }

        // 除外固定のマスタ情報をVMにロード
        private void LoadCludes()
        {
            // 除外固定画面用のVMの設定
            ObservableCollection<CludeRowViewModel> cludeList = new ObservableCollection<CludeRowViewModel>();
            foreach (var clude in Masters.Cludes)
            {
                cludeList.Add(new CludeRowViewModel(clude));
            }
            CludeRowVMs = cludeList;
        }

        // マイセットのマスタ情報をVMにロード
        private void LoadMySets()
        {
            // マイセット画面用のVMの設定
            MySetList = new ObservableCollection<EquipSet>(Masters.MySets); ;
        }
    }
}
