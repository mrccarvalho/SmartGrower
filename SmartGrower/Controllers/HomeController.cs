using SmartGrower.Data;
using SmartGrower.Models;
using SmartGrower.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.SignalR;



namespace SmartGrower.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ArduinoDbContext _context;

        public HomeController(ILogger<HomeController> logger, ArduinoDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Home page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var vm = new RelatorioVm { };

            vm.LastSet = MaisRecentes();


            return View(vm);
        }

        /// <summary>
        /// devolve leituras mais recentes de um dispositivo específico
        /// </summary>
        
        /// <returns></returns>
        public MedicaoVm MaisRecentes()
        {
            var recente = new MedicaoVm();

            var last3 = _context.Medicoes.
                OrderByDescending(m => m.DataMedicao).Take(3).ToList();

            if (last3.Any())
            {
                var humsolo = last3.FirstOrDefault(m => m.TipoMedicaoId == 1);
                var temp = last3.FirstOrDefault(m => m.TipoMedicaoId == 2);
                var humar = last3.FirstOrDefault(m => m.TipoMedicaoId == 3);

                if (humsolo != null)
                {
                    recente.DataMedicao = humsolo.DataMedicao;
                    recente.HumidadeSolo = humsolo.Leitura;
                }

                if (temp != null) { recente.Temperatura = temp.Leitura; }
                if (humar != null) { recente.HumidadeAr = humar.Leitura; }
            }

            return recente;
        }

        /// <summary>
        /// devolve leituras
        /// </summary>
        /// <returns></returns>
      

        /// <summary>
        /// devolve leituras
        /// </summary>
        /// <returns></returns>
        public IActionResult ListaTodas()
        {
            List<Medicao> recente = new List<Medicao>();

            recente = _context.Medicoes.
                Include(t => t.TipoMedicao).
                OrderByDescending(m => m.DataMedicao).ToList();

           

            return View(recente);
        }





        /// <summary>
        /// Method that receives the three values from the device and posts them to the database
        /// </summary>
        /// <param name="humidade_solo"></param>
        /// /// <param name="temperatura"></param>
        /// /// <param name="humidade_ar"></param>
        /// <returns></returns>
        public ActionResult GravarLeituras(decimal? humidade_solo, decimal? temperatura, decimal? humidade_ar)
        {
            var results = "Sem erros encontrados";
            var reported = DateTime.Now;
            try
            {
                    if (humidade_solo.HasValue)
                    {
                    // adiciona leitura sensor humidade solo
                    _context.Medicoes.Add(new Medicao
                        {
                            TipoMedicaoId = (int)TipoMedicaoEnum.HumidadeSolo,
                            Leitura = humidade_solo.Value,
                            DataMedicao = reported
                        });

                    // adiciona temperatura
                    _context.Medicoes.Add(new Medicao
                    {
                        TipoMedicaoId = (int)TipoMedicaoEnum.Temperatura,
                        Leitura = temperatura.Value,
                        DataMedicao = reported
                    });

                    // adiciona humidade ar
                    _context.Medicoes.Add(new Medicao
                    {
                        TipoMedicaoId = (int)TipoMedicaoEnum.HumidadeAr,
                        Leitura = humidade_ar.Value,
                        DataMedicao = reported
                    });
                }
                    _context.SaveChanges();
            }
            catch (Exception ex)
            {
                results = "Erro: " + ex.Message;
            }
            return Content(results);
        }

        /// <summary>
        /// Devolve dados por dia/24 horas para um determinado dispositivo
        /// </summary>
        /// <returns></returns>
        public IActionResult Last24Hour()
        {
            // establish an empty table
            var gdataTable = new GoogleVizDataTable();
          
                // next get the most recent measurement for this device
                var mostRecent = _context.Medicoes
                    .Select(m => m).OrderByDescending(m => m.DataMedicao).Take(1).FirstOrDefault();

                // if we have a recent measurement for this device
                if (mostRecent != null)
                {
                    // establish a range of previous to current day/time
                    var finish = mostRecent.DataMedicao;
                    var start = finish.AddDays(-1);

                    // fetch a set of measurements for that range
                    var recentSet = MedicaoSetRange(start, finish);

                    return Json(recentSet);
                }

                return NotFound();
           
        }

        /// <summary>
        /// Build an aggregate list last day's worth of measurements, i.e.
        /// from the most recent measurement back to 24 hours previous, but
        /// averaged by hour
        /// </summary>
        /// <param name="start">Start date/time for which to fetch set of measurements</param>
        /// <param name="finish">Finishing date/time for which to fetch set of measurements</param>
        /// <returns></returns>
        public List<MedicaoVm> MedicaoSetRange(DateTime start, DateTime finish)
        {
            // constrói o conjunto de medições
            var measureSet =
               (from m in _context.Medicoes.Select(m => m).AsEnumerable()
                where m.DataMedicao >= start && m.DataMedicao <= finish
                orderby m.DataMedicao
                group m by new { MeasuredDate = DateTime.Parse(m.DataMedicao.ToString("yyyy-MM-dd HH:mm:ss"))}
                    into g
                 select new MedicaoVm
                 {
                     DataMedicao = g.Key.MeasuredDate,
                     HumidadeSolo = g.Where(m => m.TipoMedicaoId == 1).Select(r => r.Leitura).FirstOrDefault(),
                     Temperatura = g.Where(m => m.TipoMedicaoId == 2).Select(r => r.Leitura).FirstOrDefault(),
                     HumidadeAr = g.Where(m => m.TipoMedicaoId == 3).Select(r => r.Leitura).FirstOrDefault()

                 }).ToList();

            return measureSet;
        }

        public IActionResult Todas()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
