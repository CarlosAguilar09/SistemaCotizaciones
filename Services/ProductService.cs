using SistemaCotizaciones.Models;
using SistemaCotizaciones.Repositories;

namespace SistemaCotizaciones.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepo = new();

        public List<Product> GetAll()
        {
            return _productRepo.GetAll();
        }

        public List<Product> GetByType(string type)
        {
            return _productRepo.GetByType(type);
        }

        public Product? GetById(int id)
        {
            return _productRepo.GetById(id);
        }

        public void Add(Product product)
        {
            _productRepo.Add(product);
        }

        public void Update(Product product)
        {
            _productRepo.Update(product);
        }

        public void Delete(int id)
        {
            _productRepo.Delete(id);
        }
    }
}
