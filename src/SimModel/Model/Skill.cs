using System;

namespace SimModel.Model
{
    // �X�L��
    public record Skill
    {

        // �X�L����
        public string Name { get; }

        // �X�L�����x��
        public int Level { get; set; } = 0;

        // �ǉ��X�L���t���O
        public bool IsAdditional { get; init; } = false;

        // �Œ茟���t���O
        public bool IsFixed { get; set; } = false;

        // �X�L���̃J�e�S��
        public string Category { get; init; }

        // �R���X�g���N�^
        public Skill(string name, int level, bool isAdditional = false, bool isFixed = false) : this(name, level, "", isAdditional, isFixed) { }

        public Skill(string name, int level, string category, bool isAdditional = false, bool isFixed = false)
        {
            Name = name;
            Level = level;
            IsAdditional = isAdditional;
            IsFixed = isFixed;
            Category = string.IsNullOrEmpty(category) ? @"������" : category;
        }

        // �ő僌�x��
        // �}�X�^�ɑ��݂��Ȃ��X�L���̏ꍇ0
        public int MaxLevel {
            get 
            {
                return Masters.SkillMaxLevel(Name);
            }
        }

        // �\���p������
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

        /// <summary>
        /// SkillPickerSelectorView��ComboBox�̕\���Ɏg���������Ԃ�
        /// </summary>
        public string PickerSelectorDisplayName => Level switch
        {
            0 => Name,
            _ => $"{Name}Lv{Level}"
        };
    }
}
