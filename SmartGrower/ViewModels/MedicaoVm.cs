namespace SmartGrower.ViewModels
{
    public class MedicaoVm
    {
        public DateTime? DataMedicao { get; set; }
        public decimal? HumidadeSolo { get; set; }
        public decimal? Temperatura { get; set; }
        public decimal? HumidadeAr { get; set; }

        public string GoogleDate => (DataMedicao.HasValue)
? string.Format("Date({0})", DataMedicao.Value.ToString("yyyy,M,d,H,m,s,f"))
: string.Empty;
        public string DateOnlyString => DataMedicao?.ToString("yyyy-MM-dd") ?? string.Empty;
        public string TimeOnlyString => DataMedicao?.ToString("hh:mm:ss tt") ?? string.Empty;

        public string HumidadeSoloString => (HumidadeSolo != null) ? HumidadeSolo.Value.ToString("###.0") : "0.0";
        public string TemperaturaString => (Temperatura != null) ? Temperatura.Value.ToString("###.0") : "0.0";
        public string HumidadeArString => (HumidadeAr != null) ? HumidadeAr.Value.ToString("###.0") : "0.0";

    }
}
