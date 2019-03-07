using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AElf.Common;
using AElf.Kernel.Blockchain.Application;
using AElf.Kernel.Blockchain.Domain;
using AElf.Kernel.SmartContract.Domain;
using AElf.Kernel.SmartContractExecution.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;

namespace AElf.Kernel.SmartContractExecution.Application
{
    public class BlockExecutingService : IBlockExecutingService, ITransientDependency
    {
        private readonly ITransactionExecutingService _executingService;
        private readonly IBlockManager _blockManager;
        private readonly IBlockchainStateManager _blockchainStateManager;
        private readonly IBlockGenerationService _blockGenerationService;
        public ILogger<BlockExecutingService> Logger { get; set; }
        
        public BlockExecutingService(ITransactionExecutingService executingService, IBlockManager blockManager,
            IBlockchainStateManager blockchainStateManager, IBlockGenerationService blockGenerationService)
        {
            Logger = NullLogger<BlockExecutingService>.Instance;
            _executingService = executingService;
            _blockManager = blockManager;
            _blockchainStateManager = blockchainStateManager;
            _blockGenerationService = blockGenerationService;

        }

        public async Task<Block> ExecuteBlockAsync(BlockHeader blockHeader,
            IEnumerable<Transaction> nonCancellableTransactions)
        {
            return await ExecuteBlockAsync(blockHeader, nonCancellableTransactions, new List<Transaction>(),
                CancellationToken.None);
        }

        public async Task<Block> ExecuteBlockAsync(BlockHeader blockHeader,
            IEnumerable<Transaction> nonCancellableTransactions, IEnumerable<Transaction> cancellableTransactions,
            CancellationToken cancellationToken)
        {
            // TODO: If already executed, don't execute again. Maybe check blockStateSet?

            var nonCancellable = nonCancellableTransactions.ToList();
            var cancellable = cancellableTransactions.ToList();

            var chainContext = new ChainContext()
            {
                BlockHash = blockHeader.PreviousBlockHash,
                BlockHeight = blockHeader.Height - 1
            };

            var blockStateSet = new BlockStateSet()
            {
                BlockHeight = blockHeader.Height,
                PreviousHash = blockHeader.PreviousBlockHash
            };
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            var nonCancellableReturnSets =
                await _executingService.ExecuteAsync(blockHeader, nonCancellable, CancellationToken.None);
            stopwatch.Stop();
            Logger.LogInformation($"[Performance]-ExecuteAsync-nonCancellable duration: {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            var cancellableReturnSets =
                await _executingService.ExecuteAsync(blockHeader, cancellable, cancellationToken);
            stopwatch.Stop();
            Logger.LogInformation($"[Performance]-ExecuteAsync-cancellable duration: {stopwatch.ElapsedMilliseconds}");

            foreach (var returnSet in nonCancellableReturnSets)
            {
                foreach (var change in returnSet.StateChanges)
                {
                    blockStateSet.Changes[change.Key] = change.Value;
                }
            }

            foreach (var returnSet in cancellableReturnSets)
            {
                foreach (var change in returnSet.StateChanges)
                {
                    blockStateSet.Changes[change.Key] = change.Value;
                }
            }

            // TODO: Insert deferredTransactions to TxPool
            var executed = new HashSet<Hash>(cancellableReturnSets.Select(x => x.TransactionId));
            var allExecutedTransactions =
                nonCancellable.Concat(cancellable.Where(x => executed.Contains(x.GetHash()))).ToList();
            var merkleTreeRootOfWorldState = ComputeHash(GetDeterministicByteArrays(blockStateSet));

            stopwatch.Restart();
            var block = await _blockGenerationService.FillBlockAfterExecutionAsync(blockHeader, allExecutedTransactions,
                merkleTreeRootOfWorldState);
            stopwatch.Stop();
            Logger.LogInformation($"[Performance]-FillBlockAfterExecutionAsync duration: {stopwatch.ElapsedMilliseconds}");

            blockStateSet.BlockHash = blockHeader.GetHash();

            stopwatch.Restart();
            await _blockchainStateManager.SetBlockStateSetAsync(blockStateSet);
            stopwatch.Stop();
            Logger.LogInformation($"[Performance]-SetBlockStateSetAsync duration: {stopwatch.ElapsedMilliseconds}");

            return block;
        }

        private IEnumerable<byte[]> GetDeterministicByteArrays(BlockStateSet blockStateSet)
        {
            var keys = blockStateSet.Changes.Keys;
            foreach (var k in new SortedSet<string>(keys))
            {
                yield return Encoding.UTF8.GetBytes(k);
                yield return blockStateSet.Changes[k].ToByteArray();
            }
        }

        private Hash ComputeHash(IEnumerable<byte[]> byteArrays)
        {
            using (var hashAlgorithm = SHA256.Create())
            {
                foreach (var bytes in byteArrays)
                {
                    hashAlgorithm.TransformBlock(bytes, 0, bytes.Length, null, 0);
                }

                hashAlgorithm.TransformFinalBlock(new byte[0], 0, 0);
                return Hash.LoadByteArray(hashAlgorithm.Hash);
            }
        }
    }
}