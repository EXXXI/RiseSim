using System.Collections.Generic;
using System.Linq;

namespace SimModel.Model
{
    /// <summary>
    /// �X�L��
    /// </summary>
    public record Skill
    {

        /// <summary>
        /// �X�L����
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// �X�L�����x��
        /// </summary>
        public int Level { get; set; } = 0;

        /// <summary>
        /// �ǉ��X�L���t���O
        /// </summary>
        public bool IsAdditional { get; init; } = false;

        /// <summary>
        /// �Œ茟���t���O
        /// </summary>
        public bool IsFixed { get; set; } = false;

        /// <summary>
        /// �X�L���̃J�e�S��
        /// </summary>
        public string Category { get; init; }

        /// <summary>
        /// �V���[�Y�X�L�����A���x���ɓ���Ȗ��̂�����ꍇ�����Ɋi�[
        /// </summary>
        public Dictionary<int, string> SpecificNames { get; } = new();

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="name">�X�L����</param>
        /// <param name="level">���x��</param>
        /// <param name="isAdditional">�ǉ��X�L�����ǂ����̃t���O</param>
        /// <param name="isFixed">�Œ茟���t���O</param>
        public Skill(string name, int level, bool isAdditional = false, bool isFixed = false) 
            : this(name, level, Masters.Skills.Where(s => s.Name == name).FirstOrDefault()?.Category, isAdditional, isFixed) { }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="name">�X�L����</param>
        /// <param name="level">���x��</param>
        /// <param name="category">�J�e�S��</param>
        /// <param name="isAdditional">�ǉ��X�L�����ǂ����̃t���O</param>
        /// <param name="isFixed">�Œ茟���t���O</param>
        public Skill(string name, int level, string? category, bool isAdditional = false, bool isFixed = false)
        {
            Name = name;
            Level = level;
            IsAdditional = isAdditional;
            IsFixed = isFixed;
            Category = string.IsNullOrEmpty(category) ? @"������" : category;
        }

        /// <summary>
        /// �ő僌�x��
        /// �}�X�^�ɑ��݂��Ȃ��X�L���̏ꍇ0
        /// </summary>
        public int MaxLevel {
            get 
            {
                return Masters.SkillMaxLevel(Name);
            }
        }

        /// <summary>
        /// �\���p������
        /// </summary>
        public string Description
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name) || Level == 0)
                {
                    return string.Empty;
                }

                return (IsAdditional ? "(�ǉ�)" : string.Empty) + Name + "Lv" + Level;
            }
        }
    }
}
