namespace AElf.Synchronization.BlockExecution
{
    // ReSharper disable InconsistentNaming
    public enum BlockExecutionResult
    {
        // Oh yes
        Success = 1,
        PrepareSuccess,
        CollectTransactionsSuccess,
        UpdateWorldStateSuccess,

        // Add to cache
        //     Haven't appended yet, can execute again
        InvalidSideChainInfo = 11,
        InvalidParentChainBlockInfo,
        //     Simply cache
        ExecutionCancelled = 51,
        BlockIsNull,
        NoTransaction,
        TooManyTxsForParentChainBlock,
        NotExecuted,
        AlreadyReceived,
        Expelled,
        IncorrectStateMerkleTree,
        FutureBlock,
        AlreadyAppended,
        Terminated,
        Mining,

        // Need to rollback
        Fatal = 101
    }

    public static class ExecutionResultExtensions
    {
        public static bool IsSuccess(this BlockExecutionResult result)
        {
            return (int) result < 11;
        }

        public static bool CanExecuteAgain(this BlockExecutionResult result)
        {
            return (int) result > 10;
        }

        public static bool IsFailed(this BlockExecutionResult result)
        {
            return (int) result > 10;
        }
        
        public static bool CannotExecute(this BlockExecutionResult result)
        {
            return (int) result > 50;
        }

        public static bool NeedToRollback(this BlockExecutionResult result)
        {
            return (int) result > 100;
        }
    }
}