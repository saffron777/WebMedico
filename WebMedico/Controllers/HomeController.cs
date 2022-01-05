using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using WebMedico.Models;
using IFarmacias.Models;
using IFarmacias.Models.Response;
using IFarmacias.Models.Enums;
using WebMedico.Models.Consults;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR;

namespace WebMedico.Controllers
{
    [System.Web.Mvc.Authorize]
    public class HomeController : Controller
    {

        public async Task<ActionResult> Index()
        {
            //var queries = new List<QueriesViewModel>();
            //return View(queries);

            //if (MySession.Current.Doctor == null)
            //    return RedirectToAction("Login", "Account");


            /*
             * PARA SALTARME EL LOGIN
             */

            var queries = new List<ConsultasResponse>();
            var idDoctor = (HttpContext.User as CustomPrincipal).IdDoctor;

            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idDoctor}";
            using (var client = new HttpClient())
            {
                var wcfResponse = await client.GetAsync(url);
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var jsonConsultas = wcfResponse.Content.ReadAsStringAsync().Result;
                    queries = JsonConvert.DeserializeObject<List<ConsultasResponse>>(jsonConsultas);
                }
            }
            return View(queries);
        }
        // MED-99 SignalR
        /// <summary>
        /// Method to list all consultations
        /// </summary>
        /// <returns>partial view</returns>
        public async Task<ActionResult> GetConsultationDashBoard()
        {
            try
            {
                var queries = new List<ConsultasResponse>();
                var idDoctor = (HttpContext.User as CustomPrincipal).IdDoctor;

                string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idDoctor}";
                using (var client = new HttpClient())
                {
                    var wcfResponse = await client.GetAsync(url);
                    if (wcfResponse.IsSuccessStatusCode)
                    {
                        var jsonConsultas = wcfResponse.Content.ReadAsStringAsync().Result;
                        queries = JsonConvert.DeserializeObject<List<ConsultasResponse>>(jsonConsultas);
                    }
                }

                return PartialView("~/Views/Home/_ConsultaDashBoard.cshtml", queries);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        // MED-78 Consume the Methods for list Notifications
        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> ListNotifications(string idDoctor)
        {
            var result = string.Empty;
            string url = $"https://doctorwebservice.azurewebsites.net/api/Notifications?idDoctor={idDoctor}";
            using (var client = new HttpClient())
            {
                var wcfResponse = await client.GetAsync(url);
                if (wcfResponse.IsSuccessStatusCode)
                {
                    result = wcfResponse.Content.ReadAsStringAsync().Result;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        // MED-79 Consume the Methods for list Messages
        [AcceptVerbs(HttpVerbs.Get)]
        public async Task<ActionResult> ListMessages(string idDoctor)
        {
            var result = string.Empty;
            string url = $"https://doctorwebservice.azurewebsites.net/api/Messages?idDoctor={idDoctor}";
            using (var client = new HttpClient())
            {
                var wcfResponse = await client.GetAsync(url);
                if (wcfResponse.IsSuccessStatusCode)
                {
                    result = wcfResponse.Content.ReadAsStringAsync().Result;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Para hacer assumir la consulta. Es PUT: /api/Consultas
        public async Task<ActionResult> AssumeConsultation(string idConsulta)
        {
            var url = "https://doctorwebservice.azurewebsites.net/api/Consultas/Assume";
            var loginJson = new JavaScriptSerializer().Serialize(new
            {
                IdConsultas = idConsulta,
                IdDoctor = (HttpContext.User as CustomPrincipal).IdDoctor,
                Description = "",
                Status = ConsultationStatus.InProgress
            });

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PutAsync(url, new StringContent(loginJson, Encoding.UTF8, "application/json"));
                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    //Call to signalr method to dashboard update
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<Hubs.DashBoardHub>();
                    hubContext.Clients.All.refreshTable();

                    return new EmptyResult();
                }
            }

            throw new Exception();
        }

        public async Task<ActionResult> ConsultDetail(string idConsult)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsult}/Patient";
            using (var client = new HttpClient())
            {
                var wcfResponse = await client.GetAsync(url);
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<BasicInfoViewModel>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    details.IdConsult = idConsult;
                    return View(details);
                }
            }

            return View();
        }

        public async Task<ActionResult> MantenerConsult(string idConsult)
        {
            const string url = "https://doctorwebservice.azurewebsites.net/api/Consultas/ChangeStatusConsultation";
            var loginJson = new JavaScriptSerializer().Serialize(new
            {
                IdConsultas = idConsult,
                Status = ConsultationStatus.Open
            });

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PostAsync(url, new StringContent(loginJson, Encoding.UTF8, "application/json"));
                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            throw new Exception();
        }

        public async Task<ActionResult> ArchivarConsult(string idConsult)
        {
            const string url = "https://doctorwebservice.azurewebsites.net/api/Consultas/ChangeStatusConsultation";
            var loginJson = new JavaScriptSerializer().Serialize(new
            {
                IdConsultas = idConsult,
                Status = ConsultationStatus.Invoiced
            });

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PostAsync(url, new StringContent(loginJson, Encoding.UTF8, "application/json"));
                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            throw new Exception();
        }

        public async Task<ActionResult> CancelConsult(string idConsult)
        {
            var url = "https://doctorwebservice.azurewebsites.net/api/Consultas/ChangeStatusConsultation";
            var loginJson = new JavaScriptSerializer().Serialize(new
            {
                IdConsultas = idConsult,
                Status = ConsultationStatus.Open
            });

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PostAsync(url, new StringContent(loginJson, Encoding.UTF8, "application/json"));
                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<Hubs.DashBoardHub>();
                    hubContext.Clients.All.refreshTable();


                    return RedirectToAction("Index", "Home");
                }
            }

            throw new Exception();
        }

        public PartialViewResult _TabSoap(string idConsulta)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Soap";

            ViewBag.idConsulta = idConsulta;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<SoapResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabSoap", details);
                }
            }
            return PartialView("_TabSoap");
        }

        [HttpPost]
        public async Task<JsonResult> SetTabSoap(string idConsulta, SoapResponse model)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Soap";

            var serializedModel = new JavaScriptSerializer().Serialize(model);

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PutAsync(url, new StringContent(serializedModel, Encoding.UTF8, "application/json")); ;
                return Json(new { IsSuccess = wcfResponse.IsSuccessStatusCode });
            }
        }

        public PartialViewResult _TabAnexos(string idConsulta)
        {
            return PartialView("_TabAnexos");
        }

        public ActionResult _TabReceitas(string idConsulta)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Recipes";

            ViewBag.idConsulta = idConsulta;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<RecipesResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabReceitas", details);
                }
            }

            var receitas = new List<RecipesResponse>();
            return PartialView("_TabReceitas", receitas);

        }

        public PartialViewResult _TabMCDT(string idConsulta)
        {
            ViewBag.idConsulta = idConsulta;
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Mcdt";
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<McdtResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabMCDT", details);
                }
            }
            return PartialView("_TabMCDT");
        }

        public PartialViewResult _TabMedicacao(string idConsulta)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Medication";
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<MedicationInfoResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabMedicacao", details);
                }
            }
            return PartialView("_TabMedicacao");
        }

        #region TabParametros
        public PartialViewResult _TabParametros(string idConsulta)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Parameters";

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<ParametersInfoResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Session["parametersList"] = details;
                    return PartialView("_TabParametros", details);
                }
            }


            return PartialView("_TabParametros");
        }


        public PartialViewResult _ModalViewParametroInfo(string idConsulta, int idParameter, string startDate, string endDate)
        {
            ViewBag.idConsulta = idConsulta;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate;
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/ParametersDetails/{idParameter}/From/{startDate}/To/{endDate}";

            if (Session["parametersList"] != null)
            {
                var p = Session["parametersList"] as List<ParametersInfoResponse>;

                if (p != null)
                {
                    ViewBag.ParameterName = p.SingleOrDefault(s => s.IdParameter == idParameter).ParameterName;
                }

            }

            switch ((string)ViewBag.ParameterName)
            {
                case "Colesterol"://Colesterol
                    ViewBag.LabelsGraph = "\"Total\",\"Trigliceridos\",\"HDL\",\"LDL\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "Colesterol Total", "Trigliceridos", "HDL", "LDL", "Notas" };
                    break;
                case "Glicemia"://Glicemia
                    ViewBag.LabelsGraph = "\"Valor\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "Valor", "Contexto", "Notas" };
                    break;
                case "Insulina"://Insulina
                    ViewBag.LabelsGraph = "\"UI\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "UI", "Insulina", "Contexto", "Notas" };
                    break;
                case "Outro (Default)"://Outro (Default)
                    ViewBag.LabelsGraph = "\"Valor\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "Valor", "Notas" };
                    break;
                case "Peso e Medidas"://Peso e Medidas
                    ViewBag.LabelsGraph = "\"Peso\",\"IMC\",\"Peito\",\"Cintura\",\"Ancas\",\"Bra esq\",\"Bra dto\",\"P.esq\",\"P.dta\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "Peso", "IMC", "Peito", "Cintura", "Ancas", "Bra esq", "Bra dto", "P.esq", "P.dta", "Notas" };
                    break;
                case "Tensao Arterial"://Tensao Arterial
                    ViewBag.LabelsGraph = "\"Min\",\"Max\",\"Bpm\"";
                    ViewBag.HeaderTable = new List<string>() { "Data", "Min", "Max", "Bpm", "Notas" };
                    break;

            }

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<ParametersDetailsResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    string cad = "";

                    var dataSets = new List<string>();

                    switch ((string)ViewBag.ParameterName)
                    {
                        case "Colesterol"://Colesterol

                            var data = new List<ColesterolViewModel>();

                            foreach (var item in details)
                            {
                                data.Add(new ColesterolViewModel
                                {
                                    MedicationDate = item.MedicationDate.Value.ToShortDateString(),
                                    MedicationTime = item.MedicationTime.Value.ToString(),
                                    Hdl = item.ColesterolHdl?.ToString(),
                                    Ldl = item.ColesterolLdl?.ToString(),
                                    Total = item.ColesterolTotal?.ToString(),
                                    Trigliceridos = item.ColesterolTrigliceridos?.ToString(),
                                    Observation = item.Observation

                                });

                                cad = item.ColesterolTotal.ToString() + "," + item.ColesterolTrigliceridos.ToString() + "," + item.ColesterolHdl.ToString() + "," + item.ColesterolLdl.ToString();
                                dataSets.Add(cad);
                            }                            

                            ViewBag.DataGraph = dataSets;
                            break;
                        case "Glicemia":
                            break;
                        case "Insulina":
                            break;
                        case "Peso e Medidas":
                            break;
                        case "Tensao Arterial":
                            break;
                       default:
                            break;
                    }



                    return PartialView("_ModalParametroInfo", details);
                }
            }


            //switch (idParameter)
            //{
            //    case 1://Colesterol
            //        return RedirectToAction("_ModalParametroColesterolInfo", new { idPatient  = idConsulta });                    
            //    case 2://Glicemia
            //        break;
            //    case 3://Insulina
            //        break;
            //    case 4://Outro (Default)
            //        break;
            //    case 5://Peso e Medidas
            //        break;
            //    case 6://Tensao Arterial
            //        break;

            //}

            //return null;
            return PartialView("_ModalParametroInfo");
        }

        public PartialViewResult _ModalParametroColesterolInfo(string idPatient)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idPatient}/ParametersColesterol ";
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<ParameterColesterolResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_ModalParametroColesterolInfo", details);
                }
            }

            return null;
        }

        #endregion
        public PartialViewResult _TabHistorico(string idConsulta)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/PatientHistory";
            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var details = JsonConvert.DeserializeObject<List<PatientHistoryInfoResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return PartialView("_TabHistorico", details);
                }
            }
            return PartialView("_TabHistorico");
        }

        #region Modals

        public PartialViewResult _ModalAddReceita(string idConsulta)
        {
            ViewBag.idConsulta = idConsulta;

            Session["ReceitaProducts"] = null;

            return PartialView("_ModalReceita", new RecipesResponse());
        }

        public PartialViewResult _ModalReceitaTable(bool isReadonly)
        {
            ViewBag.isReadonly = isReadonly;

            if (Session["ReceitaProducts"] == null)
                Session["ReceitaProducts"] = new List<RecipeProductViewModel>();

            var model = (List<RecipeProductViewModel>)Session["ReceitaProducts"];

            return PartialView("_ModalReceitaTable", model);
        }

        [HttpPost]
        public ActionResult _ModalReceitaTableSet(bool isReadonly, List<RecipeProductViewModel> model = null)
        {
            Session["ReceitaProducts"] = model;

            return RedirectToAction("_ModalReceitaTable", new { isReadonly = false });
        }

        public ActionResult _ModalReceitaTableDeleteRow(bool isReadonly, int index)
        {
            var model = (List<RecipeProductViewModel>)Session["ReceitaProducts"];

            model.Remove(model.Last());

            model.RemoveAt(index);

            Session["ReceitaProducts"] = model;

            return RedirectToAction("_ModalReceitaTable", new { isReadonly });
        }

        public ActionResult _ModalReceitaTableEditRow(int index)
        {
            var model = (List<RecipeProductViewModel>)Session["ReceitaProducts"];
            model.Remove(model.Last());

            ViewBag.isReadonly = true;

            ViewBag.Index = index;

            return PartialView("_ModalReceitaTable", model);
        }

        public PartialViewResult _ModalAddMcdt(string idConsulta)
        {
            ViewBag.idConsulta = idConsulta;
            Session["McdtExame"] = null;
            return PartialView("_ModalMCDT", new McdtResponse());
        }

        public PartialViewResult _ModalMcdtTable(bool isReadonly)
        {
            ViewBag.isReadonly = isReadonly;

            if (Session["McdtExame"] == null)
                Session["McdtExame"] = new List<McdtExamsResponse>();

            var model = (List<McdtExamsResponse>)Session["McdtExame"];

            return PartialView("_ModalMcdtTable", model);
        }

        [HttpPost]
        public ActionResult _ModalMcdtTableSet(bool isReadonly, List<McdtExamsResponse> model = null)
        {
            Session["McdtExame"] = model;
            return RedirectToAction("_ModalMcdtTable", new { isReadonly = false });
        }

        public ActionResult _ModalMcdtTableDeleteRow(bool isReadonly, int index)
        {
            var model = (List<McdtExamsResponse>)Session["McdtExame"];
            model.Remove(model.Last());
            model.RemoveAt(index);
            Session["McdtExame"] = model;

            return RedirectToAction("_ModalMcdtTable", new { isReadonly });
        }

        public ActionResult _ModalMcdtTableEditRow(int index)
        {
            ViewBag.isReadonly = true;
            ViewBag.Index = index;

            var model = (List<McdtExamsResponse>)Session["McdtExame"];
            model.Remove(model.Last());
            return PartialView("_ModalMcdtTable", model);
        }

        public PartialViewResult _ModalEditReceita(string idConsulta, string idReceita)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Recipes/{idReceita}";

            ViewBag.idConsulta = idConsulta;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<RecipesResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Session["ReceitaProducts"] = RecipeProductViewModel.ParseList(model.RecipeProducts);

                    return PartialView("_ModalReceita", model);
                }
            }
            return PartialView("_ModalReceita");
        }

        public PartialViewResult _ModalEditMcdt(string idConsulta, int idMcdt)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Mcdt/{idMcdt}";
            ViewBag.idConsulta = idConsulta;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<McdtResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Session["McdtExame"] = model.McdtExams;
                    return PartialView("_ModalMCDT", model);
                }
            }
            return PartialView("_ModalMCDT");
        }

        public PartialViewResult _ModalViewReceita(string idConsulta, string idReceita)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Recipes/{idReceita}";

            ViewBag.idConsulta = idConsulta;
            ViewBag.ReadOnly = true;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<RecipesResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Session["ReceitaProducts"] = RecipeProductViewModel.ParseList(model.RecipeProducts);

                    return PartialView("_ModalReceita", model);
                }
            }
            return PartialView("_ModalReceita");
        }

        public PartialViewResult _ModalViewMcdt(string idConsulta, string idMcdt)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Mcdt/{idMcdt}";

            ViewBag.idConsulta = idConsulta;
            ViewBag.ReadOnly = true;

            using (var client = new HttpClient())
            {
                var wcfResponse = client.GetAsync(url).Result;
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<McdtResponse>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Session["McdtExame"] = model.McdtExams;
                    return PartialView("_ModalMCDT", model);
                }
            }
            return PartialView("_ModalMCDT");
        }

        #endregion

        #region Receitas 
        public async Task<ActionResult> AddEditReceita(RecipesResponse model, string idConsulta)
        {
            string urlAdd = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Recipes/New";
            string urlEdit = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Recipes/{model.IdRecipes}";

            var products = RecipeProductViewModel.GetBaseList((List<RecipeProductViewModel>)Session["ReceitaProducts"]);

            products.Remove(products.Last());

            model.RecipeProducts = products;

            var receita = new JavaScriptSerializer().Serialize(model);

            using (var client = new HttpClient())
            {
                HttpResponseMessage wcfResponse;

                if (model.IdRecipes > 0)
                    wcfResponse = await client.PutAsync(urlEdit, new StringContent(receita, Encoding.UTF8, "application/json"));
                else
                    wcfResponse = await client.PostAsync(urlAdd, new StringContent(receita, Encoding.UTF8, "application/json"));

                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    return RedirectToAction("_TabReceitas", new { idConsulta });
                }
            }
            return RedirectToAction("_TabReceitas", new { idConsulta });
        }

        public async Task<ActionResult> DeleteReceita(string idConsulta, string idReceita)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Recipes/{idReceita}";


            using (var client = new HttpClient())
            {
                HttpResponseMessage wcfResponse = await client.DeleteAsync(url);
                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    return RedirectToAction("_TabReceitas", new { idConsulta });
                }
            }
            return RedirectToAction("_TabReceitas", new { idConsulta });
        }

        public async Task<ActionResult> AddEditMcdt(McdtResponse model, string idConsulta)
        {
            string urlAdd = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Mcdt/New";
            string urlEdit = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Mcdt/{model.IdMcdt}";

            var examenes = (List<McdtExamsResponse>)Session["McdtExame"];
            examenes.Remove(examenes.Last());
            model.McdtExams = examenes;

            var receita = new JavaScriptSerializer().Serialize(model);

            using (var client = new HttpClient())
            {
                HttpResponseMessage wcfResponse;

                if (model.IdMcdt > 0)
                    wcfResponse = await client.PutAsync(urlEdit, new StringContent(receita, Encoding.UTF8, "application/json"));
                else
                    wcfResponse = await client.PostAsync(urlAdd, new StringContent(receita, Encoding.UTF8, "application/json"));

                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    return RedirectToAction("_TabMCDT", new { idConsulta });
                }
            }
            return RedirectToAction("_TabMCDT", new { idConsulta });
        }

        public async Task<ActionResult> DeleteMcdt(string idConsulta, string idMcdt)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/{idConsulta}/Mcdt/{idMcdt}";

            using (var client = new HttpClient())
            {
                HttpResponseMessage wcfResponse = await client.DeleteAsync(url);
                if (wcfResponse.StatusCode == HttpStatusCode.OK || wcfResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    return RedirectToAction("_TabMCDT", new { idConsulta });
                }
            }
            return RedirectToAction("_TabMCDT", new { idConsulta });
        }

        #endregion

        public async Task<JsonResult> SearchProducts(string code, string name)
        {
            string url = $"https://doctorwebservice.azurewebsites.net/api/Consultas/Recipes/Products";

            var input = new JavaScriptSerializer().Serialize(new
            {
                code,
                name
            });

            using (var client = new HttpClient())
            {
                var wcfResponse = await client.PostAsync(url, new StringContent(input, Encoding.UTF8, "application/json"));
                if (wcfResponse.IsSuccessStatusCode)
                {
                    var json = wcfResponse.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<List<IFarmacias.Models.Response.ProductsResponse>>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    return Json(model.Select(x => $"<input type='hidden' value='{x.Code}' />{x.MedicationName}"), JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { });
        }



    }
}