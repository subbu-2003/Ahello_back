using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace ahello_backend.Models.Forms
{
    public class FormDynamicGetResponse
    {
        public int FormId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public List<FormDynamicFieldResponse> Fields { get; set; }
            = new();
        public List<FormDropdownOptionResponse> DropdownOptions
        { get; set; } = new();
    }

    public class FormDynamicFieldResponse
    {
        public int FormFieldId { get; set; }
        public string FieldName { get; set; }
        public string FieldCode { get; set; }
        public string Placeholder { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public int DataTypeId { get; set; }
        public string DataTypeName { get; set; }
        public List<FormDropdownOptionResponse> DropDownOptions
        { get; set; } = new();
    }

    public class FormDropdownOptionResponse
    {
        public int FormDropDownId { get; set; }
        public int FormFieldId { get; set; }
        public int FormId { get; set; }
        public string OptionValue { get; set; }
        public string OptionLabel { get; set; }
        public bool IsActive { get; set; }
    }

    public class FormDynamicFieldPost
    {
        public int FormFieldId { get; set; }
        public string FieldValue { get; set; }
        public List<FormDropdownOptionPost> DropDownOptions
        { get; set; } = new();
    }

    public class FormDropdownOptionPost
    {
        public string? OptionValue { get; set; }
        public string OptionLabel { get; set; }
        public bool IsActive { get; set; }
    }

    public class FormDynamicPost
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public IFormFile? FieldFile { get; set; }
        public int FormFieldId { get; set; }
        public string? FieldValue { get; set; }
        public string? DropDownOptionsJson { get; set; }
        public List<FormDynamicFieldPost> Fields
        { get; set; } = new();
    }



    public class FormDynamicPut
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public int? ModifiedBy { get; set; }
        public List<FormDynamicFieldPost> Fields
        { get; set; } = new();
    }
}