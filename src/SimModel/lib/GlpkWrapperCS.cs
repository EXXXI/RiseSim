/*    GlpkWrapperCS : Wrapper Class for GLPK by C-Sharp
 *    Copyright (C) 2017  YSR
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlpkWrapperCS
{
	using org.gnu.glpk;
	enum ObjectDirection { Minimize = 1, Maximize = 2, }
	enum BoundsType { Free = 1, Lower = 2, Upper = 3, Double = 4, Fixed = 5, }
	enum VariableKind { Continuous = 1, Integer = 2, Binary = 3, }
	enum SolverResult
	{
		OK = 0,                     //正常に解けた
		ErrorBadBasis = 1,          //初期基底に誤りがあった
		ErrorSingular = 2,          //基底行列が特異になった
		ErrorCondition = 3,         //基底行列の条件数が大きすぎる
		ErrorBound = 4,             //変数の範囲設定に誤りがある
		ErrorFail = 5,              //ソルバーに障害が発生した
		ErrorObjectLowerLimit = 6,  //目的関数が下限に達した
		ErrorObjectUpperLimit = 7,  //目的関数が上限に達した
		ErrorIterationLimit = 8,    //反復回数の制限を超えた
		ErrorTimeLimit = 9,         //計算時間の制限を超えた
		ErrorNoPrimalSolution = 10, //主問題の解が存在しない
		ErrorNoDualSolution = 11,   //双対問題の解が存在しない
		ErrorRoot = 12,             //初期解として緩和解が与えられなかった
		ErrorStop = 13,             //検索が強制的に終了させられた
		ErrorMipGap = 14,           //ギャップが公差に達したので早期に終了した
	}
	class MipProblem : IDisposable
	{
		glp_prob problem = GLPK.glp_create_prob();
		// コンストラクタ
		public MipProblem()
		{
			// 目的関数関係
			ObjCoef = new objCoef(this);
			// 制約式関係
			RowName = new rowName(this);
			RowType = new rowType(this);
			RowLB = new rowLb(this);
			RowUB = new rowUb(this);
			RowMatrix = new rowMatrix(this);
			// 変数関係
			ColumnName = new columnName(this);
			ColumnType = new columnType(this);
			ColumnLB = new columnLb(this);
			ColumnUB = new columnUb(this);
			ColumnKind = new columnKind(this);
			LpColumnValue = new lpColumnValue(this);
			MipColumnValue = new mipColumnValue(this);
		}
		// Disposeメソッド
		public void Dispose() { }
		// 問題名
		public string Name
		{
			get { return GLPK.glp_get_prob_name(problem); }
			set { GLPK.glp_set_prob_name(problem, value); }
		}
		// 最適化処理
		public SolverResult Simplex(bool messageFlg = true)
		{
			glp_smcp smcp = new glp_smcp();
			GLPK.glp_init_smcp(smcp);
			if (!messageFlg)
				smcp.msg_lev = GLPK.GLP_MSG_OFF;
			return (SolverResult)GLPK.glp_simplex(problem, smcp);
		}
		public SolverResult BranchAndCut(bool messageFlg = true)
		{
			Simplex(false);
			glp_iocp iocp = new glp_iocp();
			GLPK.glp_init_iocp(iocp);
			if (!messageFlg)
				iocp.msg_lev = GLPK.GLP_MSG_OFF;
			return (SolverResult)GLPK.glp_intopt(problem, iocp);
		}
		// 数値を符号付きで文字化する
		string SignedToString(double n)
		{
			if (n >= 0.0)
				return $"+{n}";
			else
				return $"{n}";
		}
		// LPファイルとして出力
		public string ToLpString()
		{
			var output = "";
			// 制約式の方向
			if (ObjDir == ObjectDirection.Maximize)
				output += "maximize\n";
			else
				output += "minimize\n";
			// 目的関数
			for (int ci = 0; ci < ColumnsCount; ++ci)
			{
				output += $" {SignedToString(ObjCoef[ci])} x{ci + 1}";
			}
			output += "\n";
			// 制約式
			output += "subject to\n";
			for (int ri = 0; ri < RowsCount; ++ri)
			{
				// 左辺となる数式を文字列化する
				var equationLeft = "";
				var matrix = RowMatrix[ri];
				var sortedMatrix = matrix.OrderBy((x) => x.Key);
				foreach (var pair in sortedMatrix)
				{
					equationLeft += $" {SignedToString(pair.Value)} x{pair.Key + 1}";
				}
				// 右辺の種類・下限・上限から追加する数式を決める
				switch (RowType[ri])
				{
					case BoundsType.Free:
						break;
					case BoundsType.Lower:
						output += $"{equationLeft} >= {RowLB[ri]}\n";
						break;
					case BoundsType.Upper:
						output += $"{equationLeft} <= {RowUB[ri]}\n";
						break;
					case BoundsType.Double:
						output += $"{equationLeft} >= {RowLB[ri]}\n";
						output += $"{equationLeft} <= {RowUB[ri]}\n";
						break;
					case BoundsType.Fixed:
						output += $"{equationLeft} = {RowLB[ri]}\n";
						break;
				}
			}
			// 変数条件
			output += "bounds\n";
			for (int ri = 0; ri < ColumnsCount; ++ri)
			{
				var variableString = $"x{ri + 1}";
				switch (ColumnType[ri])
				{
					case BoundsType.Free:
						break;
					case BoundsType.Lower:
						output += $"{variableString} >= {ColumnLB[ri]}\n";
						break;
					case BoundsType.Upper:
						output += $"{variableString} <= {ColumnUB[ri]}\n";
						break;
					case BoundsType.Double:
						output += $"{variableString} >= {ColumnLB[ri]}\n";
						output += $"{variableString} <= {ColumnUB[ri]}\n";
						break;
					case BoundsType.Fixed:
						output += $"{variableString} = {ColumnLB[ri]}\n";
						break;
				}
			}
			output += "general\n";
			for (int ri = 0; ri < ColumnsCount; ++ri)
			{
				if ((int)ColumnType[ri] == (int)VariableKind.Integer)
				{
					output += $" x{ri + 1}";
				}
			}
			output += "\nbinary\n";
			for (int ri = 0; ri < ColumnsCount; ++ri)
			{
				if ((int)ColumnType[ri] == (int)VariableKind.Binary)
				{
					output += $" x{ri + 1}";
				}
			}
			output += "\n";
			return output;
		}
		#region 目的関数関係
		// 最適化の方向
		public ObjectDirection ObjDir
		{
			get { return (ObjectDirection)GLPK.glp_get_obj_dir(problem); }
			set { GLPK.glp_set_obj_dir(problem, (int)value); }
		}
		// 目的関数の係数
		public class objCoef
		{
			MipProblem mip;
			public objCoef(MipProblem mip)
			{
				this.mip = mip;
			}
			public double this[int index]
			{
				get { return GLPK.glp_get_obj_coef(mip.problem, index + 1); }
				set { GLPK.glp_set_obj_coef(mip.problem, index + 1, (int)value); }
			}
		}
		public objCoef ObjCoef;
		// 最適化後の値
		public double LpObjValue
		{
			get { return GLPK.glp_get_obj_val(problem); }
		}
		public double MipObjValue
		{
			get { return GLPK.glp_mip_obj_val(problem); }
		}
		#endregion
		#region 制約式関係
		// 制約式を追加する Added By EXXXI, 2022
		public void AddRow(string name)
		{
			AddRows(1);
			RowName[RowsCount - 1] = name;
		}
		// 制約式を追加する
		public void AddRows(int n)
		{
			GLPK.glp_add_rows(problem, n);
		}
		// 制約式の数
		public int RowsCount
		{
			get { return GLPK.glp_get_num_rows(problem); }
		}
		// 制約式の名前
		public class rowName
		{
			MipProblem mip;
			public rowName(MipProblem mip)
			{
				this.mip = mip;
			}
			public string this[int index]
			{
				get { return GLPK.glp_get_row_name(mip.problem, index + 1); }
				set { GLPK.glp_set_row_name(mip.problem, index + 1, value); }
			}
		}
		public rowName RowName;
		// 制約式の範囲を設定する
		public void SetRowBounds(int index, BoundsType type, double lowerBound, double upperBound)
		{
			GLPK.glp_set_row_bnds(problem, index + 1, (int)type, lowerBound, upperBound);
		}
		// 制約式の種類
		public class rowType
		{
			MipProblem mip;
			public rowType(MipProblem mip)
			{
				this.mip = mip;
			}
			public BoundsType this[int index]
			{
				get { return (BoundsType)GLPK.glp_get_row_type(mip.problem, index + 1); }
			}
		}
		public rowType RowType;
		// 制約式の下限
		public class rowLb
		{
			MipProblem mip;
			public rowLb(MipProblem mip)
			{
				this.mip = mip;
			}
			public double this[int index]
			{
				get { return GLPK.glp_get_row_lb(mip.problem, index + 1); }
			}
		}
		public rowLb RowLB;
		// 制約式の上限
		public class rowUb
		{
			MipProblem mip;
			public rowUb(MipProblem mip)
			{
				this.mip = mip;
			}
			public double this[int index]
			{
				get { return GLPK.glp_get_row_ub(mip.problem, index + 1); }
			}
		}
		public rowUb RowUB;
		// 制約式の係数
		public void LoadMatrix(int size, int[] ia, int[] ja, double[] ar)
		{
			// 係数を問題に代入する
			var ia_ = GLPK.new_intArray(size + 1);
			var ja_ = GLPK.new_intArray(size + 1);
			var ar_ = GLPK.new_doubleArray(size + 1);
			for (int i = 0; i < size; ++i)
			{
				GLPK.intArray_setitem(ia_, i + 1, ia[i] + 1);
				GLPK.intArray_setitem(ja_, i + 1, ja[i] + 1);
				GLPK.doubleArray_setitem(ar_, i + 1, ar[i]);
			}
			GLPK.glp_load_matrix(problem, size, ia_, ja_, ar_);
			GLPK.delete_intArray(ia_);
			GLPK.delete_intArray(ja_);
			GLPK.delete_doubleArray(ar_);
		}
		public void LoadMatrix(int[] ia, int[] ja, double[] ar)
		{
			LoadMatrix(ia.Count(), ia, ja, ar);
		}
		public class rowMatrix
		{
			// 係数を問題から読み込む
			MipProblem mip;
			public rowMatrix(MipProblem mip)
			{
				this.mip = mip;
			}
			public Dictionary<int, double> this[int index]
			{
				get
				{
					var matrix = new Dictionary<int, double>();
					// まずは変数に読み込む
					var length = GLPK.glp_get_mat_row(mip.problem, index + 1, null, null);
					var key = GLPK.new_intArray(length + 1);
					var value = GLPK.new_doubleArray(length + 1);
					GLPK.glp_get_mat_row(mip.problem, index + 1, key, value);
					// 次にDictionaryにコピーする
					for (int i = 1; i <= length; ++i)
					{
						matrix.Add(
							GLPK.intArray_getitem(key, i) - 1,
							GLPK.doubleArray_getitem(value, i)
						);
					}
					// 最後に後片付けする
					GLPK.delete_intArray(key);
					GLPK.delete_doubleArray(value);
					return matrix;
				}
			}
		}
		public rowMatrix RowMatrix;
		#endregion
		#region 変数関係
		// 変数を追加する Added By EXXXI, 2022
		public void AddColumn(string name)
		{
			AddColumns(1);
			ColumnName[ColumnsCount - 1] = name;
		}
		// 変数を追加する
		public void AddColumns(int n)
		{
			GLPK.glp_add_cols(problem, n);
		}
		// 変数の数
		public int ColumnsCount
		{
			get { return GLPK.glp_get_num_cols(problem); }
		}
		// 変数の名前
		public class columnName
		{
			MipProblem mip;
			public columnName(MipProblem mip)
			{
				this.mip = mip;
			}
			public string this[int index]
			{
				get { return GLPK.glp_get_col_name(mip.problem, index + 1); }
				set { GLPK.glp_set_col_name(mip.problem, index + 1, value); }
			}
		}
		public columnName ColumnName;
		// 変数の範囲を設定する
		public void SetColumnBounds(int index, BoundsType type, double lowerBound, double upperBound)
		{
			GLPK.glp_set_col_bnds(problem, index + 1, (int)type, lowerBound, upperBound);
		}
		// 変数の種類
		public class columnType
		{
			MipProblem mip;
			public columnType(MipProblem mip)
			{
				this.mip = mip;
			}
			public BoundsType this[int index]
			{
				get { return (BoundsType)GLPK.glp_get_col_type(mip.problem, index + 1); }
			}
		}
		public columnType ColumnType;
		// 変数の下限
		public class columnLb
		{
			MipProblem mip;
			public columnLb(MipProblem mip)
			{
				this.mip = mip;
			}
			public double this[int index]
			{
				get { return GLPK.glp_get_col_lb(mip.problem, index + 1); }
			}
		}
		public columnLb ColumnLB;
		// 変数の上限
		public class columnUb
		{
			MipProblem mip;
			public columnUb(MipProblem mip)
			{
				this.mip = mip;
			}
			public double this[int index]
			{
				get { return GLPK.glp_get_col_ub(mip.problem, index + 1); }
			}
		}
		public columnUb ColumnUB;
		// 変数条件
		public class columnKind
		{
			MipProblem mip;
			public columnKind(MipProblem mip)
			{
				this.mip = mip;
			}
			public VariableKind this[int index]
			{
				get { return (VariableKind)GLPK.glp_get_col_kind(mip.problem, index + 1); }
				set { GLPK.glp_set_col_kind(mip.problem, index + 1, (int)value); }
			}
		}
		public columnKind ColumnKind;
		// 変数の値
		public class lpColumnValue
		{
			MipProblem mip;
			public lpColumnValue(MipProblem mip)
			{
				this.mip = mip;
			}
			public double this[int index]
			{
				get { return GLPK.glp_get_col_prim(mip.problem, index + 1); }
			}
		}
		public lpColumnValue LpColumnValue;
		public class mipColumnValue
		{
			MipProblem mip;
			public mipColumnValue(MipProblem mip)
			{
				this.mip = mip;
			}
			public double this[int index]
			{
				get { return GLPK.glp_mip_col_val(mip.problem, index + 1); }
			}
		}
		public mipColumnValue MipColumnValue;
		#endregion
	}
}
