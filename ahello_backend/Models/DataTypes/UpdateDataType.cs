namespace ahello_backend.Models.DataTypes
{
    public class UpdateDataType
    {
        public int DataTypeId { get; set; }

        public string DataTypeName { get; set; }

        public string DataTypeCode { get; set; }

        public bool IsActive { get; set; }

        public int ModifiedBy { get; set; }
    }
}
