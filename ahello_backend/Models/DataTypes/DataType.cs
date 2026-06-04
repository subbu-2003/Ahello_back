namespace ahello_backend.Models.DataTypes
{
    public class DataType
    {
        public int DataTypeId { get; set; }

        public string DataTypeName { get; set; }

        public string DataTypeCode { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public int? ModifiedBy { get; set; }
    }
}
