using System.ComponentModel.DataAnnotations;

namespace SmartGrower.Models
{
    public class Medicao
    {
        [Required]
        public int MedicaoId { get; set; }
        [Required]
        public int TipoMedicaoId { get; set; }

        [Required]
        public decimal Leitura { get; set; }
        [Required]
        public System.DateTime DataMedicao { get; set; }
        public virtual TipoMedicao TipoMedicao { get; set; }
    }
}
