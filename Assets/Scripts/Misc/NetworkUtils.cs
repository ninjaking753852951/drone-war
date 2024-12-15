public static class NetworkUtils
{
    public static int NetworkMachineID(ulong clientID, int localMachineID) => (int)clientID * 100 + localMachineID;
}
