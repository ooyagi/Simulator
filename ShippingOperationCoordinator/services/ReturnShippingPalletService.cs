using Microsoft.Extensions.Logging;
using CommonItems.Models;
using ShippingOperationCoordinator.Interfaces;

namespace ShippingOperationCoordinator.Services;

class ReturnShippingPalletService: IChangeShippingPalletService
{
    private readonly ILogger<ReturnShippingPalletService> _logger;
    private readonly IShippingStorageLoader _shippingStorageLoader;
    private readonly IReturnShippingPalletSelector _returnShippingPalletSelector;
    private readonly ITakeShippingPalletSelector _takeShippingPalletSelector;
    private readonly IReturnShippingPalletService _returnShippingPalletService;
    private readonly ITakeShippingPalletService _takeShippingPalletService;

    public ReturnShippingPalletService(
        ILogger<ReturnShippingPalletService> logger,
        IShippingStorageLoader shippingStorageLoader,
        IReturnShippingPalletSelector returnShippingPalletSelector,
        ITakeShippingPalletSelector takeShippingPalletSelector,
        IReturnShippingPalletService returnShippingPalletService,
        ITakeShippingPalletService takeShippingPalletService
    ) {
        _logger = logger;
        _shippingStorageLoader = shippingStorageLoader;
        _returnShippingPalletSelector = returnShippingPalletSelector;
        _takeShippingPalletSelector = takeShippingPalletSelector;
        _returnShippingPalletService = returnShippingPalletService;
        _takeShippingPalletService = takeShippingPalletService;
    }

    public bool TakeInEmptyLocation(ShippingStationCode stationCode) {
        var emptyLocations = _shippingStorageLoader.GetEmptyLocationCodes(stationCode);
        var empty = emptyLocations.Any();
        if (empty && _takeShippingPalletSelector.CheckEnableShippingPalletInShikakariStorage(stationCode)) {
            // 出荷パレット置き場に空きがあり、仕掛パレット置き場に取り寄せるべきパレットがあれば取り寄せを行う
            _takeShippingPalletService.Take(stationCode);
            return true;
        } else if (empty && _takeShippingPalletSelector.SelectInitialShippingPallet(stationCode, emptyLocations).Any()) {
            // 出荷パレット置き場に空きがあり、仕掛パレット置き場に取り寄せるべきパレットがない場合も
            // 取り寄せ可能な取り敢えず取り寄せを行う
            _takeShippingPalletService.TakeInitialPallets(stationCode);
            return true;
        }
        return false;
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

        var returnableLocation = _returnShippingPalletSelector.SelectReturnShippingPallet(stationCode);
        if (returnableLocation == null) {
            _logger.LogTrace("返却可能な出荷パレットが見つかりませんでした");
            return false;
        }
        _returnShippingPalletService.Request(returnableLocation);
        return true;
    }
}
