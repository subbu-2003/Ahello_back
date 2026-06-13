using ahello_backend.Models.Forms;

namespace ahello_backend.Models.Form
{
    public class FormTemplatePut
    {
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }

        public int? ModifiedBy { get; set; }

        public List<FormTemplateFieldPut> Fields { get; set; }
            = new();
    }
    public class FormTemplateFieldPut
    {
        public int FormFieldId { get; set; }   // For update

        public string FieldName { get; set; }

        public string Placeholder { get; set; }

        public string Description { get; set; }

        public bool IsRequired { get; set; }

        public int DataTypeId { get; set; }

        public List<FormDropdownOptionPost> DropdownOptions
        { get; set; } = new();


    }
}
