﻿using Prism.Mvvm;
using Reactive.Bindings;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiseSim.ViewModels.BindableWrapper
{
    internal class BindableIdealAugmentation : BindableBase
    {
        // 管理用装備名(GUID)
        public string Name { get; set; } = string.Empty;

        // 表示用装備名
        public string DispName { get; set; } = string.Empty;

        // テーブル
        public int Table { get; set; }

        // 下位テーブル含フラグ
        public bool IsIncludeLower { get; set; }

        // 表示用テーブル
        public string TableDisp { get; set; }

        // スロット追加数
        public int SlotIncrement { get; set; }

        // 1部位だけか否か
        public bool IsOne { get; set; }

        // 1部位だけか否か
        public string IsOneDisp { get; set; }

        // 表示用スキル一覧
        public string SimpleSkillDiscription { get; set; }

        // 表示用スキルマイナス一覧
        public string SimpleSkillMinusDiscription { get; set; }

        // 有効無効
        public ReactivePropertySlim<bool> IsEnabled { get; set; } = new(true);

        // 必須
        public ReactivePropertySlim<bool> IsRequired { get; set; } = new(false);

        public IdealAugmentation Original { get; set; }

        // コンストラクタ
        public BindableIdealAugmentation(IdealAugmentation ideal)
        {
            Name = ideal.Name;
            DispName = ideal.DispName;
            Table = ideal.Table;
            SlotIncrement = ideal.SlotIncrement;
            IsOne = ideal.IsOne;
            IsOneDisp = IsOne ? "一部位のみ" : "全部位可";
            IsIncludeLower = ideal.IsIncludeLower;
            IsEnabled.Value = ideal.IsEnabled;
            IsRequired.Value = ideal.IsRequired;
            TableDisp = ideal.Table + (IsIncludeLower ? "以下" : "のみ"); 
            SimpleSkillDiscription = ideal.SimpleSkillDiscription;
            SimpleSkillMinusDiscription = ideal.SimpleSkillMinusDiscription;
            Original = ideal;
            IsEnabled.Subscribe(x => ChangeIsEnabled(x));
            IsRequired.Subscribe(x => ChangeIsRequired(x));
        }

        private void ChangeIsEnabled(bool x)
        {
            if (Original.IsEnabled != x)
            {
                Original.IsEnabled = x;
                MainViewModel.Instance.SaveIdeal();
            }
        }

        private void ChangeIsRequired(bool x)
        {
            if (Original.IsRequired != x)
            {
                Original.IsRequired = x;
                MainViewModel.Instance.SaveIdeal();
            }
        }

        // リストをまとめてバインド用クラスに変換
        static public ObservableCollection<BindableIdealAugmentation> BeBindableList(List<IdealAugmentation> list)
        {
            ObservableCollection<BindableIdealAugmentation> bindableList = new ObservableCollection<BindableIdealAugmentation>();
            foreach (var ideal in list)
            {
                bindableList.Add(new BindableIdealAugmentation(ideal));
            }
            return bindableList;
        }
    }
}
