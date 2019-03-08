using AElf.CrossChain.Cache;
using AElf.CrossChain.Grpc;
using AElf.CrossChain.Grpc.Server;
using AElf.Kernel;
using AElf.Kernel.Blockchain.Application;
using AElf.Kernel.Miner.Application;
using AElf.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace AElf.CrossChain
 {
     [DependsOn(typeof(KernelAElfModule))]
     public class CrossChainAElfModule : AElfModule
     {
         public override void ConfigureServices(ServiceConfigurationContext context)
         {
             var services = context.Services;
             services.AddSingleton<CrossChainBlockDataRpcServer>();
             
             var configuration = context.Services.GetConfiguration();
             Configure<GrpcConfigOption>(configuration.GetSection("CrossChain"));
             services.AddSingleton<ICrossChainDataProvider, CrossChainDataProvider>();
             services.AddScoped<ISystemTransactionGenerator, CrossChainIndexingTransactionGenerator>();
             services.AddScoped<IBlockExtraDataProvider, CrossChainExtraDataProvider>();
             services.AddScoped<IBlockValidationProvider, CrossChainValidationProvider>();
             services.AddSingleton<ICrossChainService, CrossChainService>();
         }
         
         public override void OnApplicationInitialization(ApplicationInitializationContext context)
         {
//             var opt = context.ServiceProvider.GetService<IOptionsSnapshot<GrpcConfigOption>>().Value;

//             var clientService = context.ServiceProvider.GetService<GrpcProducerConsumerService>();
//             // Init client connected to parent chain if it exists.
//             clientService.Init(opt.CertificateDir);
//             if (!string.IsNullOrEmpty(opt.ParentChainId) && !string.IsNullOrEmpty(opt.ParentChainNodeIp) &&
//                 !string.IsNullOrEmpty(opt.ParentChainPort)) return;
//             var blockInfoCache = new BlockInfoCache(opt.ParentChainId.ConvertBase58ToChainId());
//             clientService.CreateConsumerProducer(new CrossChainDataProducer
//             {
//                 TargetIp = opt.ParentChainNodeIp,
//                 TargetPort = uint.Parse(opt.ParentChainPort),
//                 SideChainId = opt.ParentChainId.ConvertBase58ToChainId(),
//                 TargetIsSideChain = false,
//                 BlockInfoCache = blockInfoCache
//             });
//             context.ServiceProvider.GetService<CrossChainDataProvider>().ParentChainBlockInfoCache =
//                 blockInfoCache;
         }
     }
 }