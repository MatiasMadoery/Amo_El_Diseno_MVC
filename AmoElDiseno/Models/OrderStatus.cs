using System.ComponentModel.DataAnnotations;

namespace AmoElDiseno.Models
{
    public enum OrderStatus
    {
        Presupuestado,
        EnDiseño,
        Aprobado,
        EnProducción,
        EnConfeccción,
        EntregaPendiente,
        Entregado
    }
}
