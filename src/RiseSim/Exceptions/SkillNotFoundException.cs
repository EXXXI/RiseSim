using System;

namespace RiseSim.Exceptions;

public class SkillNotFoundException : Exception
{
    public SkillNotFoundException(string skillName) : base($"{skillName}は存在しないスキルです") { }
}
