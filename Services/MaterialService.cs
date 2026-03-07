using SistemaCotizaciones.Models;
using SistemaCotizaciones.Repositories;

namespace SistemaCotizaciones.Services
{
    public class MaterialService
    {
        private readonly MaterialRepository _materialRepo = new();

        public List<Material> GetAll()
        {
            return _materialRepo.GetAll();
        }

        public Material? GetById(int id)
        {
            return _materialRepo.GetById(id);
        }

        public void Add(Material material)
        {
            _materialRepo.Add(material);
        }

        public void Update(Material material)
        {
            _materialRepo.Update(material);
        }

        public void Delete(int id)
        {
            _materialRepo.Delete(id);
        }
    }
}
