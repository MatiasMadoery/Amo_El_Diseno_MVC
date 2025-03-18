using System.ComponentModel.DataAnnotations;

namespace AmoElDiseno.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        [Display(Name = "Nombre")]
        public string? Name { get; set; }
        [Display(Name = "Dirección")]
        public string? Adress { get; set; }
        [Display(Name = "Localidad")]
        public string? Locality { get; set; }
        [Display(Name = "Código Postal")]
        public int? PostalCode { get; set; }
        [Display(Name = "Provincia")]
        public string? Region { get; set; }
        [Display(Name = "Teléfono")]
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? DNI { get; set; }
        [Display(Name = "Insumo")]
        public string? Input { get; set; }

        //BANK DATA
        [Display(Name = "Número de Cuenta")]
        public int? AccountNumber { get; set; }
        [Display(Name = "Banco")]
        public string? Bank { get; set; }
        [Display(Name = "Sucursal")]
        public string? Branch { get; set; }
        [Display(Name = "CBU/ALIAS")]
        public string? Cbu { get; set; }

        //SHIPPING DATA
        [Display(Name = "Transporte")]
        public string? Transport { get; set; }
        [Display(Name = "Flete en")]
        public string? FreightIn {  get; set; }
        [Display(Name = "Entrega")]
        public string? Delivery { get; set; }
        [Display(Name = "Dirección de Envío")]
        public string? ShippAdress { get; set; }
        [Display(Name = "Observaciones")]
        public string? Observations { get; set; }
    }
}
