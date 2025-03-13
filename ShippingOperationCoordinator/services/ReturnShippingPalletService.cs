using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ReturnShippingPalletService: IChangeShippingPalletService
{
    private readonly ILogger<ReturnShippingPalletService> _logger;
    private readonly IReturnShippingPalletSelector _returnShippingPalletSelector;
    private readonly IReturnShippingPalletService _returnShippingPalletService;
    private readonly ITakeShippingPalletSelector _takeShippingPalletSelector;

    public ReturnShippingPalletService(
        ILogger<ReturnShippingPalletService> logger,
        IReturnShippingPalletSelector returnShippingPalletSelector,
        IReturnShippingPalletService returnShippingPalletService,
        ITakeShippingPalletSelector takeShippingPalletSelector
    ) {
        _logger = logger;
        _returnShippingPalletSelector = returnShippingPalletSelector;
        _returnShippingPalletService = returnShippingPalletService;
        _takeShippingPalletSelector = takeShippingPalletSelector;
    }

    public bool Change(ShippingStationCode stationCode) {
        return Return(stationCode);
    }
    /// <summary>
    /// 出荷パレット返却
    /// 
    /// 仕掛パレット置き場に現在の一時置き場の状況で積み込み可能なパレットがあれば
    /// 出荷作業場所の状況を確認し返却可能な出荷パレットがあれば返却する
    /// </summary>
    /// <remarks>
    /// 確認順は逆のほうが良いかもしれないが、試算の環境では計算効率はもとめない。
    /// </remarks>
    public bool Return(ShippingStationCode stationCode) {
        _logger.LogInformation($"出荷パレット返却： 出荷作業場所[{stationCode}]");

        if (!_takeShippingPalletSelector.CheckEnableShippingPalletInShikakariStorage(stationCode)) {
            _logger.LogDebug("入れ替えに有効な仕掛パレット置き場に無いため在庫パレットの入れ替わりをまちます");
            return false;
        }
        var returnableLocation = _returnShippingPalletSelector.SelectReturnShippingPallet(stationCode);
        if (returnableLocation == null) {
            _logger.LogTrace("返却可能な出荷パレットが見つかりませんでした");
            return false;
        }
        _returnShippingPalletService.Request(returnableLocation);
        return true;
    }
}
