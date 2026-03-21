using SistemaCotizaciones.Models;
using SistemaCotizaciones.Repositories;

namespace SistemaCotizaciones.Services
{
    public class ClienteService
    {
        private readonly ClienteRepository _clienteRepo = new();

        public List<Cliente> GetAll()
        {
            return _clienteRepo.GetAll();
        }

        public Cliente? GetById(int id)
        {
            return _clienteRepo.GetById(id);
        }

        public void Add(Cliente cliente)
        {
            _clienteRepo.Add(cliente);
        }

        public void Update(Cliente cliente)
        {
            _clienteRepo.Update(cliente);
        }

        public void Delete(int id)
        {
            _clienteRepo.Delete(id);
        }

        public List<Cliente> Search(string query)
        {
            return _clienteRepo.Search(query);
        }
    }
}
