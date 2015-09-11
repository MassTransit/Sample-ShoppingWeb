using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shopping.Web.Controllers
{
    using System.Threading.Tasks;
    using Contracts;
    using ViewModels;


    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit(AddItemViewModel model)
        {
            await MvcApplication.Bus.Publish<CartItemAdded>(new
            {
                UserName = model.UserName ?? "Unknown",
                Timestamp = DateTime.UtcNow,

            });

            return View("Index");
        }
    }
}