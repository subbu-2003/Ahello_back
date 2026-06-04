namespace ahello_backend.Models.Category
{
    public class UpdateCategoryField
    {
        public int CategoryFieldId { get; set; }

        public int CategoryId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public bool IsRequired { get; set; }

        public bool IsActive { get; set; }

        public int DataTypeId { get; set; }


        public string ModifiedBy { get; set; }
    }
}
