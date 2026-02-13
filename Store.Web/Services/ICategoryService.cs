using Store.Web.Models;

namespace Store.Web.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetCategoriesAsync();
    }
}
