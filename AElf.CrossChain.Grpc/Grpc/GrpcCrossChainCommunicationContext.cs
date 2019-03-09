namespace AElf.CrossChain.Grpc
{
    public class GrpcCrossChainCommunicationContext : ICrossChainCommunicationContext
    {
        public string TargetIp { get; set; }
        public int TargetPort { get; set; }
        public int RemoteChainId { get; set; }
        public int SelfChainId { get; set; }
        public bool RemoteIsSideChain { get; set; }
        
        public string CertificateFileName { get; set; }
        
        public string ToUriStr()
        {
            return string.Join(":",TargetIp, TargetPort);
        }
    }
}