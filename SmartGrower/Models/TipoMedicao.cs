using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;

namespace SmartGrower.Models
{
    public class TipoMedicao
    {
        [Required]
        public int TipoMedicaoId { get; set; }
        [Required]
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public virtual ICollection<Medicao> Medicoes { get; set; }
    }

    public enum TipoMedicaoEnum
    {

        HumidadeSolo = 1,
        Temperatura = 2,
        HumidadeAr = 3

    }
}
