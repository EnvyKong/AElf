using System.Collections.Generic;
using System.IO;
using AElf.Common;
using AElf.Contracts.Genesis;
using AElf.Database;
using AElf.Kernel.Blockchain.Application;
using AElf.Kernel.Infrastructure;
using AElf.Kernel.SmartContract.Application;
using AElf.Modularity;
using AElf.Runtime.CSharp;
using AElf.TestBase;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace AElf.Kernel.Consensus.DPoS.Tests
{
    [DependsOn(
        typeof(TestBaseKernelAElfModule),
        typeof(DPoSConsensusAElfModule),
        typeof(KernelAElfModule),
        typeof(CSharpRuntimeAElfModule)
    )]
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DPoSConsensusTestAElfModule : AElfModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            var contractZero = typeof(BasicContractZero);
            var code = File.ReadAllBytes(contractZero.Assembly.Location);
            var provider = context.ServiceProvider.GetService<IDefaultContractZeroCodeProvider>();
            provider.DefaultContractZeroRegistration = new SmartContractRegistration
            {
                Category = 2,
                Code = ByteString.CopyFrom(code),
                CodeHash = Hash.FromRawBytes(code)
            };
        }
    }
}