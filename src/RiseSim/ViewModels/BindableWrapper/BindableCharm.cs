using Reactive.Bindings;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RiseSim.ViewModels.BindableWrapper
{
    internal class BindableCharm : ChildViewModelBase
    {
        /// <summary>
        /// スキル1
        /// </summary>
        public ReactiveProperty<string> Skill1 { get; } = new(string.Empty);

        /// <summary>
        /// スキル2
        /// </summary>
        public ReactiveProperty<string> Skill2 { get; } = new(string.Empty);

        /// <summary>
        /// スロット
        /// </summary>
        public ReactiveProperty<string> Slot { get; } = new();

        /// <summary>
        /// オリジナル
        /// </summary>
        public Equipment Original { get; set; }

        /// <summary>
        /// 護石を削除するコマンド
        /// </summary>
        public ReactiveCommand DeleteCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="original">オリジナルの護石データ</param>
        /// <exception cref="ArgumentException">護石以外が渡された場合</exception>
        public BindableCharm(Equipment original)
        {
            if (original.Kind != EquipKind.charm)
            {
                throw new ArgumentException("護石以外の装備が護石として登録されています");
            }

            if (original.Skills.Count > 0)
            {
                Skill1.Value = $"{original.Skills[0].Description}";
            }
            if (original.Skills.Count > 1)
            {
                Skill2.Value = $"{original.Skills[1].Description}";
            }
            Slot.Value = $"{original.Slot1}-{original.Slot2}-{original.Slot3}";

            Original = original;

            DeleteCommand.Subscribe(() => Delete());
        }

        /// <summary>
        /// 護石削除
        /// </summary>
        private void Delete()
        {
            CharmTabVM.DeleteCharm(Original.Name, Original.DispName);
        }

        /// <summary>
        /// リストをまとめてバインド用クラスに変換
        /// </summary>
        /// <param name="list">変換前リスト</param>
        /// <returns></returns>
        static public ObservableCollection<BindableCharm> BeBindableList(List<Equipment> list)
        {
            ObservableCollection<BindableCharm> bindableList = new();
            foreach (var equip in list)
            {
                bindableList.Add(new BindableCharm(equip));
            }

            // 返却
            return bindableList;
        }
    }
}
