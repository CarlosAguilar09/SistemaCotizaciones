using SistemaCotizaciones.Models;
using SistemaCotizaciones.Repositories;

namespace SistemaCotizaciones.Services
{
    public class AreaPricingPresetService
    {
        private readonly AreaPricingPresetRepository _repository = new();

        public List<AreaPricingPreset> GetAll() => _repository.GetAll();

        public AreaPricingPreset? GetById(int id) => _repository.GetById(id);

        public void Save(AreaPricingPreset preset)
        {
            if (preset.Id == 0)
                _repository.Add(preset);
            else
                _repository.Update(preset);
        }

        public void Delete(int id) => _repository.Delete(id);
    }
}
