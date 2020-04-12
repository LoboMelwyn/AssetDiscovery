namespace AssetDiscovery
{
    public interface IPremise
    {
        string Name { get; }
        bool IsComplete { get; }
        PremiseType Premise { get; }
        void Scan(string obj);
        event MachineInfoOutput NotifyMachineInfo;
    }
}