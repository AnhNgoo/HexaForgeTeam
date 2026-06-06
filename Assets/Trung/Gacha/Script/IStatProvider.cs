using System.Collections.Generic;

public interface IStatProvider
{
    Dictionary<RuneStatType, float>
        GetStats();
}