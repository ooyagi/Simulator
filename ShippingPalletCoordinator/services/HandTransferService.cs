using System.Security.Cryptography.X509Certificates;
using CommonItems.Models;
using Microsoft.Extensions.Logging;
using ShippingPalletCoordinator.Interfaces;
using ShippingPalletCoordinator.Models;
using ShippingPalletCoordinator.Services;

namespace ShippingPalletCoordinator.Services;

/// <summary>
/// 手動乗せ換えサービス
/// 
/// 本来は ShippingOperationCoordinator に属するが、今回は搬送回数の試算が目的であり
/// 手動の乗せ換えは仕掛パレット置き場の特定品番を乗せ換え完了にするだけなので
/// 暫定的に仕掛パレット側で行う
/// </summary>
class HandTransferService: IHandTransferService
{
    private readonly ILogger<HandTransferService> _logger;
    private readonly IShippingPalletCoordinatorDbContext _context;
    private readonly IShikakariStorageLoader _shikakariStorageLoader;
    private readonly IRotateShippingPalletService _rotateShippingPalletService;

    public HandTransferService(
        ILogger<HandTransferService> logger,
        IShippingPalletCoordinatorDbContext context,
        IShikakariStorageLoader shikakariStorageLoader,
        IRotateShippingPalletService rotateShippingPalletService
    ) {
        _logger = logger;
        _context = context;
        _shikakariStorageLoader = shikakariStorageLoader;
        _rotateShippingPalletService = rotateShippingPalletService;
    }

    public bool ExecuteTransfer(IEnumerable<Hinban> storableHintans) {
        _logger.LogInformation("手動乗せ換えを実行します");
        var res = false;
        try {
            var shikakariPallets = _shikakariStorageLoader.All().Where(x => x.StoredPallet != null).Select(x => x.StoredPallet);
            if (!shikakariPallets.Any()) {
                return false;
            }
            foreach (var pallet in shikakariPallets) {
                res = res || TransferEachPallet(pallet, storableHintans);
                if (pallet.IsCompleted) {
                    _rotateShippingPalletService.Rotate(pallet.Id);
                }
            }
            return res;
        } catch (Exception ex) {
            _logger.LogError(ex, "手動乗せ換えに失敗しました");
            return false;
        }
    }
    private bool TransferEachPallet(ShippingPallet pallet, IEnumerable<Hinban> storableHintans) {
        bool res = false;
        foreach (var item in pallet.Items.Where(x => x.IsCompleted == false)) {
            if (storableHintans.Any(x => x == item.Hinban)) {
                break;
            }
            _logger.LogTrace($"品番 [{item.Hinban.Value}] を手動換え");
            item.Complete();
            res = true;
        }
        _context.SaveChanges();
        return res;
    }
}
