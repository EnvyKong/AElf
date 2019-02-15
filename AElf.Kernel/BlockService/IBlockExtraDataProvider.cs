using System.Threading.Tasks;

namespace AElf.Kernel.BlockService
{
    public interface IBlockExtraDataProvider
    {
        Task FillExtraData(Block block);
        Task<bool> ValidateExtraData(Block block);
    }
}