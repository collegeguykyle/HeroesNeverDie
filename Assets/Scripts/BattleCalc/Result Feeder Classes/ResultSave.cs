using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResultSave
{
    //This is used if the attack uses a saving throw to determine if something hits instead of a to Hit roll




}

public interface ITestSave
{
    public List<MagicSkillBonus> MagicSkillBonus { get; }
    public int GetMagicBonus();
}