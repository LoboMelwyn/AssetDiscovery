using System.Collections.Generic;

namespace AssetDiscovery
{
    public interface IPremise
    {
        string Name { get; }
        bool IsComplete { get; }
        PremiseType Premise { get; }
        List<Scripts> ScriptList { set; }
        void Scan(string obj);
        event MachineInfoOutput NotifyMachineInfo;
    }
}