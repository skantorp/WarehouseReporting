using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.DTOs;
using Services.RequestDTOs;
using System;
using System.Linq;
using UnitOfWork;

namespace Services
{
    public class DbService : UnitOfWorkDomainService, IDbService
    {
        private readonly IRepository<CategoryOfGood> _categoryOfGoodsRepository;
        private readonly IRepository<Currency> _currenciesRepository;
        private readonly IRepository<Good> _goodsRepository;
        private readonly IRepository<Warehouse> _warehousesRepository;
        private readonly IRepository<GoodWarehouse> _goodWarehousesRepository;

        public DbService(IUnitOfWork unitOfWork,
            IRepository<CategoryOfGood> categoryOfGoodsRepository,
            IRepository<Currency> currenciesRepository,
            IRepository<Good> goodsRepository,
            IRepository<Warehouse> warehousesRepository,
            IRepository<GoodWarehouse> goodWarehousesRepository) : base(unitOfWork)
        {
            _currenciesRepository = currenciesRepository;
            _categoryOfGoodsRepository = categoryOfGoodsRepository;
            _goodsRepository = goodsRepository;
            _warehousesRepository = warehousesRepository;
            _goodWarehousesRepository = goodWarehousesRepository;
        }

        public int[] GetCurrencyIds()
        {
            return _currenciesRepository.GetAll().Select(x => x.Id).ToArray();
        }

        public int[] GetGoodIds()
        {
            return _goodsRepository.GetAll().Select(x => x.Id).ToArray();
        }

        public int[] GetWarehouseIds()
        {
            return _warehousesRepository.GetAll().Select(x => x.Id).ToArray();
        }

        public CurrencyDto GetCurrencyById(int id)
        {
            var currency = _currenciesRepository.GetFirstOrDefault(x => x.Id == id);

            if (currency == null)
                return null;

            return new CurrencyDto
            {
                Id = currency.Id,
                Code = currency.Code,
                Name = currency.Name,
                ExchangeRate = currency.ExchangeRate,
                UpdateDate = currency.UpdateDate
            };
        }

        public void UpdateCurrencies(SaveCurrencyRequestDto[] currencies)
        {
            var hryvniaCurrency = _currenciesRepository.GetFirstOrDefault(x => x.Code == CurrencyCode.Hryvnia);
            var dollarCurrency = currencies.FirstOrDefault(x => x.Code == CurrencyCode.Dollar);
            var uahToUsdExchangeRate = dollarCurrency.UAHExchangeRate;

            var allGoods = _goodsRepository.GetAll().Include(x => x.Currency).ToArray();
            foreach (var good in allGoods)
            {
                if (good.Currency.Code != CurrencyCode.Hryvnia)
                {
                    var goodsUpdCurrency = currencies.FirstOrDefault(x => x.Code == good.Currency.Code);
                    double newExchangeRate = uahToUsdExchangeRate / goodsUpdCurrency.UAHExchangeRate;
                    double newPrice = good.Price * newExchangeRate / good.Currency.ExchangeRate;
                    good.Price = newPrice;
                }
                else
                {
                    double newPrice = uahToUsdExchangeRate * good.Price / good.Currency.ExchangeRate;
                    good.Price = newPrice;
                }

                good.BasePrice = uahToUsdExchangeRate * good.BasePrice / hryvniaCurrency.ExchangeRate;
            }

            foreach (var saveCurrencyRequestDto in currencies)
            {
                var currency = _currenciesRepository.GetFirstOrDefault(x => x.Code == saveCurrencyRequestDto.Code);

                if (currency != null)
                {
                    currency.ExchangeRate = uahToUsdExchangeRate / saveCurrencyRequestDto.UAHExchangeRate;
                    currency.UpdateDate = DateTime.Now;
                }
            }

            if (hryvniaCurrency != null)
            {
                hryvniaCurrency.ExchangeRate = uahToUsdExchangeRate;
                hryvniaCurrency.UpdateDate = DateTime.Now;
            }

            UnitOfWork.SaveChanges();
        }

        public DictionaryItemDto[] GetCategories()
        {
            return _categoryOfGoodsRepository.GetAll().Select(x => new DictionaryItemDto
            {
                Id = x.Id,
                Name = x.Name
            }).ToArray();
        }

        public GoodDto GetGoodById(int id)
        {
            var good = _goodsRepository.GetFirstOrDefault(x => x.Id == id);

            if (good == null)
                return null;

            return new GoodDto
            {
                Id = good.Id,
                BasePrice = good.BasePrice,
                BarCodeNumber = good.BarCodeNumber,
                CategoryId = good.CategoryId,
                Price = good.Price,
                CurrencyId = good.CurrencyId,
                Name = good.Name
            };
        }

        public WarehouseDto GetWarehouseById(int id)
        {
            var warehouse = _warehousesRepository.GetAll().Include(x => x.GoodWarehouses).FirstOrDefault(x => x.Id == id);

            if (warehouse == null)
                return null;

            return new WarehouseDto
            {
                Id = warehouse.Id,
                Name = warehouse.Name,
                Address = warehouse.Address,
                Goods = warehouse.GoodWarehouses.Select(x => new WarehouseGoodDto
                {
                    Id = x.Id,
                    Amount = x.Amount,
                    GoodId = x.GoodId
                }).ToArray()
            };
        }

        public void SaveGood(SaveGoodDto saveGoodDto)
        {
            Good good = _goodsRepository.GetFirstOrDefault(x => x.Id == saveGoodDto.Id);

            if (good == null)
            {
                good = new Good();
                _goodsRepository.Add(good);
            }

            good.CategoryId = saveGoodDto.CategoryId;
            good.Price = saveGoodDto.Price;
            good.Name = saveGoodDto.Name;
            good.CurrencyId = saveGoodDto.CurrencyId;
            good.BarCodeNumber = GenerateGoodsNumber();

            var currency = GetCurrencyById(saveGoodDto.CurrencyId);
            good.BasePrice = saveGoodDto.Price / currency.ExchangeRate;

            UnitOfWork.SaveChanges();
        }

        public void SaveWarehouse(SaveWarehouseDto saveWarehouseDto)
        {
            var warehouse = _warehousesRepository.GetAll().Include(x => x.GoodWarehouses).FirstOrDefault(x => x.Id == saveWarehouseDto.Id);

            if (warehouse == null)
            {
                warehouse = new Warehouse();
                _warehousesRepository.Add(warehouse);
            }
            warehouse.Name = saveWarehouseDto.Name;
            warehouse.Address = saveWarehouseDto.Address;
            foreach (var goodWarehouse in warehouse.GoodWarehouses.ToArray())
            {
                if (saveWarehouseDto.Goods.All(x => x.GoodId != goodWarehouse.GoodId))
                    _goodWarehousesRepository.Delete(goodWarehouse);
            }
            foreach (var good in saveWarehouseDto.Goods)
            {
                var goodWarehouse = warehouse.GoodWarehouses.FirstOrDefault(x => x.GoodId == good.GoodId);

                if (goodWarehouse == null)
                {
                    goodWarehouse = new GoodWarehouse
                    {
                        GoodId = good.GoodId,
                        Amount = good.Amount,
                        Warehouse = warehouse
                    };
                    _goodWarehousesRepository.Add(goodWarehouse);
                }
            }

            UnitOfWork.SaveChanges();
        }

        public void DeleteWarehouse(int id)
        {
            var warehouse = _warehousesRepository.GetAll().Include(x => x.GoodWarehouses).FirstOrDefault(x => x.Id == id);

            foreach (var goodWarehouse in warehouse.GoodWarehouses.ToArray())
                _goodWarehousesRepository.Delete(goodWarehouse);

            _warehousesRepository.Delete(warehouse);
            UnitOfWork.SaveChanges();
        }

        int GenerateGoodsNumber()
        {
            var codes = _goodsRepository.GetAll().Select(x => x.BarCodeNumber).ToArray();
            var code = new Random().Next(10000000, 99999999);

            if (codes.Contains(code))
                return GenerateGoodsNumber();

            return code;
        }
    }

    public interface IDbService
    {
        void UpdateCurrencies(SaveCurrencyRequestDto[] currencies);
        int[] GetCurrencyIds();
        int[] GetWarehouseIds();
        CurrencyDto GetCurrencyById(int id);
        DictionaryItemDto[] GetCategories();
        GoodDto GetGoodById(int id);
        int[] GetGoodIds();
        WarehouseDto GetWarehouseById(int id);
        void SaveWarehouse(SaveWarehouseDto saveWarehouseDto);
        void DeleteWarehouse(int id);
        void SaveGood(SaveGoodDto saveGoodDto);
    }
}
