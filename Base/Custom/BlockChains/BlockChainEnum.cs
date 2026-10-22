namespace Base.Custom.BlockChains
{
    public class BlockChainEnum
    {
        public enum BlockChainId //mapped to db
        {
            XRPL = 1,
            StellarNetwork = 2
        }
        public static class NetworkTypeAlias
        {
            public const string XRPL = "xrpl";
        }
    }
}
