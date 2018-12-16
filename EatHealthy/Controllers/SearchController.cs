using EatHealthy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EatHealthy.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult Index()
        {
            if (Session["size"] == null)
            {
                Session["size"] = "5";
            }
            return View();
        }
        public ActionResult ClearSession()
        {
            Session.Clear();
            Session["size"] = "5";

            return RedirectToAction("Index");

        }
        
        public JsonResult GetData(string title)
        {
            Session["title"] = title;

            HttpWebRequest request;
            var size = (string)Session["size"];
            if ( size != "5")
                request = WebRequest.Create("https://api.edamam.com/search?q=" + title + "&app_id=cbfa085f&app_key=94cded2d09e272d2463ea08289c97958&from=0&to="+Session["size"]) as HttpWebRequest;
            else
                request = WebRequest.Create("https://api.edamam.com/search?q=" + title + "&app_id=cbfa085f&app_key=94cded2d09e272d2463ea08289c97958&from=0&to=5") as HttpWebRequest;
            
            var response = (HttpWebResponse)request.GetResponse();

            WebHeaderCollection header = response.Headers;

            var encoding = ASCIIEncoding.ASCII;
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
            {
                string responseText = reader.ReadToEnd();

                return Json(responseText, JsonRequestBehavior.AllowGet);
            }


            return Json("",JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetProduct(string title = "", string orderByKcal = "", string sortBySize = "",  string lower ="",string upper="")
        {
            if (!string.IsNullOrEmpty(sortBySize))
                Session["size"] = sortBySize;

            if (!string.IsNullOrEmpty(orderByKcal))
                Session["orderByKcal"] = orderByKcal;


            var oldSessionTitle = (string)Session["title"]??"";
            if (!string.IsNullOrEmpty(title))
                Session["title"] = title;


            if (!string.IsNullOrEmpty(lower))
                Session["lower"] = lower;


            if (!string.IsNullOrEmpty(upper))
                Session["upper"] = upper;


            var data = (RootObject)Session["data"];
            if ((data!=null&&data.hits.Count>0  ) && oldSessionTitle ==(string)Session["title"])
            {


                if (Convert.ToInt32(Session["size"]) > data.hits.Count)
                {
                    var json = GetData((string)Session["title"]);
                    data = new JavaScriptSerializer().Deserialize<RootObject>(json.Data.ToString());
                    Session["data"] = data;
                }



                if (!string.IsNullOrEmpty( (string)Session["lower"])  && !string.IsNullOrEmpty( (string)Session["upper"]))
                {
                   data.hits = data.hits.Where(
                        x => x.recipe.calories >= Convert.ToInt32(Session["lower"]) &&
                        x.recipe.calories <= Convert.ToInt32(Session["upper"])).ToList();
                }


                if (!string.IsNullOrEmpty((string)Session["size"]))
                {


                    if (string.IsNullOrEmpty((string)Session["title"]))
                    {

                    }
                    else
                    {

                        if ((string)Session["size"] == "5")
                        {
                            data.hits = data.hits.Take(5).ToList();

                        }
                        else if ((string)Session["size"] == "10")
                        {
                            data.hits = data.hits.Take(10).ToList();

                        }
                        else if ((string)Session["size"] == "50")
                        {
                            data.hits = data.hits.Take(50).ToList();

                        }

                    }



                }

                if (!string.IsNullOrEmpty((string)Session["orderByKcal"]))
                {

                    if ((string)Session["orderByKcal"] == "Kcal: high to low")
                    {
                        data.hits = data.hits.OrderByDescending(x => x.recipe.calories).ToList();

                    }
                    else if ((string)Session["orderByKcal"] == "Kcal: low to high")
                    {
                        data.hits = data.hits.OrderBy(x => x.recipe.calories).ToList();

                    }


                }
                Session["data"] = data;
            }
            else
            {
                var json = GetData((string)Session["title"]);

                data = new JavaScriptSerializer().Deserialize<RootObject>(json.Data.ToString());
                Session["data"] = data;
            }
            return View(data.hits);

        }

        public ActionResult GetDetail(string image,string label, 
            string healthLabels, string dietLabels, string ingredientLines, string kcal,
            string protein, string fat, string sugar
            )
               
        {

            Session["label"] = label;
            var ingredientLinesLs = ingredientLines?.Split(',') ?? new string[1];
            var dietLabeLs = dietLabels?.Split(',')?? new string [1];
            var healthLabelsLs = healthLabels?.Split(',') ?? new string[1];

            List<string> lstingredientLinesLs = ingredientLinesLs.OfType<string>().ToList(); // this isn't going to be fast.
            List<string> lsdietLabeLs = dietLabeLs.OfType<string>().ToList(); // this isn't going to be fast.
            List<string> lshealthLabelsLs = healthLabelsLs.OfType<string>().ToList(); // this isn't going to be fast.

            var model = new Recipe
            {
                label = label,
                image = image,
                calories = Convert.ToDouble(kcal),
                dietLabels = lsdietLabeLs,
                ingredientLines  = lstingredientLinesLs,
                healthLabels= lshealthLabelsLs,
                totalNutrients = new TotalNutrients
                {
                    FAT = new FAT { quantity = Convert.ToDouble(fat)},
                    PROCNT = new PROCNT { quantity = Convert.ToDouble(protein)},
                    SUGAR = new SUGAR { quantity = Convert.ToDouble(sugar)},
                }

            };


            return View(model);
        }

        public JsonResult AddBasket(string image, string label,
            string healthLabels, string dietLabels, string ingredientLines, string kcal,
            string protein, string fat, string sugar,int count=1)
        {
            var ingredientLinesLs = ingredientLines.Split(',');
            var dietLabeLs = dietLabels.Split(',');
            var healthLabelsLs = healthLabels.Split(',');

            List<string> lstingredientLinesLs = ingredientLinesLs.OfType<string>().ToList(); // this isn't going to be fast.
            List<string> lsdietLabeLs = dietLabeLs.OfType<string>().ToList(); // this isn't going to be fast.
            List<string> lshealthLabelsLs = healthLabelsLs.OfType<string>().ToList(); // this isn't going to be fast.
            for (int i = 0; i < count; i++)
            {
                var model = new Recipe
                {
                    label = label,
                    image = image,
                    calories = Convert.ToDouble(kcal),
                    dietLabels = lsdietLabeLs,
                    ingredientLines = lstingredientLinesLs,
                    healthLabels = lshealthLabelsLs,
                    totalNutrients = new TotalNutrients
                    {
                        FAT = new FAT { quantity = Convert.ToDouble(fat) },
                        PROCNT = new PROCNT { quantity = Convert.ToDouble(protein) },
                        SUGAR = new SUGAR { quantity = Convert.ToDouble(sugar) },
                    }

                };
                var basket = (List<Hit>)Session["Basket"];
                if (basket == null)
                {
                    basket = new List<Hit>();
                    basket.Add(new Hit { recipe = model });
                }
                else
                {
                    basket.Add(new Hit { recipe = model });
                }

                Session["Basket"] = basket;
            }

         

            return Json("ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBasket()
        {

            if (Session["Basket"] ==  null)
            {
                return View(new List<Hit>());
            }
            var model = (List<Hit>)Session["Basket"];
            if (model == null)
            {
                return View(new List<Hit>());
            }
            return View(model);
        }


        public ActionResult GetRelated()
        {
            var data = (RootObject)Session["data"];
            var label = (string)Session["label"];

            var model = data.hits.Where(x =>  x.recipe.label != label).ToList();
            

            return View(model);
        }

        public JsonResult DeletFromBasket(string label, string img)
         {
            var oldData = (List<Hit>)Session["Basket"];
            var newData = oldData.Where(x => x.recipe.label != label && x.recipe.image != img).ToList();
            Session["Basket"] = newData;
            return Json("ok", JsonRequestBehavior.AllowGet);
        }
    }
}