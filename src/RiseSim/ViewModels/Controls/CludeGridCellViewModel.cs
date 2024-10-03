using Reactive.Bindings;
using SimModel.Model;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace RiseSim.ViewModels.Controls
{
    /// <summary>
    /// 除外固定一覧表用のセルのVM
    /// </summary>
    internal class CludeGridCellViewModel : ChildViewModelBase
    {
        /// <summary>
        /// 除外固定なしの背景色
        /// </summary>
        const string NormalBG = "Azure";

        /// <summary>
        /// 除外時の背景色
        /// </summary>
        const string ExcludeBG = "LightGray";

        /// <summary>
        /// 固定時の背景色
        /// </summary>
        const string IncludeBG = "Gold";

        /// <summary>
        /// 装備の表示名
        /// </summary>
        public ReactivePropertySlim<string> DispName { get; } = new();

        /// <summary>
        /// 除外フラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsExclude { get; } = new();

        /// <summary>
        /// 固定フラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsInclude { get; } = new();

        /// <summary>
        /// 存在フラグ
        /// </summary>
        public ReactivePropertySlim<bool> IsEquipment { get; } = new();

        /// <summary>
        /// 固定可能フラグ(装飾品は固定できない)
        /// </summary>
        public ReactivePropertySlim<bool> CanInclude { get; } = new(true);

        /// <summary>
        /// 背景色
        /// </summary>
        public ReactivePropertySlim<string> BackGround { get; } = new(NormalBG);

        /// <summary>
        /// 説明(コンテキストメニュー用)
        /// </summary>
        public ReactivePropertySlim<string> Description { get; } = new();

        /// <summary>
        /// 元の装備データ
        /// </summary>
        public Equipment? BaseEquip { get; set; } 

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="equip">装備</param>
        public CludeGridCellViewModel(Equipment? equip)
        {
            // 装備情報を登録
            if (equip == null) 
            {
                IsEquipment.Value = false;
                return;
            }
            BaseEquip = equip;
            IsEquipment.Value = true;
            DispName.Value = equip.DispName;
            Description.Value = equip.Description;

            // 除外固定情報を登録
            IsExclude.Value = false;
            IsInclude.Value = false;
            Clude? clude = Masters.Cludes.Where(c => c.Name == BaseEquip.Name).FirstOrDefault();
            if (clude != null) {
                if (clude.Kind == CludeKind.exclude)
                {
                    IsExclude.Value = true;
                }
                else
                {
                    IsInclude.Value = true;
                }
            }

            // 装飾品は固定不可
            CanInclude.Value = equip.Kind != EquipKind.deco;

            // 背景色設定
            SetBackGround();

            // コマンド登録
            IsExclude.Skip(1).Subscribe(x => Exclude(x));
            IsInclude.Skip(1).Subscribe(x => Include(x));
        }

        /// <summary>
        /// 除外フラグ変更時の処理
        /// </summary>
        /// <param name="x">変更した値</param>
        private void Exclude(bool x)
        {
            if (BaseEquip == null)
            {
                return;
            }

            if (x)
            {
                CludeTabVM.AddExclude(BaseEquip);
            }
            else
            {
                CludeTabVM.DeleteClude(BaseEquip);
            }
            SetBackGround();
        }

        /// <summary>
        /// 固定フラグ変更時の処理
        /// </summary>
        /// <param name="x">変更した値</param>
        private void Include(bool x)
        {
            if (BaseEquip == null)
            {
                return;
            }

            if (x)
            {
                CludeTabVM.AddInclude(BaseEquip);
            }
            else
            {
                CludeTabVM.DeleteClude(BaseEquip);
            }
            SetBackGround();
        }

        /// <summary>
        /// 背景色を各フラグに合わせて変更
        /// </summary>
        private void SetBackGround()
        {
            if (IsExclude.Value)
            {
                BackGround.Value = ExcludeBG;
            }
            else if (IsInclude.Value)
            {
                BackGround.Value = IncludeBG;
            }
            else
            {
                BackGround.Value = NormalBG;
            }
        }

        /// <summary>
        /// 固定の表示だけ変更する
        /// 表の再読み込みが遅いため個別に表示を変更するための関数
        /// </summary>
        /// <param name="flag">変更する値</param>
        internal void ChangeIncludeSilent(bool flag)
        {
            if (IsInclude.Value != flag)
            {
                IsInclude.Skip(1);
                IsInclude.Value = flag;
                SetBackGround();
            }
        }

        /// <summary>
        /// 除外の表示だけ変更する
        /// 表の再読み込みが遅いため個別に表示を変更するための関数
        /// </summary>
        /// <param name="flag">変更する値</param>
        internal void ChangeExcludeSilent(bool flag)
        {
            if (IsExclude.Value != flag)
            {
                IsExclude.Skip(1);
                IsExclude.Value = flag;
                SetBackGround();
            }
        }
    }
}
