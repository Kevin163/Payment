using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GemstarPaymentCore.Data
{
    [Table("cyUserInfo")]
    public class CYUserInfo
    {
        [Key]
        [Column("v_SeriesNo")]
        public string SeriesNo { get; set; }
    }
}
